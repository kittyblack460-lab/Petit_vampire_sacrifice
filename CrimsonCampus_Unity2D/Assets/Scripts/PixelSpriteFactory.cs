using System.Collections.Generic;
using UnityEngine;

namespace CrimsonCampus
{
    public static class PixelSpriteFactory
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Texture2D> IconCache = new Dictionary<string, Texture2D>();

        public static Texture2D CreateBlankTex(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color[] clear = new Color[w * h];
            tex.SetPixels(clear);
            return tex;
        }

        private static void FillRect(Texture2D tex, int x, int y, int w, int h, Color c)
        {
            int x0 = Mathf.Clamp(x, 0, tex.width);
            int y0 = Mathf.Clamp(y, 0, tex.height);
            int x1 = Mathf.Clamp(x + w, 0, tex.width);
            int y1 = Mathf.Clamp(y + h, 0, tex.height);
            for (int py = y0; py < y1; py++)
                for (int px = x0; px < x1; px++)
                    tex.SetPixel(px, py, c);
        }

        private static void SetPx(Texture2D tex, int x, int y, Color c)
        {
            if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                tex.SetPixel(pxOrClamp(x, tex.width), pxOrClamp(y, tex.height), c);
        }

        private static int pxOrClamp(int v, int max) => Mathf.Clamp(v, 0, max - 1);

        // Coeur pixel art 5x4 net (place directement dans les yeux des fideles)
        private static void DrawMiniHeart5x4(Texture2D tex, int x, int y, Color mainColor, Color glintColor)
        {
            SetPx(tex, x + 1, y + 3, mainColor);
            SetPx(tex, x + 3, y + 3, mainColor);
            for (int dx = 0; dx < 5; dx++) SetPx(tex, x + dx, y + 2, mainColor);
            for (int dx = 1; dx <= 3; dx++) SetPx(tex, x + dx, y + 1, mainColor);
            SetPx(tex, x + 2, y, mainColor);
            SetPx(tex, x + 1, y + 2, glintColor);
        }

