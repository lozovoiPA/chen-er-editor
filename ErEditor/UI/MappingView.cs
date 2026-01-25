using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace ErEditor.UI
{
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

                foreach (Control c in Controls)
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

            public new int Value
            {
                get
                {
                    if (this.manyMode)
                    {
                        return -1;
                    }
                    return (int)base.Value;
                }
                set
                {
                    if (value == -1)
                    {
                        this.manyMode = true;
                        UpdateEditText();
                    }
                    else
                    {
                        this.manyMode = false;
                        base.Value = value;
                        UpdateEditText();
                    }
                }
            }

            private void InnerTextBox_KeyPress(object? sender, KeyPressEventArgs e)
            {
                if (manyMode && char.IsDigit(e.KeyChar))
                {
                    manyMode = false;
                    innerTextBox.Text = "";
                }
                else if (!manyMode && !char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
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
                    base.Value = -1;
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

            maxPreImageNumericUpDown.ValueChanged += MaxPreImageNumericUpDown_ValueChanged;
            maxImageNumericUpDown.ValueChanged += MaxImageNumericUpDown_ValueChanged;
            minImageNumericUpDown.ValueChanged += MinImageNumericUpDown_ValueChanged;
            minPreImageNumericUpDown.ValueChanged += MinPreImageNumericUpDown_ValueChanged;
        }

        public void Open(ErMapping mapping)
        {
            this.element = null;
            this.maxImageNumericUpDown.Value = mapping.MaxCardinalityOfImage;
            this.maxPreImageNumericUpDown.Value = mapping.MaxCardinalityOfPreimage;
            this.minPreImageNumericUpDown.Value = mapping.MinCardinalityOfPreimage;
            this.minImageNumericUpDown.Value = mapping.MinCardinalityOfImage;
            this.element = mapping;
        }

        private void MaxImageNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (element != null)
            {
                element.MaxCardinalityOfImage = maxImageNumericUpDown.Value;
                ConsoleLog.Log($"Max Image: {element.MaxCardinalityOfImage}");
            }
        }
        private void MaxPreImageNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (element != null)
            {
                element.MaxCardinalityOfPreimage = maxPreImageNumericUpDown.Value;
                ConsoleLog.Log($"Max PreImage: {element.MaxCardinalityOfPreimage}");
            }
        }
        private void MinPreImageNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (element != null)
            {
                element.MinCardinalityOfPreimage = minPreImageNumericUpDown.Value;
                ConsoleLog.Log($"Min PreImage: {element.MinCardinalityOfPreimage}");
            }
        }
        private void MinImageNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (element != null)
            {
                element.MinCardinalityOfImage = minImageNumericUpDown.Value;
                ConsoleLog.Log($"Min Image: {element.MinCardinalityOfImage}");
            }
        }
    }
}
