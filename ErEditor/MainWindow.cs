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
            if (this.newSchema != null)
            {
                ErSchemaFileManager.SaveSchema(this.newSchema);
            }
        }
        private void openSchemaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ErSchema schema = DialogManager.OpenSchemaWindow();
            OpenSchema(schema);
        }
    }
}
