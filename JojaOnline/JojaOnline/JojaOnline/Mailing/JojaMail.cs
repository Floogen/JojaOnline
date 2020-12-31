﻿using MailFrameworkMod;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JojaOnline.JojaOnline.Mailing
{
    public static class JojaMail
    {
        private static IMonitor monitor = JojaResources.GetMonitor();

        public static bool CreateMailOrder(Farmer recipient, int daysToWait, List<Item> packagedItems)
        {
            try
            {
                // Determine order number
                int orderNumber = 0;
                while (MailDao.FindLetter($"JojaMailOrder[#{orderNumber}]") != null || recipient.mailReceived.Contains($"JojaMailOrder[#{orderNumber}]") || IsOrderNumberScheduled(recipient, orderNumber))
                {
                    orderNumber++;
                }
                string mailOrderID = $"JojaMailOrder[#{orderNumber}]";

                // Generate mail message
                string message = $"Valued Customer,^^Thank you for using Joja Online. Your items for order #{orderNumber:0000} are packaged below.^^We look forward to your continued business.^^- Joja Co.";

                if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
                {
                    message = $"Valued Member,^^Thank you for using Joja Online. Your items for order #{orderNumber:0000} are packaged below.^^We look forward to your continued business.^^- Joja Co.";
                }

                // Determine the deliveryDate
                int deliveryDate = daysToWait + Game1.dayOfMonth > 28 ? daysToWait : daysToWait + Game1.dayOfMonth;

                // Need to save this mail data if it can't be delivered before shutdown
                recipient.mailForTomorrow.Add($"{mailOrderID}[{message}][{deliveryDate}][{String.Join(", ", packagedItems.Select(i => $"[{i.Name}, {i.getCategoryName()}, {i.parentSheetIndex}, {i.Stack}]"))}]");

                monitor.Log($"JojaMail order [#{orderNumber}] created with delivery date of [{deliveryDate}] {String.Join(", ", packagedItems.Select(i => $"[{i.Name}, {i.getCategoryName()}, {i.parentSheetIndex}, {i.Stack}]"))}!", LogLevel.Debug);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        private static bool IsOrderNumberScheduled(Farmer recipient, int orderNumber)
        {
            return recipient.mailForTomorrow.Any(m => m.Contains($"JojaMailOrder[#{orderNumber}]"));
        }

        public static void ProcessPlayerMailbox()
        {

            // Get the mail coming in today, if it is a JojaOnline[DATE]#[ORDER_NUMBER] and [DATE] doesn't match today's [DATE], then addMailForTomorrow
            Regex mailRegex = new Regex(@"(?<orderID>JojaMailOrder\[#\d\d?\d?\d?\])\[(?<message>.*)\]\[(?<deliveryDate>\d\d?)\]\[(?<items>.*)\]", RegexOptions.IgnoreCase);
            Regex itemStockRegex = new Regex(@"(?<idToStock>[a-zA-Z0-9_ .]*, [a-zA-Z0-9_ .]*, \d+, \d+)", RegexOptions.IgnoreCase);

            List<string> jojaMailInMailbox = Game1.player.mailbox.Where(m => mailRegex.IsMatch(m)).ToList();
            foreach (string placeholder in jojaMailInMailbox)
            {
                Match jojaMatch = mailRegex.Match(placeholder);

                // Get orderID
                string orderID = jojaMatch.Groups["orderID"].ToString();

                // Validate the deliveryDate field
                int deliveryDate = -1;
                if (!Int32.TryParse(jojaMatch.Groups["deliveryDate"].ToString(), out deliveryDate))
                {
                    continue;
                }

                // See if the deliveryDate is today, otherwise push the delivery back
                if (deliveryDate != Game1.dayOfMonth)
                {
                    monitor.Log($"Delaying {orderID} due to the delivery date [{deliveryDate}] not matching to today's date [{Game1.dayOfMonth}]", LogLevel.Debug);

                    Game1.player.mailbox.Remove(placeholder);
                    Game1.player.mailForTomorrow.Add(placeholder);

                    continue;
                }

                // Send mail via MFM
                List<Item> itemsToPackage = new List<Item>();
                foreach (Match itemMatch in itemStockRegex.Matches(jojaMatch.Groups["items"].ToString()))
                {
                    int itemID = -1;
                    int stockCount = -1;
                    if (!Int32.TryParse(itemMatch.Value.Split(',')[2], out itemID) || !Int32.TryParse(itemMatch.Value.Split(',')[3], out stockCount))
                    {
                        continue;
                    }

                    string itemName = itemMatch.Value.Split(',')[0].Trim();
                    string itemCategory = itemMatch.Value.Split(',')[1].Trim();
                    if (itemName.Equals("Wallpaper"))
                    {
                        itemsToPackage.Add(new Wallpaper(itemID, false) { Stack = stockCount });
                    }
                    else if (itemName.Equals("Flooring"))
                    {
                        itemsToPackage.Add(new Wallpaper(itemID, true) { Stack = stockCount });
                    }
                    else if (itemCategory.Equals("Furniture"))
                    {
                        itemsToPackage.Add(new Furniture(itemID, Vector2.Zero) { Stack = stockCount });
                    }
                    else
                    {
                        itemsToPackage.Add(new StardewValley.Object(itemID, stockCount));
                    }
                }

                // Remove the placeholder mail from the mailbox, as we want MFM to handle it
                Game1.player.mailbox.Remove(placeholder);

                SendMail(Game1.player, orderID, jojaMatch.Groups["message"].ToString(), itemsToPackage);
            }
        }

        public static void SendMail(Farmer recipient, string orderID, string message, List<Item> packagedItems)
        {
            monitor.Log($"Sending out mail order {orderID}", LogLevel.Debug);
            Letter letter = new Letter(orderID, message, packagedItems, l => !recipient.mailReceived.Contains(l.Id), l => recipient.mailReceived.Add(l.Id))
            {
                LetterTexture = JojaResources.GetJojaMailBackground(),
                TextColor = 7
            };

            MailDao.SaveLetter(letter);
        }
    }
}
