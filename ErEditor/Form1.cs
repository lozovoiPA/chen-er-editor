using ErEditor.DbSchema;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using ErEditor.UI;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ErEditor
{
    public partial class MainWindow : Form
    {
        ErSchema? newSchema = null;
        public MainWindow()
        {
            InitializeComponent();

            //ErSchema newSchema = new("Test Schema");
            //this.newSchema = newSchema;
            //navigatorTreeView1.OpenSchema(newSchema);

            DialogManager.DiagramPanel = diagramPanel1;

            // Test log sender names
            ConsoleLog.Log("Test short sender name", this, "TEST");
            Console.WriteLine(this);
            ConsoleLog.Log("Test full sender name", this, "TEST", true);
            ConsoleLog.Log("Test short sender name", this.navigatorTreeView1, "TEST");
            Console.WriteLine(this.navigatorTreeView1);
            ConsoleLog.Log("Test full sender name", this.navigatorTreeView1, "TEST", true);

            ToolStripMenuItem debugDropDown = new("Дебаг");
            ToolStripMenuItem showSchemaObjectItem = new("Показать схему в оперативной памяти");
            showSchemaObjectItem.Click += Debug_ShowSchema;
            ToolStripMenuItem showRegistryStateItem = new("Показать состояние реестра схемы");
            showRegistryStateItem.Click += Debug_ShowRegistry;
            debugDropDown.DropDownItems.AddRange(
                [
                showSchemaObjectItem,
                showRegistryStateItem
                ]);
            this.menuStrip1.Items.Add(debugDropDown);
        }

        private void createSchemaToolstripMenuItem_Click(object sender, EventArgs e)
        {
            ErSchema newSchema = DialogManager.CreateNewErSchemaWindow();
            navigatorTreeView1.OpenSchema(newSchema);

            this.newSchema = newSchema;
        }
        private void Debug_ShowSchema(object? sender, EventArgs e)
        {
            Console.WriteLine(newSchema);
        }
        private void Debug_ShowRegistry(object? sender, EventArgs e)
        {
            ErSchemaRegistry? registry = null;
            if(newSchema != null)
            {
                registry = ErSchemaFileManager.GetRegistry(newSchema);
            }
            if(registry != null)
            {
                Console.WriteLine(registry.PrintState());
            }
        }
        private void saveSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.newSchema != null)
            {
                ErSchemaFileManager.SaveSchema(this.newSchema);
            }
        }
        private void openSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErSchema schema = DialogManager.OpenSchemaWindow();
            navigatorTreeView1.OpenSchema(schema);

            this.newSchema = schema;
        }
    }
}
