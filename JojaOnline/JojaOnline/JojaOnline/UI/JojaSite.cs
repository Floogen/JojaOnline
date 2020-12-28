using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JojaOnline.JojaOnline.UI
{
    public class JojaSite: IClickableMenu
    {
        private readonly float scale = 1f;
        private readonly Texture2D sourceSheet = JojaResources.GetJojaSiteSpriteSheet();
        private readonly List<ClickableTextureComponent> clickables = new List<ClickableTextureComponent>();

        public JojaSite(int uiWidth, int uiHeight) : base(Game1.uiViewport.Width / 2 - (uiWidth + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (uiHeight + IClickableMenu.borderWidth * 2) / 2, uiWidth + IClickableMenu.borderWidth * 2, uiHeight + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            // Check if we need to scale back the UI
            if (uiHeight > Game1.uiViewport.Height)
            {
                scale = Game1.viewport.Height / (float)uiHeight;
                this.width = (int)(this.width * scale);
                this.height = (int)(this.height * scale);
            }

            // Draw the clickables (buttons, etc)

            // Joja Ads
            drawClickable("jojaCanAd", 42, 248, sourceSheet, new Rectangle(608, 0, 208, 208));
            drawClickable("jojaJoinUsAd", 829, 260, sourceSheet, new Rectangle(0, 352, 201, 194));

            // Banner, logos
            drawClickable("jojaLogo", 35, 105, sourceSheet, new Rectangle(0, 0, 336, 128));
            drawClickable("jojaMotto", 435, 150, sourceSheet, new Rectangle(0, 144, 384, 64));
            drawClickable("shoppingCart", 900, 150, sourceSheet, new Rectangle(0, 208, 80, 48));

            // Override default close button position
            this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 50, this.yPositionOnScreen + 70, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, speaker: false, drawOnlyBox: true, r: 80, g: 123, b:186);
            Game1.dayTimeMoneyBox.drawMoneyBox(b);

            // Draw the custom store BG
            b.Draw(JojaResources.GetJojaSiteBackground(), new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 100), new Rectangle(0, 0, 1008, 1194), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);

            // Draw the static images
            //b.Draw(sourceSheet, new Vector2((this.xPositionOnScreen + 32) * scale, (this.yPositionOnScreen + 100) * scale), new Rectangle(0, 0, 1008, 1194), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            //b.Draw(sourceSheet, new Vector2((this.xPositionOnScreen + 435) * scale, (this.yPositionOnScreen + 500) * scale), new Rectangle(0, 704, 56, 56), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            //https://spacechase0.com/mods/stardew-valley/better-shop-menu
            // Draw the clickables
            foreach (ClickableTextureComponent clickable in clickables)
            {
                clickable.draw(b);
            }

            this.upperRightCloseButton.draw(b);
            this.drawMouse(b);
        }

        public void drawClickable(string identifier, int x, int y, Texture2D sourceTexture, Rectangle sourceRect)
        {
            Rectangle bounds = new Rectangle((int)((this.xPositionOnScreen + x) * scale), (int)((this.yPositionOnScreen + y) * scale), (int)(sourceRect.Width * scale), (int)(sourceRect.Height * scale));
            ClickableTextureComponent clickable = new ClickableTextureComponent(bounds, sourceTexture, sourceRect, scale)
            {
                name = identifier
            };

            clickables.Add(clickable);
        }
    }
}
