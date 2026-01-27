using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.ElementPropertiesPanelClasses
{
    partial class ElementPropertiesPanel
    {
        public abstract class ElementView : TableLayoutPanel
        {
            protected ErSchema? schema;
            protected abstract void UnsetHandlers();
            protected abstract void SetHandlers();
            public abstract void CloseAndSave();
            public abstract void CloseAndDiscard();
        }

        public abstract class ElementView<TErElement> : 
            ElementView,
            IObserver

            where TErElement : class, IObservable
        {
            protected List<Tuple<Label?, Control?>> rows = new();
            protected int rowHeight = 30;
            protected TextBox nameTextBox;

            protected TErElement? element;
            protected ObserverBase notificationParser;

            public ElementView()
            {
                ColumnCount = 2; // without panel, don't add this (uses default 2 columns) 
                RowCount = 1; // without panel, use 1. On panel this doesn't work
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
                AutoSize = false;
                this.DoubleBuffered = true;

                this.CellPaint += ElementView_CellPaint;

                int index = AddRowWithTextBox("Название");
                nameTextBox = (rows[index].Item2 as TextBox)!;

                notificationParser = new(this);

                Visible = false;
                Dock = DockStyle.Fill;
            }

            protected abstract void LoadFromElement(ErSchema schema, TErElement element);
            protected abstract void SaveIntoElement(TErElement element);

            public virtual void Open(ErSchema schema, TErElement tElement)
            {
                if (tElement != element)
                {
                    CloseAndDiscard();

                    LoadFromElement(schema, tElement);

                    this.element = tElement;
                    this.schema = schema;
                    tElement.Subscribe(this);
                    schema.Subscribe(this);
                    SetHandlers();
                }
                else
                {
                    LoadFromElement(schema, tElement);
                }
            }
            public override void CloseAndSave()
            {
                if(element != null)
                {
                    TErElement tElement = CloseElement(); 
                    SaveIntoElement(tElement);
                }
            }
            public override void CloseAndDiscard()
            {
                if (element != null)
                {
                    CloseElement();
                }
            }
            protected virtual TErElement? CloseElement()
            {
                TErElement? tElement = element;
                element.Unsubscribe(this);
                schema.Unsubscribe(this);
                this.element = null;
                this.schema = null;
                UnsetHandlers();
                return tElement;
            }

            protected Label GetCenteredPropertyLabel(string propertyName)
            {
                Label label = new Label();
                // Without this it will automatically multine if it doesn't fit IN THE INITIAL CELL SIZE and won't become
                // one-line even if you resize it to have enough space
                label.AutoEllipsis = true;
                label.AutoSize = false;
                label.Dock = DockStyle.Fill;
                label.Text = propertyName;
                label.Padding = new Padding(0);
                label.Size = new Size(label.PreferredWidth, label.PreferredHeight);
                label.TextAlign = ContentAlignment.MiddleLeft;

                return label;
            }
            protected int AddRow(Control control)
            {
                Controls.Add(control);

                rows.Add(new(null, control));
                var row = new RowStyle(SizeType.Absolute, rowHeight);
                RowStyles.Insert(rows.Count-1, row);
                
                SetColumnSpan(control, 2);
                return rows.Count - 1;
            }
            protected int AddRowWithTextBox(string propertyName)
            {
                // the correct way to do this is
                // First add controls, then add fresh RowStyle (do not modify an existing one even if using GetRow!) and then modify the row itself.
                var label = GetCenteredPropertyLabel(propertyName);

                TextBox textBox = new TextBox();
                textBox.Margin = new Padding(7, 7, 7, 0);
                textBox.BorderStyle = BorderStyle.None;
                textBox.Dock = DockStyle.Fill;

                rows.Add(new(label, textBox));
                Controls.AddRange([label, textBox]);

                var row = new RowStyle(SizeType.Absolute, rowHeight);
                RowStyles.Insert(rows.Count - 1, row);

                return rows.Count - 1;
            }
            protected int AddRow(string propertyName, Control control)
            {
                var label = GetCenteredPropertyLabel(propertyName);

                rows.Add(new(label, control));
                Controls.AddRange([label, control]);

                var row = new RowStyle(SizeType.Absolute, rowHeight);
                RowStyles.Insert(rows.Count - 1, row);

                return rows.Count - 1;
            }
            protected void AddEmptyRow()
            {
                Label label = new Label();
                label.Dock = DockStyle.Fill;
                Controls.Add(label);

                SetColumnSpan(label, 2);
                var row = new RowStyle(SizeType.AutoSize);
                RowStyles.Add(row);
            }

            protected void ElementView_CellPaint(object? sender, TableLayoutCellPaintEventArgs e)
            {
                if (e.Column == 1 && e.Row != RowStyles.Count - 1)
                    using (SolidBrush brush = new SolidBrush(Color.White))
                        e.Graphics.FillRectangle(brush, e.CellBounds);
            }

            public virtual void Recieve(Notification notification)
            {
                notificationParser.Recieve(notification);
            }
        }
    }
}
