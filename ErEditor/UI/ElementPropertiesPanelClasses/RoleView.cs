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

            AddRow("Множество сущностей", entitySetComboBox);

            AddEmptyRow();

            notificationParser = new(this);
        }

        protected override void LoadFromElement(ErSchema schema, ErRole role)
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
        protected override void SaveIntoElement(ErRole element)
        {
            element.Name = nameTextBox.Text;
            element.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.Empty;
        }

        protected override void UnsetHandlers()
        {
            nameTextBox.TextChanged -= NameTextBox_TextChanged;
            entitySetComboBox.SelectionChangeCommitted -= EntitySetComboBox_SelectionChangeCommitted;
        }
        protected override void SetHandlers()
        {
            nameTextBox.TextChanged += NameTextBox_TextChanged;
            entitySetComboBox.SelectionChangeCommitted += EntitySetComboBox_SelectionChangeCommitted;
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
            nameTextBox.TextChanged -= NameTextBox_TextChanged;
            nameTextBox.Text = notification.NewName;
            nameTextBox.TextChanged += NameTextBox_TextChanged;
        }
        public void Visit(ObjectUpdatedNotification<ErRole> notification)
        {
            LoadFromElement(schema, element);
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
