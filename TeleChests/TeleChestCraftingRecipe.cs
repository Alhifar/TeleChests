using StardewValley;
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.IO;
using StardewModdingAPI;

namespace TeleChests
{
    class TeleChestCraftingRecipe : CraftingRecipe
    {
        private const int WOOD = 388;
        private const int IRIDIUM_BAR = 337;
        private const int VOID_ESSENCE = 769;

        public TeleChestCraftingRecipe() : base("TeleChest", false)
        {
            Dictionary<int, int> recipeList = (Dictionary<int, int>)typeof(CraftingRecipe).GetField("recipeList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            this.name = "TeleChest";
            this.isCookingRecipe = false;
            recipeList.Clear();
            if (TeleChestsMod.Config.useDefaultRecipe)
            {
#if DEBUG
                recipeList.Add(WOOD, 1);
#else
                recipeList.Add(WOOD, 100);
                recipeList.Add(IRIDIUM_BAR, 1);
                recipeList.Add(VOID_ESSENCE, 1);
#endif
            }
            else
            {
                try
                {
                    Dictionary<int, int> recipe = JsonConvert.DeserializeObject<Dictionary<int, int>>(File.ReadAllText(Path.Combine(TeleChestsMod.Mod.PathOnDisk, "customRecipe.json")));
                    foreach (KeyValuePair<int, int> item in recipe)
                    {
                        recipeList.Add(item.Key, item.Value);
                    }
                }
                catch
                {
                    Log.Error("TeleChests: Error loading custom recipe, using default recipe instead");
                    recipeList.Clear();
                    recipeList.Add(WOOD, 100);
                    recipeList.Add(IRIDIUM_BAR, 1);
                    recipeList.Add(VOID_ESSENCE, 1);
                }

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
