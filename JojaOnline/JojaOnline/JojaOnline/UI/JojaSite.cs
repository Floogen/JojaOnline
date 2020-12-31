using JojaOnline.JojaOnline.Mailing;
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

namespace JojaOnline.JojaOnline.UI
{
    public class JojaSite : IClickableMenu
    {
        public readonly float scale = 1f;
        private readonly int nextDayShippingFee = 10;
        private readonly int maxUniqueCartItems = 10;
        private readonly int buttonScrollingOffset = 8;
        private readonly Texture2D sourceSheet = JojaResources.GetJojaSiteSpriteSheet();
        private readonly Texture2D bannerAdSheet = JojaResources.GetJojaAdBanners();
        private readonly IMonitor monitor = JojaResources.GetMonitor();

        private Rectangle scrollBarRunner;
        private ClickableComponent nextDayShippingButton;
        private ClickableComponent twoDayShippingButton;
        private ClickableComponent purchaseButton;
        private List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();

        private ClickableTextureComponent scrollBar;
        private ClickableTextureComponent cancelButton;
        private ClickableTextureComponent checkoutButton;
        private List<ClickableTextureComponent> clickables = new List<ClickableTextureComponent>();

        private bool scrolling = false;
        private bool isCheckingOut = false;
        private bool isNextDayShipping = false;
        private bool canAffordOrder = false;
        private int currentItemIndex = 0;

        // Tick related
        private float numberOfSecondsToDelayInput = 0.5f;
        private int lastTick = Game1.ticks;

