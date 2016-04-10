using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeleChests
{
    class TeleChestCraftingRecipe : CraftingRecipe
    {
        public TeleChestCraftingRecipe(string name, bool isCookingRecipe) : base(name, isCookingRecipe)
        {
        }

        public new Item createItem()
        {
            return new TeleChest(true);
        }

    }
}
