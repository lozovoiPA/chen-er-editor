using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.ElementPropertiesPanelClasses
{
    public class AttributeView :
        ElementPropertiesPanel.ElementView<ErAttribute>,
        IVisitor<ObjectNameChangedNotification>,
        IVisitor<ObjectUpdatedNotification<ErAttribute>>,
        IVisitor<ObjectCreatedNotification<ErValueSet>>,
        IVisitor<ObjectDeletedNotification<ErValueSet>>
    {
        private ComboBoxWithBorder valueSetComboBox = new();

        public AttributeView() : base()
        {
            valueSetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            valueSetComboBox.AutoSize = false;
            valueSetComboBox.Height = rowHeight;
            valueSetComboBox.BackColor = SystemColors.Window;
            valueSetComboBox.BorderColor = SystemColors.Window;
            valueSetComboBox.Dock = DockStyle.Fill;

            AddRow("Множество значений", valueSetComboBox);

            AddEmptyRow();
        }

        protected override void LoadFromElement(ErSchema schema, ErAttribute attribute)
        {
            nameTextBox.Text = attribute.Name;

            valueSetComboBox.DataSource = null;
            valueSetComboBox.DataSource = schema.ValueSets;
            valueSetComboBox.DisplayMember = "Name";
            if (attribute.valueSets.Count > 0 && schema.ValueSets.Count > 0)
            {
                valueSetComboBox.SelectedItem = attribute.valueSets[0];
            }
            else
            {
                valueSetComboBox.SelectedItem = null;
            }
            wereChangesMade = false;
        }
        protected override void SaveIntoElement(ErAttribute element)
        {
            if (wereChangesMade)
            {
                element.Name = nameTextBox.Text;
                ErValueSet? valueSet = valueSetComboBox.SelectedValue as ErValueSet;
                if(valueSet != null)
                {
                    element.AddValueSet(valueSet);
                }
                else
                {
                    element.RemoveFirstValueSet();
                }
            }
        }

        protected override void UnsetHandlers()
        {
            nameTextBox.TextChanged -= NameTextBox_TextChanged;
            valueSetComboBox.SelectionChangeCommitted -= ValueSetComboBox_SelectionChangeCommitted;
        }
        protected override void SetHandlers()
        {
            nameTextBox.TextChanged += NameTextBox_TextChanged;
            valueSetComboBox.SelectionChangeCommitted += ValueSetComboBox_SelectionChangeCommitted;
        }
        private void NameTextBox_TextChanged(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.Name = nameTextBox.Text;
            wereChangesMade = true;
            element.Subscribe(this);
        }
        private void ValueSetComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            ErValueSet? valueSet = valueSetComboBox.SelectedValue as ErValueSet;
            if (valueSet != null)
            {
                element.AddValueSet(valueSet);
            }
            else
            {
                element.RemoveFirstValueSet();
            }
            wereChangesMade = true; // this is not necessary as the changes are recorded immediately anyway. Either don't record them like this or remove saving changes when the panel closes
            element.Subscribe(this);
        }

        public void Visit(ObjectNameChangedNotification notification)
        {
            nameTextBox.TextChanged -= NameTextBox_TextChanged;
            nameTextBox.Text = notification.NewName;
            nameTextBox.TextChanged += NameTextBox_TextChanged;
        }
        public void Visit(ObjectUpdatedNotification<ErAttribute> notification)
        {
            UnsetHandlers();
            LoadFromElement(schema, element);
            SetHandlers();
        }
        public void Visit(ObjectCreatedNotification<ErValueSet> notification)
        {
            valueSetComboBox.SelectionChangeCommitted -= ValueSetComboBox_SelectionChangeCommitted;
            valueSetComboBox.DataSource = schema.ValueSets;
            valueSetComboBox.DisplayMember = "Name";
            if (element.valueSets.Count > 0 && schema.ValueSets.Count > 0)
            {
                valueSetComboBox.SelectedItem = element.valueSets[0];
            }
            else
            {
                valueSetComboBox.SelectedItem = null;
            }
            valueSetComboBox.SelectionChangeCommitted += ValueSetComboBox_SelectionChangeCommitted;
        }
        public void Visit(ObjectDeletedNotification<ErValueSet> notification)
        {
            valueSetComboBox.SelectionChangeCommitted -= ValueSetComboBox_SelectionChangeCommitted;
            if (element.valueSets.Contains(notification.Object))
            {
                element.Unsubscribe(this);
                element.valueSets.Remove(notification.Object);
                element.Subscribe(this);
            }
            valueSetComboBox.DataSource = schema.ValueSets;
            valueSetComboBox.DisplayMember = "Name";
            if (element.valueSets.Count > 0 && schema.ValueSets.Count > 0)
            {
                valueSetComboBox.SelectedItem = element.valueSets[0];
            }
            else
            {
                valueSetComboBox.SelectedItem = null;
            }
            valueSetComboBox.SelectionChangeCommitted += ValueSetComboBox_SelectionChangeCommitted;
        }
    }
}
