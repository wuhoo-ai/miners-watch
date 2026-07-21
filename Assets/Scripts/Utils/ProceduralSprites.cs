using UnityEngine;

namespace MinersWatch
{
    /// <summary>
    /// Procedural pixel-art enemy sprites. Each enemy type gets a distinct
    /// body shape, eye pattern, and color palette generated from Texture2D.
    /// </summary>
    public static class ProceduralSprites
    {
        private static Sprite _rockworm, _shadow, _lavabeast, _guardian;

        public static Sprite Rockworm => _rockworm ??= GenerateRockworm();
        public static Sprite Shadow => _shadow ??= GenerateShadow();
        public static Sprite Lavabeast => _lavabeast ??= GenerateLavabeast();
        public static Sprite Guardian => _guardian ??= GenerateGuardian();

        public static Sprite Get(EnemyType type) => type switch
        {
            EnemyType.Rockworm => Rockworm,
            EnemyType.Shadow => Shadow,
            EnemyType.Lavabeast => Lavabeast,
            EnemyType.Guardian => Guardian,
            _ => SpriteAtlas.WhiteSquare,
        };

        const int S = 48;

        static Sprite GenerateRockworm()
        {
            var t = new Texture2D(S, S, TextureFormat.RGBA32, false);
            var p = new Color[S * S];
            // Brown segmented worm body
            for (int y = 4; y < S - 4; y++)
            for (int x = 4; x < S - 4; x++)
            {
                int seg = y / 10;
                float dx = x - S / 2f;
                float r = 16 + Mathf.Sin(seg * 1.2f) * 3;
                if (dx * dx + (y - S / 2f) * (y - S / 2f) * 0.3f < r * r)
                    p[y * S + x] = seg % 2 == 0
                        ? new Color(0.45f, 0.28f, 0.12f)  // dark brown
                        : new Color(0.55f, 0.35f, 0.18f);  // light brown
            }
            // Eyes (top segment)
            for (int dy = -3; dy <= 3; dy++)
            for (int dx = -3; dx <= 3; dx++)
            {
                if (dx * dx + dy * dy <= 4)
                {
                    int ex = S / 2 - 8 + dx, ey = 12 + dy;
                    if (ex >= 0 && ex < S && ey >= 0 && ey < S)
                        p[ey * S + ex] = Color.white;
                    ex = S / 2 + 8 + dx;
                    if (ex >= 0 && ex < S && ey >= 0 && ey < S)
                        p[ey * S + ex] = Color.white;
                }
                if (dx * dx + dy * dy <= 1.5f)
                {
                    int px = S / 2 - 8 + dx, py = 12 + dy;
                    if (px >= 0 && px < S && py >= 0 && py < S)
                        p[py * S + px] = Color.black;
                    px = S / 2 + 8 + dx; py = 12 + dy;
                    if (px >= 0 && px < S && py >= 0 && py < S)
                        p[py * S + px] = Color.black;
                }
            }
            t.SetPixels(p); t.Apply();
            t.filterMode = FilterMode.Point;
            return Sprite.Create(t, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);
        }

        static Sprite GenerateShadow()
        {
            var t = new Texture2D(S, S, TextureFormat.RGBA32, false);
            var p = new Color[S * S];
            // Purple wispy shadow
            for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float dx = x - S / 2f, dy = y - S / 2f;
                float dist = dx * dx * 0.6f + dy * dy; // oval
                float r = S / 2f - 4;
                if (dist < r * r)
                {
                    float alpha = 1f - dist / (r * r);
                    p[y * S + x] = new Color(0.35f, 0.08f, 0.55f, alpha * 0.85f);
                }
            }
            // White glowing eyes
            DrawEye(p, S / 2 - 6, S / 2 - 4, Color.white);
            DrawEye(p, S / 2 + 6, S / 2 - 4, Color.white);
            DrawPupil(p, S / 2 - 6, S / 2 - 4, Color.red);
            DrawPupil(p, S / 2 + 6, S / 2 - 4, Color.red);
            t.SetPixels(p); t.Apply();
            t.filterMode = FilterMode.Point;
            return Sprite.Create(t, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);
        }

        static Sprite GenerateLavabeast()
        {
            var t = new Texture2D(S, S, TextureFormat.RGBA32, false);
            var p = new Color[S * S];
            // Big fiery blob
            for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float dx = x - S / 2f, dy = y - S / 2f;
                float r = S / 2f - 2;
                if (dx * dx + dy * dy < r * r)
                    p[y * S + x] = new Color(0.9f, 0.25f, 0.05f);
            }
            // Lava cracks (darker lines)
            for (int i = 0; i < 6; i++)
            {
                int cx = 8 + (i * 7) % (S - 16), cy = 8 + i * 6;
                for (int d = -1; d <= 1; d++)
                for (int e = 0; e < 10; e++)
                {
                    int px = cx + d + e, py = cy + e / 2;
                    if (px >= 0 && px < S && py >= 0 && py < S)
                        p[py * S + px] = new Color(0.5f, 0.12f, 0f);
                }
            }
            // Glowing eyes
            DrawEye(p, S / 2 - 7, S / 2 - 5, new Color(1f, 1f, 0.3f));
            DrawEye(p, S / 2 + 7, S / 2 - 5, new Color(1f, 1f, 0.3f));
            t.SetPixels(p); t.Apply();
            t.filterMode = FilterMode.Point;
            return Sprite.Create(t, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);
        }

        static Sprite GenerateGuardian()
        {
            var t = new Texture2D(S, S, TextureFormat.RGBA32, false);
            var p = new Color[S * S];
            // Gold/grey armored knight
            for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float dx = x - S / 2f, dy = y - S / 2f;
                float bodyDist = dx * dx + dy * dy * 0.7f;
                if (bodyDist < 20 * 20)
                    p[y * S + x] = new Color(0.6f, 0.5f, 0.15f); // gold
            }
            // Shield outline
            for (int y = 4; y < S - 4; y++)
            for (int x = 4; x < S - 4; x++)
            {
                float dx = x - S / 2f, dy = y - S / 2f;
                float edge = 18f;
                if (dx * dx + dy * dy * 0.6f < edge * edge &&
                    dx * dx + dy * dy * 0.6f > (edge - 3) * (edge - 3))
                    p[y * S + x] = new Color(0.85f, 0.7f, 0.2f);
            }
            // Red visor eyes
            for (int ex = -8; ex <= 8; ex += 16)
            for (int dy = -2; dy <= 2; dy++)
            for (int dx = -4; dx <= 4; dx++)
            {
                int px = S / 2 + ex + dx, py = S / 2 - 6 + dy;
                if (px >= 0 && px < S && py >= 0 && py < S)
                    p[py * S + px] = new Color(1f, 0.15f, 0.1f);
            }
            t.SetPixels(p); t.Apply();
            t.filterMode = FilterMode.Point;
            return Sprite.Create(t, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);
        }

        static void DrawEye(Color[] p, int cx, int cy, Color c)
        {
            for (int dy = -2; dy <= 2; dy++)
            for (int dx = -2; dx <= 2; dx++)
            {
                if (dx * dx + dy * dy > 4) continue;
                int px = cx + dx, py = cy + dy;
                if (px >= 0 && px < S && py >= 0 && py < S)
                    p[py * S + px] = c;
            }
        }

        static void DrawPupil(Color[] p, int cx, int cy, Color c)
        {
            int px = cx, py = cy;
            if (px >= 0 && px < S && py >= 0 && py < S)
                p[py * S + px] = c;
        }
    }
}
