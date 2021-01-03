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
        public int minSalePercentage { get; set; }
        public int maxSalePercentage { get; set; }

        public ModConfig()
        {
            this.areAllSeedsAvailableBeforeYearOne = false;
            this.minSalePercentage = 5;
            this.maxSalePercentage = 35;
        }
    }
}
