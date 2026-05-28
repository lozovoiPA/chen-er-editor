using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ErEditor
{
    public partial class NewErSchemaWindow : Form
    {
        public string ErSchemaName
        {
            get
            {
                return schemaNameTextBox.Text;
            }
        }
        public string ErSchemaFileName
        {
            get
            {
                return fileNameTextBox.Text;
            }
        }
        public string ErSchemaFolderPath
        {
            get
            {
                return filePathTextBox.Text;
            }
        }
        public NewErSchemaWindow()
        {
            InitializeComponent();

            filePathTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\test_db3";
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void folderBrowserDialogButton_Click(object sender, EventArgs e)
        {
            switch (folderBrowserDialog1.ShowDialog())
            {
                case DialogResult.OK:
                    filePathTextBox.Text = folderBrowserDialog1.SelectedPath;
                    break;
                default:
                    break;
            }
        }
    }
}
