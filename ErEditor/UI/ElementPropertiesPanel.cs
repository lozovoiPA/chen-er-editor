using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System.Configuration;
using System.Reflection;


namespace ErEditor.UI
{
    public class ElementPropertiesPanel : Panel
    {
        private RoleView roleView = new();
        private MappingView mappingView = new MappingView();

        public ElementPropertiesPanel()
        {
            roleView.Visible = false;
            mappingView.Visible = false;
            roleView.Dock = DockStyle.Fill;
            mappingView.Dock = DockStyle.Fill;
            Controls.Add(roleView);
            Controls.Add(mappingView);

            this.DoubleBuffered = true;
        }

        // Этот объект и MainWindow не интересует конкретный тип TErElement. Они с ним не работают, они его перенапрявлют туда, куда надо.
        public void OpenProperties<TErElement>(ErSchema schema, TErElement element)
        {
            CloseProperties();
            ElementView<TErElement>? elementView = null;

            switch (element)
            {
                case ErRole role:
                    elementView = roleView as ElementView<TErElement>;
                    roleView.Open(schema, role);
                    break;
                case ErMapping mapping:
                    elementView = mappingView as ElementView<TErElement>;
                    mappingView.Open(mapping);
                    break;
            }

            if (elementView != null)
            {
                elementView.Schema = schema;
                elementView.Visible = true;
            }
        }
        public void CloseProperties()
        {
            roleView.CommitChanges();
            
            roleView.Visible = false;
            mappingView.Visible = false;
        }
    }

    public class ElementView<TErElement> : TableLayoutPanel
    {
        protected List<Tuple<Label, Control?>> rows = new();
        protected int rowHeight = 30;

        protected ErSchema? schema;
        protected TErElement? element;

        public ElementView()
        {
            ColumnCount = 2; // without panel, don't add this (uses default 2 columns) 
            RowCount = 1; // without panel, use 1. On panel this doesn't work
            CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            AutoSize = false;

            this.CellPaint += ElementView_CellPaint;

            AddRow("Название");
        }

        public virtual ErSchema? Schema
        {
            get { return schema; }
            set { schema = value; }
        }
        public virtual TErElement? Element
        {
            get { return element; }
            set { element = value; }
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
            textBox.Margin = new Padding(7, 7, 7, 0);
            textBox.BorderStyle = BorderStyle.None;
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
            RowStyles.Insert(rows.Count-1, row);

            return rows.Count - 1;
        }
        protected void AddEmptyRow()
        {
            Label label = new Label();
            label.Dock = DockStyle.Fill;
            Controls.Add(label);

            SetColumnSpan(label, 2);
            var row = new RowStyle(SizeType.AutoSize);
            RowStyles.Add(row);
        }

        private void ElementView_CellPaint(object? sender, TableLayoutCellPaintEventArgs e)
        {
            if (e.Column == 1 && e.Row != RowStyles.Count-1)
                using (SolidBrush brush = new SolidBrush(Color.White))
                    e.Graphics.FillRectangle(brush, e.CellBounds);
        }
    }
}
