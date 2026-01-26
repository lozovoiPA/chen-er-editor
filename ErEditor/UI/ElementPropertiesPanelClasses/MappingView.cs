using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.ElementPropertiesPanelClasses
{
    public partial class MappingView : 
        ElementPropertiesPanel.ElementView<ErMapping>, 
        IObserver,
        IVisitor<ObjectNameChangedNotification>,
        IVisitor<ObjectUpdatedNotification<ErMapping>>
    {
        private Label preImageLabel = new();
        private Label imageLabel = new();
        private CardinalNumberUpDown maxPreImageNumericUpDown = new();
        private CardinalNumberUpDown minPreImageNumericUpDown = new();
        private CardinalNumberUpDown maxImageNumericUpDown = new();
        private CardinalNumberUpDown minImageNumericUpDown = new();

        private ObserverBase notificationParser;
        public MappingView()
        {
            AddRow("Область\nопределения", preImageLabel = GetCenteredPropertyLabel(""));
            AddRow("Область\nзначений", imageLabel = GetCenteredPropertyLabel(""));

            var label = GetCenteredPropertyLabel("Прямое отображение");
            label.Font = new(label.Font, FontStyle.Bold);
            Controls.Add(label);
            RowStyles.Add(new(SizeType.Absolute, 30));
            SetColumnSpan(label, 2);

            AddRow("Макс. кардинальное\nчисло", maxImageNumericUpDown);
            AddRow("Мин. кардинальное\nчисло", minImageNumericUpDown);

            label = GetCenteredPropertyLabel("Обратное отображение");
            label.Font = new(label.Font, FontStyle.Bold);
            Controls.Add(label);
            RowStyles.Add(new(SizeType.Absolute, 30));
            SetColumnSpan(label, 2);

            AddRow("Макс. кардинальное\nчисло", maxPreImageNumericUpDown);
            AddRow("Мин. кардинальное\nчисло", minPreImageNumericUpDown);

            AddEmptyRow();

            notificationParser = new(this);

            this.CellPaint -= ElementView_CellPaint;
            this.CellPaint += MappingView_CellPaint;
        }

        private void MappingView_CellPaint(object? sender, TableLayoutCellPaintEventArgs e)
        {
            if ((e.Row < 1 || e.Row > 3) && e.Row != 6
                && e.Column == 1 
                && e.Row != RowStyles.Count - 1
                )
                using (SolidBrush brush = new SolidBrush(Color.White))
                    e.Graphics.FillRectangle(brush, e.CellBounds);
        }

        private void UnsetHandlers()
        {
            nameTextBox.TextChanged -= NameTextBox_TextChanged;

            maxPreImageNumericUpDown.ValueChanged -= MaxPreImageNumericUpDown_ValueChanged;
            maxImageNumericUpDown.ValueChanged -= MaxImageNumericUpDown_ValueChanged;
            minImageNumericUpDown.ValueChanged -= MinImageNumericUpDown_ValueChanged;
            minPreImageNumericUpDown.ValueChanged -= MinPreImageNumericUpDown_ValueChanged;
        }
        private void SetHandlers()
        {
            nameTextBox.TextChanged += NameTextBox_TextChanged;

            maxPreImageNumericUpDown.ValueChanged += MaxPreImageNumericUpDown_ValueChanged;
            maxImageNumericUpDown.ValueChanged += MaxImageNumericUpDown_ValueChanged;
            minImageNumericUpDown.ValueChanged += MinImageNumericUpDown_ValueChanged;
            minPreImageNumericUpDown.ValueChanged += MinPreImageNumericUpDown_ValueChanged;
        }
        private void SetNormalNameStyle()
        {
            this.nameTextBox.Font = new(nameTextBox.Font, FontStyle.Regular);
            this.nameTextBox.ForeColor = Color.Black;
        }
        private void SetDefaultNameStyle()
        {
            this.nameTextBox.Font = new(nameTextBox.Font, FontStyle.Italic);
            this.nameTextBox.ForeColor = Color.Gray;
        }

        public void Open(ErMapping mapping)
        {
            if (element != null)
            {
                element.Unsubscribe(this);
                UnsetHandlers();
                this.element = null;
            }

            // set properties
            this.nameTextBox.PlaceholderText = mapping.DefaultName;
            this.nameTextBox.Text = mapping.Name;
            preImageLabel.Text = mapping.GetPreImageName();
            imageLabel.Text = mapping.GetImageName();
            if (mapping.Name == "")
            {
                SetDefaultNameStyle();
            }
            else
            {
                SetNormalNameStyle();
            }
            this.maxImageNumericUpDown.Value = mapping.MaxCardinalityOfImage;
            this.maxPreImageNumericUpDown.Value = mapping.MaxCardinalityOfPreimage;
            this.minPreImageNumericUpDown.Value = mapping.MinCardinalityOfPreimage;
            this.minImageNumericUpDown.Value = mapping.MinCardinalityOfImage;

            this.element = mapping;
            SetHandlers();
            mapping.Subscribe(this);
        }

        private void NameTextBox_TextChanged(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            if(nameTextBox.Text == element.DefaultName || nameTextBox.Text == "")
            {
                element.Name = "";
                nameTextBox.TextChanged -= NameTextBox_TextChanged;
                this.nameTextBox.PlaceholderText = element.DefaultName;
                this.nameTextBox.Text = "";
                SetDefaultNameStyle();
                nameTextBox.TextChanged += NameTextBox_TextChanged;
            }
            else
            {
                element.Name = nameTextBox.Text;
                SetNormalNameStyle();
            }
            element.Subscribe(this);
        }
        private void MaxImageNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.MaxCardinalityOfImage = maxImageNumericUpDown.Value;
            element.Subscribe(this);
        }
        private void MaxPreImageNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.MaxCardinalityOfPreimage = maxPreImageNumericUpDown.Value;
            element.Subscribe(this);
        }
        private void MinPreImageNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.MinCardinalityOfPreimage = minPreImageNumericUpDown.Value;
            element.Subscribe(this);
        }
        private void MinImageNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            element.Unsubscribe(this);
            element.MinCardinalityOfImage = minImageNumericUpDown.Value;
            element.Subscribe(this);
        }

        public void Recieve(Notification notification)
        {
            notificationParser.Recieve(notification);
        }

        public void Visit(ObjectNameChangedNotification notification)
        {
            this.Open(element);   
        }
        public void Visit(ObjectUpdatedNotification<ErMapping> notification)
        {
            this.Open(element);   
        }
    }
}
