using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleChests
{
    public class TeleChestConfig : Config
    {
        public bool useDefaultRecipe { get; set; }
        public bool allowMultipleInventories { get; set; }

        public override T GenerateDefaultConfig<T>()
        {
            useDefaultRecipe = true;
            allowMultipleInventories = true;
            return this as T;
        }
    }
}
