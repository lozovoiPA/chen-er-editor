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
                case ErRole es:
                    elementView = roleView as ElementView<TErElement>;
                    break;
                case ErMapping mapping:
                    elementView = mappingView as ElementView<TErElement>;
                    break;
            }

            if (elementView != null)
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

    public class RoleView : ElementView<ErRole>
    {
        // Used temporarily to draw a white border (no border)
        public class ComboBoxWithBorder : ComboBox
        {
            private System.Drawing.Color _borderColor = System.Drawing.Color.Black;
            private ButtonBorderStyle _borderStyle = ButtonBorderStyle.Solid;
            private static int WM_PAINT = 0x000F;

            public System.Drawing.Color BorderColor
            {
                get { return _borderColor; }
                set
                {
                    _borderColor = value;
                    Invalidate();
                }
            }
            public ButtonBorderStyle BorderStyle
            {
                get { return _borderStyle; }
                set
                {
                    _borderStyle = value;
                    Invalidate();
                }
            }

            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);

                if (m.Msg == WM_PAINT)
                {
                    Graphics g = Graphics.FromHwnd(Handle);
                    System.Drawing.Brush brush = new SolidBrush(BackColor);

                    Rectangle bounds = new Rectangle(0, 0, Width, Height);
                    ControlPaint.DrawBorder(g, bounds, _borderColor, _borderStyle);
                    brush.Dispose();
                }
                
            }
        }
        private ComboBoxWithBorder entitySetComboBox = new();
        public RoleView() : base()
        {
            entitySetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            //entitySetComboBox.Margin = new Padding(0);
            entitySetComboBox.AutoSize = false;
            entitySetComboBox.Height = rowHeight;
            entitySetComboBox.BackColor = SystemColors.Window;
            entitySetComboBox.BorderColor = SystemColors.Window;
            entitySetComboBox.Dock = DockStyle.Fill;

            // Used temporarily to make DropDownList style background white (setting BackColor doesn't work with this style)
            entitySetComboBox.DrawMode = DrawMode.OwnerDrawVariable;
            entitySetComboBox.DrawItem += EntitySetComboBox_DrawItem;
            AddRow("Множество сущностей", entitySetComboBox);

            AddEmptyRow();
        }

        // see above what it's used for
        private void EntitySetComboBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            System.Drawing.Brush brush = new SolidBrush(e.BackColor);
            System.Drawing.Brush tBrush = new SolidBrush(e.ForeColor);

            e.DrawBackground();
            if (e.Index >= 0)
            {
                g.FillRectangle(brush, new(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, rowHeight));
                e.Graphics.DrawString(entitySetComboBox.Items[e.Index].ToString(), e.Font,
                           tBrush, e.Bounds, StringFormat.GenericDefault);

            }
            brush.Dispose();
            tBrush.Dispose();
            e.DrawFocusRectangle();
        }

        private void EntitySetComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            if(element != null)
            {
                element.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.Empty;
            }
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

    public class MappingView : ElementView<ErMapping>
    {
        public class CardinalNumberUpDown : NumericUpDown
        {
            private TextBox innerTextBox;
            private Control innerButtons;

            private Button buttonUp = new();
            private Button buttonDown = new();

            private bool manyMode = false;

            public CardinalNumberUpDown()
            {
                this.Minimum = -1;
                this.BorderStyle = BorderStyle.None;
                this.Dock = DockStyle.Fill;
                this.Margin = new Padding(7, 7, 7, 0);

                foreach(Control c in Controls)
                {
                    switch (c)
                    {
                        case TextBox textBox:
                            innerTextBox = textBox;
                            break;
                        default:
                            innerButtons = c;
                            break;
                    }
                }
                innerButtons.Paint += InnerButtons_Paint;
                innerButtons.Size = new Size(0, 0);
                innerButtons.Enabled = false;

                innerTextBox.KeyPress += InnerTextBox_KeyPress;
            }

            public new string Value
            {
                get
                {
                    if (this.manyMode)
                    {
                        return "M";
                    }
                    return base.Value.ToString();
                }
                set
                {
                    if (Decimal.TryParse(value, out decimal result))
                    {
                        base.Value = result;
                    }
                    else
                    {
                        this.manyMode = true;
                        UpdateEditText();
                    }
                }
            }

            private void InnerTextBox_KeyPress(object? sender, KeyPressEventArgs e)
            {
                if(manyMode && char.IsDigit(e.KeyChar))
                {
                    manyMode = false;
                    innerTextBox.Text = "";
                }
                else if(!manyMode && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    manyMode = true;
                    e.Handled = true;
                    innerTextBox.Text = "";
                    UpdateEditText();
                }
            }
            protected override void UpdateEditText()
            {
                base.UpdateEditText();
                if (manyMode)
                {
                    this.innerTextBox.Text = "M";
                }
            }

            private void InnerButtons_Paint(object? sender, PaintEventArgs e)
            {
                var g = e.Graphics;
                int h = innerButtons.Height;
                int w = innerButtons.Width;

                g.Clear(BackColor);
            }
        }
        private CardinalNumberUpDown maxPreImageNumericUpDown = new();
        private CardinalNumberUpDown minPreImageNumericUpDown = new();
        private CardinalNumberUpDown maxImageNumericUpDown = new();
        private CardinalNumberUpDown minImageNumericUpDown = new();
        public MappingView()
        {
            var label = AddPropertyLabel("Прямое отображение");
            label.Dock = DockStyle.Fill;
            Controls.Add(label);
            RowStyles.Add(new(SizeType.Absolute, 30));
            SetColumnSpan(label, 2);
            

            AddRow("Макс. кардинальное\nчисло", maxImageNumericUpDown);
            AddRow("Мин. кардинальное\nчисло", minImageNumericUpDown);

            label = AddPropertyLabel("Обратное отображение");
            label.Dock = DockStyle.Fill;
            Controls.Add(label);
            RowStyles.Add(new(SizeType.Absolute, 30));
            SetColumnSpan(label, 2);

            AddRow("Макс. кардинальное\nчисло", maxPreImageNumericUpDown);
            AddRow("Мин. кардинальное\nчисло", minPreImageNumericUpDown);

            AddEmptyRow();
        }
    }
}
