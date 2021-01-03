using Harmony;
using JojaOnline.JojaOnline.UI;
using StardewValley;
using StardewValley.Objects;
using System.Reflection;

namespace JojaOnline.JojaOnline.Patches
{
    [HarmonyPatch]
    public class FurniturePatch
    {
        internal static MethodInfo TargetMethod()
        {
            return AccessTools.Method(typeof(StardewValley.Objects.Furniture), nameof(StardewValley.Objects.Furniture.checkForAction));
        }

        internal static bool Prefix(Furniture __instance, Farmer who, bool justCheckingForActivity = false)
        {
            if (__instance.name == "Computer")
            {
                // May need to scale
                // Check if we need to scale back the UI
                int width = 750;
                int height = 1000;
                float scale = 1f;

                if (height > Game1.uiViewport.Height)
                {
                    scale = 750 / 1000f;
                    width = 525;
                    height = 700;
                }

                Game1.activeClickableMenu = new JojaSite(width, height, scale);
                return false;
            }

            return true;
        }
    }
}
