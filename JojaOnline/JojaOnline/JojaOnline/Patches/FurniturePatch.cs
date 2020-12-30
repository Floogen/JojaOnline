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
                Game1.activeClickableMenu = new JojaSite(1000, 1250);
                return false;
            }

            return true;
        }
    }
}
