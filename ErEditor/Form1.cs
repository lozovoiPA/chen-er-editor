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

            // я гарантирую, что все елементы в конструкторе могут обращаться к DialogManager и получать везде результат
            new DialogManager(new(navigatorTreeView1, elementPropertiesPanel1, diagramPanel1));

            this.InitializeDebugMenu();
        }

        private void createSchemaToolstripMenuItem_Click(object sender, EventArgs e)
        {
            ErSchema? newSchema = DialogManager.CreateNewErSchemaWindow();
            if(newSchema != null)
            {
                DialogManager.Instance.OpenSchema(newSchema);
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
            DialogManager.Instance.OpenSchema(schema);
        }
    }
}
