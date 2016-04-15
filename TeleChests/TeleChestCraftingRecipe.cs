using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TeleChests
{
    class TeleChestCraftingRecipe : CraftingRecipe
    {
        private const int WOOD = 388;
        private const int GOLD_BAR = 336;
        private const int VOID_ESSENCE = 769;

        public TeleChestCraftingRecipe() : base("TeleChest", false)
        {
            Dictionary<int, int> recipeList = (Dictionary<int, int>)typeof(CraftingRecipe).GetField("recipeList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            this.name = "TeleChest";
            this.isCookingRecipe = false;
            recipeList.Clear();
#if DEBUG
            recipeList.Add(WOOD, 1);
#else
            recipeList.Add(WOOD, 100);
            recipeList.Add(GOLD_BAR, 2);
            recipeList.Add(VOID_ESSENCE, 1);
#endif
            List<int> itemToProduce = (List<int>)typeof(CraftingRecipe).GetField("itemToProduce", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            itemToProduce.Clear();
            itemToProduce.Add(130);
            this.numberProducedPerCraft = 1;
            this.bigCraftable = true;
            typeof(CraftingRecipe).GetField("description", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, "These chests seem to all be linked together!");
            this.timesCrafted = (Game1.player.craftingRecipes.ContainsKey(name) ? Game1.player.craftingRecipes[name] : 0);

        }
    }
}
