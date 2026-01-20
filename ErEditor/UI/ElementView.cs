using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace ErEditor.UI
{
    public class ElementView<TErElement> : TableLayoutPanel
    {
        protected List<Tuple<Label, Control?>> rows = new();
        protected int rowHeight = 30;

        protected ErSchema? schema;
        protected TErElement? element;

        public ElementView()
        {
            ColumnCount = 2; // without panel, don't add this (uses default 2 columns) 
            RowCount = 0; // without panel, use 1. On panel this doesn't work
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            AutoSize = false;

            AddRow("Название");
        }

        public virtual ErSchema? Schema
        {
            get { return schema; }
            set
            {
                if (value != null)
                {
                    // only happens once!
                    schema = value;
                }
            }
        }
        public virtual TErElement? Element
        {
            get { return element; }
            set
            {
                if (value != null)
                {
                    element = value;
                }
            }
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

        // Этот объект и MainWindow не интересует конкретный тип TErElement. Они с ним не работают, они его перенапрявлют туда, куда надо.
        public void OpenProperties<TErElement>(ErSchema schema, TErElement element)
        {
            CloseProperties();
            ElementView<TErElement>? elementView = null;

            switch (element){
                case ErRole es:
                    elementView = roleView as ElementView<TErElement>;
                    break;
            }
            
            if(elementView != null)
            {
                elementView.Schema = schema;
                elementView.Element = element;

                elementView.Visible = true;
            }
        }
        public void CloseProperties()
        {
            roleView.CommitChanges();
            roleView.Visible = false;
        }
    }

    public class RoleView : ElementView<ErRole>
    {
        private ComboBox entitySetComboBox = new();
        public RoleView() : base()
        {
            entitySetComboBox.SelectionChangeCommitted += EntitySetComboBox_SelectionChangeCommitted;

            AddRow("Множество сущностей", entitySetComboBox);
        }

        private void EntitySetComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            ConsoleLog.Log($"Type of value chosen: {entitySetComboBox.SelectedValue?.GetType()}");
        }

        public void CommitChanges()
        {
            if(element != null)
            {
                element.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.Empty;
            }
        }

        public override ErSchema? Schema
        {
            get { return schema; }
            set
            {
                if(value != null)
                {
                    // only happens once!
                    schema = value;
                    entitySetComboBox.DataSource = value.EntitySets;
                    entitySetComboBox.DisplayMember = "Name";
                }
            }
        }
        public override ErRole? Element
        {
            get { return element; }
            set
            {
                if (value != null)
                {
                    element = value;
                    entitySetComboBox.SelectedItem = value.EntitySet;
                }
            }
        }
    }
}
