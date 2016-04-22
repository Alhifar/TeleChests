using StardewValley.Objects;
using System;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using StardewValley.Locations;
using Microsoft.Xna.Framework.Input;

namespace TeleChests
{
    public class TeleChest : Chest
    {
        public GameLocation location;
        public int key;
        private const int CHEST_INDEX = 1130;

        public TeleChest()
        {
            setInitialValues();
        }
        public TeleChest(bool playerChest) : base(playerChest)
        {
            setInitialValues();
        }
        public TeleChest(Vector2 location) : base(location)
        {
            setInitialValues();
        }
        public TeleChest(string type, Vector2 location) : base(type, location)
        {
            setInitialValues();
        }
        public TeleChest(int coins, List<Item> items, Vector2 location, bool giftBox = false) : base(coins, items, location, giftBox)
        {
            setInitialValues();
        }

        private void setInitialValues()
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
            Vector2 vector = new Vector2((x / Game1.tileSize), (y / Game1.tileSize));
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
                this.location.debris.Add(new Debris(this, this.tileLocation * new Vector2(Game1.tileSize, Game1.tileSize), new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y)));
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
        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (justCheckingForActivity)
            {
                return true;
            }
            items = TeleChestsMod.SharedInventory[this.key];
            bool check = base.checkForAction(who, justCheckingForActivity);
            TeleChestsMod.SharedInventory[this.key] = items;
            return check;
        }
        public override bool performObjectDropInAction(StardewValley.Object dropIn, bool probe, Farmer who)
        {
            if (TeleChestsMod.Config.allowMultipleInventories && !probe && dropIn != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
            {
                this.key = dropIn.parentSheetIndex + (this.bigCraftable ? 1000 : 0);
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
                Game1.playSound("throwDownITem");
                items = TeleChestsMod.SharedInventory[this.key];
            }
            return false;
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
        public new void itemTakenCallback(Item item, Farmer who)
        {
            base.itemTakenCallback(item, who);
            TeleChestsMod.SharedInventory[this.key] = items;
        }
    }
}
