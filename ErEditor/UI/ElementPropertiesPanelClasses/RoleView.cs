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
            nameTextBox.Text = role.Name;

            entitySetComboBox.DataSource = null;
            entitySetComboBox.DataSource = schema.EntitySets;
            entitySetComboBox.DisplayMember = "Name";
            if(role.EntitySet != ErEntitySet.None && schema.EntitySets.Count > 0)
            {
                entitySetComboBox.SelectedItem = role.EntitySet;
            }
            else
            {
                entitySetComboBox.SelectedItem = null;
            }
            wereChangesMade = false;
        }
        protected override void SaveIntoElement(ErRole element)
        {
            if (wereChangesMade)
            {
                element.Name = nameTextBox.Text;
                element.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.None;
            }
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
            wereChangesMade = true;
            element.Subscribe(this);
        }
        private void EntitySetComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.EntitySet = entitySetComboBox.SelectedValue as ErEntitySet ?? ErEntitySet.None;
            wereChangesMade = true;
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
            UnsetHandlers();
            LoadFromElement(schema, element);
            SetHandlers();
        }
        public void Visit(ObjectCreatedNotification<ErEntitySet> notification)
        {
            entitySetComboBox.SelectionChangeCommitted -= EntitySetComboBox_SelectionChangeCommitted;
            entitySetComboBox.DataSource = schema.EntitySets;
            entitySetComboBox.DisplayMember = "Name";
            if (element.EntitySet != ErEntitySet.None)
            {
                entitySetComboBox.SelectedItem = element.EntitySet;
            }
            else
            {
                entitySetComboBox.SelectedItem = null;
            }
            entitySetComboBox.SelectionChangeCommitted += EntitySetComboBox_SelectionChangeCommitted;
        }
        public void Visit(ObjectDeletedNotification<ErEntitySet> notification)
        {
            entitySetComboBox.SelectionChangeCommitted -= EntitySetComboBox_SelectionChangeCommitted;
            if (notification.Object == element.EntitySet)
            {
                element.Unsubscribe(this);
                element.EntitySet = ErEntitySet.None;
                element.Subscribe(this);
            }
            entitySetComboBox.DataSource = schema.EntitySets;
            entitySetComboBox.DisplayMember = "Name";
            entitySetComboBox.SelectedItem = element.EntitySet;
            entitySetComboBox.SelectionChangeCommitted += EntitySetComboBox_SelectionChangeCommitted;
        }
    }
}
