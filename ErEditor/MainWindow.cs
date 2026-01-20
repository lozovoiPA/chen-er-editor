using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using ErEditor.UI;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ErEditor
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            MainWindow.Instance = this; // redirected all mediator calls to this form

            this.InitializeDebugMenu();
        }
        private void createSchemaToolstripMenuItem_Click(object sender, EventArgs e)
        {
            ErSchema? newSchema = DialogManager.CreateNewErSchemaWindow();
            if(newSchema != null)
            {
                OpenSchema(newSchema);
            }
        }
        private void saveSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var schemas = navigatorTreeView1.Schemas;
            if (schemas.Count > 0)
            {
                ErSchemaFileManager.SaveSchema(schemas[0]);
            }
        }
        private void openSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErSchema schema = DialogManager.OpenSchemaWindow();
            OpenSchema(schema);
        }
    }
}
