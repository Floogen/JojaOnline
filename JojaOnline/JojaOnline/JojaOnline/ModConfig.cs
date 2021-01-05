using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JojaOnline.JojaOnline
{
    public class ModConfig
    {
        public bool areAllSeedsAvailableBeforeYearOne { get; set; }
        public bool giveJojaMemberDiscount { get; set; }
        public int minSalePercentage { get; set; }
        public int maxSalePercentage { get; set; }

        public Dictionary<string, int> itemNameToPriceOverrides { get; set; }

        public ModConfig()
        {
            this.areAllSeedsAvailableBeforeYearOne = false;
            this.giveJojaMemberDiscount = false;

            this.minSalePercentage = 5;
            this.maxSalePercentage = 35;

            this.itemNameToPriceOverrides = new Dictionary<string, int>();
        }
    }
}
