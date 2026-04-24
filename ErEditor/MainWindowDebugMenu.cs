using ErEditor.ErSchemaClasses;
using ErEditor.ExportClasses;
using ErEditor.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Miscellaneous;
using System.Xml.Linq;
using P = Microsoft.Msagl.Core.Geometry.Point;

namespace ErEditor
{
    partial class MainWindow
    {
        private void InitializeDebugMenu()
        {
            ToolStripMenuItem debugDropDown = new("Дебаг");

            ToolStripMenuItem showSchemaObjectItem = new("Показать схему в оперативной памяти");
            showSchemaObjectItem.Click += Debug_ShowSchema;
            ToolStripMenuItem showRegistryStateItem = new("Показать состояние реестра схемы");
            showRegistryStateItem.Click += Debug_ShowRegistry;
            ToolStripMenuItem showMsaglGraph = new("Генерация графа (MSAGL)");
            showMsaglGraph.Click += Debug_ShowMsaglGraph;
            ToolStripMenuItem executeSQLCreateQuery = new("Выполнить создание БД");
            executeSQLCreateQuery.Click += Debug_ExecuteSQLCreateQuery;
            ToolStripMenuItem executeDiagramGeneration = new("Выполнить генерацию диаграммы");
            executeDiagramGeneration.Click += Debug_ExecuteDiagramGeneration;

            debugDropDown.DropDownItems.AddRange(
                [
                showSchemaObjectItem,
                showRegistryStateItem,
                showMsaglGraph,
                executeSQLCreateQuery,
                executeDiagramGeneration
                ]);
            menuStrip1.Items.Add(debugDropDown);
        }

        private void Debug_ShowSchema(object? sender, EventArgs e)
        {
            var schemas = navigatorTreeView1.Schemas;
            if (schemas.Count > 0)
            {
                Console.WriteLine(schemas[0].PrintState());
            }
        }
        private void Debug_ShowRegistry(object? sender, EventArgs e)
        {
            ErSchemaRegistry? registry = null;
            var schemas = navigatorTreeView1.Schemas;
            if (schemas.Count > 0)
            {
                registry = ErSchemaFileManager.GetRegistry(schemas[0]);
            }
            if (registry != null)
            {
                Console.WriteLine(registry.PrintState());
            }
        }
        private void Debug_ShowMsaglGraph(object? sender, EventArgs e)
        {
            MsaglTestsForm msaglTestsForm = new();
            msaglTestsForm.ShowDialog();
        }
        private string BuildQuery(ExportTable table)
        {
            string sqlQuery = $"CREATE TABLE \"{table.Source.Name}\"(";
            if(table.Columns.Count > 0 || table.ForeignKeys.Count == 0)
            {
                sqlQuery += "id INT PRIMARY KEY NOT NULL,";
            }

            foreach (var column in table.Columns)
            {
                sqlQuery += $"\"{column.Name}\"";
                // Type и Constraints задаются некоторым форматом самой ExportTable,
                // а провайдеры БД из этих абстрактных типов их уже резолвят в конкретные
                switch (column.Type)
                {
                    case "bool":
                    case "int":
                        sqlQuery += "INT";
                        break;
                    case "float":
                        sqlQuery += "REAL";
                        break;
                    case "text":
                        sqlQuery += "TEXT";
                        break;
                }
                sqlQuery += ",";
            }
            foreach (var foreignKey in table.ForeignKeys)
            {
                sqlQuery += $"\"{foreignKey.Name}\" INT";
                sqlQuery += ",";
            }
            foreach (var foreignKey in table.ForeignKeys)
            {
                sqlQuery += $"FOREIGN KEY(\"{foreignKey.Name}\") REFERENCES \"{foreignKey.LinkedTable.Name}\"(id),";
            }
            sqlQuery = sqlQuery.Remove(sqlQuery.Length - 1);
            sqlQuery += ");";
            return sqlQuery;
        }

