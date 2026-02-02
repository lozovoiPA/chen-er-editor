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
        public static void Log(string message, object? sender = null)
        {
            DateTime now = DateTime.Now;
            string? senderName = ParseSenderName(sender);
            string logMessage = string.Empty;

            logMessage += (senderName == null) ? "[" : $"[{senderName} at ";
            logMessage += $"{now.Hour}:{now.Minute}:{now.Second}:{now.Millisecond}] " + message;
            Console.WriteLine(logMessage);
        }
        public static void Log(Notification notification, object? @object = null, object? receiver = null)
        {
            string logMessage = string.Empty;

            logMessage += $"recieved notification {GetShortTypeName(notification)} ";
            logMessage += (@object != null) ? $"for object {GetShortTypeName(@object)} {@object}" : "";
            Log(logMessage, receiver);
        }
        private static string? ParseSenderName(object? sender)
        {
            string? senderName = null;
            string? senderToString = null;
            if (sender is string)
            {
                senderName = sender as string;
            }
            else if (((sender != null) && ((senderName = sender.ToString()) != null)))
            {
                var senderTypeName = GetShortTypeName(sender);
                senderToString = sender.ToString();
                if (GetSystemName(sender) != senderToString 
                    && senderTypeName != null && senderToString != null
                    && !(senderToString.Contains('\n') || senderToString.Contains('\t')))
                {
                    if(senderToString.Length > 32)
                    {
                        senderToString = senderToString.Remove(32);
                        senderToString += "...";
                    }
                    senderName = senderTypeName + " " + senderToString;
                }
                else
                {
                    senderName = senderTypeName;
                }
            }
            return senderName;
        }

        public static string GetSystemName(object @object)
        {
            string systemName = @object.GetType().ToString();
            return systemName;
        }
        public static string GetFullTypeName(object @object)
        {
            string systemName = GetSystemName(@object);
            var names = systemName.Split(",");
            string fullTypeName = names.First();
            fullTypeName = fullTypeName.Replace("`1", "");
            fullTypeName = fullTypeName.Replace("+", ".");
            return fullTypeName;
        }
        public static string GetShortTypeName(object @object)
        {
            string fullTypeName = GetFullTypeName(@object);
            fullTypeName = fullTypeName.Replace("]", "");

            var genericFullNames = fullTypeName.Split('[');
            List<string> genericShortNames = new();
            string shortTypeName = string.Empty;

            int i = 0;
            foreach(var genericFullName in genericFullNames)
            {
                var splitNames = genericFullName.Split(".");
                string shortName = splitNames.Last();
                genericShortNames.Add(shortName);
                shortTypeName += shortName;
                if (i < genericFullNames.Length - 1)
                {
                    shortTypeName += "[";
                    i++;
                }
            }
            while(i > 0)
            {
                shortTypeName += "]";
                i--;
            }
            
            return shortTypeName;
        }
    }
}
