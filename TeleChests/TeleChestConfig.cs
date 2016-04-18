using StardewModdingAPI;

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
