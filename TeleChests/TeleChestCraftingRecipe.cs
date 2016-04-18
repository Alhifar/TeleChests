using StardewValley;
using System.Collections.Generic;
using System.Reflection;

namespace TeleChests
{
    class TeleChestCraftingRecipe : CraftingRecipe
    {
        public TeleChestCraftingRecipe() : base("TeleChest", false)
        {
            this.name = "TeleChest";
            this.isCookingRecipe = false;
            Dictionary<int, int> recipeList = (Dictionary<int, int>)typeof(CraftingRecipe).GetField("recipeList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            recipeList.Clear();
            foreach (KeyValuePair<int, int> item in TeleChestsMod.Recipe)
            {
                recipeList.Add(item.Key, item.Value);
            }
            
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