        private void Debug_ExecuteSQLCreateQuery(object? sender, EventArgs e)
        {
            if(MainWindow.Navigator.Schemas.Count > 0)
            {
                ErSchema schema = navigatorTreeView1.Schemas[0];

                // Db creation
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\test_db2\\test_SQL2.db";
                ConsoleLog.Log($"[1/2] Initializing ExportDbContext instance... (path to DB: {path})", this);
                ExportDbContext dbcontext = new(path);
                dbcontext.Database.EnsureDeleted();
                dbcontext.Database.EnsureCreated();
                dbcontext.Dispose();

                // connecting to Db
                string connectionPath = $"Data Source={path}";
                using (SqliteConnection connection = new SqliteConnection(connectionPath))
                {
                    connection.Open();

                    ExportSchema exportSchema = new();
                    exportSchema.BuildFrom(schema);

                    string sqlQuery = "SELECT 1";
                    using (SqliteCommand command = new SqliteCommand(sqlQuery, connection))
                    {
                        foreach (var table in exportSchema.tables)
                        {
                            command.CommandText = BuildQuery(table);
                            using (SqliteDataReader reader = command.ExecuteReader())
                            {

                            }
                            ConsoleLog.Log(command.CommandText, this);
                        }

                    }

                        
                }
                ConsoleLog.Log($"[2/2] Query executed successfully.", this);
            }
        }

