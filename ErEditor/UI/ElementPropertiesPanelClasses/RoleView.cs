using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System.Data;

namespace ErEditor.UI.ElementPropertiesPanelClasses
{
    public class RoleView : 
        ElementPropertiesPanel.ElementView<ErRole>,
        IObserver,
        IVisitor<ObjectNameChangedNotification>,
        IVisitor<ObjectUpdatedNotification<ErRole>>,
        IVisitor<ObjectCreatedNotification<ErEntitySet>>,
        IVisitor<ObjectDeletedNotification<ErEntitySet>>
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

        private ObserverBase notificationParser;
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

            notificationParser = new(this);
        }

        private void LoadFrom(ErSchema schema, ErRole role)
        {
            UnsetHandlers();
            nameTextBox.Text = role.Name;

            entitySetComboBox.DataSource = null;
            entitySetComboBox.DataSource = schema.EntitySets;
            entitySetComboBox.DisplayMember = "Name";
            entitySetComboBox.SelectedItem = role.EntitySet;

            this.Refresh();
            SetHandlers();
        }

        public void Open(ErSchema schema, ErRole role)
        {
            if(role != element)
            {
                CloseAndDiscard();

                LoadFrom(schema, role);

                this.element = role;
                this.schema = schema;
                role.Subscribe(this);
                schema.Subscribe(this);
                SetHandlers();
            }
            else
            {
                LoadFrom(schema, role);
            }
        }

        private void UnsetHandlers()
        {
            nameTextBox.TextChanged -= NameTextBox_TextChanged;
            entitySetComboBox.SelectionChangeCommitted -= EntitySetComboBox_SelectionChangeCommitted;
        }
        private void SetHandlers()
        {
            nameTextBox.TextChanged += NameTextBox_TextChanged;
            entitySetComboBox.SelectionChangeCommitted += EntitySetComboBox_SelectionChangeCommitted;
        }

        public override void CloseAndSave()
        {
            if (element != null)
            {
                ErRole role = element;
                element.Unsubscribe(this);
                schema.Unsubscribe(this);
                this.element = null;
                this.schema = null;
                UnsetHandlers();

                role.Name = role.Name;
                role.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.Empty;
            }
        }
        public override void CloseAndDiscard()
        {
            if (element != null)
            {
                element.Unsubscribe(this);
                schema.Unsubscribe(this);
                this.element = null;
                this.schema = null;
                UnsetHandlers();
            }
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
        private void NameTextBox_TextChanged(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.Name = nameTextBox.Text;
            element.Subscribe(this);
        }
        private void EntitySetComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.Empty;
            element.Subscribe(this);
        }

        public void Recieve(Notification notification)
        {
            notificationParser.Recieve(notification);
        }
        public void Visit(ObjectNameChangedNotification notification)
        {
            nameTextBox.Text = notification.NewName;
        }
        public void Visit(ObjectUpdatedNotification<ErRole> notification)
        {
            LoadFrom(schema, element);
        }
        public void Visit(ObjectCreatedNotification<ErEntitySet> notification)
        {
            ConsoleLog.Log("I received a message es was crewated");
            entitySetComboBox.DataSource = schema.EntitySets;
            entitySetComboBox.DisplayMember = "Name";
            entitySetComboBox.SelectedItem = element.EntitySet;
        }
        public void Visit(ObjectDeletedNotification<ErEntitySet> notification)
        {
            ConsoleLog.Log("I received a message es was delwed");
            if (notification.Object == element.EntitySet)
            {
                element.Unsubscribe(this);
                element.EntitySet = ErEntitySet.Empty;
                element.Subscribe(this);
            }
            entitySetComboBox.DataSource = schema.EntitySets;
            entitySetComboBox.DisplayMember = "Name";
            entitySetComboBox.SelectedItem = element.EntitySet;
        }
    }
}
