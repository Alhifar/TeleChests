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

namespace TeleChests
{
    public class TeleChestsMod : Mod
    {
        private static TeleChestsMod mod;

        private string teleChestLocationChestsSavePath;
        private string teleChestInvSavePath;
        private string teleChestInvChestsSavePath;
        private string teleChestChestChestsSavePath;

        public List<Item> SharedInventory { set; get; }
        private SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>> inWorldTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>();
        Stack<KeyValuePair<int, KeyValuePair<Vector2, TeleChest>>> chestStack = new Stack<KeyValuePair<int, KeyValuePair<Vector2, TeleChest>>>();
        SerializableDictionary<int, TeleChest> invTeleChests = new SerializableDictionary<int, TeleChest>();
        SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>> chestTeleChests = new SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>>();

        XmlSerializer invSerializer = new XmlSerializer(typeof(List<Item>), new Type[] { typeof(Item), typeof(TeleChest) });
        XmlSerializer inWorldSerializer = new XmlSerializer(typeof(SerializableDictionary<int, SerializableDictionary<Vector2, TeleChest>>), new Type[] { typeof(int), typeof(Vector2), typeof(TeleChest) });
        XmlSerializer invChestsSerializer = new XmlSerializer(typeof(SerializableDictionary<int, TeleChest>), new Type[] { typeof(int), typeof(TeleChest) });
        XmlSerializer chestChestsSerializer = new XmlSerializer(typeof(SerializableDictionary<int, SerializableDictionary<Vector2, SerializableDictionary<int, TeleChest>>>), new Type[] { typeof(int), typeof(Vector2), typeof(TeleChest) });

        public override void Entry(params object[] objects)
        {
            mod = this;
            SharedInventory = new List<Item>();
            Command.RegisterCommand("givetelechest", "Gives a TeleChest").CommandFired += giveTeleChest;
            PlayerEvents.LoadedGame += onFileLoad;
            MenuEvents.MenuChanged += onMenuChange;
            MenuEvents.MenuClosed += onMenuClosed;
            GameEvents.UpdateTick += onUpdateTick;
        }
        public static TeleChestsMod GetMod()
        {
            return mod;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void onMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            try
            {
                // After saving game, reload TeleChests
                if (e.PriorMenu is SaveGameMenu)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void onFileLoad(object sender, EventArgsLoadedGameChanged e)
        {
            try
            {
                if (e.LoadedGame)
                {
                    teleChestLocationChestsSavePath = Path.Combine(this.PathOnDisk, String.Format("teleChestLocationSave_{0}_{1}.xml", Game1.player.Name, Game1.uniqueIDForThisGame));
                    teleChestInvSavePath = Path.Combine(this.PathOnDisk, String.Format("teleChestInvSave_{0}_{1}.xml", Game1.player.Name, Game1.uniqueIDForThisGame));
                    teleChestInvChestsSavePath = Path.Combine(this.PathOnDisk, String.Format("teleChestInvChestsSave_{0}_{1}.xml", Game1.player.Name, Game1.uniqueIDForThisGame));
                    teleChestChestChestsSavePath = Path.Combine(this.PathOnDisk, String.Format("teleChestChestChestsSave_{0}_{1}.xml", Game1.player.Name, Game1.uniqueIDForThisGame));

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
                            SharedInventory = (List<Item>)invSerializer.Deserialize(xmlReader);
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
