﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace HugsAndKisses.Framework
{
    /// <summary>The mod entry point.</summary>
    public class Misc
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }


        public static bool GetFacingRight(string name)
        {
            switch (name)
            {
                case "Sam":
                    return true;
                case "Penny":
                    return true;
                case "Sebastian":
                    return false;
                case "Alex":
                    return true;
                case "Krobus":
                    return true;
                case "Maru":
                    return false;
                case "Emily":
                    return false;
                case "Harvey":
                    return false;
                case "Shane":
                    return false;
                case "Elliott":
                    return false;
                case "Leah":
                    return true;
                case "Abigail":
                    return false;
                default:
                    return true;
            }
        }

        public static int GetKissingFrame(string name)
        {
            if (Game1.getCharacterFromName(name)?.datable.Value == false && !Config.UseNonDateableNPCsKissFrames)
            {
                return 0;
            }

            List<string> customFrames = Config.CustomKissFrames.Split(',').ToList();
            foreach (string nameframe in customFrames)
            {
                if (nameframe.StartsWith(name + ":") && int.TryParse(nameframe.Substring(name.Length + 1), out int frame))
                {
                        return frame;
                }
            }

            return name switch
            {
                "Sam" => 36,
                "Penny" => 35,
                "Sebastian" => 40,
                "Alex" => 42,
                "Krobus" => 16,
                "Maru" => 28,
                "Emily" => 33,
                "Harvey" => 31,
                "Shane" => 34,
                "Elliott" => 35,
                "Leah" => 25,
                "Abigail" => 33,
                _ => 28,
            };
        }
        public static void ShowHeart(NPC npc)
        {
            ModEntry.mp.broadcastSprites(npc.currentLocation, new TemporaryAnimatedSprite[]
            {
                new("LooseSprites\\Cursors", new Rectangle(211, 428, 7, 6), 2000f, 1, 0, npc.Tile * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                {
                    motion = new Vector2(0f, -0.5f),
                    alphaFade = 0.01f
                }
            });
        }
        public static void ShowSmiley(NPC npc)
        {
            ModEntry.mp.broadcastSprites(npc.currentLocation, new TemporaryAnimatedSprite[]
            {
                new("LooseSprites\\emojis", new Rectangle(0, 0, 9, 9), 2000f, 1, 0, npc.Tile * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                {
                    motion = new Vector2(0f, -0.5f),
                    alphaFade = 0.01f
                }
            });
        }

        public static Dictionary<string, NPC> GetSpouses(Farmer farmer, int all)
        {
            Dictionary<string, NPC> spouses = new();
            if (all < 0)
            {
                NPC ospouse = farmer.getSpouse();
                if (ospouse != null)
                {
                    spouses.Add(ospouse.Name, ospouse);
                }
            }
            foreach (string friend in farmer.friendshipData.Keys)
            {
                if (Game1.getCharacterFromName(friend, true) != null && farmer.friendshipData[friend].IsMarried() && (all > 0 || friend != farmer.spouse))
                {
                    spouses.Add(friend, Game1.getCharacterFromName(friend, true));
                }
            }

            return spouses;
        }

        public static void ResetSpouses(Farmer f)
        {
            Dictionary<string, NPC> spouses = GetSpouses(f, 1);
            if (f.spouse == null)
            {
                if (spouses.Count > 0)
                {
                    Monitor.Log("No official spouse, setting official spouse to: " + spouses.First().Key);
                    f.spouse = spouses.First().Key;
                }
            }

            foreach (string name in f.friendshipData.Keys)
            {
                var data = f.friendshipData[name];
                if (data.IsEngaged())
                {
                    Monitor.Log($"{f.Name} is engaged to: {name} {data.CountdownToWedding} days until wedding");
                    if (data.WeddingDate.TotalDays < new WorldDate(Game1.Date).TotalDays)
                    {
                        Monitor.Log("invalid engagement: " + name);
                        data.WeddingDate.TotalDays = new WorldDate(Game1.Date).TotalDays + 1;
                    }
                    if (f.spouse != name)
                    {
                        Monitor.Log("setting spouse to engagee: " + name);
                        f.spouse = name;
                    }
                }
                if (data.IsMarried() && f.spouse != name)
                {
                    //Monitor.Log($"{f.Name} is married to: {name}");
                    var spouseData = f.friendshipData[f.spouse];
                    if (f.spouse != null && spouseData != null && !spouseData.IsMarried() && !spouseData.IsEngaged())
                    {
                        Monitor.Log("invalid ospouse, setting ospouse to " + name);
                        f.spouse = name;
                    }
                    if (f.spouse == null)
                    {
                        Monitor.Log("null ospouse, setting ospouse to " + name);
                        f.spouse = name;
                    }
                }
            }
        }
        public static void SetNPCRelations()
        {
            ModEntry.relationships.Clear();
            foreach (var (key, value) in Game1.characterData)
            {
                ModEntry.relationships[key] = value.FriendsAndFamily ?? new Dictionary<string, string>();
            }
        }

        public static readonly string[] relativeRoles = new string[]
        {
            "son",
            "daughter",
            "sister",
            "brother",
            "dad",
            "mom",
            "father",
            "mother",
            "aunt",
            "uncle",
            "cousin",
            "nephew",
            "niece",
            "offspring",
            "parent",
            "relative",
            "grandmother",
            "grandfather",
            "grandparent",
            "granddaughter",
            "grandson",
            "grandchild",
            "grandniece",
            "grandnephew"
        };

        public static readonly string[] spouseRoles = new string[]
        {
            "husband",
            "wife",
            "partner",
            "girlfriend",
            "boyfriend",
            "lover"
        };

        public static bool AreNPCsMarried(string npc1, string npc2)
        {
            return IsRelated(npc1, npc2, spouseRoles) || IsRelated(npc2, npc1, spouseRoles);
        }

        public static bool AreNPCsRelated(string npc1, string npc2)
        {
            return IsRelated(npc1, npc2, relativeRoles) || IsRelated(npc2, npc1, relativeRoles);
        }

        private static bool IsRelated(string npc1, string npc2, IEnumerable<string> roles)
        {
            if (npc1 is null || npc2 is null)
            {
                return false;
            }

            if (ModEntry.relationships.TryGetValue(npc1, out var relations) && relations.TryGetValue(npc2, out var relation))
            {
                foreach (string role in roles)
                {
                    if (relation is not null && relation.Contains(role))
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        public static void ShuffleList<T>(ref List<T> list)
        {
            int n = list.Count;
            while (n-- > 1)
            {
                int k = ModEntry.myRand.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }

        public static void ShuffleDict<T1, T2>(ref Dictionary<T1, T2> list)
        {
            int n = list.Count;
            while (n-- > 1)
            {
                int k = ModEntry.myRand.Next(n + 1);
                var key_k = list.Keys.ToArray()[k];
                var key_n = list.Keys.ToArray()[n];
                (list[key_n], list[key_k]) = (list[key_k], list[key_n]);
            }
        }
    }
}