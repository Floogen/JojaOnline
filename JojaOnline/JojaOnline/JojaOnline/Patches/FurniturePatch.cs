using Harmony;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                Game1.activeClickableMenu = new ShopMenu(Utility.getAllFurnituresForFree(), 0, null, null, null, "Furniture Catalogue");
                return false;
            }

            return true;
        }
    }
}
