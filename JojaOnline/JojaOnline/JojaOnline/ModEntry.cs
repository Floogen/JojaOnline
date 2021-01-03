using System;
using System.Reflection;
using Harmony;
using JojaOnline.JojaOnline;
using JojaOnline.JojaOnline.Mailing;
using JojaOnline.JojaOnline.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace JojaOnline
{
    public class ModEntry : Mod
    {
        private ModConfig config;

        public override void Entry(IModHelper helper)
        {
            // PyTK (required for Custom Furniture) has compatibility issue with SpaceCore, must be v1.4.1 and below until SpaceCore or PyTK make the required changes
            if (Helper.ModRegistry.IsLoaded("spacechase0.SpaceCore"))
            {
                if (Helper.ModRegistry.Get("spacechase0.SpaceCore").Manifest.Version.IsNewerThan("1.4.1"))
                {
                    throw new InvalidOperationException("JojaOnline is only compatible with SpaceCore v1.4.1 and below due to a compatibility issue with PyTK. " +
                        "SpaceCore v1.4.1 works with Stardew Valley v1.5, so please use that if you wish to use this mod.");
                }
            }

            // Load our Harmony patches
            try
            {
                harmonyPatch();
            }
            catch (Exception e)
            {
                Monitor.Log($"Issue with Harmony patch: {e}", LogLevel.Error);
            }

            // Load the config
            this.config = helper.ReadConfig<ModConfig>();

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
            JojaResources.SetJojaOnlineStock(this.config.areAllSeedsAvailableBeforeYearOne);

            JojaSite.PickRandomItemForDiscount(this.config.minSalePercentage, this.config.maxSalePercentage);
            this.Monitor.Log($"Picked a random item for discount at JojaOnline store.", LogLevel.Debug);
        }
    }
}
