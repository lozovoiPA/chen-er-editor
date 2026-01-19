using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace ErEditor.UI
{
    public class ElementView : TableLayoutPanel
    {
        protected List<Tuple<Label, Control?>> rows = new();
        protected int rowHeight = 30;
        public ElementView()
        {
            ColumnCount = 2; // without panel, don't add this (uses default 2 columns) 
            RowCount = 0; // without panel, use 1. On panel this doesn't work
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            AutoSize = false;

            AddRow("Название");
        }

        protected Label AddPropertyLabel(string propertyName)
        {
            Label label = new Label();
            // Without this it will automatically multine if it doesn't fit IN THE INITIAL CELL SIZE and won't become
            // one-line even if you resize it to have enough space
            label.AutoEllipsis = true;
            label.AutoSize = false;
            label.Dock = DockStyle.Fill;
            label.Text = propertyName;
            label.Padding = new Padding(0);
            label.Size = new Size(label.PreferredWidth, label.PreferredHeight);
            label.Anchor = AnchorStyles.Left;

            return label;
        }

        protected int AddRow(string propertyName)
        {
            // the correct way to do this is
            // First add controls, then add fresh RowStyle (do not modify an existing one even if using GetRow!) and then modify the row itself.
            var label = AddPropertyLabel(propertyName);

            TextBox textBox = new TextBox();
            textBox.Margin = new Padding(0);
            textBox.AutoSize = false;
            textBox.Height = rowHeight; // alternatively fill cell with white and keep text box the same size. See TableLayoutPanel page example
            textBox.BorderStyle = BorderStyle.None;
            //textBox.Font = new Font(textBox.Font.FontFamily, (int)(rowHeight * 0.6), textBox.Font.Style, GraphicsUnit.Pixel);
            textBox.Dock = DockStyle.Fill;

            rows.Add(new(label, textBox));
            Controls.AddRange([label, textBox]);

            var row = new RowStyle(SizeType.Absolute, rowHeight);
            RowStyles.Insert(rows.Count-1, row);

            return rows.Count - 1;
        }

        protected int AddRow(string propertyName, Control control)
        {
            var label = AddPropertyLabel(propertyName);

            rows.Add(new(label, control));
            Controls.AddRange([label, control]);

            var row = new RowStyle(SizeType.Absolute, rowHeight);
            RowStyles.Add(row);

            return rows.Count - 1;
        }
    }

    public class ElementPropertiesPanel : Panel
    {
        private RoleView roleView = new();

        public ElementPropertiesPanel()
        {
            roleView.Visible = false;
            roleView.Dock = DockStyle.Fill;
            Controls.Add(roleView);
        }

        public void OpenProperties(ErSchema schema, ErRole role)
        {
            // В панели же можно какие-то CheckConstraints сделать
            roleView.Schema = schema;
            roleView.Role = role;
            roleView.Visible = true;
        }

        public void CloseProperties()
        {
            roleView.CommitChanges();
            roleView.Visible = false;
        }
    }

    public class RoleView : ElementView
    {
        private ErSchema? schema; // if each element knew its parent schema (as readonly) this wouldn't be necessary. I'm thinking of adding that to every element.
        private ErRole? role;

        private ComboBox entitySetComboBox = new();
        public RoleView() : base()
        {
            AddRow("Множество сущностей", entitySetComboBox);
        }

        public void CommitChanges()
        {

        }

        public ErSchema? Schema
        {
            get { return schema; }
            set
            {
                if(value != null)
                {
                    // only happens once!
                    schema = value;
                    entitySetComboBox.DataSource = value.EntitySets;
                    ConsoleLog.Log("idk");
                    entitySetComboBox.DisplayMember = "Name";
                    //entitySetComboBox.ValueMember = "Name";
                }
            }
        }
        public ErRole? Role
        {
            get { return role; }
            set
            {
                if (value != null)
                {
                    role = value;
                }
            }
        }
    }
}
