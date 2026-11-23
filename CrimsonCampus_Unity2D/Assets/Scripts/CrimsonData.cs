using System;
using System.Collections.Generic;
using UnityEngine;

namespace CrimsonCampus
{
    public static class WorldConfig
    {
        // Conversion 1 unite Unity = 16 pixels (carte originale 1440 x 1040 -> 90 x 65)
        public const float WorldW = 90f;
        public const float WorldH = 65f;
        public const float BorderPad = 1.8f;

        // Autel Cramoisi au centre de la cour (+50% sang)
        public static readonly Vector2 AltarPos = new Vector2(45f, 32.5f);
        public const float AltarRadius = 4.0f;

        // Point de depart du joueur
        public static readonly Vector2 PlayerStartPos = new Vector2(45f, 25.5f);

        public const string SaveKey = "crimson_campus_unity_v2";
    }

    [Serializable]
    public class BuildingData
    {
        public string Id;
        public string DisplayName;
        public Rect Bounds;
        public Color RoofColor;
        public Color WallColor;
        public Color TrimColor;
        public Color WindowColor;
    }

    [Serializable]
    public class TreeData
    {
        public Vector2 Position;
        public bool IsCrimsonCherry;
    }

    [Serializable]
    public class GirlArchetype
    {
        public int Index;
        public string Id;
        public string Name;
        public string Zone;
        public int ReqAllure;
        public double SacrificeBlood;
        public float CharmTime;
        public Color HairColor;
        public Color DressColor;
        public Color AccentColor;
        public Color SkinColor;
        public Rect SpawnBox;
    }

    [Serializable]
    public class SexyUpgrade
    {
        public string Id;
        public string Name;
        public string Description;
        public double Cost;
        public int Allure;
        public double BloodMult;
        public float AuraBonus;
        public float CharmSpeedBonus;
        public float SpeedBonus;
        public bool ChangesSprite;
        public string IconKey;
    }

    [Serializable]
    public class RepeatableUpgrade
    {
        public string Id;
        public string Name;
        public string Description;
        public double BaseCost;
        public double CostMult;
        public int MaxLevel;
        public string IconKey;
    }

    [Serializable]
    public class SaveState
    {
        public double blood;
        public double lifetimeBlood;
        public int lifetimeGirls;
        public int lifetimeSacrificed;
        public int roses;
        public int semesters;

        public int[] girlCounts = new int[6];
        public int[] sacrificedCounts = new int[6];
        public List<string> upgrades = new List<string>();

        public int repGlamour;
        public int repBoots;
        public int repParty;

        public float moonCharge;
        public float moonTimeLeft;
        public bool soundOn = true;

        public void EnsureValid()
        {
            if (girlCounts == null || girlCounts.Length != 6) girlCounts = new int[6];
            if (sacrificedCounts == null || sacrificedCounts.Length != 6) sacrificedCounts = new int[6];
            if (upgrades == null) upgrades = new List<string>();
        }
    }

