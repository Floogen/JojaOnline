using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Harmony;
using MailFrameworkMod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;

namespace JojaOnline
{
    // NOTE: Compability issue with SpaceCore, must be v1.4.0 and below until they fix save serializer
    public class ModEntry : Mod
    {
        // Make this available to other methods in the class to access
        private IModHelper modHelper;

        public override void Entry(IModHelper helper)
        {
            modHelper = helper;
            try
            {
                harmonyPatch();
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patch: {e}", LogLevel.Error);
            }

            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarting;
        }

        public void harmonyPatch()
        {
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Fires after game is launched, right before first update tick. Happens once per game session (unrelated to loading saves).
        /// All mods are loaded and initialized at this point, so this is a good time to set up mod integrations.
        /// </summary>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            // TODO: Make custom computer item
            // TODO: Look into Content Patcher
            string jojaLetterBGPath = Path.Combine("assets", "jojaLetterBG.png");
            Letter letter = new Letter("JojaExample", "Valued Customer,^^Thank you for using Joja Online. Your order is attached below.^^We look forward to your continued business.^^- Joja Co.", new List<Item> { new StardewValley.Object(60, 5), new StardewValley.Object(388, 50) }, l => !Game1.player.mailReceived.Contains(l.Id), l => Game1.player.mailReceived.Add(l.Id))
            {
                LetterTexture = modHelper.Content.Load<Texture2D>(jojaLetterBGPath),
                TextColor = 7
            };
            MailDao.SaveLetter(letter);
            //https://github.com/Digus/StardewValleyMods/blob/5ba26d37bdaccf155ae2c652e17e2ed926c5d1d0/MailFrameworkMod/MailDao.cs
            //https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started
        }

        private void OnDayStarting(object sender, DayStartedEventArgs e)
        {
            this.Monitor.Log($"{Game1.player.Name} got mail!", LogLevel.Debug);
        }
    }
}
