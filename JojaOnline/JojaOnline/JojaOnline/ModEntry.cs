using System;
using System.Reflection;
using Harmony;
using JojaOnline.JojaOnline.Mailing;
using JojaOnline.JojaOnline.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;

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

            // Hook into the game's daily events
            helper.Events.GameLoop.DayStarted += this.OnDayStarting;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saved += this.OnSaved;
        }

        public void harmonyPatch()
        {
            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            JojaMail.ProcessPlayerMailbox();
            this.Monitor.Log($"Processed player's mailbox to check for any scheduled JojaMail orders.");
        }

        private void OnSaved(object sender, SavedEventArgs e)
        {
            JojaMail.ProcessPlayerMailbox();
            this.Monitor.Log($"Processed player's mailbox to check for any scheduled JojaMail orders.");
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
