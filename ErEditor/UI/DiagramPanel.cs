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
        private ErSchema? schema;
        private ErDiagram? diagram;

        private ContextMenuStrip panelContextMenu = new();
        private ContextMenuStrip primitiveContextMenu = new();
        private TextBox selectedPrimitiveRenameTextBox = new();
        private ErDiagramPrimitive? selectedPrimitive;

        Rectangle selectedPrimitiveLocation = Rectangle.Empty;
        bool isEdgeBeingDrawn = false;
        bool isMouseBeingHeld = false;

        public DiagramPanel()
        {
            ConsoleLog.Log("Started constructor", this, "INFO");
            Console.WriteLine(this);
            Initialize();
        }

        private void Initialize()
        {
            InitializeDrawingContextMenu();
            InitializePrimitiveContextMenu();

            this.MouseDown += new MouseEventHandler(DiagramPanel_MouseDown);
            this.MouseUp += new MouseEventHandler(DiagramPanel_MouseUp);
            this.MouseDoubleClick += new MouseEventHandler(DiagramPanel_MouseDoubleClick);
            this.MouseMove += new MouseEventHandler(DiagramPanel_MouseMove);

            selectedPrimitiveRenameTextBox.Width = 100;
            selectedPrimitiveRenameTextBox.Height = 30;
            selectedPrimitiveRenameTextBox.Text = string.Empty;
            selectedPrimitiveRenameTextBox.Visible = false;
            selectedPrimitiveRenameTextBox.KeyDown += new KeyEventHandler(EndRename);
            Controls.Add(selectedPrimitiveRenameTextBox);
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
            panelContextMenu.Items.Add(createPrimitiveDropdown);
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

        public void OpenDiagram(ErSchema schema, ErDiagram diagram)
        {
            this.schema = schema;
            this.diagram = diagram;
            this.Invalidate();
        }
        public void CloseDiagram()
        {
            schema = null;
            diagram = null;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (diagram != null)
            {
                if (isEdgeBeingDrawn) // links are drawn under the figures
                {
                    Console.WriteLine(selectedPrimitiveLocation);
                    e.Graphics.DrawLine(new Pen(Color.Black), selectedPrimitiveLocation.X, selectedPrimitiveLocation.Y, selectedPrimitiveLocation.Width, selectedPrimitiveLocation.Height);
                }
                diagram.Draw(e.Graphics);
            }
        }
        

        private void CreateEntitySet(object? sender, EventArgs e)
        {
            Point point = PointToClient(new Point(panelContextMenu.Bounds.X, panelContextMenu.Bounds.Y));
            
            if(diagram != null && schema != null)
            {
                ErDiagramRectangle pr = diagram.AddRectangle(schema.EntitySets.Add(), point.X, point.Y);
                selectedPrimitive = pr;
                Invalidate();
                RenamePrimitive(pr);
            }
        }
        private void CreateRelationshipSet(object? sender, EventArgs e)
        {
            ConsoleLog.Log("New relationship set creation (diagram) handler was called", this, "INFO");
            Point point = PointToClient(new Point(panelContextMenu.Bounds.X, panelContextMenu.Bounds.Y));

            if (diagram != null && schema != null)
            {
                ErDiagramDiamond pr = diagram.AddDiamond(schema.RelationshipSets.Add(), point.X, point.Y);
                selectedPrimitive = pr;
                Invalidate();
                RenamePrimitive(pr);
            }

        }

        private void DiagramPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                if (selectedPrimitiveRenameTextBox.Visible)
                {
                    this.EndRenameActivity();
                }
                isMouseBeingHeld = false;
                ErDiagramPrimitive? pr = diagram.FindAt(e.X, e.Y);
                Console.WriteLine("\n" + e.X + " " + e.Y);
                if (e.Button == MouseButtons.Right)
                {
                    if (pr == null)
                    {
                        panelContextMenu.Show(this, e.X, e.Y);
                    }
                    else
                    {
                        selectedPrimitive = pr;
                        primitiveContextMenu.Show(this, e.X, e.Y);
                    }
                }
                else if (e.Button == MouseButtons.Left)
                {
                    if (pr != null)
                    {
                        if (!isEdgeBeingDrawn) // we are not currently drawing a link so we can start holding
                        {
                            isMouseBeingHeld = true;
                            selectedPrimitive = pr;
                            selectedPrimitiveLocation = new Rectangle(pr.X, pr.Y, e.X - pr.X, e.Y - pr.Y);
                        }
                        else
                        {
                            //diagram.AddEdge(active_pr, pr, new Point(active_at.X, active_at.Y), new Point(active_at.Width, active_at.Height));
                            isEdgeBeingDrawn = false;
                            Invalidate();
                        }
                    }
                    else if (isEdgeBeingDrawn)
                    {
                        isEdgeBeingDrawn = false;
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
                ErDiagramPrimitive? pr = diagram.FindAt(point.X, point.Y);
                if (pr != null)
                {
                    selectedPrimitive = pr;
                    this.RenamePrimitive(pr);
                }
            }
        }
        private void DiagramPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                if (isMouseBeingHeld && selectedPrimitive != null)
                {
                    selectedPrimitive.X = e.X - selectedPrimitiveLocation.Width;
                    selectedPrimitive.Y = e.Y - selectedPrimitiveLocation.Height;

                    Invalidate();
                }
                else if (isEdgeBeingDrawn)
                {
                    selectedPrimitiveLocation.Width = e.X;
                    selectedPrimitiveLocation.Height = e.Y;

                    Invalidate();
                }
            }
        }
        private void DiagramPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                isMouseBeingHeld = false;
            }
        }

        private void RenamePrimitive(ErDiagramPrimitive pr)
        {
            isMouseBeingHeld = false;
            selectedPrimitiveRenameTextBox.Left = (int)(pr.X + pr.width * 0.5);
            selectedPrimitiveRenameTextBox.Top = (int)(pr.Y + pr.height * 0.5);
            selectedPrimitiveRenameTextBox.Text = pr.Label;

            selectedPrimitiveRenameTextBox.Visible = true;
            selectedPrimitiveRenameTextBox.Focus();

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
            if(selectedPrimitive != null && selectedPrimitiveRenameTextBox.Visible)
            {
                selectedPrimitive.Label = selectedPrimitiveRenameTextBox.Text;
                selectedPrimitiveRenameTextBox.Visible = false;
                selectedPrimitiveRenameTextBox.Clear();
                selectedPrimitive = null;
                Invalidate();
            }
        }

    }
}
