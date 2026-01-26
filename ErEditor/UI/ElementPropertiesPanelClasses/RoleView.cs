using ErEditor.ErSchemaClasses;

namespace ErEditor.UI.ElementPropertiesPanelClasses
{
    public class RoleView : ElementPropertiesPanel.ElementView<ErRole>
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

        public void Open(ErSchema schema, ErRole role)
        {
            this.schema = null;
            this.element = null;
            entitySetComboBox.DataSource = schema.EntitySets;
            entitySetComboBox.DisplayMember = "Name";
            entitySetComboBox.SelectedItem = role.EntitySet;
            this.element = role;
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
            if (element != null)
            {
                element.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.Empty;
            }
        }
        public void CommitChanges()
        {
            if (element != null)
            {
                element.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.Empty;
            }
        }
    }
}