        // Description related
        private ISalable hoveredItem;
        private int hoverPrice = -1;
        private string hoverText = "";
        private string boldTitleText = "";

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
            Dictionary<ISalable, int[]> jojaOnlineStock = GetItemsToSell();
            foreach (ISalable j in jojaOnlineStock.Keys)
            {
                if (j is StardewValley.Object && (bool)(j as StardewValley.Object).isRecipe)
                {
                    continue;
                }

                this.forSale.Add(j);
                this.itemPriceAndStock.Add(j, new int[2]
                {
                    // Increase sale price by 25% without membership
                    Game1.MasterPlayer.mailReceived.Contains("JojaMember") ? jojaOnlineStock[j][0] : jojaOnlineStock[j][0] + (jojaOnlineStock[j][0] / 4),
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
                this.forSaleButtons.Add(new ClickableComponent(GetScaledSourceBounds(35 + 16, 695 + 16 + i * ((this.height - 256) / 8), (this.width - 128) / 2, (this.height - 256) / 8 + 4), string.Concat(i))
                {
                    myID = i + 3546,
                    fullyImmutable = true
                });

                this.forSaleButtons.Add(new ClickableComponent(GetScaledSourceBounds(60 + 16 + ((this.width - 128) / 2), 695 + 16 + i * ((this.height - 256) / 8), (this.width - 128) / 2, (this.height - 256) / 8 + 4), string.Concat(i))
                {
                    myID = (7 - i) + 3546,
                    fullyImmutable = true
                });
            }

            // Scroll bar
            scrollBar = new ClickableTextureComponent(GetScaledSourceBounds(527, 715, 25, 40), sourceSheet, new Rectangle(0, 848, 24, 40), scale);
            scrollBarRunner = new Rectangle(scrollBar.bounds.X, scrollBar.bounds.Y, scrollBar.bounds.Width, 535);

            // Joja Ads
            drawClickable("jojaLeftAd", 42, 248, sourceSheet, new Rectangle(608, 0, 208, 208));
            drawClickable("jojaRightAd", 829, 260, sourceSheet, new Rectangle(0, 352, 201, 194));
            drawClickable("jojaBannerAd", 249, 247, bannerAdSheet, new Rectangle(0, (207 * Game1.random.Next(0, 4)), 574, 207));

            // Banner, logos
            drawClickable("jojaLogo", 35, 105, sourceSheet, new Rectangle(0, 0, 336, 128));
            drawClickable("jojaMotto", 435, 150, sourceSheet, new Rectangle(0, 144, 384, 64));

            // Checkout button
            checkoutButton = new ClickableTextureComponent(GetScaledSourceBounds(900, 145, 78, 48), sourceSheet, new Rectangle(0, 208, 78, 48), scale);

            // Cancel button
            cancelButton = new ClickableTextureComponent(GetScaledSourceBounds(45, 110, 32, 32), sourceSheet, new Rectangle(0, 1088, 32, 32), scale);

            // Shipping option buttons
            nextDayShippingButton = new ClickableComponent(GetScaledSourceBounds(79, this.height - 150, (int)Game1.dialogueFont.MeasureString($"Next Day (+{nextDayShippingFee}%)").X + 15, (int)Game1.dialogueFont.MeasureString($"Next Day (+{nextDayShippingFee}%)").Y + 24), "");
            twoDayShippingButton = new ClickableComponent(GetScaledSourceBounds(79, this.height - 235, (int)Game1.dialogueFont.MeasureString($"Two Day (FREE)").X + 24, (int)Game1.dialogueFont.MeasureString($"Two Day (FREE)").Y + 24), "");

            // Purchase button
            purchaseButton = new ClickableComponent(GetScaledSourceBounds(this.width - (int)Game1.dialogueFont.MeasureString($"Purchase Order").X - 125, this.height - 115, (int)Game1.dialogueFont.MeasureString($"Purchase Order").X + 24, (int)Game1.dialogueFont.MeasureString($"Purchase Order").Y + 24), "");

            // Pick an item for sale
            randomSaleItem = forSale[randomSaleItemId];
            randomSaleButton = new ClickableComponent(GetScaledSourceBounds(485, 495, 104, 99), "randomSaleButton");

            // Override default close button position
            this.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(this.xPositionOnScreen + this.width - 50, this.yPositionOnScreen + 70, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
        }

        public static void PickRandomItemForDiscount()
        {
            // Set the random percentage
            randomSalePercentageOff = Game1.random.Next(5, 35) / 100f;

            // Set the item id to be sold at discount
            randomSaleItemId = Game1.random.Next(GetItemsToSell().Count);
        }

        public static Dictionary<ISalable, int[]> GetItemsToSell()
        {
            return JojaResources.GetJojaOnlineStock();
        }

        public Rectangle GetScaledSourceBounds(int x, int y, int width, int height, bool offsetWithParentPosition = true)
        {
            if (offsetWithParentPosition)
            {
                return new Rectangle((int)((this.xPositionOnScreen + x) * scale), (int)((this.yPositionOnScreen + y) * scale), (int)(width * scale), (int)(height * scale));
            }

            return new Rectangle(x, y, width, height);
        }

        public override void draw(SpriteBatch b)
        {
            // Fade the area
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);

            // Draw the main box, along with the money box
            Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, speaker: false, drawOnlyBox: true, r: 80, g: 123, b: 186);
            Game1.dayTimeMoneyBox.drawMoneyBox(b);

            // See if we're checking out
            if (isCheckingOut)
            {
                b.Draw(JojaResources.GetJojaCheckoutBackground(), new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 100), new Rectangle(0, 0, 1008, 1194), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);

                // Draw cancel button
                cancelButton.draw(b);

                // Draw item icon, name and quantity
                int subTotal = 0;
                int stackCount = 0;
                int uniqueItemCount = 1;
                foreach (ISalable item in itemsInCart.Keys)
                {
                    // Draw item
                    item.Stack = itemsInCart[item][1];
                    item.drawInMenu(b, new Vector2(this.xPositionOnScreen + 80, (this.yPositionOnScreen + 180) + (75 * uniqueItemCount)), scale, 1f, 0.9f, StackDrawType.Hide, Color.White, drawShadow: false);

                    // Draw name / quantity and price
                    SpriteText.drawString(b, $"{item.DisplayName} x{item.Stack}", this.xPositionOnScreen + 160, (this.yPositionOnScreen + 190) + (75 * uniqueItemCount), 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);

                    int stackCost = item.Stack * itemsInCart[item][0];
                    SpriteText.drawString(b, stackCost + "g", (this.xPositionOnScreen + this.width) - SpriteText.getWidthOfString(stackCost + "g") - 100, (this.yPositionOnScreen + 190) + (75 * uniqueItemCount), 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);

                    stackCount += item.Stack;
                    subTotal += stackCost;
                    uniqueItemCount++;
                }

                // Draw the subtotal
                SpriteText.drawString(b, $"Subtotal: {subTotal}g", (this.xPositionOnScreen + this.width) - SpriteText.getWidthOfString($"Subtotal: {subTotal}g") - 100, this.yPositionOnScreen + this.height - 290, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);

                // Draw the shipping options (free 2 day or next day with fee)
                SpriteText.drawString(b, $"Shipping Options", this.xPositionOnScreen + 85, this.yPositionOnScreen + this.height - 290, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.twoDayShippingButton.bounds.X, this.twoDayShippingButton.bounds.Y, this.twoDayShippingButton.bounds.Width, this.twoDayShippingButton.bounds.Height, isNextDayShipping ? Color.White : Color.Gray, 4f * this.twoDayShippingButton.scale);
                Utility.drawTextWithShadow(b, "Two Day (Free)", Game1.dialogueFont, new Vector2(this.twoDayShippingButton.bounds.X + 12, this.twoDayShippingButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor * (isNextDayShipping ? 1f : 0.25f));

                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.nextDayShippingButton.bounds.X, this.nextDayShippingButton.bounds.Y, this.nextDayShippingButton.bounds.Width, this.nextDayShippingButton.bounds.Height, isNextDayShipping ? Color.Gray : Color.White, 4f * this.nextDayShippingButton.scale);
                Utility.drawTextWithShadow(b, $"Next Day (+{nextDayShippingFee}%)", Game1.dialogueFont, new Vector2(this.nextDayShippingButton.bounds.X + 12, this.nextDayShippingButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor * (isNextDayShipping ? 0.25f : 1f));

                // Draw the shipping costs
                int shippingCosts = isNextDayShipping ? subTotal / nextDayShippingFee : 0;
                SpriteText.drawString(b, $"Shipping:      ", (this.xPositionOnScreen + this.width) - SpriteText.getWidthOfString($"Shipping:      ") - 100, this.yPositionOnScreen + this.height - 230, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);
                SpriteText.drawString(b, $"{shippingCosts}g", (this.xPositionOnScreen + this.width) - SpriteText.getWidthOfString($"{shippingCosts}g") - 100, this.yPositionOnScreen + this.height - 230, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);

                // Draw the total
                int total = subTotal + shippingCosts;
                SpriteText.drawString(b, $"Total:      ", (this.xPositionOnScreen + this.width) - SpriteText.getWidthOfString($"Total:      ") - 100, this.yPositionOnScreen + this.height - 170, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);
                SpriteText.drawString(b, $"{total}g", (this.xPositionOnScreen + this.width) - SpriteText.getWidthOfString($"{total}g") - 100, this.yPositionOnScreen + this.height - 170, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);

                // Draw the purchase button
                canAffordOrder = total <= Game1.player.Money ? true : false;
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.purchaseButton.bounds.X, this.purchaseButton.bounds.Y, this.purchaseButton.bounds.Width, this.purchaseButton.bounds.Height, canAffordOrder ? Color.White : Color.Gray, 4f * this.purchaseButton.scale);
                Utility.drawTextWithShadow(b, $"Purchase Order", Game1.dialogueFont, new Vector2(this.purchaseButton.bounds.X + 12, this.purchaseButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor * (canAffordOrder ? 1f : 0.25f));


                // Draw the mouse
                this.drawMouse(b);

                return;
            }

            // Draw the custom store BG
            b.Draw(JojaResources.GetJojaSiteBackground(), new Vector2(this.xPositionOnScreen + 32, this.yPositionOnScreen + 100), new Rectangle(0, 0, 1008, 1194), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);

            // Draw the current unique amount of items in cart
            drawCartQuantity(930, 125, sourceSheet, new Rectangle(0, 272, 30, 45)).draw(b);

            // Draw the usage instructions
            //IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(144, 576, 76, 64), this.xPositionOnScreen + 100, this.yPositionOnScreen + 600, 76, 64, Color.White, scale, drawShadow: false);
            //SpriteText.drawString(b, "to order", this.xPositionOnScreen + 100, this.yPositionOnScreen + 600, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);
            //SpriteText.drawString(b, "to remove", (this.xPositionOnScreen + this.width) - SpriteText.getWidthOfString("Right Click to remove"), this.yPositionOnScreen + 600, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", 4);

            // Draw the general clickables
            foreach (ClickableTextureComponent clickable in clickables)
            {
                clickable.draw(b);
            }

            // Draw the checkout button
            checkoutButton.draw(b, itemsInCart.Count > 0 ? Color.White : Color.Black, 0.99f);

            // Draw the scroll bar
            IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 896, 24, 24), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, scale, drawShadow: false);
            scrollBar.draw(b);

