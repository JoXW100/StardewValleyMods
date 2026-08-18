using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;
using System.Collections.Generic;
using System;
using System.Reflection;

namespace HugsAndKisses.Framework
{
    public static class NPCPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
        }

        public static bool NPC_checkAction_Prefix(ref NPC __instance, ref Farmer who, GameLocation l, ref bool __result)
        {
            try
            {
                if (!Config.EnableMod || __instance.IsInvisible || __instance.isSleeping.Value || !who.canMove || who.pantsItem.Value?.ParentSheetIndex == 15 && (__instance.Name.Equals("Lewis") || __instance.Name.Equals("Marnie")) || __instance.Name.Equals("Krobus") && who.hasQuest("28") || !who.IsLocalPlayer)
                {
                    return true;
                }

                var localInstance = __instance;
                if (who.NotifyQuests((Quest quest) => quest.OnNpcSocialized(localInstance, true)) && Game1.dialogueUp)
                {
                    return true;
                }

                Monitor.Log($"Checking action for {who.Name} kissing/hugging {__instance.Name}", LogLevel.Debug);
                if (!who.friendshipData.TryGetValue(__instance.Name, out var data))
                {
                    Monitor.Log($"Checking action failed, {__instance.Name} is missing relation data.", LogLevel.Debug);
                    return true;
                }

                if (!data.IsMarried() && !data.IsEngaged() && !((__instance.datable.Value || Config.AllowNonDateableNPCsToHugAndKiss) && ((data.IsDating() && Config.DatingKisses) || (who.getFriendshipHeartLevelForNPC(__instance.Name) >= Config.HeartsForFriendship && Config.FriendHugs))))
                {
                    Monitor.Log($"Checking action failed, config disallow it. married = {data.IsMarried()}, engaged = {data.IsEngaged()}, dateable = {__instance.datable.Value}, dating = {data.IsDating()}, hearts = {who.getFriendshipHeartLevelForNPC(__instance.Name)}", LogLevel.Debug);
                    return true;
                }

                __instance.faceDirection(-3);
                List<FarmerSprite.AnimationFrame> previousAnimation = null;
                int? previousAnimationIndex = null;
                if (__instance.Sprite.CurrentAnimation is not null)
                {
                    previousAnimation = new List<FarmerSprite.AnimationFrame>(__instance.Sprite.CurrentAnimation);
                    previousAnimationIndex = TryGetAnimationIndex(__instance.Sprite);
                    Monitor.Log($"{__instance.Name} is in an animation. It will be restored after kissing/hugging.", LogLevel.Debug);
                }

                if (__instance.hasTemporaryMessageAvailable())
                {
                    Monitor.Log($"Checking action failed, {__instance.Name} has temporary message available.", LogLevel.Debug);
                    return true;
                }

                if (__instance.currentMarriageDialogue.Count > 0 || __instance.CurrentDialogue.Count > 0)
                {
                    Monitor.Log($"Checking action failed, {__instance.Name} has dialoge available.", LogLevel.Debug);
                    return true;
                }

                if (__instance.isMoving())
                {
                    Monitor.Log($"Checking action may continue despite {__instance.Name} moving.", LogLevel.Debug);
                    //return true;
                }

                if (who.ActiveObject is not null)
                {
                    Monitor.Log($"Checking action failed, {__instance.Name} is holding an object.", LogLevel.Debug);
                    return true;
                }

                if (who.isRidingHorse())
                {
                    Monitor.Log($"Checking action failed, {__instance.Name} is riding a horse.", LogLevel.Debug);
                    return true;
                }

                bool kissing = data.IsDating() || data.IsMarried() || data.IsEngaged();
                if (kissing && __instance.hasBeenKissedToday.Value && !Config.UnlimitedDailyKisses)
                {
                    Monitor.Log($"Kissing failed, already kissed {__instance.Name}", LogLevel.Debug);
                    return true;
                }

                string actionName = kissing ? "kissing" : "hugging";
                __instance.faceGeneralDirection(who.getStandingPosition(), 0, false);
                who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
                if (__instance.FacingDirection != 3 && __instance.FacingDirection != 1)
                {
                    Monitor.Log($"{actionName} failed, {__instance.Name} is facing the wrong direction", LogLevel.Debug);
                    return true;
                }

                Monitor.Log($"{who.Name} {actionName} {__instance.Name}", LogLevel.Debug);
                if (kissing)
                {
                    Kissing.PlayerNPCKiss(who, __instance);
                }
                else
                {
                    Kissing.PlayerNPCHug(who, __instance);
                }

                if (previousAnimation is not null)
                {
                    NPC npc = __instance;
                    string name = __instance.Name;
                    int restoreDelayMs = Game1.IsMultiplayer ? 1000 : 10;
                    DelayedAction.functionAfterDelay(() =>
                    {
                        try
                        {
                            if (npc?.Sprite is null)
                            {
                                return;
                            }

                            npc.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>(previousAnimation));
                            if (previousAnimationIndex.HasValue)
                            {
                                TrySetAnimationIndex(npc.Sprite, previousAnimationIndex.Value);
                            }
                            npc.Sprite.UpdateSourceRect();
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log($"Failed to restore animation for {name}:\n{ex}", LogLevel.Warn);
                        }
                    }, restoreDelayMs + 10);
                }

                __result = true;
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        private static int? TryGetAnimationIndex(AnimatedSprite sprite)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            FieldInfo field = sprite.GetType().GetField("currentAnimationIndex", flags);
            if (field?.FieldType == typeof(int) && field.GetValue(sprite) is int fieldValue)
            {
                return fieldValue;
            }

            PropertyInfo property = sprite.GetType().GetProperty("currentAnimationIndex", flags);
            if (property?.PropertyType == typeof(int) && property.CanRead && property.GetValue(sprite) is int propertyValue)
            {
                return propertyValue;
            }

            return null;
        }

        private static void TrySetAnimationIndex(AnimatedSprite sprite, int index)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            FieldInfo field = sprite.GetType().GetField("currentAnimationIndex", flags);
            if (field?.FieldType == typeof(int))
            {
                field.SetValue(sprite, index);
                return;
            }

            PropertyInfo property = sprite.GetType().GetProperty("currentAnimationIndex", flags);
            if (property?.PropertyType == typeof(int) && property.CanWrite)
            {
                property.SetValue(sprite, index);
            }
        }
    }
}