        public static void Debug_ExecuteDiagramGeneration(object? sender, EventArgs e)
        {
            if (MainWindow.Navigator.Schemas.Count > 0)
            {
                ErSchema schema = MainWindow.Navigator.Schemas[0];

                double w = 110;
                double h = 40;
                double l = 40;

                double ratio = 8;

                 // MSAGL
                GeometryGraph graph = new GeometryGraph();

                Dictionary<ErEntitySet, Node> entitySetNodes = new();
                Dictionary<ErRelationshipSet, Node> relationshipSetNodes = new();
                Dictionary<ErRole, bool> order = new();
                Dictionary<ErRole, Edge> roleEdges = new();
                foreach(var es in schema.EntitySets)
                {
                    Node esNode = new Node(CurveFactory.CreateRectangle(w, h, new P()), es);
                    entitySetNodes.Add(es, esNode);
                    graph.Nodes.Add(esNode);
                }
                foreach (var rs in schema.RelationshipSets)
                {
                    Node rsNode = new Node(CurveFactory.CreateDiamond((w + 20) / 2, (h + 40) / 2, new P()), rs);
                    relationshipSetNodes.Add(rs, rsNode);
                    graph.Nodes.Add(rsNode);
                    var roleCounts = rs.Roles.Count;
                    var outCounts = 0;
                    var inCounts = 0;
                    foreach(var role in rs.Roles)
                    {
                        roleCounts--;
                        var maps = rs.Mappings.Where(x => x.PreImage.Contains(role)).ToList();
                        Edge roleEdge;

                        if(roleCounts == 0 || maps.Count == 0)
                        {
                            if(outCounts == 0)
                            {
                                roleEdge = new Edge(rsNode, entitySetNodes[role.EntitySet]) { Length = l };
                                order.Add(role, false);
                            }
                            else
                            {
                                roleEdge = new Edge(entitySetNodes[role.EntitySet], rsNode) { Length = l };
                                order.Add(role, true);
                            }
                        }
                        else
                        {
                            var map = maps[0];
                            if (map.MaxCardinalityOfImage == -1)
                            {
                                roleEdge = new Edge(entitySetNodes[role.EntitySet], rsNode) { Length = l };
                                order.Add(role, true);
                                inCounts += 1;
                            }
                            else
                            {
                                roleEdge = new Edge(rsNode, entitySetNodes[role.EntitySet]) { Length = l };
                                order.Add(role, false);
                                outCounts += 1;
                            }
                        }

                        graph.Edges.Add(roleEdge);
                        roleEdges.Add(role, roleEdge);
                    }
                }

                var settings1 = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings();
                var settings2 = new Microsoft.Msagl.Layout.Incremental.FastIncrementalLayoutSettings();
                settings1.ScaleX = 1;
                settings1.ScaleY = 1;
                settings1.EdgeRoutingSettings.EdgeRoutingMode = EdgeRoutingMode.StraightLine;
                settings1.EdgeRoutingSettings.Padding = 40;
                settings2.EdgeRoutingSettings.EdgeRoutingMode = EdgeRoutingMode.StraightLine;
                settings2.EdgeRoutingSettings.Padding = 40;

                //settings2.LayerSeparation = 1;
                //settings2.PackingMethod = PackingMethod.Compact;
                settings2.LiftCrossEdges = true;

                LayoutHelpers.CalculateLayout(graph, settings1, null);
                LayoutHelpers.CalculateLayout(graph, settings2, null);
                MainWindow.MsaglGraph = graph;


                graph.UpdateBoundingBox();
                graph.Translate(new P(-graph.Left, -graph.Bottom));

                var cr = MainWindow.DiagramPanel.ClientRectangle;
                Console.WriteLine($"diagram ratio: {graph.Width / graph.Height}");

                Console.WriteLine($"L {graph.Left} T {graph.Top} R {graph.Right} B {graph.Bottom}");
                Console.WriteLine($"L {cr.Left} T {cr.Top} R {cr.Right} B {cr.Bottom}");
                double s_x = (cr.Width / (double)graph.Width);
                double s_y = (cr.Height / (double)(graph.Height));

                s_x = (110 / (double)graph.Nodes[0].Width);
                s_y = (40 / (double)graph.Nodes[0].Height);

                Console.WriteLine($"Sx {s_x} Sy {s_y}");
                var s = Math.Min(s_x, s_y) * 0.9;

                double g0 = (double)(graph.Left + graph.Right) / 2;
                double g1 = (double)(graph.Top + graph.Bottom) / 2;
                double c0 = (double)(cr.Left + cr.Right) / 2;
                double c1 = (double)(cr.Top + cr.Bottom) / 2;
                double dx = c0 - s * g0;
                double dy = c1 + s * g1;


                PlaneTransformation matrix = new(s, 0, dx, 0, -s, dy);

                graph.Transform(matrix);
                graph.UpdateBoundingBox();
               
                int origin_x = (int)(cr.Width * 0.1);
                int origin_y = (int)(cr.Height * 0.1) + (int)graph.Height;

                ErDiagram diagram = schema.Diagrams.Add("test diagram");
                foreach(Node node in graph.Nodes)
                {
                    var bRect = node.BoundingBox;
                    Point point = new((int)bRect.Left, (int)bRect.Top);

                    ErEntitySet? entitySet = entitySetNodes.FirstOrDefault(x => x.Value == node).Key;
                    if(entitySet != null)
                    {
                        diagram.AddRectangle(entitySet, point.X, point.Y - (int)node.Height, (int)node.Width, (int)node.Height);
                        Console.WriteLine($"Rectangle dimensions: {node.Width}, {node.Height}");
                    }
                    ErRelationshipSet? relationshipSet = relationshipSetNodes.FirstOrDefault(x => x.Value == node).Key;
                    if (relationshipSet != null)
                    {
                        diagram.AddDiamond(relationshipSet, point.X, point.Y - (int)node.Height, (int)(node.Width), (int)(node.Height));
                        Console.WriteLine($"Diamond dimensions: {node.Width}, {node.Height}");
                    }
                }
                foreach(Edge edge in graph.Edges)
                {
                    ErRole? role = roleEdges.FirstOrDefault(x => x.Value == edge).Key;
                    ErRelationshipSet? relationshipSet = schema.RelationshipSets.FirstOrDefault(x => x.Roles.Contains(role));
                    LineSegment? line = edge.Curve as LineSegment;

                    if (role != null && relationshipSet != null && line != null)
                    {
                        var p1 = MsaglPointToDrawingPoint(line.Start);
                        var p2 = MsaglPointToDrawingPoint(line.End);

                        Point p1t = new Point(p1.X, p1.Y);
                        Point p2t = new Point(p2.X, p2.Y);
                        if (order[role])
                        {
                            diagram.AddEdge(role, relationshipSet, p1t, p2t);
                        }
                        else
                        {
                            diagram.AddEdge(role, relationshipSet, p2t, p1t);
                        }
                        
                    }
                }


            }
        }

        public static System.Drawing.Point MsaglPointToDrawingPoint(P point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        private System.Drawing.Drawing2D.Matrix? GetTransform(GeometryGraph graph)
        {
            RectangleF rect_t = MainWindow.DiagramPanel.ClientRectangle;
            float margin = 30;
            RectangleF r = new(
                rect_t.X + margin,
                rect_t.Y + margin,
                rect_t.Width - margin,
                rect_t.Height - margin);
            
            var gr = graph.BoundingBox;
            if (r.Height > 1 && r.Width > 1)
            {
                float scale = Math.Min(r.Width / (float)gr.Width, r.Height / (float)gr.Height);
                float g0 = (float)(gr.Left + gr.Right) / 2;
                float g1 = (float)(gr.Top + gr.Bottom) / 2;

                float c0 = (r.Left + r.Right) / 2;
                float c1 = (r.Top + r.Bottom) / 2;
                float dx = c0 - scale * g0;
                float dy = c1 + scale * g1;
                return new System.Drawing.Drawing2D.Matrix(scale, 0, 0, -scale, dx, dy);
            }
            return null;
        }
    }
}