            // Draw the sale button
            IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 768, 60, 60), randomSaleButton.bounds.X, randomSaleButton.bounds.Y, randomSaleButton.bounds.Width, randomSaleButton.bounds.Height, Color.White, scale, drawShadow: false);
            randomSaleItem.drawInMenu(b, new Vector2(randomSaleButton.bounds.X + 20, randomSaleButton.bounds.Y + 15), scale, 1f, 0.9f, StackDrawType.Hide, Color.White, drawShadow: true);

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
                forSale[buttonPosition].drawInMenu(b, new Vector2(button.bounds.X + 32 - 8, button.bounds.Y + 15), scale, 1f, 0.9f, StackDrawType.Hide, Color.White * (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.25f), drawShadow: true);

                // Draw the quantity in the cart
                SpriteText.drawString(b, $"In Cart: {currentlyInCart}", button.bounds.X + 96 + 8, button.bounds.Y + 35, 999999, -1, 999999, (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.5f), 0.88f, junimoText: false, -1, "", 8);

                // Check if item is on sale, if so then add visual marker
                if (forSale[buttonPosition] == randomSaleItem)
                {
                    // Draw sale marker
                    IClickableMenu.drawTextureBox(b, sourceSheet, new Rectangle(0, 944, 77, 38), button.bounds.Right - 89, button.bounds.Y + 12, 77, 77, Color.White, scale, drawShadow: false);

                    // Draw the (discounted) price
                    string price = ((int)(itemPriceAndStock[forSale[buttonPosition]][0] - (itemPriceAndStock[forSale[buttonPosition]][0] * randomSalePercentageOff))) + " ";
                    SpriteText.drawString(b, (randomSalePercentageOff * 100) + "% OFF", button.bounds.Left + 35, button.bounds.Bottom - 55, 999999, -1, 999999, (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.5f), 0.88f, junimoText: false, -1, "", 7);
                    SpriteText.drawString(b, price, button.bounds.Right - SpriteText.getWidthOfString(price) - 30, button.bounds.Bottom - 55, 999999, -1, 999999, (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.5f), 0.88f, junimoText: false, -1, "", 7);
                    Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(button.bounds.Right - 52, button.bounds.Bottom - 50), new Rectangle(193, 373, 9, 10), Color.White * (itemsInCart.Count < maxUniqueCartItems || currentlyInCart > 0 ? 1f : 0.25f), 0f, Vector2.Zero, 4f, flipped: false, 1f, -1, -1, 0f);
                }
                else
                {
                    // Draw the price
                    string price = itemPriceAndStock[forSale[buttonPosition]][0] + " ";
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
            lastTick = Game1.ticks;

            if (Game1.activeClickableMenu == null)
            {
                return;
            }

            if (isCheckingOut)
            {
                if (cancelButton.containsPoint(x, y))
                {
                    isCheckingOut = false;
                    Game1.playSound("cancel");
                }
                else if (nextDayShippingButton.containsPoint(x, y))
                {
                    isNextDayShipping = true;
                    Game1.playSound("select");
                }
                else if (twoDayShippingButton.containsPoint(x, y))
                {
                    isNextDayShipping = false;
                    Game1.playSound("select");
                }
                else if (purchaseButton.containsPoint(x, y))
                {
                    if (canAffordOrder)
                    {
                        // Close this menu
                        base.exitThisMenu();

                        // Create mail order
                        if (JojaMail.CreateMailOrder(Game1.player, isNextDayShipping ? 1 : 2, itemsInCart.Keys.Select(i => i as Item).ToList()))
                        {
                            this.monitor.Log("Order placed via JojaMail!", LogLevel.Debug);

                            // Display order success dialog
                            if (isNextDayShipping)
                            {
                                Game1.player.Money = Game1.player.Money - (itemsInCart.Keys.Sum(i => i.Stack * itemsInCart[i][0]) + (itemsInCart.Keys.Sum(i => i.Stack * itemsInCart[i][0]) / nextDayShippingFee));
                                Game1.activeClickableMenu = new DialogueBox("Your order has been placed! ^It will arrive tomorrow.");
                            }
                            else
                            {
                                Game1.player.Money = Game1.player.Money - itemsInCart.Keys.Sum(i => i.Stack * itemsInCart[i][0]);
                                Game1.activeClickableMenu = new DialogueBox("Your order has been placed! ^It will arrive in 2 days.");
                            }

                            Game1.playSound("moneyDial");
                            Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
                        }
                        else
                        {
                            this.monitor.Log("Issue ordering items, failed to dispatch JojaMail!", LogLevel.Error);

                            // Display order error dialog
                            Game1.activeClickableMenu = new DialogueBox($"Order failed to place! Please try again later.");
                        }
                    }
                    else
                    {
                        // Shake money bag
                        Game1.dayTimeMoneyBox.moneyShakeTimer = 2000;
                        Game1.playSound("cancel");
                    }
                }

                return;
            }

            if (this.scrollBar.containsPoint(x, y))
            {
                this.scrolling = true;
            }
            else if (randomSaleButton.containsPoint(x, y))
            {
                // Move the forSaleButtons until the randomSaleItem is displayed
                for (this.currentItemIndex = 0; this.currentItemIndex < Math.Max(0, this.forSale.Count - buttonScrollingOffset); currentItemIndex++)
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
            else if (checkoutButton.containsPoint(x, y) && itemsInCart.Count > 0)
            {
                this.monitor.Log("Starting checkout...");
                isCheckingOut = true;
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

                        // DEBUG: monitor.Log($"{index} | {this.forSale[index].Name}");
                        int toBuy = (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : 5;
                        toBuy = Math.Min(toBuy, this.forSale[index].maximumStackSize());

                        // Skip if we're trying to buy more then what we can in a stack via mail
                        if (itemsInCart.ContainsKey(this.forSale[index]) && (itemsInCart[this.forSale[index]][1] >= this.forSale[index].maximumStackSize() || itemsInCart[this.forSale[index]][1] + toBuy > this.forSale[index].maximumStackSize()))
                        {
                            continue;
                        }

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
                int correctedForSaleCount = (this.forSale.Count % 2 == 0 ? this.forSale.Count : this.forSale.Count + 1);
                this.currentItemIndex = Math.Min((correctedForSaleCount - buttonScrollingOffset) / 2, Math.Max(0, (int)(((float)correctedForSaleCount / 2) * percentage)));
                this.updateSaleButtonNeighbors();
            }
            else if (Game1.ticks >= lastTick + (60 * numberOfSecondsToDelayInput))
            {
                for (int i = 0; i < this.forSaleButtons.Count; i++)
                {
                    if (this.currentItemIndex + i >= this.forSale.Count || !this.forSaleButtons[i].containsPoint(x, y))
                    {
                        continue;
                    }

                    int index = (this.currentItemIndex * 2) + i;
                    if (itemsInCart.Count >= maxUniqueCartItems && !itemsInCart.ContainsKey(this.forSale[index]))
                    {
                        continue;
                    }

                    if (this.forSale[index] != null)
                    {
                        // Skip if we're at max for the cart size
                        if (itemsInCart.Count >= maxUniqueCartItems && !itemsInCart.ContainsKey(this.forSale[index]))
                        {
                            continue;
                        }

                        // DEBUG: monitor.Log($"{index} | {this.forSale[index].Name}");
                        int toBuy = (!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : 5;
                        toBuy = Math.Min(toBuy, this.forSale[index].maximumStackSize());

                        // Skip if we're trying to buy more then what we can in a stack via mail
                        if (itemsInCart.ContainsKey(this.forSale[index]) && (itemsInCart[this.forSale[index]][1] >= this.forSale[index].maximumStackSize() || itemsInCart[this.forSale[index]][1] + toBuy > this.forSale[index].maximumStackSize()))
                        {
                            continue;
                        }

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
                y += 18;
            }


            // Create the ClickableTextureComponent object
            Rectangle bounds = GetScaledSourceBounds(x, y, sourceRect.Width, sourceRect.Height);
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
            else if (direction < 0 && this.currentItemIndex * 2 < Math.Max(0, this.forSale.Count - buttonScrollingOffset))
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
                this.scrollBar.bounds.Y = this.scrollBarRunner.Y + this.scrollBarRunner.Height / Math.Max(1, this.forSale.Count - buttonScrollingOffset + 1) * (this.currentItemIndex * 2);
                if (this.currentItemIndex * 2 == this.forSale.Count - buttonScrollingOffset + (this.forSale.Count % 2 == 0 ? 0 : 1))
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
            this.hoverText = "";
            this.hoveredItem = null;
            this.boldTitleText = "";

            if (this.scrolling)
            {
                return;
            }

            for (int j = 0; j < this.forSaleButtons.Count; j++)
            {
                if (this.forSaleButtons[j].containsPoint(x, y))
                {
                    if (j + (currentItemIndex * 2) >= forSale.Count)
                    {
                        return;
                    }

                    ISalable item = this.forSale[j + (currentItemIndex * 2)];
                    this.hoverText = item.getDescription();
                    this.boldTitleText = item.DisplayName;
                    this.hoveredItem = item;
                }
            }
        }
    }
}