    public static class GameCatalog
    {
        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color c)) return c;
            return Color.white;
        }

        public static readonly BuildingData[] Buildings = new BuildingData[]
        {
            new BuildingData
            {
                Id = "library", DisplayName = "BIBLIOTHEQUE",
                Bounds = new Rect(7f, 48.5f, 20f, 10.5f),
                RoofColor = Hex("#3b1c4a"), WallColor = Hex("#583166"),
                TrimColor = Hex("#f59e0b"), WindowColor = Hex("#fde68a")
            },
            new BuildingData
            {
                Id = "cafe", DisplayName = "CAFE LUNAIRE",
                Bounds = new Rect(62f, 48.5f, 20f, 10.5f),
                RoofColor = Hex("#6b1d3a"), WallColor = Hex("#88294a"),
                TrimColor = Hex("#fda4af"), WindowColor = Hex("#ffedd5")
            },
            new BuildingData
            {
                Id = "dorms", DisplayName = "SORORITE",
                Bounds = new Rect(7f, 6f, 21f, 11f),
                RoofColor = Hex("#7f1d1d"), WallColor = Hex("#991b1b"),
                TrimColor = Hex("#fecdd3"), WindowColor = Hex("#fef08a")
            },
            new BuildingData
            {
                Id = "observatory", DisplayName = "OBSERVATOIRE",
                Bounds = new Rect(62.5f, 6f, 19.5f, 11f),
                RoofColor = Hex("#1e293b"), WallColor = Hex("#334155"),
                TrimColor = Hex("#93c5fd"), WindowColor = Hex("#bae6fd")
            }
        };

        public static readonly TreeData[] Trees = new TreeData[]
        {
            new TreeData { Position = new Vector2(32.5f, 54f), IsCrimsonCherry = true },
            new TreeData { Position = new Vector2(57f, 54f), IsCrimsonCherry = true },
            new TreeData { Position = new Vector2(32.2f, 11f), IsCrimsonCherry = true },
            new TreeData { Position = new Vector2(57.8f, 11f), IsCrimsonCherry = true },
            new TreeData { Position = new Vector2(7.2f, 40.5f), IsCrimsonCherry = false },
            new TreeData { Position = new Vector2(11.5f, 43.5f), IsCrimsonCherry = true },
            new TreeData { Position = new Vector2(82.5f, 41f), IsCrimsonCherry = false },
            new TreeData { Position = new Vector2(78.5f, 43.8f), IsCrimsonCherry = true },
            new TreeData { Position = new Vector2(7.2f, 25f), IsCrimsonCherry = false },
            new TreeData { Position = new Vector2(12.2f, 22.5f), IsCrimsonCherry = true },
            new TreeData { Position = new Vector2(82.8f, 25f), IsCrimsonCherry = false },
            new TreeData { Position = new Vector2(78f, 22.5f), IsCrimsonCherry = true },
            new TreeData { Position = new Vector2(29.5f, 42.5f), IsCrimsonCherry = false },
            new TreeData { Position = new Vector2(60.5f, 42.5f), IsCrimsonCherry = false },
            new TreeData { Position = new Vector2(29.5f, 22f), IsCrimsonCherry = false },
            new TreeData { Position = new Vector2(60.5f, 22f), IsCrimsonCherry = false }
        };

        public static readonly Vector2[] Lamps = new Vector2[]
        {
            new Vector2(36.5f, 39.5f), new Vector2(53.5f, 39.5f),
            new Vector2(36.5f, 25.5f), new Vector2(53.5f, 25.5f),
            new Vector2(41.2f, 50f),   new Vector2(48.8f, 50f),
            new Vector2(41.2f, 15f),   new Vector2(48.8f, 15f),
            new Vector2(23.8f, 35.3f), new Vector2(66.2f, 35.3f)
        };

        public static readonly GirlArchetype[] GirlTypes = new GirlArchetype[]
        {
            new GirlArchetype
            {
                Index = 0, Id = "study", Name = "Etudiante Serieuse", Zone = "Bibliotheque & Cour",
                ReqAllure = 1, SacrificeBlood = 16, CharmTime = 0.65f,
                HairColor = Hex("#6b3e26"), DressColor = Hex("#60a5fa"),
                AccentColor = Hex("#fef08a"), SkinColor = Hex("#ffe0cc"),
                SpawnBox = new Rect(7.5f, 25f, 40f, 22f)
            },
            new GirlArchetype
            {
                Index = 1, Id = "barista", Name = "Barista de Minuit", Zone = "Pres du Cafe Lunaire",
                ReqAllure = 3, SacrificeBlood = 58, CharmTime = 0.85f,
                HairColor = Hex("#9a3412"), DressColor = Hex("#15803d"),
                AccentColor = Hex("#fff7ed"), SkinColor = Hex("#f8d5bd"),
                SpawnBox = new Rect(45f, 26f, 39f, 22f)
            },
            new GirlArchetype
            {
                Index = 2, Id = "artist", Name = "Etudiante en Art", Zone = "Jardins & Cour",
                ReqAllure = 7, SacrificeBlood = 200, CharmTime = 1.0f,
                HairColor = Hex("#c026d3"), DressColor = Hex("#a855f7"),
                AccentColor = Hex("#f43f5e"), SkinColor = Hex("#ffe4d6"),
                SpawnBox = new Rect(26f, 12.5f, 38f, 40f)
            },
            new GirlArchetype
            {
                Index = 3, Id = "goth", Name = "Poetesse Gothique", Zone = "Pres de la Sororite",
                ReqAllure = 14, SacrificeBlood = 680, CharmTime = 1.1f,
                HairColor = Hex("#18181b"), DressColor = Hex("#4c0519"),
                AccentColor = Hex("#e11d48"), SkinColor = Hex("#fff1f2"),
                SpawnBox = new Rect(7f, 5f, 39f, 28f)
            },
            new GirlArchetype
            {
                Index = 4, Id = "cheer", Name = "Pom-Pom Girl", Zone = "Pelouse Est & Observatoire",
                ReqAllure = 24, SacrificeBlood = 2300, CharmTime = 1.2f,
                HairColor = Hex("#facc15"), DressColor = Hex("#e11d48"),
                AccentColor = Hex("#ffffff"), SkinColor = Hex("#fed7aa"),
                SpawnBox = new Rect(45f, 6f, 39f, 29f)
            },
            new GirlArchetype
            {
                Index = 5, Id = "queen", Name = "Reine de Sororite", Zone = "Tout le Campus",
                ReqAllure = 40, SacrificeBlood = 8200, CharmTime = 1.35f,
                HairColor = Hex("#fef08a"), DressColor = Hex("#db2777"),
                AccentColor = Hex("#fbbf24"), SkinColor = Hex("#ffe4d6"),
                SpawnBox = new Rect(10f, 18f, 70f, 28f)
            }
        };

        public static readonly SexyUpgrade[] SexyUpgrades = new SexyUpgrade[]
        {
            new SexyUpgrade
            {
                Id = "hair", Name = "Coiffure Tenebreuse",
                Description = "Cheveux gothiques sur ton sprite. +2 Charme & +25% Sang.",
                Cost = 25, Allure = 2, BloodMult = 1.25, AuraBonus = 0.5f, CharmSpeedBonus = 0.25f,
                ChangesSprite = true, IconKey = "star"
            },
            new SexyUpgrade
            {
                Id = "eyes", Name = "Regard Rubis Hypnotique",
                Description = "Yeux rouges brillants sur ton sprite. +4 Charme & Aura agrandie.",
                Cost = 110, Allure = 4, BloodMult = 1.4, AuraBonus = 0.9f, CharmSpeedBonus = 0.2f,
                ChangesSprite = true, IconKey = "eye"
            },
            new SexyUpgrade
            {
                Id = "cologne", Name = "Parfum Rose de Minuit",
                Description = "Laisse des petales derriere toi. +5 Charme & +60% Sang.",
                Cost = 380, Allure = 5, BloodMult = 1.6, AuraBonus = 0.9f, CharmSpeedBonus = 0.2f,
                ChangesSprite = false, IconKey = "rose"
            },
            new SexyUpgrade
            {
                Id = "cape", Name = "Cape de Velours Haute",
                Description = "Cape de vampire sur ton sprite. +8 Charme, +22% Vitesse & x1.8 Sang.",
                Cost = 1400, Allure = 8, BloodMult = 1.8, AuraBonus = 0.9f, SpeedBonus = 0.22f,
                ChangesSprite = true, IconKey = "cape"
            },
            new SexyUpgrade
            {
                Id = "fangs", Name = "Sourire & Crocs Irresistibles",
                Description = "+12 Charme, x2.0 Sang & Clin d'oeil plus rapide.",
                Cost = 5200, Allure = 12, BloodMult = 2.0, AuraBonus = 1.0f, CharmSpeedBonus = 0.45f,
                ChangesSprite = false, IconKey = "blood"
            },
            new SexyUpgrade
            {
                Id = "rose", Name = "Medaillon & Rose Enchantee",
                Description = "Ornement sur ton sprite. +16 Charme & debloque les Reines.",
                Cost = 20000, Allure = 16, BloodMult = 2.3, AuraBonus = 1.15f,
                ChangesSprite = true, IconKey = "rose"
            },
            new SexyUpgrade
            {
                Id = "wings", Name = "Ailes de Chauve-Souris",
                Description = "Ailes animees, Dash Chauve-Souris plus rapide, +30 Charme & x3.0 Sang.",
                Cost = 90000, Allure = 30, BloodMult = 3.0, AuraBonus = 1.6f, SpeedBonus = 0.25f,
                ChangesSprite = true, IconKey = "wings"
            }
        };

        public static readonly RepeatableUpgrade[] RepeatableUpgrades = new RepeatableUpgrade[]
        {
            new RepeatableUpgrade
            {
                Id = "rep_glamour", Name = "Eclat Glamour",
                Description = "+3 Charme & +30% Sang de sacrifice par niveau.",
                BaseCost = 55, CostMult = 1.75, MaxLevel = 999, IconKey = "star"
            },
            new RepeatableUpgrade
            {
                Id = "rep_boots", Name = "Pas de l'Ombre",
                Description = "+12% Vitesse & +0.5u d'Aura par niveau.",
                BaseCost = 85, CostMult = 2.1, MaxLevel = 8, IconKey = "boots"
            },
            new RepeatableUpgrade
            {
                Id = "rep_party", Name = "Soiree Etudiante",
                Description = "+4 filles sur le campus & reapparition plus rapide.",
                BaseCost = 130, CostMult = 1.9, MaxLevel = 12, IconKey = "heart"
            }
        };

        public static string FormatNumber(double n)
        {
            if (double.IsNaN(n) || double.IsInfinity(n)) return "0";
            if (Math.Abs(n) < 1000) return Math.Floor(n).ToString("N0");
            string[] units = { "", "K", "M", "B", "T", "Qa", "Qi" };
            int t = (int)Math.Floor(Math.Log10(Math.Abs(n)) / 3.0);
            if (t >= units.Length) return n.ToString("0.00e0");
            double v = n / Math.Pow(1000.0, t);
            return (v >= 100 ? v.ToString("0.0") : v.ToString("0.00")) + units[t];
        }
    }
}
