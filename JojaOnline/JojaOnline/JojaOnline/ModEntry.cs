using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Harmony;
using JojaOnline.JojaOnline.UI;
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
        public override void Entry(IModHelper helper)
        {
            try
            {
                harmonyPatch();
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patch: {e}", LogLevel.Error);
            }

            // Load the monitor
            JojaResources.LoadMonitor(this.Monitor);

            // Get the image resources needed for the mod
            JojaResources.LoadTextures(helper);

            helper.Events.GameLoop.DayStarted += this.OnDayStarting;
        }

        public void harmonyPatch()
        {
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnDayStarting(object sender, DayStartedEventArgs e)
        {
            // Modify JojaStock to include all year seed stock (if past year 1) & other items
            JojaResources.SetJojaOnlineStock();

            JojaSite.PickRandomItemForDiscount();
            this.Monitor.Log($"Picked a random item for discount at JojaOnline store.", LogLevel.Debug);
        }
    }
}
