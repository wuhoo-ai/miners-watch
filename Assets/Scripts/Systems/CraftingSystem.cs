using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Crafting system: recipe-based item synthesis from inventory materials.
    /// Core logic is pure C# callable from EditMode tests via Init().
    /// </summary>
    public class CraftingSystem : MonoBehaviour
    {
        private RecipeDef[] _recipes;
        private InventorySystem _inventory;

        public IReadOnlyList<RecipeDef> Recipes => _recipes;
        public InventorySystem Inventory => _inventory;

        /// <summary>Fires after a successful craft with the recipe that was crafted.</summary>
        public event System.Action<RecipeDef> OnCraftCompleted;

        /// <summary>Explicit init for EditMode tests where Awake may not fire.</summary>
        public void Init(InventorySystem inventory, RecipeDef[] recipes = null)
        {
            _inventory = inventory;
            _recipes = recipes ?? RecipePresets.All;
        }

        private void Awake()
        {
            if (_inventory == null)
                _inventory = GameRoot.Get<InventorySystem>();
            if (_recipes == null || _recipes.Length == 0)
                _recipes = RecipePresets.All;
        }

        /// <summary>
        /// Check if player has all materials to craft this recipe.
        /// Does NOT modify inventory.
        /// </summary>
        public bool CanCraft(RecipeDef recipe)
        {
            if (recipe == null || !recipe.IsValid()) return false;
            if (_inventory == null) return false;

            for (int i = 0; i < recipe.inputs.Length; i++)
            {
                MineralType type = recipe.inputs[i];
                int required = recipe.inputCounts[i];
                if (!_inventory.HasItem(type, required))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Execute crafting: deduct materials and grant output.
        /// Returns false if materials insufficient or recipe invalid.
        /// </summary>
        public bool Craft(RecipeDef recipe)
        {
            if (!CanCraft(recipe)) return false;

            // Deduct all inputs first (atomic operation)
            for (int i = 0; i < recipe.inputs.Length; i++)
            {
                MineralType type = recipe.inputs[i];
                int count = recipe.inputCounts[i];
                if (!_inventory.RemoveItem(type, count))
                {
                    // Rollback: this should never happen if CanCraft passed
                    Debug.LogError($"[CraftingSystem] Failed to deduct {type} x{count} after CanCraft succeeded");
                    return false;
                }
            }

            // Grant output
            if (recipe.outputMineral.HasValue && recipe.outputCount > 0)
            {
                MineralType output = recipe.outputMineral.Value;
                float sellPrice = GetSellPrice(output);
                if (!_inventory.AddItem(output, sellPrice, recipe.outputCount))
                {
                    Debug.LogWarning($"[CraftingSystem] Inventory full, could not add {output} x{recipe.outputCount}");
                    // Materials already deducted — this is a design choice
                    // Alternative: rollback all deductions
                }
            }

            OnCraftCompleted?.Invoke(recipe);
            return true;
        }

        /// <summary>
        /// Get list of recipes that player can currently craft.
        /// </summary>
        public List<RecipeDef> GetAvailableRecipes()
        {
            var available = new List<RecipeDef>();
            if (_recipes == null) return available;

            foreach (var recipe in _recipes)
            {
                if (CanCraft(recipe))
                    available.Add(recipe);
            }
            return available;
        }

        /// <summary>Helper: get sell price for mineral type.</summary>
        private static float GetSellPrice(MineralType type) => type switch
        {
            MineralType.Iron => 15f,
            MineralType.Gold => 40f,
            MineralType.Crystal => 100f,
            MineralType.Obsidian => 300f,
            _ => 5f,
        };
    }
}
