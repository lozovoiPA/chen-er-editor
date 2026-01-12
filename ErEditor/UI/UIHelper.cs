using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI
{
    public static class UIHelper
    {
        public static void EmptyTestHandler(object? sender, EventArgs e)
        {
            if (sender != null)
            {
                Console.WriteLine("The test event handler was triggered by " + sender.ToString());
                return;
            }
            Console.WriteLine("The test event handler was triggered by null sender");
        }

        public static ContextMenuStrip AddContextMenu(Control control, Dictionary<string, EventHandler> items)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripItem strip;
            foreach (var item in items)
            {
                strip = menu.Items.Add(item.Key);
                strip.Click += item.Value;
            }
            control.ContextMenuStrip = menu;
            return menu;
        }

        public static ContextMenuStrip AddContextMenu(IExtTreeNode node, Dictionary<string, EventHandler> items)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            ToolStripItem strip;
            foreach (var item in items)
            {
                strip = menu.Items.Add(item.Key);
                strip.Click += item.Value;
            }
            node.TreeNode.ContextMenuStrip = menu;
            return menu;
        }
    }
}
