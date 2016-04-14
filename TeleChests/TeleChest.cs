using StardewValley.Objects;
using System;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using StardewValley.Locations;
using Microsoft.Xna.Framework.Input;

namespace TeleChests
{
    [Serializable]
    public class TeleChest : Chest
    {
        private GameLocation location;
        private int key;
        private const int CHEST_INDEX = 130;

        public TeleChest() : base()
        {
            this.Name = "TeleChest";
            this.key = CHEST_INDEX;
            if (!TeleChestsMod.SharedInventory.ContainsKey(this.key))
            {
                TeleChestsMod.SharedInventory.Add(this.key, new List<Item>());
            }

        }
        public TeleChest(bool playerChest) : base(playerChest)
        {
            this.Name = "TeleChest";
            this.key = CHEST_INDEX;
            if (!TeleChestsMod.SharedInventory.ContainsKey(this.key))
            {
                TeleChestsMod.SharedInventory.Add(this.key, new List<Item>());
            }
        }
        public TeleChest(Vector2 location) : base(location)
        {
            this.Name = "TeleChest";
            this.key = CHEST_INDEX;
            if (!TeleChestsMod.SharedInventory.ContainsKey(this.key))
            {
                TeleChestsMod.SharedInventory.Add(this.key, new List<Item>());
            }
        }
        public TeleChest(string type, Vector2 location) : base(type, location)
        {
            this.Name = "TeleChest";
            this.key = CHEST_INDEX;
            if (!TeleChestsMod.SharedInventory.ContainsKey(this.key))
            {
                TeleChestsMod.SharedInventory.Add(this.key, new List<Item>());
            }
        }
        public TeleChest(int coins, List<Item> items, Vector2 location, bool giftBox = false) : base(coins, items, location, giftBox)
        {
            this.Name = "TeleChest";
            this.key = CHEST_INDEX;
            if (!TeleChestsMod.SharedInventory.ContainsKey(this.key))
            {
                TeleChestsMod.SharedInventory.Add(this.key, new List<Item>());
            }
        }
        public override string getDescription()
        {
            return "These chests seem to all be linked together!";
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            this.health = 10;
            Vector2 vector = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            if (location.objects.ContainsKey(vector) || Game1.currentLocation is MineShaft)
            {
                Game1.showRedMessage("Unsuitable Location");
                return false;
            }
            this.tileLocation = vector;
            location.objects.Add(this.tileLocation, this);
            this.location = location;
            Game1.playSound("axe");
            return true;
        }
        public override bool performToolAction(Tool t)
        {
            try
            {
                if (this.location == null)
                {
                    return false;
                }
                this.clearNulls();
                Game1.playSound("hammer");
                this.location.debris.Add(new Debris(this, this.tileLocation * new Vector2(Game1.tileSize, Game1.tileSize), new Vector2((float)Game1.player.GetBoundingBox().Center.X, (float)Game1.player.GetBoundingBox().Center.Y)));
                this.location.objects[this.tileLocation].performRemoveAction(this.tileLocation, Game1.player.currentLocation);
                this.location.objects.Remove(this.tileLocation);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        public override int salePrice()
        {
            return 1000;
        }

        public new Item addItem(Item item)
        {
            Item i = base.addItem(item);
            TeleChestsMod.SharedInventory[this.key] = items;
            return i;
        }
        public new void grabItemFromChest(Item item, Farmer who)
        {
            base.grabItemFromChest(item, who);
            TeleChestsMod.SharedInventory[this.key] = items;
        }
        public new void grabItemFromInventory(Item item, Farmer who)
        {
            base.grabItemFromInventory(item, who);
            TeleChestsMod.SharedInventory[this.key] = items;
        }
        public new void clearNulls()
        {
            base.clearNulls();
            TeleChestsMod.SharedInventory[this.key] = items;
        }
        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            items = TeleChestsMod.SharedInventory[this.key];
            if (justCheckingForActivity)
            {
                return true;
            }
            if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
            {
                performObjectDropInAction(Game1.player.ActiveObject, false, Game1.player);
                return true;
            }
            bool check = base.checkForAction(who, justCheckingForActivity);
            TeleChestsMod.SharedInventory[this.key] = items;
            return check;
        }
        public new void itemTakenCallback(Item item, Farmer who)
        {
            base.itemTakenCallback(item, who);
            TeleChestsMod.SharedInventory[this.key] = items;
        }
        public override bool performObjectDropInAction(StardewValley.Object dropIn, bool probe, Farmer who)
        {
            if (!probe && dropIn != null)
            {
                this.key = dropIn.parentSheetIndex;
                if (this.key == CHEST_INDEX)
                {
                    this.name = "Telechest";
                }
                else
                {
                    this.name = $"TeleChest - {dropIn.name}";
                }
                if (!TeleChestsMod.SharedInventory.ContainsKey(this.key))
                {
                    TeleChestsMod.SharedInventory.Add(this.key, new List<Item>());
                }
                this.currentLidFrame = 135;
                this.frameCounter = 2;
                Game1.playSound("dropItemInWater");
            }
            return false;
        }
    }
}
