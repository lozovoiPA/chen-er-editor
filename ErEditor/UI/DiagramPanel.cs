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

        public void OpenDiagram(ErSchema schema, ErDiagram diagram)
        {
            if (this.diagram != null) { diagram.Unsubscribe(this); }
            this.schema = schema;
            this.diagram = diagram;

            diagram.referenceSize = new Rectangle(0, 0, this.Width, this.Height);
            this.diagramSize = diagram.GetSize();
            bitmapOffset = new Point(-diagramSize.X + 10, -diagramSize.Y + 10);

            selectedPrimitiveRegion = Rectangle.Empty;
            isEdgeBeingDrawn = false;
            isMouseBeingHeld = false;
            dragOffset = new Point(0, 0);
            viewPortOffset = new Point(0, 0);
            diagramOffset = new Point(0, 0);


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
                // Bitmap bitmap = new Bitmap(size.Width + 20, size.Height + 20);
                Bitmap bitmap = new Bitmap(this.Width + 20, this.Height + 20);
                var g = Graphics.FromImage(bitmap);

                bitmapOffset = new Point(-diagramSize.X + 10, -diagramSize.Y + 10);

                g.TranslateTransform(bitmapOffset.X, bitmapOffset.Y);

                g.Clear(this.BackColor);

                if (isEdgeBeingDrawn)
                {
                    g.DrawLine(new Pen(Color.Black),
                        GetOnScreenPoint(selectedPrimitiveRegion.X, selectedPrimitiveRegion.Y),
                        GetOnScreenPoint(selectedPrimitiveRegion.Width, selectedPrimitiveRegion.Height));

                    Console.WriteLine($"Drawing edge from {selectedPrimitiveRegion.X}, {selectedPrimitiveRegion.Y} to {selectedPrimitiveRegion.Width}, {selectedPrimitiveRegion.Height}");
                }

                diagram.Draw(g);

                g.Flush();

                int sourceX = Math.Max(0, -bitmapOffset.X - viewPortOffset.X);
                int sourceY = Math.Max(0, -bitmapOffset.Y - viewPortOffset.Y);
                int sourceWidth = Math.Min(bitmap.Width, this.Width - (-bitmapOffset.X - viewPortOffset.X));
                int sourceHeight = Math.Min(bitmap.Height, this.Height - (-bitmapOffset.Y - viewPortOffset.Y));

                Console.WriteLine($"Viewport offset: {viewPortOffset.X}, {viewPortOffset.Y}");

                if (!(sourceX >= this.Width || sourceY >= this.Height))
                {
                    e.Graphics.DrawImage(bitmap,
                        new Rectangle(
                            sourceX,
                            sourceY,
                            sourceWidth,
                            sourceHeight
                            ),
                        new Rectangle(
                            Math.Max(0, bitmapOffset.X + viewPortOffset.X), 
                            Math.Max(0, bitmapOffset.Y + viewPortOffset.Y), 
                            sourceWidth, 
                            sourceHeight
                            ),
                        GraphicsUnit.Pixel
                    );

                    Console.WriteLine($"Drawing area (source): {sourceX}, {sourceY}, {sourceWidth}, {sourceHeight}");
                }
                else
                {
                    Console.WriteLine("Area out of bounds, not drawing");
                }
                
                //e.Graphics.DrawImage(bitmap, -viewPortOffset.X, -viewPortOffset.Y);

                g.Dispose();
                bitmap.Dispose();
            } else
            {
                e.Graphics.Clear(this.BackColor);
            }
        }

        private Point GetOnScreenPoint(int x, int y)
        {
            return new Point(x + viewPortOffset.X, y + viewPortOffset.Y);
        }

        private Point GetDiagramPoint(int x, int y)
        {
            return new Point(x - viewPortOffset.X, y - viewPortOffset.Y);
        }

        private void CreateEntitySet_Handler(object? sender, EventArgs e)
        {
            Point point = PointToClient(GetOnScreenPoint(panelContextMenu.Bounds.X, panelContextMenu.Bounds.Y));
            
            if(diagram != null && schema != null)
            {
                ErDiagramRectangle pr = diagram.AddRectangle(schema.EntitySets.Add(), point.X, point.Y);
                Console.WriteLine($"Adding rectangle at {point.X}, {point.Y}");
                selectedPrimitive = pr;
                Invalidate();
                RenamePrimitive(pr);
            }
        }
        private void CreateRelationshipSet_Handler(object? sender, EventArgs e)
        {
            Point point = PointToClient(GetOnScreenPoint(panelContextMenu.Bounds.X, panelContextMenu.Bounds.Y));

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

            Console.WriteLine($"started drawing edge at: {point}");
            selectedPrimitiveRegion = new Rectangle(point.X, point.Y, point.X, point.Y);
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
                Point point = PointToClient(GetOnScreenPoint(primitiveContextMenu.Bounds.X, primitiveContextMenu.Bounds.Y));
                DeletePrimitive(point.X, point.Y);
                Invalidate();
            }
        }

        private void DeleteElement_Handler(object? sender, EventArgs e)
        {
            if (diagram != null)
            {
                Point point = PointToClient(GetOnScreenPoint(primitiveContextMenu.Bounds.X, primitiveContextMenu.Bounds.Y));
                var pr = DeletePrimitive(point.X, point.Y);
                if (pr != null && schema != null)
                {
                    switch (pr)
                    {
                        case ErDiagramDiamond pr1:
                            schema.RelationshipSets.Remove(pr1.ErElement);
                            break;
                        case ErDiagramRectangle pr2:
                            schema.EntitySets.Remove(pr2.ErElement);
                            break;
                        case ErDiagramEdge pr3:
                            var rs = schema.RelationshipSets.ToList().Find(x => x.Roles.Contains(pr3.ErElement));
                            if (rs != null)
                            {
                                rs.RemoveRole(pr3.ErElement);
                            }
                            break;
                    }
                    Invalidate();
                }
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
                Point point = GetOnScreenPoint(e.X, e.Y);
                ErDiagramPrimitive? clickedPrimitive = diagram.FindAt(point.X, point.Y);
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

                            Point point1 = PointToClient(GetOnScreenPoint(clickedPrimitive.X, clickedPrimitive.Y));
                            Point point2 = PointToClient(GetOnScreenPoint(e.X - clickedPrimitive.X, e.Y - clickedPrimitive.Y));
                            selectedPrimitiveRegion = new Rectangle(point1.X, point1.Y, point2.X, point2.Y);
                        }
                        else
                        {
                            ErRelationshipSet? relationshipSet = (selectedPrimitive as ErDiagramDiamond)?.ErElement
                                ?? (clickedPrimitive as ErDiagramDiamond)?.ErElement;
                            ErEntitySet? entitySet = (selectedPrimitive as ErDiagramRectangle)?.ErElement
                                ?? (clickedPrimitive as ErDiagramRectangle)?.ErElement;
                            if(relationshipSet != null && entitySet != null)
                            {
                                ErRole role = relationshipSet.AddRole(entitySet, entitySet.Name, true);
                                diagram.AddEdge(role, selectedPrimitive!, clickedPrimitive, 
                                    GetOnScreenPoint(selectedPrimitiveRegion.X, selectedPrimitiveRegion.Y), 
                                    GetOnScreenPoint(selectedPrimitiveRegion.Width, selectedPrimitiveRegion.Height));
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
                Point point = GetOnScreenPoint(e.X, e.Y);
                Console.WriteLine($"Double clicked at: {point}");
                Console.WriteLine($"Viewport offset: {viewPortOffset}");
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
                        Point point = PointToClient(GetOnScreenPoint(e.X - selectedPrimitiveRegion.Width, e.Y - selectedPrimitiveRegion.Height));

                        selectedPrimitive.X = point.X;
                        selectedPrimitive.Y = point.Y;
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
                    Point point = new Point(e.X, e.Y);
                    selectedPrimitiveRegion.Width = point.X;
                    selectedPrimitiveRegion.Height = point.Y;

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

            Point point = GetDiagramPoint((int)(pr.X + pr.Width * 0.5), (int)(pr.Y + pr.Height * 0.5));
            selectedPrimitiveRenameTextBox.Left = point.X;
            selectedPrimitiveRenameTextBox.Top = point.Y;

            Console.WriteLine($"Renaming at: {point.X}, {point.Y}");
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
