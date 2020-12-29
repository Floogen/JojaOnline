﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        private Rectangle scrollBarRunner;
        private ClickableTextureComponent scrollBar;
        private List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();

        private bool scrolling = false;
        private int currentItemIndex = 0;
        private List<ISalable> forSale = new List<ISalable>();

        // Item: [price, quantity]
        private Dictionary<ISalable, int[]> itemsInCart = new Dictionary<ISalable, int[]>();
        private Dictionary<ISalable, int[]> itemPriceAndStock = new Dictionary<ISalable, int[]>();

        public JojaSite(int uiWidth, int uiHeight) : base(Game1.uiViewport.Width / 2 - (uiWidth + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (uiHeight + IClickableMenu.borderWidth * 2) / 2, uiWidth + IClickableMenu.borderWidth * 2, uiHeight + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            // Get the items to be sold
            foreach (ISalable j in Utility.getJojaStock().Keys)
            {
                if (j is StardewValley.Object && (bool)(j as StardewValley.Object).isRecipe)
                {
                    continue;
                }

                this.forSale.Add(j);
                this.itemPriceAndStock.Add(j, new int[2]
                {
                    j.salePrice(),
                    j.maximumStackSize()
                });
            }

            // Check if we need to scale back the UI
            if (uiHeight > Game1.uiViewport.Height)
            {
                scale = Game1.viewport.Height / (float)uiHeight;
                this.width = (int)(this.width * scale);
                this.height = (int)(this.height * scale);
            }

            // Draw the clickables (buttons, etc)
            for (int i = 0; i < 4; i++)
            {
                this.forSaleButtons.Add(new ClickableComponent(new Rectangle(775 + 16, 750 + 16 + i * ((this.height - 256) / 8), (this.width - 128) / 2, (this.height - 256) / 8 + 4), string.Concat(i))
                {
                    myID = i + 3546,
                    fullyImmutable = true
                });

                this.forSaleButtons.Add(new ClickableComponent(new Rectangle(800 + 16 + ((this.width - 128) / 2), 750 + 16 + i * ((this.height - 256) / 8), (this.width - 128) / 2, (this.height - 256) / 8 + 4), string.Concat(i))
                {
                    myID = (7 - i) + 3546,
                    fullyImmutable = true
                });
            }


            scrollBar = new ClickableTextureComponent(new Rectangle(1267, 770, 25, 40), sourceSheet, new Rectangle(0, 848, 24, 40), 1f);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, scrollBar.bounds.Y, scrollBar.bounds.Width, 535);


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

        private bool tryToPurchaseItem(ISalable item, int numberToBuy, int x, int y, int indexInForSaleList)
        {
            if (item.GetSalableInstance().maximumStackSize() < numberToBuy)
            {
                numberToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
            }

            if (itemsInCart.ContainsKey(item))
            {
                itemsInCart[item][1] += numberToBuy;
            }
            else
            {
                itemsInCart.Add(item, new int[] { this.itemPriceAndStock[item][0], numberToBuy });
            }
            
            return true;
        }


        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            if (this.scrollBar.containsPoint(x, y))
            {
                this.scrolling = true;
            }
            else
            {
                for (int i = 0; i < this.forSaleButtons.Count; i++)
                {
                    if (this.currentItemIndex + i >= this.forSale.Count || !this.forSaleButtons[i].containsPoint(x, y))
                    {
                        continue;
                    }
                    int index = i + (currentItemIndex * 2);
                    if (this.forSale[index] != null)
                    {
                        int toBuy = (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : 5;
                        toBuy = Math.Min(toBuy, this.forSale[index].maximumStackSize());
                        if (this.tryToPurchaseItem(this.forSale[index], toBuy, x, y, index))
                        {
                            DelayedAction.playSoundAfterDelay("coin", 100);
                        }
                        else
                        {
                            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                            Game1.playSound("cancel");
                        }
                    }
                    
                    this.updateSaleButtonNeighbors();
                    this.setScrollBarToCurrentIndex();
                    return;
                }
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (this.scrolling)
            {
                int y2 = this.scrollBar.bounds.Y;
                this.scrollBar.bounds.Y = Math.Min(scrollBarRunner.Bottom - 35, Math.Max(y, scrollBarRunner.Top));
                float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
                this.currentItemIndex = Math.Min(this.forSale.Count - 12, Math.Max(0, (int)((float)this.forSale.Count * percentage)));
                this.updateSaleButtonNeighbors();
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            if (this.scrolling)
            {
                this.scrolling = false;
                this.setScrollBarToCurrentIndex();
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            for (int i = 0; i < this.forSaleButtons.Count; i++)
            {
                if (this.currentItemIndex + i >= this.forSale.Count || !this.forSaleButtons[i].containsPoint(x, y))
                {
                    continue;
                }

                int index = i + (currentItemIndex * 2);
                if (this.forSale[index] != null && itemsInCart.ContainsKey(this.forSale[index]))
                {
                    int toRemove = (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : 5;
                    toRemove = Math.Min(toRemove, itemsInCart[this.forSale[index]][1]);

                    itemsInCart[this.forSale[index]][1] -= toRemove;
                    if (itemsInCart[this.forSale[index]][1] <= 0)
                    {
                        itemsInCart.Remove(this.forSale[index]);
                    }

                    Game1.playSound("cancel");
                }

                this.updateSaleButtonNeighbors();
                this.setScrollBarToCurrentIndex();
                return;
            }
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

            IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 896, 24, 24), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 1f, drawShadow: false);
            scrollBar.draw(b);

            // Draw the for sale buttons
            foreach (ClickableComponent button in forSaleButtons)
            {
                int buttonPosition = forSaleButtons.IndexOf(button) + (currentItemIndex * 2);
                if (buttonPosition >= forSale.Count)
                {
                    continue;
                }

                IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 768, 60, 60), button.bounds.X, button.bounds.Y, button.bounds.Width, button.bounds.Height, Color.White, scale, drawShadow: false);

                // Draw the item for sale
                forSale[buttonPosition].drawInMenu(b, new Vector2(button.bounds.X + 32 - 8, button.bounds.Y + 15), scale, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);

                // Draw the quantity that is in the cart (if any)
                int currentlyInCart = 0;
                if (itemsInCart.ContainsKey(forSale[buttonPosition]))
                {
                    currentlyInCart = itemsInCart[forSale[buttonPosition]][1];
                }

                SpriteText.drawString(b, $"In Cart: {currentlyInCart}", button.bounds.X + 96 + 8, button.bounds.Y + 35, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 8);

                // Draw the price
                string price = forSale[buttonPosition].salePrice() + " ";
                SpriteText.drawString(b, price, button.bounds.Right - SpriteText.getWidthOfString(price) - 30, button.bounds.Bottom - 55, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 1);
                Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(button.bounds.Right - 52, button.bounds.Bottom - 50), new Rectangle(193, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, 0f);
            }

            //Utility.getJojaStock().First().Key.drawInMenu(b, new Vector2(775 + 32 - 8, 750 + 40), scale, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);
            //SpriteText.drawString(b, "In Cart: 0", 775 + 96 + 8, 750 + 60, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 8);

            //SpriteText.drawString(b, Utility.getJojaStock().First().Key.salePrice() + " ", 1150, 750 + 60, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 1);
            //Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(1200 + 25, 750 + 65), new Rectangle(193, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, 0f);

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

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.currentItemIndex > 0)
            {
                this.currentItemIndex--;
                this.setScrollBarToCurrentIndex();
                this.updateSaleButtonNeighbors();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && this.currentItemIndex < Math.Max(0, this.forSale.Count - 12))
            {
                this.currentItemIndex++;
                this.setScrollBarToCurrentIndex();
                this.updateSaleButtonNeighbors();
                Game1.playSound("shiny4");
            }
        }

        private void setScrollBarToCurrentIndex()
        {
            if (forSale.Count > 0)
            {
                this.scrollBar.bounds.Y = this.scrollBarRunner.Y + this.scrollBarRunner.Height / Math.Max(1, this.forSale.Count - 12) * this.currentItemIndex;
                if (this.currentItemIndex == this.forSale.Count - 12)
                {
                    this.scrollBar.bounds.Y = this.scrollBarRunner.Y + this.scrollBarRunner.Height - 35;
                }
            }
        }

        public void updateSaleButtonNeighbors()
        {
            ClickableComponent last_valid_button = this.forSaleButtons[0];
            for (int i = 0; i < this.forSaleButtons.Count; i++)
            {
                ClickableComponent button = this.forSaleButtons[i];
                button.upNeighborImmutable = true;
                button.downNeighborImmutable = true;
                button.upNeighborID = ((i > 0) ? (i + 3546 - 1) : (-7777));
                button.downNeighborID = ((i < 3 && i < this.forSale.Count - 1) ? (i + 3546 + 1) : (-7777));
                if (i >= this.forSale.Count)
                {
                    if (button == base.currentlySnappedComponent)
                    {
                        base.currentlySnappedComponent = last_valid_button;
                        if (Game1.options.SnappyMenus)
                        {
                            this.snapCursorToCurrentSnappedComponent();
                        }
                    }
                }
                else
                {
                    last_valid_button = button;
                }
            }
        }

        public override void snapToDefaultClickableComponent()
        {
            base.currentlySnappedComponent = base.getComponentWithID(3546);
            this.snapCursorToCurrentSnappedComponent();
        }
    }
}