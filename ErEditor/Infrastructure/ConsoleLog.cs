using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
