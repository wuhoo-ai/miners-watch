using System.Collections.Generic;
using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Spawns mineral nodes in the scene based on the current depth level.
    /// Places 8-15 nodes with a minimum 2.0f spacing within a defined area.
    /// </summary>
    public class MineralSpawner : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private GameObject mineralNodePrefab;

        [Header("Spawn Settings")]
        [SerializeField] private int minNodes = 8;
        [SerializeField] private int maxNodes = 15;
        [SerializeField] private float minSpacing = 2.0f;
        [SerializeField] private float spawnMinX = -7f;
        [SerializeField] private float spawnMaxX = 7f;
        [SerializeField] private float spawnMinY = -3f;
        [SerializeField] private float spawnMaxY = 3f;

        private static readonly Dictionary<MineralType, float>[] DepthDistributions =
        {
            // Shallow
            new Dictionary<MineralType, float> { { MineralType.Stone, 0.60f }, { MineralType.Iron, 0.40f } },
            // Medium
            new Dictionary<MineralType, float> { { MineralType.Iron, 0.40f }, { MineralType.Gold, 0.35f }, { MineralType.Crystal, 0.25f } },
            // Deep
            new Dictionary<MineralType, float> { { MineralType.Gold, 0.30f }, { MineralType.Crystal, 0.40f }, { MineralType.Obsidian, 0.30f } }
        };

        private static readonly Dictionary<MineralType, Color> ColorMap = new Dictionary<MineralType, Color>
        {
            { MineralType.Stone,    Color.gray },
            { MineralType.Iron,     new Color(0.30f, 0.30f, 0.30f) },
            { MineralType.Gold,     new Color(1.00f, 0.84f, 0.00f) },
            { MineralType.Crystal,  new Color(0.68f, 0.85f, 0.90f) },
            { MineralType.Obsidian, new Color(0.40f, 0.00f, 0.50f) }
        };

        private static Sprite _cachedPlaceholderSprite;

        private static Sprite PlaceholderSprite
        {
            get
            {
                if (_cachedPlaceholderSprite == null)
                {
                    int size = 32;
                    Texture2D tex = new Texture2D(size, size);
                    Color[] pixels = new Color[size * size];
                    for (int i = 0; i < pixels.Length; i++)
                        pixels[i] = Color.white;
                    tex.SetPixels(pixels);
                    tex.Apply();
                    _cachedPlaceholderSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                }
                return _cachedPlaceholderSprite;
            }
        }

        private void Awake()
        {
            SpawnMinerals();
        }

        public void SpawnMinerals()
        {
            DepthLevel depth = levelConfig != null ? levelConfig.levelDepth : DepthLevel.Shallow;
            var distribution = DepthDistributions[(int)depth];

            int count = Random.Range(minNodes, maxNodes + 1);
            int maxAttempts = count * 10;
            var spawnedPositions = new List<Vector2>();
            int attempts = 0;

            while (spawnedPositions.Count < count && attempts < maxAttempts)
            {
                attempts++;
                Vector2 pos = new Vector2(
                    Random.Range(spawnMinX, spawnMaxX),
                    Random.Range(spawnMinY, spawnMaxY)
                );

                // Enforce minimum spacing
                bool tooClose = false;
                foreach (var existing in spawnedPositions)
                {
                    if (Vector2.Distance(pos, existing) < minSpacing)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (tooClose) continue;

                spawnedPositions.Add(pos);
                SpawnNode(pos, distribution);
            }
        }

        private void SpawnNode(Vector2 position, Dictionary<MineralType, float> distribution)
        {
            MineralType type = PickMineral(distribution);
            MineralData data = CreateMineralData(type);

            GameObject nodeObj;
            if (mineralNodePrefab != null)
            {
                nodeObj = Instantiate(mineralNodePrefab, position, Quaternion.identity, transform);
            }
            else
            {
                nodeObj = new GameObject($"Mineral_{type}");
                nodeObj.transform.position = position;
                nodeObj.transform.SetParent(transform);
                nodeObj.tag = "MineralNode";

                var sr = nodeObj.AddComponent<SpriteRenderer>();
                sr.sprite = PlaceholderSprite;
                sr.color = ColorMap.TryGetValue(type, out Color c) ? c : Color.white;
                sr.sortingOrder = 0;

                var col = nodeObj.AddComponent<CircleCollider2D>();
                col.isTrigger = true;
                col.radius = 0.5f;
            }

            var node = nodeObj.GetComponent<MineralNode>();
            if (node == null)
                node = nodeObj.AddComponent<MineralNode>();
            node.Init(data);
        }

        private MineralType PickMineral(Dictionary<MineralType, float> distribution)
        {
            float roll = Random.value;
            float cumulative = 0f;
            foreach (var kvp in distribution)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                    return kvp.Key;
            }
            // Fallback in case of floating-point rounding
            foreach (var kvp in distribution)
                return kvp.Key;
            return MineralType.Stone;
        }

        private MineralData CreateMineralData(MineralType type)
        {
            switch (type)
            {
                case MineralType.Stone:    return MineralData.Create(type, "Stone",    1f,   5f,  new[] { DepthLevel.Shallow });
                case MineralType.Iron:     return MineralData.Create(type, "Iron",     2f,  15f,  new[] { DepthLevel.Shallow, DepthLevel.Medium });
                case MineralType.Gold:     return MineralData.Create(type, "Gold",     5f,  40f,  new[] { DepthLevel.Medium, DepthLevel.Deep });
                case MineralType.Crystal:  return MineralData.Create(type, "Crystal", 10f, 100f,  new[] { DepthLevel.Medium, DepthLevel.Deep });
                case MineralType.Obsidian: return MineralData.Create(type, "Obsidian", 20f, 300f,  new[] { DepthLevel.Deep });
                default:                   return MineralData.Create(type, "Unknown",  1f,   1f,  new[] { DepthLevel.Shallow });
            }
        }
    }
}
