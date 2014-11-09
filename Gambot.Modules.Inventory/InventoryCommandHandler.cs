﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Modules.Inventory
{
    class InventoryCommandHandler : IMessageHandler
    {
        private IDataStore invDataStore;
        private IDataStore factoidDataStore;
        private IVariableHandler variableHandler;

        public HandlerPriority Priority
        {
            get { return HandlerPriority.Normal; }
        }

        public InventoryCommandHandler(IVariableHandler variableHandler)
        {
            this.variableHandler = variableHandler;
        }
        
        public void Initialize(IDataStoreManager dataStoreManager)
        {
            invDataStore = dataStoreManager.Get("Inventory");
            factoidDataStore = dataStoreManager.Get("Factoid");

            invDataStore.Put("SuccessfulAdd", "now contains");
            invDataStore.Put("SuccessfulAdd", "is now carrying");
            invDataStore.Put("SuccessfulAdd", "is now holding");
            invDataStore.Put("SuccessfulAdd", "takes");

            factoidDataStore.Put("item already exists reply", "No thanks, $who, I've already got one.");
            factoidDataStore.Put("item already exists reply", "I already have $item.");
            factoidDataStore.Put("item already exists reply", "But I've already got $item!");
            factoidDataStore.Put("item already exists reply", "$who: I already have $item.");
            
            variableHandler.DefineMagicVariable("item", GetRandomItem);
            variableHandler.DefineMagicVariable("giveitem", GetRandomItemAndDiscard);
            variableHandler.DefineMagicVariable("newitem", GetNewItem);
        }

        private string GetRandomItem(IMessage msg)
        {
            return invDataStore.GetRandomValue("Items") ?? "$item"; // possibly replace with "nothing"
        }

        private string GetRandomItemAndDiscard(IMessage msg)
        {
            var item = invDataStore.GetRandomValue("Items");

            if (item == null)
                return "$item";

            invDataStore.RemoveValue("Items", item);
            return item;
        }

        private string GetNewItem(IMessage msg)
        {
            return "$newitem"; // todo: ????????
        }

        public string Process(string currentResponse, IMessage message, bool addressed)
        {
            if (message.Action)
            {
                var botName = Config.Get("Name");
                var match = Regex.Match(message.Text,
                                        String.Format(@"gives (?:(.+) to {0}|{0} (.+))", botName),
                                        RegexOptions.IgnoreCase);

                if (!match.Success)
                    return currentResponse;

                var itemName = String.IsNullOrEmpty(match.Groups[1].Value)
                                   ? match.Groups[2].Value
                                   : match.Groups[1].Value;
                if (itemName.EndsWith("?"))
                    return currentResponse;

                var inventoryLimit =
                    Int32.Parse(Config.Get("InventoryLimit"));
                var allItems = invDataStore.GetAllValues("Items").ToList();
                var currentInventorySize = allItems.Count(); // we dont have a .GetCount lololo

                if (allItems.Contains(itemName))
                {
                    var randomDuplicateAddReply = factoidDataStore.GetRandomValue("item already exists reply");
                    return variableHandler.Substitute(randomDuplicateAddReply,
                                                      message,
                                                      Replace.VarWith("who", message.Who));
                }

                if (currentInventorySize >= inventoryLimit)
                {
                    var randomItemToDrop =
                        invDataStore.GetRandomValue("Items");
                    if (randomItemToDrop == null)
                        return currentResponse;
                    invDataStore.RemoveValue("Items", randomItemToDrop);

                    invDataStore.Put("Items", itemName);

                    const string reply = "/me drops $item and takes $newitem.";
                    return variableHandler.Substitute(reply,
                                                      message,
                                                      Replace.VarWith("item", randomItemToDrop),
                                                      Replace.VarWith("newitem", itemName));
                }
                else
                {
                    invDataStore.Put("Items", itemName);

                    var randomSuccessfulAddReply = invDataStore.GetRandomValue("SuccessfulAdd");
                    return variableHandler.Substitute(String.Format("/me {0} $newitem.", randomSuccessfulAddReply),
                                                      message,
                                                      Replace.VarWith("newitem", itemName));
                }
            }
            else
            {
                
            }

            return currentResponse;
        }
    }
}
