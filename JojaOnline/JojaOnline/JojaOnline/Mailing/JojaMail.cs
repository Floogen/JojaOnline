using MailFrameworkMod;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JojaOnline.JojaOnline.Mailing
{
    public class JojaMail
    {
        private string message;
        private string mailOrderID;
        private List<Item> packagedItems;

        private int deliveryDate;
        private Farmer recipient;

        public JojaMail(Farmer who, int daysToWait, List<Item> items)
        {
            recipient = who;
            packagedItems = items;
            
            // Determine order number
            int orderNumber = 0;
            while (MailDao.FindLetter($"JojaMailOrder#{orderNumber}") != null || Game1.player.mailReceived.Contains($"JojaMailOrder#{orderNumber}"))
            {
                orderNumber++;
            }
            mailOrderID = $"JojaMailOrder#{orderNumber}";

            // Generate mail message
            message = $"Valued Customer,^^Thank you for using Joja Online. Your items for order #{orderNumber:0000} are packaged below.^^We look forward to your continued business.^^- Joja Co.";

            if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
            {
                message = $"Valued Member,^^Thank you for using Joja Online. Your items for order #{orderNumber:0000} are packaged below.^^We look forward to your continued business.^^- Joja Co.";
            }

            // Determine the deliveryDate
            deliveryDate = daysToWait + Game1.dayOfMonth > 28 ? daysToWait: daysToWait + Game1.dayOfMonth;
        }

        public bool SendMail()
        {
            try
            {
                Letter letter = new Letter(mailOrderID, message, packagedItems, l => !recipient.mailReceived.Contains(l.Id) && Game1.dayOfMonth == deliveryDate, l => recipient.mailReceived.Add(l.Id))
                {
                    LetterTexture = JojaResources.GetJojaMailBackground(),
                    TextColor = 7
                };

                MailDao.SaveLetter(letter);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
