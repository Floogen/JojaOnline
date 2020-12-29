using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JojaOnline
{
    public static class JojaResources
    {
        private static IMonitor modMonitor;

        private static Texture2D jojaMailBackground;
        private static Texture2D jojaSiteBackground;
        private static Texture2D jojaSiteSpriteSheet;

        public static void LoadMonitor(IMonitor monitor)
        {
            modMonitor = monitor;
        }

        public static IMonitor GetMonitor()
        {
            return modMonitor;
        }


        public static void LoadTextures(IModHelper helper)
        {
            // Load the MFM related background(s)
            jojaMailBackground = helper.Content.Load<Texture2D>(Path.Combine("assets", "jojaLetterBG.png"));

            // Load in the JojaSite background
            jojaSiteBackground = helper.Content.Load<Texture2D>(Path.Combine("assets", "jojaStoreBG.png"));

            // Load in the JojaSite spritesheet
            jojaSiteSpriteSheet = helper.Content.Load<Texture2D>(Path.Combine("assets", "jojaSiteSprites.png"));
        }

        public static Texture2D GetJojaMailBackground()
        {
            return jojaMailBackground;
        }

        public static Texture2D GetJojaSiteBackground()
        {
            return jojaSiteBackground;
        }

        public static Texture2D GetJojaSiteSpriteSheet()
        {
            return jojaSiteSpriteSheet;
        }
    }
}
