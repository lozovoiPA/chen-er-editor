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

        Rectangle selectedPrimitiveRegion = Rectangle.Empty;
        bool isEdgeBeingDrawn = false;
        bool isMouseBeingHeld = false;

        public DiagramPanel()
        {
            ConsoleLog.Log("Started constructor", this);
            Console.WriteLine(this);
            Initialize();

            this.DoubleBuffered = true;
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
            ConsoleLog.Log("Started InitializeDrawingContextMenu()", this);
            ToolStripMenuItem createPrimitiveDropdown = new();
            createPrimitiveDropdown.Text = "Создать...";

            ToolStripMenuItem createEntitySetItem = new();
            createEntitySetItem.Text = "Множество сущностей";
            createEntitySetItem.Click += CreateEntitySet_Handler;
            ToolStripMenuItem createRelationshipSetItem = new();
            createRelationshipSetItem.Text = "Множество связей";
            createRelationshipSetItem.Click += CreateRelationshipSet_Handler;

            createPrimitiveDropdown.DropDownItems.AddRange([
                createEntitySetItem,
                createRelationshipSetItem
                ]);
            panelContextMenu.Items.Add(createPrimitiveDropdown);
        }
        private void InitializePrimitiveContextMenu()
        {
            ToolStripMenuItem createEdgeItem = new();
            createEdgeItem.Text = "Создать связь...";
            createEdgeItem.Click += StartDrawingEdge_Handler;
            ToolStripMenuItem deletePrimitiveOnlyItem = new();
            deletePrimitiveOnlyItem.Text = "Убрать элемент";
            ToolStripMenuItem deleteErElementItem = new();
            deleteErElementItem.Text = "Удалить элемент";

            primitiveContextMenu.Items.AddRange([
                createEdgeItem,
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
                    Console.WriteLine(selectedPrimitiveRegion);
                    e.Graphics.DrawLine(new Pen(Color.Black), selectedPrimitiveRegion.X, selectedPrimitiveRegion.Y, selectedPrimitiveRegion.Width, selectedPrimitiveRegion.Height);
                }
                diagram.Draw(e.Graphics);
            }
        }
        

        private void CreateEntitySet_Handler(object? sender, EventArgs e)
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
        private void CreateRelationshipSet_Handler(object? sender, EventArgs e)
        {
            ConsoleLog.Log("New relationship set creation (diagram) handler was called", this);
            Point point = PointToClient(new Point(panelContextMenu.Bounds.X, panelContextMenu.Bounds.Y));

            if (diagram != null && schema != null)
            {
                ErDiagramDiamond pr = diagram.AddDiamond(schema.RelationshipSets.Add(), point.X, point.Y);
                selectedPrimitive = pr;
                Invalidate();
                RenamePrimitive(pr);
            }

        }
        private void StartDrawingEdge_Handler(object? sender, EventArgs e)
        {
            isEdgeBeingDrawn = true;
            Point point = PointToClient(new Point(primitiveContextMenu.Bounds.X, primitiveContextMenu.Bounds.Y));
            Console.WriteLine(point);
            selectedPrimitiveRegion = new Rectangle(point.X, point.Y, point.X, point.Y);
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
                ErDiagramPrimitive? clickedPrimitive = diagram.FindAt(e.X, e.Y);
                if (e.Button == MouseButtons.Right)
                {
                    if (clickedPrimitive == null)
                    {
                        panelContextMenu.Show(this, e.X, e.Y);
                    }
                    else
                    {
                        selectedPrimitive = clickedPrimitive;
                        primitiveContextMenu.Show(this, e.X, e.Y);
                    }
                }
                else if (e.Button == MouseButtons.Left)
                {
                    if (clickedPrimitive != null)
                    {
                        if (!isEdgeBeingDrawn) // we are not currently drawing a link so we can start holding
                        {
                            isMouseBeingHeld = true;
                            selectedPrimitive = clickedPrimitive;
                            selectedPrimitiveRegion = new Rectangle(clickedPrimitive.X, clickedPrimitive.Y, e.X - clickedPrimitive.X, e.Y - clickedPrimitive.Y);
                        }
                        else
                        {
                            ErRelationshipSet? relationshipSet = (selectedPrimitive as ErDiagramDiamond)?.ErElement
                                ?? (clickedPrimitive as ErDiagramDiamond)?.ErElement;
                            ErEntitySet? entitySet = (selectedPrimitive as ErDiagramRectangle)?.ErElement
                                ?? (clickedPrimitive as ErDiagramRectangle)?.ErElement;
                            if(relationshipSet != null && entitySet != null)
                            {
                                ErRole role = relationshipSet.AddRole(entitySet, "", true);
                                diagram.AddEdge(role, selectedPrimitive!, clickedPrimitive, 
                                    new Point(selectedPrimitiveRegion.X, selectedPrimitiveRegion.Y), 
                                    new Point(selectedPrimitiveRegion.Width, selectedPrimitiveRegion.Height));
                            }

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
                    selectedPrimitive.X = e.X - selectedPrimitiveRegion.Width;
                    selectedPrimitive.Y = e.Y - selectedPrimitiveRegion.Height;

                    Invalidate();
                }
                else if (isEdgeBeingDrawn)
                {
                    selectedPrimitiveRegion.Width = e.X;
                    selectedPrimitiveRegion.Height = e.Y;

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