        private static Sprite ToSprite(Texture2D tex, float ppu = 24f, Vector2? pivot = null)
        {
            tex.Apply();
            Vector2 pv = pivot ?? new Vector2(0.5f, 0.08f);
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pv, ppu);
        }

        // =====================================================================
        // 1. SPRITE DU VAMPIRE EVOLUTIF (32x40)
        // =====================================================================
        public static Sprite GetVampireSprite(bool hasHair, bool hasEyes, bool hasCape, bool hasRose, bool hasWings, int dir, int frame)
        {
            string key = $"vamp_{hasHair}_{hasEyes}_{hasCape}_{hasRose}_{hasWings}_{dir}_{frame}";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(32, 40);
            int bob = frame == 1 ? 1 : 0;

            FillRect(tex, 9, 1, 14, 4, new Color(0f, 0f, 0f, 0.42f));

            if (hasWings)
            {
                int flap = frame == 1 ? 2 : 0;
                Color wDark = GameCatalog.Hex("#3b0724");
                Color wRed = GameCatalog.Hex("#881337");
                FillRect(tex, 0, 17 + flap, 7, 6, wDark);
                FillRect(tex, 1, 21 + flap, 5, 4, wRed);
                FillRect(tex, 25, 17 + flap, 7, 6, wDark);
                FillRect(tex, 26, 21 + flap, 5, 4, wRed);
            }

            if (hasCape)
            {
                int sway = frame == 1 ? 1 : -1;
                FillRect(tex, 6 + sway, 9 + bob, 20, 15, GameCatalog.Hex("#4c0519"));
                FillRect(tex, 8 + sway, 10 + bob, 16, 13, GameCatalog.Hex("#9f1239"));
                FillRect(tex, 8, 22 + bob, 3, 5, GameCatalog.Hex("#be123c"));
                FillRect(tex, 21, 22 + bob, 3, 5, GameCatalog.Hex("#be123c"));
            }

            int lLeg = frame == 1 ? 4 : 3;
            int rLeg = frame == 1 ? 3 : 4;
            FillRect(tex, 11, lLeg, 4, 7, GameCatalog.Hex("#18181b"));
            FillRect(tex, 17, rLeg, 4, 7, GameCatalog.Hex("#18181b"));

            Color coat = hasCape ? GameCatalog.Hex("#27091c") : GameCatalog.Hex("#1e1b2e");
            FillRect(tex, 9, 10 + bob, 14, 12, coat);
            if (dir != 1)
            {
                FillRect(tex, 14, 15 + bob, 4, 7, GameCatalog.Hex("#f8fafc"));
                if (hasCape) FillRect(tex, 15, 15 + bob, 2, 6, GameCatalog.Hex("#e11d48"));
                if (hasRose)
                {
                    FillRect(tex, 14, 19 + bob, 4, 2, GameCatalog.Hex("#fde68a"));
                    FillRect(tex, 19, 16 + bob, 3, 3, GameCatalog.Hex("#fb7185"));
                }
            }

            Color sleeve = hasCape ? GameCatalog.Hex("#3b0724") : GameCatalog.Hex("#1e1b2e");
            FillRect(tex, 6, 12 + bob, 3, 9, sleeve);
            FillRect(tex, 23, 12 + bob, 3, 9, sleeve);
            FillRect(tex, 6, 10 + bob, 3, 3, GameCatalog.Hex("#fff1f2"));
            FillRect(tex, 23, 10 + bob, 3, 3, GameCatalog.Hex("#fff1f2"));

            FillRect(tex, 9, 22 + bob, 14, 12, GameCatalog.Hex("#fff1f2"));

            Color hair = GameCatalog.Hex("#140a1c");
            FillRect(tex, 8, 32 + bob, 16, 5, hair);
            FillRect(tex, 7, 27 + bob, 3, 7, hair);
            FillRect(tex, 22, 27 + bob, 3, 7, hair);
            if (hasHair)
            {
                FillRect(tex, 6, 29 + bob, 3, 6, hair);
                FillRect(tex, 23, 29 + bob, 3, 6, hair);
                FillRect(tex, 10, 36 + bob, 12, 3, hair);
                FillRect(tex, 13, 30 + bob, 4, 3, hair);
                FillRect(tex, 18, 34 + bob, 4, 2, GameCatalog.Hex("#6b21a8"));
            }

            if (dir != 1)
            {
                Color eyeColor = hasEyes ? GameCatalog.Hex("#f43f5e") : GameCatalog.Hex("#1e1b2e");
                int ex = dir == 2 ? -1 : (dir == 3 ? 1 : 0);
                FillRect(tex, 11 + ex, 26 + bob, 3, 3, eyeColor);
                FillRect(tex, 18 + ex, 26 + bob, 3, 3, eyeColor);
                if (hasEyes)
                {
                    SetPx(tex, 11 + ex, 28 + bob, Color.white);
                    SetPx(tex, 18 + ex, 28 + bob, Color.white);
                }
                FillRect(tex, 14 + ex, 23 + bob, 4, 1, GameCatalog.Hex("#be123c"));
            }
            else
            {
                FillRect(tex, 9, 23 + bob, 14, 10, hair);
            }

            Sprite sp = ToSprite(tex, 22f);
            SpriteCache[key] = sp;
            return sp;
        }

        // =====================================================================
        // 1B. SPRITE CHAUVE-SOURIS POUR LE DASH (32x24) + MINI CHAUVE-SOURIS
        // =====================================================================
        public static Sprite GetBatDashSprite(int frame)
        {
            string key = $"bat_dash_{frame}";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(32, 24);
            Color bodyDark = GameCatalog.Hex("#1a0724");
            Color wingDark = GameCatalog.Hex("#3b0724");
            Color wingCrimson = GameCatalog.Hex("#be123c");
            Color eyeRed = GameCatalog.Hex("#ff2a6d");

            // Ombre legere au sol
            FillRect(tex, 11, 1, 10, 3, new Color(0f, 0f, 0f, 0.30f));

            // Corps central + oreilles pointues
            FillRect(tex, 12, 8, 8, 9, bodyDark);
            FillRect(tex, 12, 16, 2, 4, bodyDark);
            FillRect(tex, 18, 16, 2, 4, bodyDark);
            SetPx(tex, 13, 18, wingCrimson);
            SetPx(tex, 18, 18, wingCrimson);

            // Yeux rouges brillants + crocs
            FillRect(tex, 13, 13, 2, 2, eyeRed);
            FillRect(tex, 17, 13, 2, 2, eyeRed);
            SetPx(tex, 13, 14, Color.white);
            SetPx(tex, 17, 14, Color.white);
            SetPx(tex, 14, 10, Color.white);
            SetPx(tex, 17, 10, Color.white);

            // Ailes deployees (frame 0 = haut, frame 1 = bas)
            if (frame == 0)
            {
                // Aile gauche haute
                FillRect(tex, 2, 12, 10, 6, wingDark);
                FillRect(tex, 1, 15, 6, 5, wingDark);
                FillRect(tex, 4, 13, 7, 4, wingCrimson);
                // Aile droite haute
                FillRect(tex, 20, 12, 10, 6, wingDark);
                FillRect(tex, 25, 15, 6, 5, wingDark);
                FillRect(tex, 21, 13, 7, 4, wingCrimson);
            }
            else
            {
                // Aile gauche basse
                FillRect(tex, 2, 7, 10, 6, wingDark);
                FillRect(tex, 1, 5, 6, 5, wingDark);
                FillRect(tex, 4, 8, 7, 4, wingCrimson);
                // Aile droite basse
                FillRect(tex, 20, 7, 10, 6, wingDark);
                FillRect(tex, 25, 5, 6, 5, wingDark);
                FillRect(tex, 21, 8, 7, 4, wingCrimson);
            }

            Sprite sp = ToSprite(tex, 20f, new Vector2(0.5f, 0.2f));
            SpriteCache[key] = sp;
            return sp;
        }

        public static Sprite GetMiniBatParticleSprite()
        {
            const string key = "mini_bat_fx";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(12, 8);
            Color dark = GameCatalog.Hex("#2e0824");
            Color glow = GameCatalog.Hex("#f43f5e");
            FillRect(tex, 4, 2, 4, 4, dark);
            FillRect(tex, 0, 3, 4, 3, dark);
            FillRect(tex, 8, 3, 4, 3, dark);
            FillRect(tex, 1, 5, 2, 2, glow);
            FillRect(tex, 9, 5, 2, 2, glow);
            SetPx(tex, 5, 4, Color.white);
            SetPx(tex, 6, 4, Color.white);

            Sprite sp = ToSprite(tex, 16f, new Vector2(0.5f, 0.5f));
            SpriteCache[key] = sp;
            return sp;
        }

        // =====================================================================
        // 2. SPRITE DES ETUDIANTES (32x40) AVEC COEURS DANS LES YEUX SI FIDELE
        // =====================================================================
        public static Sprite GetGirlSprite(GirlArchetype type, bool isFollowing, int dir, int frame)
        {
            string key = $"girl_{type.Id}_{isFollowing}_{dir}_{frame}";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(32, 40);
            int bob = frame == 1 ? 1 : 0;

            FillRect(tex, 10, 1, 12, 3, new Color(0f, 0f, 0f, 0.36f));
            FillRect(tex, 6, 17 + bob, 20, 17, type.HairColor);

            int lLeg = frame == 1 ? 4 : 3;
            int rLeg = frame == 1 ? 3 : 4;
            FillRect(tex, 11, lLeg, 3, 6, type.SkinColor);
            FillRect(tex, 18, rLeg, 3, 6, type.SkinColor);
            FillRect(tex, 11, lLeg, 3, 2, GameCatalog.Hex("#1f2937"));
            FillRect(tex, 18, rLeg, 3, 2, GameCatalog.Hex("#1f2937"));

            FillRect(tex, 9, 10 + bob, 14, 10, type.DressColor);
            FillRect(tex, 7, 8 + bob, 18, 5, type.DressColor);
            FillRect(tex, 13, 13 + bob, 6, 6, type.AccentColor);

            FillRect(tex, 7, 21 + bob, 18, 12, type.SkinColor);

            FillRect(tex, 6, 32 + bob, 20, 5, type.HairColor);
            FillRect(tex, 6, 25 + bob, 2, 8, type.HairColor);
            FillRect(tex, 24, 25 + bob, 2, 8, type.HairColor);

            if (type.Id == "queen")
            {
                FillRect(tex, 10, 36 + bob, 12, 3, GameCatalog.Hex("#fbbf24"));
                SetPx(tex, 11, 39 + bob, GameCatalog.Hex("#fef08a"));
                SetPx(tex, 16, 39 + bob, GameCatalog.Hex("#fef08a"));
                SetPx(tex, 20, 39 + bob, GameCatalog.Hex("#fef08a"));
            }
            else if (type.Id == "artist")
            {
                FillRect(tex, 7, 36 + bob, 15, 3, GameCatalog.Hex("#f43f5e"));
            }

            if (dir != 1)
            {
                if (isFollowing)
                {
                    // COEURS PIXEL ART 5x4 DIRECTEMENT DANS LES YEUX DES FIDELES
                    Color heartEye = GameCatalog.Hex("#ff2a6d");
                    DrawMiniHeart5x4(tex, 9, 25 + bob, heartEye, Color.white);
                    DrawMiniHeart5x4(tex, 18, 25 + bob, heartEye, Color.white);

                    FillRect(tex, 8, 23 + bob, 2, 1, GameCatalog.Hex("#fb7185"));
                    FillRect(tex, 22, 23 + bob, 2, 1, GameCatalog.Hex("#fb7185"));
                    FillRect(tex, 14, 22 + bob, 4, 1, GameCatalog.Hex("#e11d48"));
                }
                else
                {
                    Color eyeDark = GameCatalog.Hex("#1e1b2e");
                    FillRect(tex, 11, 25 + bob, 2, 4, eyeDark);
                    FillRect(tex, 19, 25 + bob, 2, 4, eyeDark);
                    SetPx(tex, 11, 28 + bob, Color.white);
                    SetPx(tex, 19, 28 + bob, Color.white);
                }
            }
            else
            {
                FillRect(tex, 7, 22 + bob, 18, 12, type.HairColor);
            }

            Sprite sp = ToSprite(tex, 24f);
            SpriteCache[key] = sp;
            return sp;
        }

        // =====================================================================
        // 3. CHASSEUR DE VAMPIRE (32x40)
        // =====================================================================
        public static Sprite GetHunterSprite(int frame)
        {
            string key = $"hunter_{frame}";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(32, 40);
            int bob = frame == 1 ? 1 : 0;

            FillRect(tex, 8, 1, 16, 4, new Color(0f, 0f, 0f, 0.45f));

            int lLeg = frame == 1 ? 4 : 3;
            int rLeg = frame == 1 ? 3 : 4;
            FillRect(tex, 10, lLeg, 4, 7, GameCatalog.Hex("#1c1917"));
            FillRect(tex, 18, rLeg, 4, 7, GameCatalog.Hex("#1c1917"));

            FillRect(tex, 8, 9 + bob, 16, 14, GameCatalog.Hex("#292524"));
            FillRect(tex, 10, 10 + bob, 12, 12, GameCatalog.Hex("#44403c"));
            FillRect(tex, 11, 15 + bob, 10, 2, GameCatalog.Hex("#cbd5e1"));
            FillRect(tex, 15, 15 + bob, 2, 2, GameCatalog.Hex("#fbbf24"));
            FillRect(tex, 10, 21 + bob, 12, 3, GameCatalog.Hex("#991b1b"));

            FillRect(tex, 10, 24 + bob, 12, 8, GameCatalog.Hex("#f5d0b5"));
            FillRect(tex, 12, 27 + bob, 2, 2, GameCatalog.Hex("#38bdf8"));
            FillRect(tex, 18, 27 + bob, 2, 2, GameCatalog.Hex("#38bdf8"));

            FillRect(tex, 4, 31 + bob, 24, 3, GameCatalog.Hex("#1c1917"));
            FillRect(tex, 9, 34 + bob, 14, 5, GameCatalog.Hex("#292524"));
            FillRect(tex, 9, 34 + bob, 14, 1, GameCatalog.Hex("#b91c1c"));

            // Arbalete d'argent
            FillRect(tex, 24, 14 + bob, 6, 3, GameCatalog.Hex("#78350f"));
            FillRect(tex, 26, 11 + bob, 2, 9, GameCatalog.Hex("#e2e8f0"));
            FillRect(tex, 28, 15 + bob, 4, 1, GameCatalog.Hex("#f8fafc"));

            Sprite sp = ToSprite(tex, 22f);
            SpriteCache[key] = sp;
            return sp;
        }

        // =====================================================================
        // 4. LOUP-GAROU (34x38)
        // =====================================================================
        public static Sprite GetWerewolfSprite(int frame)
        {
            string key = $"werewolf_{frame}";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(34, 38);
            int bob = frame == 1 ? 2 : 0;

            Color furDark = GameCatalog.Hex("#27272a");
            Color furMid = GameCatalog.Hex("#3f3f46");
            Color furLight = GameCatalog.Hex("#71717a");
            Color clawCol = GameCatalog.Hex("#f8fafc");
            Color eyeGlow = GameCatalog.Hex("#facc15");

            FillRect(tex, 7, 1, 20, 4, new Color(0f, 0f, 0f, 0.48f));
            FillRect(tex, 3, 12 + bob, 6, 4, furDark);
            FillRect(tex, 2, 15 + bob, 4, 3, furMid);

            int lLeg = frame == 1 ? 5 : 3;
            int rLeg = frame == 1 ? 3 : 5;
            FillRect(tex, 9, lLeg, 5, 9, furDark);
            FillRect(tex, 20, rLeg, 5, 9, furDark);
            FillRect(tex, 9, lLeg, 5, 2, clawCol);
            FillRect(tex, 20, rLeg, 5, 2, clawCol);

            FillRect(tex, 7, 11 + bob, 20, 14, furDark);
            FillRect(tex, 10, 13 + bob, 14, 11, furMid);
            FillRect(tex, 13, 14 + bob, 8, 8, furLight);

            FillRect(tex, 3, 12 + bob, 4, 11, furDark);
            FillRect(tex, 27, 12 + bob, 4, 11, furDark);
            FillRect(tex, 3, 10 + bob, 4, 2, clawCol);
            FillRect(tex, 27, 10 + bob, 4, 2, clawCol);

            FillRect(tex, 9, 24 + bob, 16, 10, furDark);
            FillRect(tex, 11, 25 + bob, 12, 8, furMid);
            FillRect(tex, 9, 33 + bob, 4, 4, furDark);
            FillRect(tex, 10, 34 + bob, 2, 2, GameCatalog.Hex("#9f1239"));
            FillRect(tex, 21, 33 + bob, 4, 4, furDark);
            FillRect(tex, 22, 34 + bob, 2, 2, GameCatalog.Hex("#9f1239"));

            FillRect(tex, 12, 29 + bob, 3, 2, eyeGlow);
            FillRect(tex, 19, 29 + bob, 3, 2, eyeGlow);
            FillRect(tex, 13, 25 + bob, 8, 3, GameCatalog.Hex("#18181b"));
            SetPx(tex, 14, 27 + bob, clawCol);
            SetPx(tex, 19, 27 + bob, clawCol);
            FillRect(tex, 15, 25 + bob, 4, 1, GameCatalog.Hex("#dc2626"));

            Sprite sp = ToSprite(tex, 21f);
            SpriteCache[key] = sp;
            return sp;
        }

        // =====================================================================
        // 5. DECOR DU CAMPUS
        // =====================================================================
        public static Sprite GetSolidRectSprite(string key, int w, int h, Color c, float ppu = 16f)
        {
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;
            Texture2D tex = CreateBlankTex(w, h);
            FillRect(tex, 0, 0, w, h, c);
            Sprite sp = ToSprite(tex, ppu, new Vector2(0.5f, 0.5f));
            SpriteCache[key] = sp;
            return sp;
        }

        public static Sprite GetGrassTileSprite()
        {
            const string key = "campus_grass";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(64, 64);
            Color c1 = GameCatalog.Hex("#182c25");
            Color c2 = GameCatalog.Hex("#1d352c");
            FillRect(tex, 0, 0, 64, 64, c1);
            FillRect(tex, 0, 0, 32, 32, c2);
            FillRect(tex, 32, 32, 32, 32, c2);
            tex.wrapMode = TextureWrapMode.Repeat;
            Sprite sp = ToSprite(tex, 16f, new Vector2(0.5f, 0.5f));
            SpriteCache[key] = sp;
            return sp;
        }

        public static Sprite GetBuildingSprite(BuildingData b)
        {
            string key = "bldg_" + b.Id;
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            int w = Mathf.RoundToInt(b.Bounds.width * 8f);
            int h = Mathf.RoundToInt(b.Bounds.height * 8f);
            Texture2D tex = CreateBlankTex(w, h);

            FillRect(tex, 2, 0, w - 4, h - 24, b.WallColor);
            FillRect(tex, 0, h - 28, w, 28, b.RoofColor);
            FillRect(tex, 0, h - 28, w, 3, b.TrimColor);

            int doorW = 18;
            int doorX = w / 2 - doorW / 2;
            FillRect(tex, doorX, 0, doorW, 24, GameCatalog.Hex("#291528"));
            FillRect(tex, doorX + doorW - 4, 10, 2, 2, b.TrimColor);

            for (int wx = 12; wx < w - 18; wx += 24)
            {
                if (Mathf.Abs((wx + 6) - w / 2) < 18) continue;
                FillRect(tex, wx - 1, 20, 14, 18, GameCatalog.Hex("#1e1b2e"));
                FillRect(tex, wx, 21, 12, 16, b.WindowColor);
                FillRect(tex, wx + 5, 21, 2, 16, GameCatalog.Hex("#1e1b2e"));
                FillRect(tex, wx, 28, 12, 2, GameCatalog.Hex("#1e1b2e"));
            }

            Sprite sp = ToSprite(tex, 8f, new Vector2(0f, 0f));
            SpriteCache[key] = sp;
            return sp;
        }

        public static Sprite GetTreeSprite(bool pink)
        {
            string key = pink ? "tree_pink" : "tree_pine";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(36, 44);
            FillRect(tex, 6, 1, 24, 6, new Color(0, 0, 0, 0.35f));
            FillRect(tex, 14, 4, 8, 16, GameCatalog.Hex("#451a03"));
            FillRect(tex, 2, 16, 32, 20, pink ? GameCatalog.Hex("#881337") : GameCatalog.Hex("#14532d"));
            FillRect(tex, 6, 22, 24, 18, pink ? GameCatalog.Hex("#e11d48") : GameCatalog.Hex("#166534"));
            FillRect(tex, 10, 28, 9, 7, pink ? GameCatalog.Hex("#fda4af") : GameCatalog.Hex("#22c55e"));

            Sprite sp = ToSprite(tex, 16f, new Vector2(0.5f, 0.14f));
            SpriteCache[key] = sp;
            return sp;
        }

        public static Sprite GetLampSprite()
        {
            const string key = "lamp";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(16, 32);
            FillRect(tex, 6, 0, 4, 22, GameCatalog.Hex("#1e1b2e"));
            FillRect(tex, 5, 22, 6, 6, GameCatalog.Hex("#fde68a"));
            FillRect(tex, 4, 28, 8, 2, GameCatalog.Hex("#1e1b2e"));
            Sprite sp = ToSprite(tex, 16f, new Vector2(0.5f, 0.08f));
            SpriteCache[key] = sp;
            return sp;
        }

        // =====================================================================
        // 6. AUTEL CRAMOISI, SCEAU DE SACRIFICE, PILIER DE LUMIERE & EFFETS
        // =====================================================================
        public static Sprite GetRingSprite(string key, int size, Color fill, Color border, float borderThickness = 2f)
        {
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(size, size);
            float r = size * 0.48f;
            float cx = size * 0.5f;
            float cy = size * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    if (d <= r && d >= r - borderThickness)
                        tex.SetPixel(x, y, border);
                    else if (d < r - borderThickness && fill.a > 0f)
                        tex.SetPixel(x, y, fill);
                }
            }
            Sprite sp = ToSprite(tex, size / 2f, new Vector2(0.5f, 0.5f));
            SpriteCache[key] = sp;
            return sp;
        }

        public static Sprite GetPentagramSigilSprite(string key, int size, Color mainColor, Color accentColor)
        {
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(size, size);
            float cx = size * 0.5f;
            float cy = size * 0.5f;
            float rOuter = size * 0.46f;
            float rInner = size * 0.38f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                    if (Mathf.Abs(d - rOuter) < 1.6f)
                        tex.SetPixel(x, y, mainColor);
                    else if (Mathf.Abs(d - rInner) < 1.1f)
                        tex.SetPixel(x, y, accentColor);
                    else if (d < rInner)
                        tex.SetPixel(x, y, new Color(mainColor.r, mainColor.g, mainColor.b, 0.14f));
                }
            }

            Vector2[] pts = new Vector2[5];
            for (int i = 0; i < 5; i++)
            {
                float a = Mathf.PI * 0.5f + i * (2f * Mathf.PI / 5f);
                pts[i] = new Vector2(cx + Mathf.Cos(a) * rInner, cy + Mathf.Sin(a) * rInner);
            }
            for (int i = 0; i < 5; i++)
            {
                DrawLine(tex, pts[i], pts[(i + 2) % 5], mainColor);
            }

            DrawMiniHeart5x4(tex, size / 2 - 2, size / 2 - 2, accentColor, Color.white);

            Sprite sp = ToSprite(tex, size / 2f, new Vector2(0.5f, 0.5f));
            SpriteCache[key] = sp;
            return sp;
        }

        private static void DrawLine(Texture2D tex, Vector2 a, Vector2 b, Color col)
        {
            int steps = Mathf.CeilToInt(Vector2.Distance(a, b) * 1.5f);
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(a, b, i / (float)Mathf.Max(1, steps));
                SetPx(tex, Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y), col);
            }
        }

        public static Sprite GetLightBeamSprite()
        {
            const string key = "sacrifice_beam";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(12, 64);
            for (int y = 0; y < 64; y++)
            {
                float vFade = 1f - (y / 64f);
                for (int x = 0; x < 12; x++)
                {
                    float distCenter = Mathf.Abs(x - 5.5f) / 6f;
                    float core = Mathf.Clamp01(1f - distCenter);
                    Color c = Color.Lerp(GameCatalog.Hex("#e11d48"), GameCatalog.Hex("#fef08a"), core * 0.65f);
                    c.a = core * vFade * 0.9f;
                    tex.SetPixel(x, y, c);
                }
            }
            Sprite sp = ToSprite(tex, 16f, new Vector2(0.5f, 0f));
            SpriteCache[key] = sp;
            return sp;
        }

        public static Sprite GetPixelHeartSprite(Color col)
        {
            string key = "heart_" + ColorUtility.ToHtmlStringRGBA(col);
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(7, 6);
            DrawMiniHeart5x4(tex, 1, 1, col, Color.white);
            Sprite sp = ToSprite(tex, 14f, new Vector2(0.5f, 0.5f));
            SpriteCache[key] = sp;
            return sp;
        }

        public static Sprite GetBloodOrbSprite()
        {
            const string key = "blood_orb";
            if (SpriteCache.TryGetValue(key, out Sprite cached)) return cached;

            Texture2D tex = CreateBlankTex(6, 6);
            FillRect(tex, 1, 1, 4, 4, GameCatalog.Hex("#e11d48"));
            FillRect(tex, 2, 0, 2, 6, GameCatalog.Hex("#be123c"));
            FillRect(tex, 0, 2, 6, 2, GameCatalog.Hex("#be123c"));
            SetPx(tex, 2, 3, GameCatalog.Hex("#fda4af"));
            Sprite sp = ToSprite(tex, 16f, new Vector2(0.5f, 0.5f));
            SpriteCache[key] = sp;
            return sp;
        }

        // =====================================================================
        // 7. ICONES PIXEL ART POUR L'INTERFACE (ZERO EMOJI)
        // =====================================================================
        public static Texture2D GetUiIcon(string iconKey)
        {
            if (IconCache.TryGetValue(iconKey, out Texture2D cached)) return cached;

            Texture2D tex = CreateBlankTex(12, 12);
            switch (iconKey)
            {
                case "blood":
                    FillRect(tex, 5, 9, 2, 2, GameCatalog.Hex("#f43f5e"));
                    FillRect(tex, 4, 6, 4, 3, GameCatalog.Hex("#e11d48"));
                    FillRect(tex, 3, 2, 6, 4, GameCatalog.Hex("#be123c"));
                    FillRect(tex, 4, 1, 4, 1, GameCatalog.Hex("#9f1239"));
                    SetPx(tex, 4, 4, Color.white);
                    break;
                case "heart":
                    DrawMiniHeart5x4(tex, 3, 4, GameCatalog.Hex("#fb7185"), Color.white);
                    break;
                case "star":
                    FillRect(tex, 5, 1, 2, 10, GameCatalog.Hex("#f0abfc"));
                    FillRect(tex, 1, 5, 10, 2, GameCatalog.Hex("#f0abfc"));
                    FillRect(tex, 4, 4, 4, 4, Color.white);
                    break;
                case "rose":
                    FillRect(tex, 5, 1, 2, 5, GameCatalog.Hex("#16a34a"));
                    FillRect(tex, 3, 3, 2, 2, GameCatalog.Hex("#22c55e"));
                    FillRect(tex, 3, 6, 6, 5, GameCatalog.Hex("#e11d48"));
                    FillRect(tex, 4, 7, 4, 3, GameCatalog.Hex("#fb7185"));
                    break;
                case "eye":
                    FillRect(tex, 1, 4, 10, 4, Color.white);
                    FillRect(tex, 4, 4, 4, 4, GameCatalog.Hex("#e11d48"));
                    FillRect(tex, 5, 5, 2, 2, GameCatalog.Hex("#18181b"));
                    break;
                case "cape":
                    FillRect(tex, 3, 8, 6, 3, GameCatalog.Hex("#9f1239"));
                    FillRect(tex, 2, 2, 8, 6, GameCatalog.Hex("#4c0519"));
                    FillRect(tex, 4, 2, 4, 5, GameCatalog.Hex("#be123c"));
                    break;
                case "wings":
                    FillRect(tex, 1, 5, 4, 4, GameCatalog.Hex("#881337"));
                    FillRect(tex, 7, 5, 4, 4, GameCatalog.Hex("#881337"));
                    FillRect(tex, 4, 4, 4, 4, GameCatalog.Hex("#3b0724"));
                    break;
                case "boots":
                    FillRect(tex, 3, 2, 4, 8, GameCatalog.Hex("#a855f7"));
                    FillRect(tex, 3, 2, 7, 3, GameCatalog.Hex("#581c87"));
                    break;
                case "hunter":
                    FillRect(tex, 1, 7, 10, 2, GameCatalog.Hex("#38bdf8"));
                    FillRect(tex, 3, 9, 6, 2, GameCatalog.Hex("#0284c7"));
                    FillRect(tex, 3, 2, 6, 5, GameCatalog.Hex("#e2e8f0"));
                    break;
                case "wolf":
                    FillRect(tex, 2, 3, 8, 6, GameCatalog.Hex("#71717a"));
                    FillRect(tex, 2, 9, 2, 2, GameCatalog.Hex("#52525b"));
                    FillRect(tex, 8, 9, 2, 2, GameCatalog.Hex("#52525b"));
                    SetPx(tex, 4, 6, GameCatalog.Hex("#facc15"));
                    SetPx(tex, 7, 6, GameCatalog.Hex("#facc15"));
                    break;
                default:
                    FillRect(tex, 2, 2, 8, 8, GameCatalog.Hex("#f43f5e"));
                    break;
            }

            tex.Apply();
            IconCache[iconKey] = tex;
            return tex;
        }
    }
}
