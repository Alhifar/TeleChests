﻿using StardewValley.Objects;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using StardewValley.Locations;
using StardewValley.Tools;

namespace TeleChests
{
    [Serializable]
    public class TeleChest : Chest
    {
        private GameLocation teleChestLocation;
        private GameLocation TeleChestLocation
        {
            get
            {
                Console.WriteLine("Returning {0} for TeleChestLocation", teleChestLocation);
                return teleChestLocation;
            }
            set
            {
                teleChestLocation = value;
                Console.WriteLine("Setting TeleChestLocation to {0}", value);
            }
        }
        public TeleChest() : base()
        {
            this.name = "TeleChest";
        }
        public TeleChest(bool playerChest) : base(playerChest)
        {
            this.Name = "TeleChest";
        }
        public TeleChest(Vector2 location) : base(location)
        {
            this.name = "TeleChest";
        }
        public TeleChest(string type, Vector2 location) : base(type, location)
        {
            this.name = "TeleChest";
        }
        public TeleChest(int coins, List<Item> items, Vector2 location, bool giftBox = false) : base(coins, items, location, giftBox)
        {
            this.name = "TeleChest";
        }

        public override string getDescription()
        {
            return "These chests seem to all be linked together!";
        }

        public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
        {
            Console.WriteLine(location);
            this.health = 10;
            Vector2 vector = new Vector2((float)(x / Game1.tileSize), (float)(y / Game1.tileSize));
            if (location.objects.ContainsKey(vector) || Game1.currentLocation is MineShaft)
            {
                Game1.showRedMessage("Unsuitable Location");
                return false;
            }
            location.objects.Add(vector, new TeleChest(true));
            this.TeleChestLocation = location;
            Console.WriteLine(this.TeleChestLocation);
            Game1.playSound("axe");
            return true;
        }
        public override bool performToolAction(Tool t)
        {
            try
            {
                if (this.TeleChestLocation == null)
                {
                    return false;
                }
                this.clearNulls();
                Vector2 vector2 = this.tileLocation;
                //vector2.X = (float)Math.Floor(vector2.X / Game1.tileSize);
                //vector2.Y = (float)Math.Floor(vector2.Y / Game1.tileSize);
                Game1.playSound("hammer");
                this.TeleChestLocation.debris.Add(new Debris(this, Game1.player.GetToolLocation(false), new Vector2((float)Game1.player.GetBoundingBox().Center.X, (float)Game1.player.GetBoundingBox().Center.Y)));
                this.TeleChestLocation.objects[vector2].performRemoveAction(vector2, Game1.player.currentLocation);
                this.TeleChestLocation.objects.Remove(vector2);
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
            TeleChestsMod.GetMod().SharedInventory = items;
            return i;
        }
        public new void grabItemFromChest(Item item, Farmer who)
        {
            base.grabItemFromChest(item, who);
            TeleChestsMod.GetMod().SharedInventory = items;
        }
        public new void grabItemFromInventory(Item item, Farmer who)
        {
            base.grabItemFromInventory(item, who);
            TeleChestsMod.GetMod().SharedInventory = items;
        }
        public new void clearNulls()
        {
            base.clearNulls();
            TeleChestsMod.GetMod().SharedInventory = items;
        }
        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            items = TeleChestsMod.GetMod().SharedInventory;
            bool check = base.checkForAction(who, justCheckingForActivity);
            TeleChestsMod.GetMod().SharedInventory = items;
            return check;
        }
        public new void itemTakenCallback(Item item, Farmer who)
        {
            base.itemTakenCallback(item, who);
            TeleChestsMod.GetMod().SharedInventory = items;
        }
    }
}