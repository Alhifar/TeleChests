using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using System.Xml;
using System.IO;
using StardewValley.Objects;
using System.Reflection;
using System.Linq;

namespace TeleChests
{
    public class TeleChestsMod : Mod
    {
        private static TeleChestsMod mod;
        private static TeleChestConfig config;

        public static TeleChestsMod Mod
        {
            get
            {
                return mod;
            }
        }
        public static TeleChestConfig Config
        {
            get
            {
                return config;
            }
        }

        private static string teleChestLocationChestsSavePath;
        private static string teleChestInvSavePath;
        private static string teleChestInvChestsSavePath;
        private static string teleChestChestChestsSavePath;

        public static SerializableDictionary<int, List<Item>> SharedInventory { set; get; }
        private static SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>> inWorldTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>();
        private static Stack<KeyValuePair<int, KeyValuePair<Vector2, TeleChest>>> chestStack = new Stack<KeyValuePair<int, KeyValuePair<Vector2, TeleChest>>>();
        private static SerializableDictionary<int, TeleChest> invTeleChests = new SerializableDictionary<int, TeleChest>();
        private static SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>> chestTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>>();

        private static XmlSerializer invSerializer = new XmlSerializer(typeof(SerializableDictionary<int, List<Item>>), new Type[] { typeof(Item), typeof(TeleChest), typeof(int), typeof(List<Item>) });
        private static XmlSerializer inWorldSerializer = new XmlSerializer(typeof(SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>), new Type[] { typeof(int), typeof(Vector2), typeof(TeleChest) });
        private static XmlSerializer invChestsSerializer = new XmlSerializer(typeof(SerializableDictionary<int, TeleChest>), new Type[] { typeof(int), typeof(TeleChest) });
        private static XmlSerializer chestChestsSerializer = new XmlSerializer(typeof(SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>>), new Type[] { typeof(int), typeof(Vector2), typeof(TeleChest) });

