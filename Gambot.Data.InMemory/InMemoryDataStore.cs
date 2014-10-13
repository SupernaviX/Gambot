﻿using System;
using System.Collections.Generic;
using System.Linq;
using MiscUtil;
using MiscUtil.Linq;

namespace Gambot.Data.InMemory
{
    internal class InMemoryDataStore : IDataStore
    {
        private readonly EditableLookup<string, string> data;

        public InMemoryDataStore()
        {
            data = new EditableLookup<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public bool Put(string key, string val)
        {
            var alreadyExists = data.Contains(key, val);
            if (!alreadyExists) data.Add(key, val);

            return !alreadyExists;
        }

        public int RemoveAllValues(string key)
        {
            var count = data[key].Count();
            if (count > 0) data.Remove(key);

            return count;
        }

        public bool RemoveValue(string key, string val)
        {
            return data.Remove(key, val);
        }

        public IEnumerable<string> GetAllKeys()
        {
            return data.Select(group => group.Key);
        }

        public IEnumerable<string> GetAllValues(string key)
        {
            return data[key];
        }

        public string GetRandomValue(string key)
        {
            var values = data[key].ToList();
            return values.Count == 0 ? null : values.ElementAt(StaticRandom.Next(0, values.Count));
        }
    }
}