using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI
{
    public class ComboBoxWithBorder : ComboBox
    {
        private System.Drawing.Color _borderColor = System.Drawing.Color.Black;
        private ButtonBorderStyle _borderStyle = ButtonBorderStyle.Solid;
        private static int WM_PAINT = 0x000F;

        public ComboBoxWithBorder()
        {
            // Used temporarily to make DropDownList style background white (setting BackColor doesn't work with this style)
            DrawMode = DrawMode.OwnerDrawFixed;
            DrawItem += ComboBoxWithBorder_DrawItem;
        }

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

        private void ComboBoxWithBorder_DrawItem(object? sender, DrawItemEventArgs e)
        {
            Graphics g = e.Graphics;
            System.Drawing.Brush brush = new SolidBrush(e.BackColor);
            System.Drawing.Brush tBrush = new SolidBrush(e.ForeColor);

            e.DrawBackground();
            if (e.Index >= 0)
            {
                g.FillRectangle(brush, new(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height));
                e.Graphics.DrawString(Items[e.Index].ToString(), e.Font, tBrush, e.Bounds, StringFormat.GenericDefault);
            }
            brush.Dispose();
            tBrush.Dispose();
            e.DrawFocusRectangle();
        }
    }
}
