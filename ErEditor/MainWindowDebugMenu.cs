using ErEditor.ErSchemaClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor
{
    partial class MainWindow
    {
        private void InitializeDebugMenu()
        {
            ToolStripMenuItem debugDropDown = new("Дебаг");
            ToolStripMenuItem showSchemaObjectItem = new("Показать схему в оперативной памяти");
            showSchemaObjectItem.Click += Debug_ShowSchema;
            ToolStripMenuItem showRegistryStateItem = new("Показать состояние реестра схемы");
            showRegistryStateItem.Click += Debug_ShowRegistry;
            ToolStripMenuItem showMsaglGraph = new("Генерация графа (MSAGL)");
            showMsaglGraph.Click += Debug_ShowMsaglGraph;
            
            debugDropDown.DropDownItems.AddRange(
                [
                showSchemaObjectItem,
                showRegistryStateItem,
                showMsaglGraph
                ]);
            menuStrip1.Items.Add(debugDropDown);
        }
        private void Debug_ShowSchema(object? sender, EventArgs e)
        {
            Console.WriteLine(newSchema?.PrintState());
        }
        private void Debug_ShowRegistry(object? sender, EventArgs e)
        {
            ErSchemaRegistry? registry = null;
            if (newSchema != null)
            {
                registry = ErSchemaFileManager.GetRegistry(newSchema);
            }
            if (registry != null)
            {
                Console.WriteLine(registry.PrintState());
            }
        }
        private void Debug_ShowMsaglGraph(object? sender, EventArgs e)
        {
            MsaglTestsForm msaglTestsForm = new();
            msaglTestsForm.ShowDialog();
        }
    }
}
