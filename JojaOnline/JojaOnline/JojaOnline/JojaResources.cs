using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using Object = StardewValley.Object;

namespace JojaOnline
{
	public static class JojaResources
	{
		private static IMonitor modMonitor;

		private static Dictionary<ISalable, int[]> jojaOnlineStock;
		private static List<int> cachedItemSheetIndexes = new List<int>();

		private static Texture2D jojaMailBackground;
		private static Texture2D jojaSiteBackground;
		private static Texture2D jojaCheckoutBackground;

		private static Texture2D jojaSiteSpriteSheet;

		private static Texture2D jojaAdBanners;

		public static void LoadMonitor(IMonitor monitor)
		{
			modMonitor = monitor;
		}

		public static IMonitor GetMonitor()
		{
			return modMonitor;
		}

		public static void SetJojaOnlineStock()
		{
			// Clone the current stock
			jojaOnlineStock = Utility.getJojaStock();

			// If past year one, unlock all seeds (that aren't in the current season due to initial cloning)
			if (Game1.year > 1)
			{
				modMonitor.Log("Loading JojaMart's stock for all seasons for JojaOnline", LogLevel.Debug);
				if (!Game1.currentSeason.Equals("spring"))
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, 472, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[472].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 473, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[473].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 474, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[474].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 475, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[475].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 427, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[427].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 429, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[429].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 477, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[477].Split('/')[1])), 2147483647
					});
				}
				if (!Game1.currentSeason.Equals("summer"))
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, 480, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[480].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 482, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[482].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 483, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[483].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 484, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[484].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 479, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[479].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 302, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[302].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 453, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[453].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 455, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[455].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(431, int.MaxValue, isRecipe: false, 100), new int[2]
					{
						(int)(50f), 2147483647
					});
				}
				if (!Game1.currentSeason.Equals("fall"))
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, 487, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[487].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 488, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[488].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 483, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[483].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 490, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[490].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 299, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[299].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 301, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[301].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 492, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[492].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 491, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[491].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 493, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[493].Split('/')[1])), 2147483647
					});
					AddToJojaOnlineStock(new Object(431, int.MaxValue, isRecipe: false, 100), new int[2]
					{
						(int)(50f), 2147483647
					});
					AddToJojaOnlineStock(new Object(Vector2.Zero, 425, int.MaxValue), new int[2]
					{
						(int)((float)Convert.ToInt32(Game1.objectInformation[425].Split('/')[1])), 2147483647
					});
				}
			}
		}

		public static void AddToJojaOnlineStock(Object item, int[] details)
        {
			if (cachedItemSheetIndexes.Contains(item.parentSheetIndex))
            {
				return;
            }
			cachedItemSheetIndexes.Add(item.parentSheetIndex);

			// Add the unique item
			jojaOnlineStock.Add(item, details);
		}

		public static Dictionary<ISalable, int[]> GetJojaOnlineStock()
        {
			return jojaOnlineStock;
        }

		public static void LoadTextures(IModHelper helper)
		{
			// Load the MFM related background(s)
			jojaMailBackground = helper.Content.Load<Texture2D>(Path.Combine("assets", "jojaLetterBG.png"));

			// Load in the JojaSite background
			jojaSiteBackground = helper.Content.Load<Texture2D>(Path.Combine("assets", "jojaStoreBG.png"));

			// Load in the JojaSite spritesheet
			jojaSiteSpriteSheet = helper.Content.Load<Texture2D>(Path.Combine("assets", "jojaSiteSprites.png"));

			// Load JojaSite checkout background
			jojaCheckoutBackground = helper.Content.Load<Texture2D>(Path.Combine("assets", "jojaCheckoutBG.png"));

			// Load JojaSite ad banners
			jojaAdBanners = helper.Content.Load<Texture2D>(Path.Combine("assets", "jojaBanners.png"));
		}

		public static Texture2D GetJojaMailBackground()
		{
			return jojaMailBackground;
		}

		public static Texture2D GetJojaSiteBackground()
		{
			return jojaSiteBackground;
		}

		public static Texture2D GetJojaCheckoutBackground()
		{
			return jojaCheckoutBackground;
		}

		public static Texture2D GetJojaSiteSpriteSheet()
		{
			return jojaSiteSpriteSheet;
		}

		public static Texture2D GetJojaAdBanners()
		{
			return jojaAdBanners;
		}
	}
}
