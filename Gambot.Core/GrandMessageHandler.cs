﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gambot.Core;

namespace Gambot
{
    public static class GrandMessageHandler
    {
        private static readonly List<IMessageHandler> MessageHandlers = new List<IMessageHandler>();

        public static void AddHandler<T>() where T : IMessageHandler, new()
        {
            var handler = new T();
            handler.Initialize();
            MessageHandlers.Add(handler);
        }

        public static void Digest(IMessenger messenger, IMessage message)
        {
            foreach (var handler in MessageHandlers)
            {
                if (!handler.Digest(messenger, message))
                    break;
            }
        }
    }
}
