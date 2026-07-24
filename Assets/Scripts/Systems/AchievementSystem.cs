using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>Static achievement definition.</summary>
    public class AchievementDef
    {
        public readonly string id;
        public readonly string title;
        public readonly string description;

        public AchievementDef(string id, string title, string description)
        {
            this.id = id;
            this.title = title;
            this.description = description;
        }
    }

    /// <summary>
    /// Achievement system: event-driven unlock, JSON persistence, UI notification.
    /// Core logic is pure C# callable from EditMode tests via Init().
    /// Subscribe to game events at runtime; call Notify* methods directly in tests.
    /// </summary>
    public class AchievementSystem : MonoBehaviour
    {
        // ── 17 predefined achievements ──────────────────────────
        public static readonly AchievementDef[] AllAchievements =
        {
            new AchievementDef("first_kill",        "First Blood",       "Kill your first enemy"),
            new AchievementDef("kill_10",           "Pest Control",      "Kill 10 enemies"),
            new AchievementDef("kill_50",           "Exterminator",      "Kill 50 enemies"),
            new AchievementDef("kill_100",          "Annihilator",       "Kill 100 enemies"),
            new AchievementDef("first_craft",       "Apprentice",        "Craft your first item"),
            new AchievementDef("craft_5",           "Journeyman",        "Craft 5 items"),
            new AchievementDef("craft_20",          "Master Crafter",    "Craft 20 items"),
            new AchievementDef("no_damage_night",   "Untouchable",       "Survive a night without taking damage"),
            new AchievementDef("survive_3_nights",  "Veteran",           "Survive 3 nights"),
            new AchievementDef("survive_10_nights", "Nightmare Slayer",  "Survive 10 nights"),
            new AchievementDef("first_gold",        "Gold Digger",       "Earn your first gold"),
            new AchievementDef("rich_500",          "Rich Miner",        "Accumulate 500 gold"),
            new AchievementDef("collect_all",       "Geologist",         "Collect all 5 mineral types"),
            new AchievementDef("reach_medium",      "Spelunker",         "Reach the Medium cave"),
            new AchievementDef("reach_deep",        "Deep Diver",        "Reach the Deep cave"),
            new AchievementDef("first_upgrade",     "Upgraded",          "Buy your first upgrade"),
            new AchievementDef("full_inventory",    "Hoarder",           "Fill your inventory completely"),
        };

        // ── runtime state ───────────────────────────────────────
        private readonly HashSet<string> _unlocked = new HashSet<string>();
        private int _killCount;
        private int _craftCount;
        private int _nightsSurvived;
        private bool _tookDamageThisNight;
        private readonly HashSet<MineralType> _minedTypes = new HashSet<MineralType>();
        private int _peakGold;

        // ── events ──────────────────────────────────────────────
        /// <summary>Fires when an achievement is newly unlocked. UI subscribes for toast.</summary>
        public event Action<AchievementDef> OnAchievementUnlocked;

        // ── properties ──────────────────────────────────────────
        public int UnlockedCount => _unlocked.Count;
        public int KillCount => _killCount;
        public int CraftCount => _craftCount;
        public int NightsSurvived => _nightsSurvived;

        // ── lifecycle ───────────────────────────────────────────

        /// <summary>Explicit init for EditMode tests where Awake may not fire.</summary>
        public void Init()
        {
            _unlocked.Clear();
            _killCount = 0;
            _craftCount = 0;
            _nightsSurvived = 0;
            _tookDamageThisNight = false;
            _minedTypes.Clear();
            _peakGold = 0;
        }

        private void Awake() => Init();

        // ── runtime wiring (call from GameRoot or scene bootstrap) ──

        /// <summary>Subscribe to all game events. Call once at runtime after systems exist.</summary>
        public void WireEvents(
            CraftingSystem crafting,
            DayNightCycle cycle,
            PlayerHP playerHP,
            MiningSystem mining,
            UpgradeSystem upgrades,
            DepthProgression depth,
            InventorySystem inventory)
        {
            if (crafting != null) crafting.OnCraftCompleted += _ => NotifyCraftCompleted();
            if (cycle != null) cycle.OnPhaseChanged += OnPhaseChanged;
            if (mining != null) mining.OnMineralMined += NotifyMineralMined;
            if (upgrades != null)
            {
                upgrades.OnGoldChanged += NotifyGoldChanged;
                upgrades.OnUpgraded += _ => NotifyUpgradeBought();
            }
            if (depth != null) depth.OnDepthUnlocked += NotifyDepthReached;
            if (inventory != null) inventory.OnItemAdded += _ => NotifyInventoryChanged(inventory);
        }

        private void OnPhaseChanged(DayNightPhase phase)
        {
            if (phase == DayNightPhase.Night)
            {
                _tookDamageThisNight = false; // reset at night start
            }
            else if (phase == DayNightPhase.Day)
            {
                NotifyNightSurvived(_tookDamageThisNight);
            }
        }

        // ── Notify* — pure logic, testable without MonoBehaviour ──

        /// <summary>Call when an enemy is killed.</summary>
        public void NotifyEnemyKilled()
        {
            _killCount++;
            TryUnlock("first_kill", _killCount >= 1);
            TryUnlock("kill_10", _killCount >= 10);
            TryUnlock("kill_50", _killCount >= 50);
            TryUnlock("kill_100", _killCount >= 100);
        }

        /// <summary>Call when a craft completes.</summary>
        public void NotifyCraftCompleted()
        {
            _craftCount++;
            TryUnlock("first_craft", _craftCount >= 1);
            TryUnlock("craft_5", _craftCount >= 5);
            TryUnlock("craft_20", _craftCount >= 20);
        }

        /// <summary>Call when a night is survived. tookDamage = whether player was hit during the night.</summary>
        public void NotifyNightSurvived(bool tookDamage)
        {
            _nightsSurvived++;
            TryUnlock("no_damage_night", !tookDamage);
            TryUnlock("survive_3_nights", _nightsSurvived >= 3);
            TryUnlock("survive_10_nights", _nightsSurvived >= 10);
        }

        /// <summary>Call when player takes damage (for no-damage-night tracking).</summary>
        public void NotifyPlayerDamaged()
        {
            _tookDamageThisNight = true;
        }

        /// <summary>Call when a mineral is mined.</summary>
        public void NotifyMineralMined(MineralType type)
        {
            _minedTypes.Add(type);
            TryUnlock("collect_all", _minedTypes.Count >= Enum.GetValues(typeof(MineralType)).Length);
        }

        /// <summary>Call when gold amount changes.</summary>
        public void NotifyGoldChanged(int gold)
        {
            if (gold > _peakGold) _peakGold = gold;
            TryUnlock("first_gold", gold > 0);
            TryUnlock("rich_500", _peakGold >= 500);
        }

        /// <summary>Call when a depth level is reached/unlocked.</summary>
        public void NotifyDepthReached(DepthLevel depth)
        {
            TryUnlock("reach_medium", depth >= DepthLevel.Medium);
            TryUnlock("reach_deep", depth >= DepthLevel.Deep);
        }

        /// <summary>Call when an upgrade is purchased.</summary>
        public void NotifyUpgradeBought()
        {
            TryUnlock("first_upgrade", true);
        }

        /// <summary>Call when inventory changes to check full-inventory achievement.</summary>
        public void NotifyInventoryChanged(InventorySystem inventory)
        {
            if (inventory == null) return;
            TryUnlock("full_inventory", inventory.UsedSlots >= inventory.Capacity);
        }

        // ── query ───────────────────────────────────────────────

        /// <summary>Check if an achievement is unlocked by id.</summary>
        public bool IsUnlocked(string id) => _unlocked.Contains(id);

        /// <summary>Get all unlocked achievement ids.</summary>
        public IReadOnlyCollection<string> UnlockedIds => _unlocked;

        /// <summary>Get the AchievementDef for an id, or null.</summary>
        public static AchievementDef GetDef(string id) =>
            AllAchievements.FirstOrDefault(a => a.id == id);

        // ── persistence ─────────────────────────────────────────

        /// <summary>Write unlocked achievement ids into SaveData.</summary>
        public void SaveTo(SaveData data)
        {
            if (data == null) return;
            data.unlockedAchievements = _unlocked.ToList();
        }

        /// <summary>Restore unlocked achievements from SaveData.</summary>
        public void LoadFrom(SaveData data)
        {
            if (data?.unlockedAchievements == null) return;
            foreach (string id in data.unlockedAchievements)
            {
                if (!string.IsNullOrEmpty(id))
                    _unlocked.Add(id);
            }
        }

        // ── internal ────────────────────────────────────────────

        private void TryUnlock(string id, bool condition)
        {
            if (!condition || _unlocked.Contains(id)) return;

            _unlocked.Add(id);
            var def = GetDef(id);
            if (def != null)
            {
                OnAchievementUnlocked?.Invoke(def);
                Debug.Log($"[Achievement] Unlocked: {def.title} — {def.description}");
            }
        }
    }
}
