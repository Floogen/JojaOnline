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

        public JojaSite(int uiWidth, int uiHeight) : base(Game1.uiViewport.Width / 2 - (uiWidth + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (uiHeight + IClickableMenu.borderWidth * 2) / 2, uiWidth + IClickableMenu.borderWidth * 2, uiHeight + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            // Check if we need to scale back the UI
            if (uiHeight > Game1.uiViewport.Height)
            {
                scale = Game1.viewport.Height / (float)uiHeight;
                this.width = (int)(this.width * scale);
                this.height = (int)(this.height * scale);
            }

            this.initializeUpperRightCloseButton();
            this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 50, this.yPositionOnScreen + 70, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, speaker: false, drawOnlyBox: true);
            Game1.dayTimeMoneyBox.drawMoneyBox(b);

            this.upperRightCloseButton.draw(b);
            this.drawMouse(b);
        }
    }
}
