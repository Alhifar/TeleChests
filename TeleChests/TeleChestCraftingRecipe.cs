using StardewValley;
using System;
using System.Collections.Generic;

namespace TeleChests
{
    class TeleChestCraftingRecipe : CraftingRecipe
    {
        private const int WOOD = 388;
        private const int GOLD_BAR = 336;
        private const int VOID_ESSENCE = 769;
        public Dictionary<int, int> recipeList = new Dictionary<int, int>();
        public List<int> itemToProduce = new List<int>();
        public string description;
        public TeleChestCraftingRecipe() : base("TeleChest", false)
        {
            this.name = "TeleChest";
            this.isCookingRecipe = false;
            this.recipeList.Add(WOOD, 100);
            this.recipeList.Add(GOLD_BAR, 5);
            this.recipeList.Add(VOID_ESSENCE, 1);
            this.itemToProduce.Add(190);
            this.numberProducedPerCraft = 1;
            this.bigCraftable = true;
            this.description = "Linked chests";
            this.timesCrafted = (Game1.player.craftingRecipes.ContainsKey(name) ? Game1.player.craftingRecipes[name] : 0);
        }

        public new Item createItem()
        {
            Console.WriteLine("Calling TeleChestCraftingRecipe.createItem()");
            return new TeleChest(true);
        }

    }
}
