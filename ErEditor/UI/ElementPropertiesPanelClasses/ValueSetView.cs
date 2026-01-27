using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.ElementPropertiesPanelClasses
{
    public class ValueSetView :
        ElementPropertiesPanel.ElementView<ErValueSet>,
        IObserver,
        IVisitor<ObjectNameChangedNotification>,
        IVisitor<ObjectUpdatedNotification<ErValueSet>>
    {
        private ComboBoxWithBorder baseValueTypeComboBox = new();


        public class BaseValueType
        {
            private readonly string name;
            private readonly string baseType;

            public BaseValueType(string name, string baseType)
            {
                this.name = name;
                this.baseType = baseType;
            }

            public string TypeName
            {
                get
                {
                    return name;
                }
                set
                {
                    TypeName = value;
                }
            }
            public string BaseType
            {
                get
                {
                    return baseType;
                }
                set
                {
                    BaseType = value;
                }
            }
            
            public override string ToString()
            {
                return this.TypeName;
            }
        }

        private List<BaseValueType> types = new();
        public ValueSetView() : base()
        {
            baseValueTypeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            baseValueTypeComboBox.AutoSize = false;
            baseValueTypeComboBox.Height = rowHeight;
            baseValueTypeComboBox.BackColor = SystemColors.Window;
            baseValueTypeComboBox.BorderColor = SystemColors.Window;
            baseValueTypeComboBox.Dock = DockStyle.Fill;

            
            types.Add(new BaseValueType("Целые числа", "int"));
            types.Add(new BaseValueType("Вещественные числа", "float"));
            types.Add(new BaseValueType("Текстовое значение", "text"));
            types.Add(new BaseValueType("Булево значение", "bool"));
            foreach (var type in types)
            {
                ConsoleLog.Log($"{type.BaseType}, {type.TypeName}");
            }

            baseValueTypeComboBox.DataSource = Types;
            baseValueTypeComboBox.DisplayMember = nameof(BaseValueType.TypeName); // Очень странно, но не работает без перегрузки ToString()
            baseValueTypeComboBox.ValueMember = "BaseType";

            AddRow("Базовый тип", baseValueTypeComboBox);
            AddEmptyRow();

            notificationParser = new(this);
        }

        public ReadOnlyCollection<BaseValueType> Types
        {
            get { return types.AsReadOnly(); }
        }

        protected override void LoadFromElement(ErSchema schema, ErValueSet valueSet)
        {
            UnsetHandlers();
            nameTextBox.Text = valueSet.Name;
            baseValueTypeComboBox.SelectedValue = valueSet.BaseValueType;
            SetHandlers();
        }
        protected override void SaveIntoElement(ErValueSet valueSet)
        {
            valueSet.Name = nameTextBox.Text;
            valueSet.BaseValueType = (string)baseValueTypeComboBox.SelectedValue;
        }
        
        protected override void UnsetHandlers()
        {
            nameTextBox.TextChanged -= NameTextBox_TextChanged;
            baseValueTypeComboBox.SelectionChangeCommitted -= BaseValueTypeComboBox_SelectionChangeCommitted;
        }
        protected override void SetHandlers()
        {
            nameTextBox.TextChanged += NameTextBox_TextChanged;
            baseValueTypeComboBox.SelectionChangeCommitted += BaseValueTypeComboBox_SelectionChangeCommitted;
        }

        private void NameTextBox_TextChanged(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.Name = nameTextBox.Text;
            element.Subscribe(this);
        }
        private void BaseValueTypeComboBox_SelectionChangeCommitted(object? sender, EventArgs e)
        {
            ConsoleLog.Log($"{baseValueTypeComboBox.SelectedValue}");
            element.Unsubscribe(this);
            element.BaseValueType = (string)baseValueTypeComboBox.SelectedValue;
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
        public void Visit(ObjectUpdatedNotification<ErValueSet> notification)
        {
            LoadFromElement(schema, element);
        }
    }
}
