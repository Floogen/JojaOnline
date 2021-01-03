using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public static void SetJojaOnlineStock(bool doStockAllSeedsBeforeYearOne)
		{
			// Clone the current stock
			jojaOnlineStock = new Dictionary<ISalable, int[]>();

			// Add wood, stone and hardwood
			AddToJojaOnlineStock(new Object(Vector2.Zero, 388, int.MaxValue));
			AddToJojaOnlineStock(new Object(Vector2.Zero, 390, int.MaxValue));
			AddToJojaOnlineStock(new Object(Vector2.Zero, 709, int.MaxValue), 500);

			// Add coal
			AddToJojaOnlineStock(new Object(Vector2.Zero, 382, int.MaxValue));

			// Add battery pack
			AddToJojaOnlineStock(new Object(Vector2.Zero, 390, int.MaxValue), 2500);

			// Add cloth
			AddToJojaOnlineStock(new Object(Vector2.Zero, 428, int.MaxValue), 2000);

			// Add energy tonic
			AddToJojaOnlineStock(new Object(Vector2.Zero, 349, int.MaxValue));

			// Add some of Pierre's goods (if available)
			foreach (Item item in (new SeedShop()).shopStock().Keys)
            {
				if (item.parentSheetIndex == 368)
                {
					AddToJojaOnlineStock(new Object(Vector2.Zero, item.parentSheetIndex, int.MaxValue), 100);
				}
				else if (item.parentSheetIndex == 369)
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, item.parentSheetIndex, int.MaxValue), 150);
				}
				else if (item.parentSheetIndex == 370)
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, item.parentSheetIndex, int.MaxValue), 100);
				}
				else if (item.parentSheetIndex == 371)
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, item.parentSheetIndex, int.MaxValue), 150);
				}
				else if (item.parentSheetIndex == 465)
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, item.parentSheetIndex, int.MaxValue), 100);
				}
				else if (item.parentSheetIndex == 466)
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, item.parentSheetIndex, int.MaxValue), 150);
				}
			}

			// Add some of Marnie's goods (only hay for now)
			AddToJojaOnlineStock(new Object(Vector2.Zero, 178, int.MaxValue), 50);

			// Add the current JojaMart items
			Utility.getJojaStock().Where(x => !jojaOnlineStock.ContainsKey(x.Key)).ToList().ForEach(x => jojaOnlineStock.Add(x.Key, x.Value));

			// If past year one (or doStockAllSeedsBeforeYearOne), unlock all seeds (that aren't in the current season due to initial cloning)
			if (Game1.year > 1 || doStockAllSeedsBeforeYearOne)
			{
				modMonitor.Log("Loading JojaMart's stock for all seasons for JojaOnline", LogLevel.Debug);
				if (!Game1.currentSeason.Equals("spring"))
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, 472, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 473, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 474, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 475, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 427, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 429, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 477, int.MaxValue));
				}
				if (!Game1.currentSeason.Equals("summer"))
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, 480, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 482, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 483, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 484, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 479, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 302, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 453, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 455, int.MaxValue));
					AddToJojaOnlineStock(new Object(431, int.MaxValue, isRecipe: false, 100));
				}
				if (!Game1.currentSeason.Equals("fall"))
				{
					AddToJojaOnlineStock(new Object(Vector2.Zero, 487, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 488, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 483, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 490, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 299, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 301, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 492, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 491, int.MaxValue));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 493, int.MaxValue));
					AddToJojaOnlineStock(new Object(431, int.MaxValue, isRecipe: false, 100));
					AddToJojaOnlineStock(new Object(Vector2.Zero, 425, int.MaxValue));
				}
			}
		}

		public static void AddToJojaOnlineStock(Object item, int salePrice = -1, int stock = -1)
        {
			if (cachedItemSheetIndexes.Contains(item.parentSheetIndex))
            {
				return;
            }
			cachedItemSheetIndexes.Add(item.parentSheetIndex);

			// Add the unique item
			jojaOnlineStock.Add(item, new int[] { salePrice == -1 ? item.salePrice() : salePrice, stock == -1 ? int.MaxValue : stock });
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
