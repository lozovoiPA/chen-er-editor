using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ErEditor.UI
{
    public class DiagramPanel : Panel
    {
        ErDiagram? diagram;

        TextBox renameTextBox = new();
        DiagramPrimitive? active_pr;

        Rectangle active_at = Rectangle.Empty;
        bool edge_draw = false;
        bool hold = false;

        private ContextMenuStrip drawingContextMenu = new();
        private ContextMenuStrip primitiveContextMenu = new();

        public ErDiagram? Diagram
        {
            get
            {
                return diagram;
            }
            set
            {
                diagram = value;
                this.Invalidate();
            }
        }

        public DiagramPanel()
        {
            ConsoleLog.Log("Started constructor", this, "INFO");
            Console.WriteLine(this);
            Initialize();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (diagram != null)
            {
                if (edge_draw) // links are drawn under the figures
                {
                    Console.WriteLine(active_at);
                    e.Graphics.DrawLine(new Pen(Color.Black), active_at.X, active_at.Y, active_at.Width, active_at.Height);
                }
                diagram.Draw(e.Graphics);
            }
        }

        private void Initialize()
        {
            ConsoleLog.Log("Started Initialize()", this, "INFO");
            InitializeDrawingContextMenu();
            InitializePrimitiveContextMenu();

            this.MouseDown += new MouseEventHandler(DiagramPanel_MouseDown);
            this.MouseUp += new MouseEventHandler(DiagramPanel_MouseUp);
            this.MouseDoubleClick += new MouseEventHandler(DiagramPanel_MouseDoubleClick);
            this.MouseMove += new MouseEventHandler(DiagramPanel_MouseMove);

            renameTextBox.Width = 100;
            renameTextBox.Height = 30;
            renameTextBox.Text = String.Empty;
            renameTextBox.Visible = false;
            renameTextBox.KeyDown += new KeyEventHandler(EndRename);
            Controls.Add(renameTextBox);
        }
        private void InitializeDrawingContextMenu()
        {
            ConsoleLog.Log("Started InitializeDrawingContextMenu()", this, "INFO");
            ToolStripMenuItem createPrimitiveDropdown = new();
            createPrimitiveDropdown.Text = "Создать...";

            ToolStripMenuItem createEntitySetItem = new();
            createEntitySetItem.Text = "Множество сущностей";
            createEntitySetItem.Click += CreateEntitySet;
            ToolStripMenuItem createRelationshipSetItem = new();
            createRelationshipSetItem.Text = "Множество связей";
            createRelationshipSetItem.Click += CreateRelationshipSet;

            createPrimitiveDropdown.DropDownItems.AddRange([
                createEntitySetItem, 
                createRelationshipSetItem
                ]);
            drawingContextMenu.Items.Add(createPrimitiveDropdown);
        }
        private void InitializePrimitiveContextMenu()
        {
            ToolStripMenuItem createLinkItem = new();
            createLinkItem.Text = "Создать связь...";
            ToolStripMenuItem deletePrimitiveOnlyItem = new();
            deletePrimitiveOnlyItem.Text = "Убрать элемент";
            ToolStripMenuItem deleteErElementItem = new();
            deleteErElementItem.Text = "Удалить элемент";

            primitiveContextMenu.Items.AddRange([
                createLinkItem,
                deletePrimitiveOnlyItem,
                deleteErElementItem
                ]);
        }

        private void CreateEntitySet(object? sender, EventArgs e)
        {
            ConsoleLog.Log("New entity set creation (diagram) handler was called", this, "INFO");
            Point point = PointToClient(new Point(drawingContextMenu.Bounds.X, drawingContextMenu.Bounds.Y));
            
            DiagramRectangle pr = diagram!.AddRectangle(point.X, point.Y);
            active_pr = pr;
            Invalidate();
            RenamePrimitive(pr);
        }
        private void CreateRelationshipSet(object? sender, EventArgs e)
        {
            ConsoleLog.Log("New relationship set creation (diagram) handler was called", this, "INFO");
            Point point = PointToClient(new Point(drawingContextMenu.Bounds.X, drawingContextMenu.Bounds.Y));
            
            DiagramRhombus pr = diagram!.AddRhombus(point.X, point.Y);
            active_pr = pr;
            Invalidate();
            RenamePrimitive(pr);
        }

        private void DiagramPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                if (renameTextBox.Visible)
                {
                    this.EndRenameActivity();
                }
                hold = false;
                DiagramPrimitive? pr = diagram.FindAt(e.X, e.Y);
                Console.WriteLine("\n" + e.X + " " + e.Y);
                if (e.Button == MouseButtons.Right)
                {
                    if (pr == null)
                    {
                        drawingContextMenu.Show(this, e.X, e.Y);
                    }
                    else
                    {
                        active_pr = pr;
                        primitiveContextMenu.Show(this, e.X, e.Y);
                    }
                }
                else if (e.Button == MouseButtons.Left)
                {
                    if (pr != null)
                    {
                        if (!edge_draw) // we are not currently drawing a link so we can start holding
                        {
                            hold = true;
                            active_pr = pr;
                            active_at = new Rectangle(pr.X, pr.Y, e.X - pr.X, e.Y - pr.Y);
                        }
                        else
                        {
                            //diagram.AddEdge(active_pr, pr, new Point(active_at.X, active_at.Y), new Point(active_at.Width, active_at.Height));
                            edge_draw = false;
                            Invalidate();
                        }
                    }
                    else if (edge_draw)
                    {
                        edge_draw = false;
                        Invalidate();
                    }
                }
            }
        }
        private void DiagramPanel_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                Point point = new Point(e.X, e.Y);
                Console.WriteLine(point);
                DiagramPrimitive? pr = diagram.FindAt(point.X, point.Y);
                if (pr != null)
                {
                    active_pr = pr;
                    this.RenamePrimitive(pr);
                }
            }
        }
        private void DiagramPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                if (hold && active_pr != null)
                {
                    active_pr.X = e.X - active_at.Width;
                    active_pr.Y = e.Y - active_at.Height;

                    Invalidate();
                }
                else if (edge_draw)
                {
                    active_at.Width = e.X;
                    active_at.Height = e.Y;

                    Invalidate();
                }
            }
        }
        private void DiagramPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                hold = false;
            }
        }

        private void RenamePrimitive(DiagramPrimitive pr)
        {
            hold = false;
            renameTextBox.Left = (int)(pr.X + pr.width * 0.5);
            renameTextBox.Top = (int)(pr.Y + pr.height * 0.5);
            renameTextBox.Text = pr.Label;

            renameTextBox.Visible = true;
            renameTextBox.Focus();

            Invalidate();
        }
        private void EndRename(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                EndRenameActivity();
            }
        }
        private void EndRenameActivity()
        {
            if(active_pr != null && renameTextBox.Visible)
            {
                active_pr.Label = renameTextBox.Text;
                renameTextBox.Visible = false;
                renameTextBox.Clear();
                active_pr = null;
                Invalidate();
            }
        }

    }
}
