using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.Infrastructure
{
    public static class ConsoleLog
    {
        public static void Log(string message, object? sender = null, string messageType = "", bool fullSenderName = true)
        {
            DateTime now = DateTime.Now;
            Console.Write($"[{now.Hour}:{now.Minute}:{now.Second}:{now.Millisecond}] ");

            string? senderName = null;
            if (sender is string)
            {
                senderName = sender as string;
                Console.Write($"{messageType} from {senderName} - {message}");
            }
            else if (((sender != null) && ((senderName = sender.ToString()) != null)))
            {
                var names = senderName.Split(",");
                senderName = names.First();
                if (!fullSenderName)
                {
                    names = senderName.Split(".");
                    senderName = names.Last();
                }
                Console.Write($"{messageType} from {senderName} - {message}");
            }
            else
            {
                Console.Write($"{messageType} - {message}");
            }
            Console.WriteLine();
        }
        public static string GetFullTypeName(object @object)
        {
            string systemName = @object.GetType().ToString();
            var names = systemName.Split(",");
            string fullTypeName = names.First();
            return fullTypeName;
        }
        public static string GetShortTypeName(object @object)
        {
            string fullTypeName = GetFullTypeName(@object);
            var names = fullTypeName.Split(".");
            string shortTypeName = names.Last();
            return shortTypeName;
        }
    }
}
