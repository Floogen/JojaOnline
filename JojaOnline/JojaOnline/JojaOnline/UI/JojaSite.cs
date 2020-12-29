using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
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
        private readonly int maxUniqueCartItems = 10;
        private readonly Texture2D sourceSheet = JojaResources.GetJojaSiteSpriteSheet();
        private readonly IMonitor monitor = JojaResources.GetMonitor();

        private List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();

        private Rectangle scrollBarRunner;
        private ClickableTextureComponent scrollBar;
        private List<ClickableTextureComponent> clickables = new List<ClickableTextureComponent>();

        private bool scrolling = false;
        private int currentItemIndex = 0;

        // Description related
        private ISalable hoveredItem;
        private int hoverPrice = -1;
        private string hoverText = "";
        private string boldTitleText = "";
        private string descriptionText = "";

        // Random sale item related
        private static int randomSaleItemId = -1;
        private static float randomSalePercentageOff = 0f;
        private ISalable randomSaleItem;
        private ClickableComponent randomSaleButton;

        // Items for sale in shop
        private List<ISalable> forSale = new List<ISalable>();

        // Item: [price, quantity]
        private Dictionary<ISalable, int[]> itemsInCart = new Dictionary<ISalable, int[]>();
        private Dictionary<ISalable, int[]> itemPriceAndStock = new Dictionary<ISalable, int[]>();

        public JojaSite(int uiWidth, int uiHeight) : base(Game1.uiViewport.Width / 2 - (uiWidth + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (uiHeight + IClickableMenu.borderWidth * 2) / 2, uiWidth + IClickableMenu.borderWidth * 2, uiHeight + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
        {
            // Get the items to be sold
            List<ISalable> itemsToSell = GetItemsToSell();
            foreach (ISalable j in itemsToSell)
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

            // Scroll bar
            scrollBar = new ClickableTextureComponent(new Rectangle(1267, 770, 25, 40), sourceSheet, new Rectangle(0, 848, 24, 40), 1f);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, scrollBar.bounds.Y, scrollBar.bounds.Width, 535);

            // Joja Ads
            drawClickable("jojaCanAd", 42, 248, sourceSheet, new Rectangle(608, 0, 208, 208));
            drawClickable("jojaJoinUsAd", 829, 260, sourceSheet, new Rectangle(0, 352, 201, 194));

            // Banner, logos
            drawClickable("jojaLogo", 35, 105, sourceSheet, new Rectangle(0, 0, 336, 128));
            drawClickable("jojaMotto", 435, 150, sourceSheet, new Rectangle(0, 144, 384, 64));
            drawClickable("shoppingCart", 900, 150, sourceSheet, new Rectangle(0, 208, 80, 48));

            // Pick an item for sale
            randomSaleItem = itemsToSell[randomSaleItemId];
            randomSaleButton = new ClickableComponent(new Rectangle(1225, 550, 104, 99), "randomSaleButton");

            // Override default close button position
            this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 50, this.yPositionOnScreen + 70, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
        }

        public static void PickRandomItemForDiscount()
        {
            // Set the random percentage
            randomSalePercentageOff = Game1.random.Next(5, 50) / 100f;

            // Set the item id to be sold at discount
            randomSaleItemId = Game1.random.Next(GetItemsToSell().Count);
        }

        public static List<ISalable> GetItemsToSell()
        {
            return Utility.getJojaStock().Keys.ToList();
        }

        public override void draw(SpriteBatch b)
        {
            // Fade the area
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // Draw the main box, along with the money box
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, speaker: false, drawOnlyBox: true, r: 80, g: 123, b: 186);
            Game1.dayTimeMoneyBox.drawMoneyBox(b);

            // Draw the custom store BG
            b.Draw(JojaResources.GetJojaSiteBackground(), new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 100), new Rectangle(0, 0, 1008, 1194), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);

            // Draw the current unique amount of items in cart
            drawCartQuantity(930, 125, sourceSheet, new Rectangle(0, 272, 30, 45)).draw(b);

            // Draw the general clickables
            foreach (ClickableTextureComponent clickable in clickables)
            {
                clickable.draw(b);
            }

            // Draw the scroll bar
            IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 896, 24, 24), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, scale, drawShadow: false);
            scrollBar.draw(b);

            // Draw the sale button
            IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 768, 60, 60), randomSaleButton.bounds.X, randomSaleButton.bounds.Y, randomSaleButton.bounds.Width, randomSaleButton.bounds.Height, Color.White, scale, drawShadow: false);
            randomSaleItem.drawInMenu(b, new Vector2(1245, 565), scale, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);

            // Draw the individual store buttons
            foreach (ClickableComponent button in forSaleButtons)
            {
                int buttonPosition = forSaleButtons.IndexOf(button) + (currentItemIndex * 2);
                if (buttonPosition >= forSale.Count)
                {
                    continue;
                }

                // Draw the button grid
                IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 768, 60, 60), button.bounds.X, button.bounds.Y, button.bounds.Width, button.bounds.Height, Color.White, scale, drawShadow: false);

                // Get the quantity that is in the cart (if any)
                int currentlyInCart = 0;
                if (itemsInCart.ContainsKey(forSale[buttonPosition]))
                {
                    currentlyInCart = itemsInCart[forSale[buttonPosition]][1];
                }

                // Draw the item for sale
                forSale[buttonPosition].drawInMenu(b, new Vector2(button.bounds.X + 32 - 8, button.bounds.Y + 15), scale, 1f, 0.9f, StackDrawType.Draw, Color.White * (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.25f), drawShadow: true);

                // Draw the quantity in the cart
                SpriteText.drawString(b, $"In Cart: {currentlyInCart}", button.bounds.X + 96 + 8, button.bounds.Y + 35, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 8);

                // Check if item is on sale, if so then add visual marker
                if (forSale[buttonPosition] == randomSaleItem)
                {
                    // Draw sale marker
                    IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 944, 77, 38), button.bounds.Right - 89, button.bounds.Y + 12, 77, 77, Color.White, scale, drawShadow: false);

                    // Draw the (discounted) price
                    string price = ((int) (forSale[buttonPosition].salePrice() - (forSale[buttonPosition].salePrice() * randomSalePercentageOff))) + " ";
                    SpriteText.drawString(b, (randomSalePercentageOff * 100) + "% OFF", button.bounds.Left + 35, button.bounds.Bottom - 55, 999999, -1, 999999, (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.5f), 0.88f, junimoText: false, -1, "", 7);
                    SpriteText.drawString(b, price, button.bounds.Right - SpriteText.getWidthOfString(price) - 30, button.bounds.Bottom - 55, 999999, -1, 999999, (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.5f), 0.88f, junimoText: false, -1, "", 7);
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(button.bounds.Right - 52, button.bounds.Bottom - 50), new Rectangle(193, 373, 9, 10), Color.White * (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.25f), 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, 0f);
                }
                else
                {
                    // Draw the price
                    string price = forSale[buttonPosition].salePrice() + " ";
                    SpriteText.drawString(b, price, button.bounds.Right - SpriteText.getWidthOfString(price) - 30, button.bounds.Bottom - 55, 999999, -1, 999999, (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.5f), 0.88f, junimoText: false, -1, "", 1);
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(button.bounds.Right - 52, button.bounds.Bottom - 50), new Rectangle(193, 373, 9, 10), Color.White * (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.25f), 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, 0f);
                }
            }

            // Draw the tooltip
            if (!this.hoverText.Equals(""))
            {
                if (this.hoveredItem is StardewValley.Object && (bool)(this.hoveredItem as StardewValley.Object).isRecipe)
                {
                    IClickableMenu.drawToolTip(b, " ", this.boldTitleText, this.hoveredItem as Item, false, -1, 0, -1, -1, new CraftingRecipe(this.hoveredItem.Name.Replace(" Recipe", "")), (this.hoverPrice > 0) ? this.hoverPrice : (-1));
                }
                else
                {
                    IClickableMenu.drawToolTip(b, this.hoverText, this.boldTitleText, this.hoveredItem as Item, false, -1, 0, -1, -1, null, (this.hoverPrice > 0) ? this.hoverPrice : (-1));
                }
            }

            this.upperRightCloseButton.draw(b);
            this.drawMouse(b);
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

            // TODO: Check if cart is clicked, if so then open a new submenu containing overview of cost
            // Plus option of 2 day shipping (free) or next day (10% of total cost)
            if (this.scrollBar.containsPoint(x, y))
            {
                this.scrolling = true;
            }
            else if (randomSaleButton.containsPoint(x, y))
            {
                // Move the forSaleButtons until the randomSaleItem is displayed
                for (this.currentItemIndex = 0; this.currentItemIndex < Math.Max(0, this.forSale.Count - 12); currentItemIndex++)
                {
                    bool matchedItem = false;

                    for (int i = 0; i < this.forSaleButtons.Count; i++)
                    {
                        int index = (this.currentItemIndex * 2) + i;

                        if (this.forSale[index] == randomSaleItem)
                        {
                            matchedItem = true;
                            break;
                        }
                    }

                    if (matchedItem)
                    {
                        break;
                    }
                }

                this.setScrollBarToCurrentIndex();
                this.updateSaleButtonNeighbors();
            }
            else
            {
                for (int i = 0; i < this.forSaleButtons.Count; i++)
                {
                    if ((this.currentItemIndex * 2) + i >= this.forSale.Count || !this.forSaleButtons[i].containsPoint(x, y))
                    {
                        continue;
                    }

                    int index = (this.currentItemIndex * 2) + i;
                    if (this.forSale[index] != null)
                    {
                        // Skip if we're at max for the cart size
                        if (itemsInCart.Count >= maxUniqueCartItems && !itemsInCart.ContainsKey(this.forSale[index]))
                        {
                            continue;
                        }

                        // Skip if we're trying to buy more then what we can in a stack via mail
                        if (itemsInCart.ContainsKey(this.forSale[index]) && itemsInCart[this.forSale[index]][1] >= this.forSale[index].maximumStackSize())
                        {
                            continue;
                        }

                        // DEBUG: monitor.Log($"{index} | {this.forSale[index].Name}");
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

                int index = (this.currentItemIndex * 2) + i;
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

        public void drawClickable(string identifier, int x, int y, Texture2D sourceTexture, Rectangle sourceRect)
        {
            Rectangle bounds = new Rectangle((int)((this.xPositionOnScreen + x) * scale), (int)((this.yPositionOnScreen + y) * scale), (int)(sourceRect.Width * scale), (int)(sourceRect.Height * scale));
            ClickableTextureComponent clickable = new ClickableTextureComponent(bounds, sourceTexture, sourceRect, scale)
            {
                name = identifier
            };

            clickables.Add(clickable);
        }

        public ClickableTextureComponent drawCartQuantity(int x, int y, Texture2D sourceTexture, Rectangle sourceRect)
        {
            // Shift sourceRect according to itemsInCart.Count() 
            int currentCount = itemsInCart.Count;
            sourceRect.X = sourceRect.X + (sourceRect.Width * currentCount);

            if (currentCount >= maxUniqueCartItems)
            {
                sourceRect = new Rectangle(0, 320, 53, 19);
                x -= 12;
                y += 23;
            }


            // Create the ClickableTextureComponent object
            Rectangle bounds = new Rectangle((int)((this.xPositionOnScreen + x) * scale), (int)((this.yPositionOnScreen + y) * scale), (int)(sourceRect.Width * scale), (int)(sourceRect.Height * scale));
            ClickableTextureComponent quantityIcon = new ClickableTextureComponent(bounds, sourceTexture, sourceRect, scale)
            {
                name = "shoppingCartQuantity"
            };

            return quantityIcon;
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

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            this.descriptionText = "";
            this.hoverText = "";
            this.hoveredItem = null;
            this.hoverPrice = -1;
            this.boldTitleText = "";

            if (this.scrolling)
            {
                return;
            }

            for (int j = 0; j < this.forSaleButtons.Count; j++)
            {
                if (this.forSaleButtons[j].containsPoint(x, y))
                {
                    ISalable item = this.forSale[j + (currentItemIndex * 2)];
                    this.hoverText = item.getDescription();
                    this.boldTitleText = item.DisplayName;
                    this.hoverPrice = item.salePrice();
                    this.hoveredItem = item;
                }
            }
        }
    }
}
