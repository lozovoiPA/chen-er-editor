using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.ElementPropertiesPanelClasses
{
    partial class MappingView
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
    }
}
