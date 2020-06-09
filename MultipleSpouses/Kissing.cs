﻿ using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using static Harmony.AccessTools;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;
using System.Linq;
using StardewValley.Network;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using StardewValley.Events;
using StardewValley.Menus;

namespace MultipleSpouses
{
    /// <summary>The mod entry point.</summary>
    public class Kissing
    {

		public static List<string> kissingSpouses = new List<string>();
		public static int lastKissTime = 0;
		public static SoundEffect kissEffect = null;


        public static void TrySpousesKiss()
        {
			GameLocation location = Game1.currentLocation;

			lastKissTime++;

			if (location == null || location.characters == null)
				return;

			List<NPC> list = location.characters.ToList();

			ModEntry.ShuffleList(ref list);

			foreach (NPC npc1 in list)
			{
				foreach (NPC npc2 in list)
				{
					if (npc1.Name == npc2.Name)
						continue;

					if (lastKissTime >= ModEntry.config.MinSpouseKissInterval)
						kissingSpouses.Clear();




					float distance = Vector2.Distance(npc1.position, npc2.position);
					if (
						npc1.getSpouse() != null && npc2.getSpouse() != null  
						&& npc1.getSpouse().Name == npc2.getSpouse().Name 
						&& distance < ModEntry.config.MaxDistanceToKiss 
						&& !kissingSpouses.Contains(npc1.Name) 
						&& !kissingSpouses.Contains(npc2.Name) 
						&& lastKissTime > ModEntry.config.MinSpouseKissInterval 
						&& ModEntry.myRand.NextDouble() < ModEntry.config.SpouseKissChance
					)
                    {
						kissingSpouses.Add(npc1.Name);
						kissingSpouses.Add(npc2.Name);
						ModEntry.PMonitor.Log("spouses kissing"); 
						lastKissTime = 0;
						Vector2 npc1pos = npc1.position;
						Vector2 npc2pos = npc2.position;
						int npc1face = npc1.facingDirection;
						int npc2face = npc1.facingDirection;
						Vector2 midpoint = new Vector2((npc1.position.X + npc2.position.X) / 2, (npc1.position.Y + npc2.position.Y) / 2);
						PerformKiss(npc1, midpoint, npc2.Name);
						PerformKiss(npc2, midpoint, npc1.Name);
						DelayedAction action = new DelayedAction(1000);
						var t = Task.Run(async delegate
						{
							await Task.Delay(TimeSpan.FromSeconds(1));
							npc1.position.Value = npc1pos;
							npc2.position.Value = npc2pos;
							npc1.FacingDirection = npc1face;
							npc2.FacingDirection = npc2face;
							return;
						});
					}
				}
			}
		}

        private static void PerformKiss(NPC npc, Vector2 midpoint, string partner)
        {
			int spouseFrame = 28;
			bool facingRight = true;
			string name = npc.Name;
			if (name == "Sam")
			{
				spouseFrame = 36;
				facingRight = true;
			}
			else if (name == "Penny")
			{
				spouseFrame = 35;
				facingRight = true;
			}
			else if (name == "Sebastian")
			{
				spouseFrame = 40;
				facingRight = false;
			}
			else if (name == "Alex")
			{
				spouseFrame = 42;
				facingRight = true;
			}
			else if (name == "Krobus")
			{
				spouseFrame = 16;
				facingRight = true;
			}
			else if (name == "Maru")
			{
				spouseFrame = 28;
				facingRight = false;
			}
			else if (name == "Emily")
			{
				spouseFrame = 33;
				facingRight = false;
			}
			else if (name == "Harvey")
			{
				spouseFrame = 31;
				facingRight = false;
			}
			else if (name == "Shane")
			{
				spouseFrame = 34;
				facingRight = false;
			}
			else if (name == "Elliott")
			{
				spouseFrame = 35;
				facingRight = false;
			}
			else if (name == "Leah")
			{
				spouseFrame = 25;
				facingRight = true;
			}
			else if (name == "Abigail")
			{
				spouseFrame = 33;
				facingRight = false;
			}

			bool right = npc.position.X < midpoint.X;
			if(npc.position == midpoint)
            {
				right = String.Compare(npc.Name, partner) < 0;
			}
			else if(npc.position.X == midpoint.X)
            {
				right = npc.position.Y > midpoint.Y;
            }

			bool flip = (facingRight && !right) || (!facingRight && right);

			int offset = 24;
			if (right)
				offset *= -1;

			npc.position.Value = new Vector2(midpoint.X+offset,midpoint.Y);

			int delay = 1000;
			npc.movementPause = delay;
			npc.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(npc.haltMe), true)
				});
			npc.doEmote(20, true);
            if (ModEntry.config.RealKissSound && kissEffect != null)
            {
				float distance = 1f / ((Vector2.Distance(midpoint, Game1.player.position) / 256) + 1);
				float pan = (float)(Math.Atan((midpoint.X - Game1.player.position.X) / Math.Abs(midpoint.Y - Game1.player.position.Y)) /(Math.PI/2));
				ModEntry.PMonitor.Log($"kiss distance: {distance} pan: {pan}");
				kissEffect.Play(distance, 0, pan);
			}
			else
            {
				Game1.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
			}

			npc.Sprite.UpdateSourceRect();
		}

	}
}