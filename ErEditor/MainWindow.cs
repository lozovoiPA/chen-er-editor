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
            Initialize();

            MainWindow.Instance = this; // redirected all mediator calls to this form

            this.InitializeDebugMenu();
        }

        private void Initialize()
        {
            DisableSchemaControls();
        }
        private void DisableSchemaControls()
        {
            toolStrip2.Enabled = false;
            toolStrip3.Enabled = false;

            navigatorTreeView1.Enabled = false;
        }
        private void EnableSchemaControls()
        {
            toolStrip2.Enabled = true;
            toolStrip3.Enabled = true;

            navigatorTreeView1.Enabled = true;
        }

        private void createSchemaToolstripMenuItem_Click(object sender, EventArgs e)
        {
            ErSchema? newSchema = DialogManager.CreateNewErSchemaWindow();
            if(newSchema != null)
            {
                if (navigatorTreeView1.Schemas.Count == 0)
                {
                    EnableSchemaControls();
                }
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
            if (navigatorTreeView1.Schemas.Count == 0)
            {
                EnableSchemaControls();
            }
            OpenSchema(schema);
        }
    }
}