        public override void Entry(params object[] objects)
        {
            mod = this;
            config = ConfigExtensions.InitializeConfig<TeleChestConfig>(new TeleChestConfig(), this.BaseConfigPath);
            SharedInventory = new SerializableDictionary<int, List<Item>>();
            Command.RegisterCommand("givetelechest", "Gives a TeleChest").CommandFired += giveTeleChest;
            PlayerEvents.LoadedGame += onFileLoad;
            MenuEvents.MenuChanged += onMenuChange;
            MenuEvents.MenuClosed += onMenuClosed;
            GameEvents.UpdateTick += onUpdateTick;
        }
        public void giveTeleChest(object sender, EventArgsCommand e)
        {
            Game1.player.addItemToInventory(new TeleChest(true));
        }
        public void onMenuChange(object sender, EventArgsClickableMenuChanged e)
        {
            try
            {
                // Save TeleChests before saving to prevent crash
                if (e.NewMenu is SaveGameMenu)
                {
                    handleOpenSaveGameMenu();
                }
                else if (e.NewMenu is GameMenu)
                {
                    CraftingPage craftingPage = null;
                    List<IClickableMenu> pages = ((List<IClickableMenu>)typeof(GameMenu).GetField("pages", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((GameMenu)e.NewMenu));
                    foreach (IClickableMenu menu in pages)
                    {
                        if (menu is CraftingPage)
                        {
                            craftingPage = (CraftingPage)menu;
                        }
                    }
                    if (craftingPage == null)
                    {
                        return;
                    }
                    handleOpenCraftingPage(craftingPage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private static void handleOpenSaveGameMenu()
        {
            // Save in-world chests
            foreach (GameLocation location in Game1.locations)
            {
                SerializableDictionary<Vector2, TeleChest> localTeleChests = new SerializableDictionary<Vector2, TeleChest>();
                foreach (KeyValuePair<Vector2, StardewValley.Object> objPair in location.objects)
                {
                    if (objPair.Value is TeleChest)
                    {
                        localTeleChests.Add(objPair.Key, (TeleChest)objPair.Value);
                    }
                }
                inWorldTeleChests.Add(Game1.locations.IndexOf(location), localTeleChests);
            }
            if (inWorldTeleChests.Count > 0)
            {
                foreach (int location in inWorldTeleChests.Keys)
                {
                    foreach (KeyValuePair<Vector2, TeleChest> localTeleChestLocations in inWorldTeleChests[location])
                    {
                        Game1.locations[location].objects.Remove(localTeleChestLocations.Key);
                    }
                }
                if (File.Exists(teleChestLocationChestsSavePath))
                {
                    File.Delete(teleChestLocationChestsSavePath);
                }
                using (XmlWriter xmlWriter = XmlWriter.Create(teleChestLocationChestsSavePath))
                {
                    xmlWriter.WriteStartDocument();
                    inWorldSerializer.Serialize(xmlWriter, inWorldTeleChests);
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else
            {
                File.Delete(teleChestLocationChestsSavePath);
            }

            // Save TeleChests in player inventory
            foreach (Item item in Game1.player.items)
            {
                if (item is TeleChest)
                {
                    invTeleChests.Add(Game1.player.items.IndexOf(item), (TeleChest)item);
                }
            }

            // If any chests found in inventory, delete and serialize to be restored later
            if (invTeleChests.Count > 0)
            {
                foreach (KeyValuePair<int, TeleChest> slot in invTeleChests)
                {
                    Game1.player.removeItemFromInventory(slot.Key);
                }

                if (File.Exists(teleChestInvChestsSavePath))
                {
                    File.Delete(teleChestInvChestsSavePath);
                }
                using (XmlWriter xmlWriter = XmlWriter.Create(teleChestInvChestsSavePath))
                {
                    xmlWriter.WriteStartDocument();
                    invChestsSerializer.Serialize(xmlWriter, invTeleChests);
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else
            {
                File.Delete(teleChestInvChestsSavePath);
            }

            // Save TeleChests inside chests to be restored later
            foreach (GameLocation location in Game1.locations)
            {
                SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>> localChests = new SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>();
                foreach (KeyValuePair<Vector2, StardewValley.Object> obj in location.objects)
                {
                    if (obj.Value is Chest)
                    {
                        Chest chest = (Chest)obj.Value;
                        SerializableDictionary<int, TeleChest> localChestContents = new SerializableDictionary<int, TeleChest>();
                        foreach (Item it in chest.items)
                        {
                            if (it is TeleChest)
                            {
                                localChestContents.Add(chest.items.IndexOf(it), (TeleChest)it);
                            }
                        }
                        localChests.Add(obj.Key, localChestContents);
                    }
                }
                chestTeleChests.Add(Game1.locations.IndexOf(location), localChests);
            }

            if (chestTeleChests.Count > 0)
            {
                foreach (int location in chestTeleChests.Keys)
                {
                    foreach (Vector2 tileLocation in chestTeleChests[location].Keys)
                    {
                        foreach (KeyValuePair<int, TeleChest> slot in chestTeleChests[location][tileLocation])
                        {
                            Chest chest = (Chest)Game1.locations[location].objects[tileLocation];
                            chest.items[slot.Key] = new StardewValley.Object(0, 0);
                        }
                    }
                }

                if (File.Exists(teleChestChestChestsSavePath))
                {
                    File.Delete(teleChestChestChestsSavePath);
                }
                using (XmlWriter xmlWriter = XmlWriter.Create(teleChestChestChestsSavePath))
                {
                    xmlWriter.WriteStartDocument();
                    chestChestsSerializer.Serialize(xmlWriter, chestTeleChests);
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            else
            {
                File.Delete(teleChestChestChestsSavePath);
            }

            // Also save global chest inventory no matter what
            if (File.Exists(teleChestInvSavePath))
            {
                File.Delete(teleChestInvSavePath);
            }
            using (XmlWriter xmlWriter = XmlWriter.Create(teleChestInvSavePath))
            {
                xmlWriter.WriteStartDocument();
                invSerializer.Serialize(xmlWriter, SharedInventory);
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                xmlWriter.Close();
            }
        }
        public void handleOpenCraftingPage(CraftingPage craftingPage)
        {
            List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = ((List<Dictionary<ClickableTextureComponent, CraftingRecipe>>)typeof(CraftingPage).GetField("pagesOfCraftingRecipes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(craftingPage));
            int num = craftingPage.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - Game1.tileSize / 4;
            int num2 = craftingPage.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - Game1.tileSize / 4;
            int num3 = 8;
            int num4 = 10;
            int num5 = pagesOfCraftingRecipes.Last().Count;
            if (num5 % 40 == 0)
            {
                num5++;
            }
            int num7 = num5 / num4 % (40 / num4);

            Rectangle CTCRect = new Rectangle(num + num5 % num4 * (Game1.tileSize + num3), num2 + num7 * (Game1.tileSize + 8), Game1.tileSize, Game1.tileSize * 2);
            Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, 130, 16, 32);
            pagesOfCraftingRecipes.Last().Add(new ClickableTextureComponent(CTCRect, "", "", Game1.bigCraftableSpriteSheet, sourceRect, Game1.pixelZoom), new TeleChestCraftingRecipe());
            PlayerEvents.InventoryChanged += onInventoryChanged;
        }
        public void onMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            try
            {
                // After saving game, reload TeleChests
                if (e.PriorMenu is SaveGameMenu)
                {
                    handleCloseSaveGameMenu();
                }
                if (e.PriorMenu is GameMenu)
                {
                    handleCloseCraftingPage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void handleCloseSaveGameMenu()
        {
            // Restore in-world TeleChests
            foreach (int location in inWorldTeleChests.Keys)
            {
                foreach (KeyValuePair<Vector2, TeleChest> localTeleChestLocations in inWorldTeleChests[location])
                {
                    Game1.locations[location].objects.Add(localTeleChestLocations.Key, localTeleChestLocations.Value);
                }
            }

            // Restore inventory TeleChests
            foreach (int slot in invTeleChests.Keys)
            {
                Game1.player.addItemToInventory(invTeleChests[slot], slot);
            }

            // Restore chest TeleChests
            foreach (int location in chestTeleChests.Keys)
            {
                foreach (Vector2 tileLocation in chestTeleChests[location].Keys)
                {
                    foreach (KeyValuePair<int, TeleChest> slot in chestTeleChests[location][tileLocation])
                    {
                        Chest chest = (Chest)Game1.locations[location].objects[tileLocation];
                        chest.items[slot.Key] = slot.Value;
                    }
                }
            }

            chestTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>>();
            invTeleChests = new SerializableDictionary<int, TeleChest>();
            inWorldTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>();
        }
        public void handleCloseCraftingPage()
        {
            PlayerEvents.InventoryChanged -= onInventoryChanged;
        }
        public void onInventoryChanged(object sender, EventArgsInventoryChanged e)
        {
            List<int> itemsToReplace = new List<int>();
            foreach (Item item in Game1.player.items)
            {
                if (item != null && item is StardewValley.Object && (item as StardewValley.Object).bigCraftable && item.parentSheetIndex == 130 && !(item is Chest))
                {
                    itemsToReplace.Add(Game1.player.items.IndexOf(item));
                }
            }
            foreach (int index in itemsToReplace)
            {
                Game1.player.items[index] = new TeleChest(true);
            }
        }
        public void onFileLoad(object sender, EventArgsLoadedGameChanged e)
        {
            try
            {
                if (e.LoadedGame)
                {
                    teleChestLocationChestsSavePath = Path.Combine(this.PathOnDisk, $"teleChestLocationSave_{Game1.player.Name}_{Game1.uniqueIDForThisGame}.xml");
                    teleChestInvSavePath = Path.Combine(this.PathOnDisk, $"teleChestInvSave_{Game1.player.Name}_{Game1.uniqueIDForThisGame}.xml");
                    teleChestInvChestsSavePath = Path.Combine(this.PathOnDisk, $"teleChestInvChestsSave_{Game1.player.Name}_{Game1.uniqueIDForThisGame}.xml");
                    teleChestChestChestsSavePath = Path.Combine(this.PathOnDisk, $"teleChestChestChestsSave_{Game1.player.Name}_{Game1.uniqueIDForThisGame}.xml");

                    // Load in-world TeleChests
                    if (File.Exists(teleChestLocationChestsSavePath))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(teleChestLocationChestsSavePath))
                        {
                            inWorldTeleChests = (SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>)inWorldSerializer.Deserialize(xmlReader);
                        }
                        foreach (int location in inWorldTeleChests.Keys)
                        {
                            foreach (KeyValuePair<Vector2, TeleChest> localTeleChestLocations in inWorldTeleChests[location])
                            {
                                // Add TeleChests to stack to be placed during first possible tick
                                // Objects will not actually be placed if added at this time.
                                chestStack.Push(new KeyValuePair<int, KeyValuePair<Vector2, TeleChest>>(location, new KeyValuePair<Vector2, TeleChest>(localTeleChestLocations.Key, localTeleChestLocations.Value)));
                            }
                        }
                        inWorldTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>();
                    }

                    // Load inventory TeleChests
                    if (File.Exists(teleChestInvChestsSavePath))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(teleChestInvChestsSavePath))
                        {
                            invTeleChests = (SerializableDictionary<int, TeleChest>)invChestsSerializer.Deserialize(xmlReader);
                        }
                        foreach (int slot in invTeleChests.Keys)
                        {
                            Game1.player.addItemToInventory(invTeleChests[slot], slot);
                        }
                        inWorldTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>();
                    }

                    // Load chest TeleChests
                    if (File.Exists(teleChestChestChestsSavePath))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(teleChestChestChestsSavePath))
                        {
                            chestTeleChests = (SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>>)chestChestsSerializer.Deserialize(xmlReader);
                        }
                        foreach (int location in chestTeleChests.Keys)
                        {
                            foreach (Vector2 tileLocation in chestTeleChests[location].Keys)
                            {
                                foreach (KeyValuePair<int, TeleChest> slot in chestTeleChests[location][tileLocation])
                                {
                                    Chest chest = (Chest)Game1.locations[location].objects[tileLocation];
                                    chest.items[slot.Key] = slot.Value;
                                }
                            }
                        }
                    }

                    // Load global inventory
                    if (File.Exists(teleChestInvSavePath))
                    {
                        using (XmlReader xmlReader = XmlReader.Create(teleChestInvSavePath))
                        {
                            SharedInventory = (SerializableDictionary<int, List<Item>>)invSerializer.Deserialize(xmlReader);
                        }
                    }
                    chestTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>>();
                    invTeleChests = new SerializableDictionary<int, TeleChest>();
                    inWorldTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void onUpdateTick(object sender, EventArgs e)
        {
            while (chestStack.Count > 0)
            {
                KeyValuePair<int, KeyValuePair<Vector2, TeleChest>> chestLocations = chestStack.Pop();
                Game1.locations[chestLocations.Key].objects.Add(chestLocations.Value.Key, chestLocations.Value.Value);
            }
        }
    }
}
