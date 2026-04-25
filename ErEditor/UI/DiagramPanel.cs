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
    public class DiagramPanel : Panel, IObserver,
        IVisitor<ObjectDeletedNotification<ErDiagramPrimitive>>
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
        Point dragOffset = new Point(0, 0);
        Point viewPortOffset = new Point(0, 0);
        Point diagramOffset = new Point(0, 0);

        Rectangle diagramSize = new Rectangle(0, 0, 0, 0);
        Point bitmapOffset = new Point(0, 0);

        public ObserverBase observerLogic;

        public DiagramPanel()
        {
            ConsoleLog.Log("Started constructor", this);
            Console.WriteLine(this);
            Initialize();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;

            observerLogic = new(this);
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
            deletePrimitiveOnlyItem.Click += DeletePrimitive_Handler;
            ToolStripMenuItem deleteErElementItem = new();
            deleteErElementItem.Text = "Удалить элемент";
            deleteErElementItem.Click += DeleteElement_Handler;

            primitiveContextMenu.Items.AddRange([
                createEdgeItem,
                deletePrimitiveOnlyItem,
                deleteErElementItem
                ]);
        }

        private ErDiagramPrimitive? DeletePrimitive(int x, int y)
        {
            var pr = diagram?.FindAt(x, y);
            if (pr != null)
            {
                diagram?.Remove(pr);
            }
            return pr;
        }

        private void DeletePrimitive_Handler(object? sender, EventArgs e)
        {
            if (diagram != null)
            {
                Point point = PointToClient(new Point(primitiveContextMenu.Bounds.X, primitiveContextMenu.Bounds.Y));
                DeletePrimitive(point.X + viewPortOffset.X - bitmapOffset.X, point.Y + viewPortOffset.Y - bitmapOffset.Y);
                Invalidate();
            }
        }

        private void DeleteElement_Handler(object? sender, EventArgs e)
        {
            if (diagram != null)
            {
                Point point = PointToClient(new Point(primitiveContextMenu.Bounds.X, primitiveContextMenu.Bounds.Y));
                var pr = DeletePrimitive(point.X + viewPortOffset.X - bitmapOffset.X, point.Y + viewPortOffset.Y - bitmapOffset.Y);
                if(pr != null && schema != null)
                {
                    switch(pr)
                    {
                        case ErDiagramDiamond pr1:
                            schema.RelationshipSets.Remove(pr1.ErElement);
                            break;
                        case ErDiagramRectangle pr2:
                            schema.EntitySets.Remove(pr2.ErElement);
                            break;
                        case ErDiagramEdge pr3:
                            var rs = schema.RelationshipSets.ToList().Find(x => x.Roles.Contains(pr3.ErElement));
                            if(rs != null)
                            {
                                rs.RemoveRole(pr3.ErElement);
                            }
                            break;
                    }
                    Invalidate();
                }
            }
        }

        public void OpenDiagram(ErSchema schema, ErDiagram diagram)
        {
            if (this.diagram != null) { diagram.Unsubscribe(this); }
            this.schema = schema;
            this.diagram = diagram;
            this.diagramSize = diagram.GetSize();
            bitmapOffset = new Point(-diagramSize.X + 10, -diagramSize.Y + 10);


            diagram.Subscribe(this);
            this.Invalidate();
        }
        public void CloseDiagram()
        {
            if(diagram != null)
                diagram.Unsubscribe(this);
            schema = null;
            diagram = null;
            diagramSize = Rectangle.Empty;
            bitmapOffset = Point.Empty;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // base.OnPaint(e);
            if (diagram != null)
            {
                var size = diagramSize = diagram.GetSize();
                Bitmap bitmap = new Bitmap(size.Width + 20, size.Height + 20);
                var g = Graphics.FromImage(bitmap);

                bitmapOffset = new Point(-diagramSize.X + 10, -diagramSize.Y + 10);

                g.TranslateTransform(bitmapOffset.X, bitmapOffset.Y);

                g.Clear(this.BackColor);

                if (isEdgeBeingDrawn)
                {
                    g.DrawLine(new Pen(Color.Black),
                        selectedPrimitiveRegion.X, selectedPrimitiveRegion.Y,
                        selectedPrimitiveRegion.Width, selectedPrimitiveRegion.Height);
                }

                diagram.Draw(g);
                g.Flush();

                int sourceX = viewPortOffset.X;
                int sourceY = viewPortOffset.Y;
                int sourceWidth = Math.Min(bitmap.Width - sourceX, this.Width);
                int sourceHeight = Math.Min(bitmap.Height - sourceY, this.Height);

                e.Graphics.DrawImage(bitmap,
                    new Rectangle(0, 0, sourceWidth, sourceHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
                //e.Graphics.DrawImage(bitmap, -viewPortOffset.X, -viewPortOffset.Y);

                g.Dispose();
                bitmap.Dispose();
            } else
            {
                e.Graphics.Clear(this.BackColor);
            }
        }

        private void CreateEntitySet_Handler(object? sender, EventArgs e)
        {
            Point point = PointToClient(new Point(panelContextMenu.Bounds.X, panelContextMenu.Bounds.Y));
            
            if(diagram != null && schema != null)
            {
                ErDiagramRectangle pr = diagram.AddRectangle(schema.EntitySets.Add(), point.X + viewPortOffset.X - bitmapOffset.X, point.Y + viewPortOffset.Y - bitmapOffset.Y);
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
                ErDiagramDiamond pr = diagram.AddDiamond(schema.RelationshipSets.Add(), point.X + viewPortOffset.X - bitmapOffset.X, point.Y + viewPortOffset.Y - bitmapOffset.Y);
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
            selectedPrimitiveRegion = new Rectangle(point.X + viewPortOffset.X - bitmapOffset.X, point.Y + viewPortOffset.Y - bitmapOffset.Y, point.X + viewPortOffset.X - bitmapOffset.X, point.Y + viewPortOffset.Y - bitmapOffset.Y);
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
                ErDiagramPrimitive? clickedPrimitive = diagram.FindAt(e.X + viewPortOffset.X - bitmapOffset.X, e.Y + viewPortOffset.Y - bitmapOffset.Y);
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
                    if (!isEdgeBeingDrawn) // we are not currently drawing a link so we can start holding
                    {
                        isMouseBeingHeld = true;
                        dragOffset = e.Location;
                    }

                    if (clickedPrimitive != null)
                    {
                        if (!isEdgeBeingDrawn) // we are not currently drawing a link so we can start holding
                        {
                            isMouseBeingHeld = true;
                            selectedPrimitive = clickedPrimitive;
                            selectedPrimitiveRegion = new Rectangle(clickedPrimitive.X + viewPortOffset.X - bitmapOffset.X, clickedPrimitive.Y + viewPortOffset.Y - bitmapOffset.Y, e.X - clickedPrimitive.X + viewPortOffset.X - bitmapOffset.X, e.Y - clickedPrimitive.Y + viewPortOffset.Y - bitmapOffset.Y);
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
                    else
                    {
                        selectedPrimitive = null;
                    }
                }
            }
        }
        private void DiagramPanel_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                Point point = new Point(e.X + viewPortOffset.X - bitmapOffset.X, e.Y + viewPortOffset.Y - bitmapOffset.Y);
                Console.WriteLine(point);
                Console.WriteLine(viewPortOffset);
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
                if (isMouseBeingHeld)
                {
                    if (selectedPrimitive != null)
                    {
                        selectedPrimitive.X = e.X - selectedPrimitiveRegion.Width + viewPortOffset.X - bitmapOffset.X;
                        selectedPrimitive.Y = e.Y - selectedPrimitiveRegion.Height + viewPortOffset.Y - bitmapOffset.Y;
                    }
                    else
                    {
                        Point newLocation = PointToClient(
                            this.PointToScreen(new Point(
                                diagramOffset.X + e.X - dragOffset.X,
                                diagramOffset.Y + e.Y - dragOffset.Y
                            ))
                        );

                        this.viewPortOffset = newLocation;
                        Console.WriteLine($"dragging mouse: {-viewPortOffset.X}, {-viewPortOffset.Y}");
                    }
                    Invalidate();
                }
                else if (isEdgeBeingDrawn)
                {
                    selectedPrimitiveRegion.Width = e.X + viewPortOffset.X - bitmapOffset.X;
                    selectedPrimitiveRegion.Height = e.Y + viewPortOffset.Y - bitmapOffset.Y;

                    Invalidate();
                }
            }
        }
        private void DiagramPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (diagram != null)
            {
                isMouseBeingHeld = false;
                diagramOffset = viewPortOffset;
                //selectedPrimitive = null;
            }
        }

        private void RenamePrimitive(ErDiagramPrimitive pr)
        {
            isMouseBeingHeld = false;
            selectedPrimitiveRenameTextBox.Left = (int)(pr.X + pr.Width * 0.5 - viewPortOffset.X + bitmapOffset.X);
            selectedPrimitiveRenameTextBox.Top = (int)(pr.Y + pr.Height * 0.5 - viewPortOffset.Y + bitmapOffset.Y);
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

        public void Visit(ObjectDeletedNotification<ErDiagramPrimitive> notification)
        {
            this.Invalidate();
        }

        public void Recieve(Notification notification)
        {
            ((IObserver)observerLogic).Recieve(notification);
        }
    }
}
