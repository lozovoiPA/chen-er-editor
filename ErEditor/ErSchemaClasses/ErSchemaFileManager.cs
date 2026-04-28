using ErEditor.DbSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using P = Microsoft.Msagl.Core.Geometry.Point;

namespace ErEditor.ErSchemaClasses
{

    // Используется для передачи данных из БД один раз. По сути, результат запроса к БД для получения всех элементов схемы.
    public class ErSchemaDbObjects
    {
        private List<DbEntitySet> dbEntitySets = new();
        private List<DbRelationshipSet> dbRelationshipSets = new();
        private List<DbValueSet> dbValueSets = new();
        private List<DbDiagram> dbDiagrams = new();

        public ReadOnlyCollection<DbEntitySet> DbEntitySets
        {
            get { return dbEntitySets.AsReadOnly(); }
        }
        public ReadOnlyCollection<DbRelationshipSet> DbRelationshipSets
        {
            get { return dbRelationshipSets.AsReadOnly(); }
        }
        public ReadOnlyCollection<DbValueSet> DbValueSets
        {
            get { return dbValueSets.AsReadOnly(); }
        }
        public ReadOnlyCollection<DbDiagram> DbDiagrams
        {
            get { return dbDiagrams.AsReadOnly(); }
        }

        private void AddRange<TDbEntity>(List<TDbEntity> entities, IEnumerable<TDbEntity> range)
        {
            entities.AddRange(range);
        }
        public void AddEntitySetRange(List<DbEntitySet> range)
        {
            AddRange(dbEntitySets, range);
        }
        public void AddRelationshipSetRange(List<DbRelationshipSet> range)
        {
            AddRange(dbRelationshipSets, range);
        }
        public void AddValueSetRange(List<DbValueSet> range)
        {
            AddRange(dbValueSets, range);
        }
        public void AddDiagramRange(List<DbDiagram> range)
        {
            AddRange(dbDiagrams, range);
        }
    }

    // Используется для передачи данных в БД один раз.
    public class DbSchemaChanges
    {
        private List<IDbEntry> created = new();
        private List<IDbEntry> updated = new();
        private List<IDbEntry> deleted = new();

        public ReadOnlyCollection<IDbEntry> Created
        {
            get { return created.AsReadOnly(); }
        }
        public ReadOnlyCollection<IDbEntry> Updated
        {
            get { return updated.AsReadOnly(); }
        }
        public ReadOnlyCollection<IDbEntry> Deleted
        {
            get { return deleted.AsReadOnly(); }
        }

        private void AddRange(List<IDbEntry> entities, IEnumerable<IDbEntry> range)
        {
            entities.AddRange(range);
        }
        public void AddCreatedRange(List<IDbEntry> range)
        {
            AddRange(created, range);
        }
        public void AddUpdatedRange(List<IDbEntry> range)
        {
            AddRange(updated, range);
        }
        public void AddDeletedRange(List<IDbEntry> range)
        {
            AddRange(deleted, range);
        }
    }


    public class ErSchemaDbData
    {
        public readonly ErSchemaRegistry SchemaRegistry;
        public readonly string Filepath;

        public ErSchemaDbData(ErSchema schema, string filepath)
        {
            this.Filepath = filepath;
            this.SchemaRegistry = new(schema);
        }

        public ErSchema Schema { get { return SchemaRegistry.Schema; } }

        // this maps what is in the database to the memory, i.e., elements that exist in memory but not in database will remain
        public ErSchema OpenSchemaFromDatabase()
        {
            ErDbContext dbcontext = new ErDbContext(Filepath);
            if (dbcontext.Database.EnsureCreated()) // file doesn't exist, there's nothing to map
            {
                return Schema;
            }

            ErSchemaDbObjects dbschema = new();
            dbschema.AddEntitySetRange(dbcontext.EntitySets
                .Include(el => el.Attributes).ThenInclude(x => x.ValueSets)
                .ToList());
            dbschema.AddRelationshipSetRange(dbcontext.RelationshipSets
                .Include(el => el.Attributes).ThenInclude(x => x.ValueSets)
                .Include(el => el.Roles)
                .Include(el => el.Mappings).ThenInclude(x => x.MappingRoles)
                .ToList());
            dbschema.AddValueSetRange(dbcontext.ValueSets.ToList());
            dbschema.AddDiagramRange(dbcontext.Diagrams.Include(el => el.Primitives).ToList());
            dbcontext.Dispose();

            SchemaRegistry.GetSchemaFromDb(dbschema);
            return Schema;
        }
        public bool SaveSchemaToDatabase()
        {
            // Check if the database from the file can be connected to
            ErDbContext dbcontext = new ErDbContext(Filepath);
            if (!dbcontext.Database.CanConnect())
            {
                ConsoleLog.Log($"Saving schema {Schema.Name} failed because the database connection couldn't be established." +
                    $"Please check whether there is another open connection.",
                    "ErSchemaFileManager");
                return false;
            }

            foreach (var vs in dbcontext.ValueSets)
            {
                SchemaRegistry.dbVsRegistryTemp.AddRetrieved(vs.Id, vs);
            }
            Console.WriteLine($"Vs count: {dbcontext.ValueSets.Count()}");
            Console.WriteLine($"Es count: {dbcontext.EntitySets.Count()}");
            Console.WriteLine($"Rs count: {dbcontext.RelationshipSets.Count()}");
            foreach (var el in dbcontext.EntitySets.Include(el => el.Attributes).ThenInclude(el => el.ValueSets))
            {

                foreach(var attr in el.Attributes)
                {
                    SchemaRegistry.dbAttrRegistryTemp.AddRetrieved(attr.Id, attr);
                    Console.WriteLine($"Added attr: {attr.Name} with vs: {attr.ValueSets.Count}");
                }
                dbcontext.Entry(el).State = EntityState.Detached;
            }
            foreach (var el in dbcontext.RelationshipSets.Include(el => el.Attributes).ThenInclude(el => el.ValueSets))
            {
                foreach (var attr in el.Attributes)
                {
                    SchemaRegistry.dbAttrRegistryTemp.AddRetrieved(attr.Id, attr);
                    Console.WriteLine($"Added attr: {attr.Name}");
                }
                dbcontext.Entry(el).State = EntityState.Detached;
            }
            SchemaRegistry.dbcontext = dbcontext;


            ConsoleLog.Log("[0/3] Initiating mapping to database.", this);
            var changes = SchemaRegistry.MakeChangedDbEntries();
            ConsoleLog.Log("[1/3] Entries in registry were created for database.", this);

            dbcontext.RemoveRange(changes.Deleted);
            dbcontext.AddRange(changes.Created);
            dbcontext.UpdateRange(changes.Updated);

            try
            {
                dbcontext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in DB: {e}");
                MessageBox.Show("Ошибка при сохранении схемы. Схема не была сохранена (попробуйте закрыть и заново открыть приложение)", "Ошибка");
            }
            

            dbcontext.Dispose();
            ConsoleLog.Log("[2/3] Schema was saved to database.", this);

            SchemaRegistry.FlushRegistries();
            ConsoleLog.Log("[3/3] Registries have been flushed.", this);
            ConsoleLog.Log("[3/3] Schema was fully saved, you can continue working.", this);

            return true;
        }
    }

    public static class ErSchemaFileManager
    {
        private static List<ErSchemaDbData> openSchemas = new();
        public static ErSchema NewErSchema(string schemaName, string schemaFileName, string folderPath)
        {
            openSchemas.Clear();
            string fullPath = Path.Combine(folderPath, schemaFileName + ".db");
            if (File.Exists(fullPath))
            {
                ConsoleLog.Log("File already exists, it will be deleted and recreated", "ErSchemaFileManager");
            }

            ErDbContext dbcontext = new ErDbContext(fullPath);
            dbcontext.Database.EnsureDeleted();
            dbcontext.Database.EnsureCreated();
            dbcontext.Dispose();

            ErSchema schema = new(schemaName);
            ErSchemaDbData newSchemaData = new(schema, fullPath);

            openSchemas.Add(newSchemaData);
            return newSchemaData.Schema;
        }
        public static bool SaveSchema(ErSchema schema)
        {
            // Check if filedata for this schema exists
            ErSchemaDbData? data = openSchemas.Find(x => x.Schema == schema);
            if (data == null)
            {
                ConsoleLog.Log($"You are trying to save schema that doesn't have a corresponding file data ({schema.Name})." +
                    $"This may be because schema wasn't created through the Schema File Manager." +
                    $"Schema won't be saved, you can save this schema by using a different overload of this method.", 
                    "ErSchemaFileManager");
                return false;
            }

            // Check if the file for this filedata exists
            if (!File.Exists(data.Filepath))
            {
                ConsoleLog.Log($"Saving schema failed because the file specified in this schema file data doesn't exist ({data.Filepath}).",
                    "ErSchemaFileManager");
                return false;
            }

            // Try to save schema
            return data.SaveSchemaToDatabase();
        }
        public static ErSchema? OpenErSchema(string filepath)
        {
            openSchemas.Clear();
            string schemaName = Path.GetFileNameWithoutExtension(filepath);

            // Check if the file for this filedata exists
            if (!File.Exists(filepath))
            {
                ConsoleLog.Log($"Opening schema failed because the file specified doesn't exist ({filepath}).",
                    "ErSchemaFileManager");
                return null;
            }

            ErSchema schema = new(schemaName);
            ErSchemaDbData data = new(schema, filepath);
            data.OpenSchemaFromDatabase();

            openSchemas.Add(data);

            return data.Schema;
        }

        private static GeometryGraph GetMsaglGraph(ErSchema schema,
            out Dictionary<ErEntitySet, Node> entitySetNodes,
            out Dictionary<ErRelationshipSet, Node> relationshipSetNodes,
            out Dictionary<ErRole, bool> order,
            out Dictionary<ErRole, Edge> roleEdges
            )
        {
            double w = 110;
            double h = 40;
            double l = 30;
            double lw = 30;

            GeometryGraph graph = new GeometryGraph();

            entitySetNodes = new();
            relationshipSetNodes = new();
            order = new();
            roleEdges = new();
            foreach (var es in schema.EntitySets)
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
                foreach (var role in rs.Roles)
                {
                    roleCounts--;
                    var maps = rs.Mappings.Where(x => x.PreImage.Contains(role)).ToList();
                    Edge roleEdge;

                    if (roleCounts == 0 || maps.Count == 0)
                    {
                        if (outCounts == 0)
                        {
                            roleEdge = new Edge(rsNode, entitySetNodes[role.EntitySet]) { Length = l, LineWidth = lw };
                            order.Add(role, false);
                            outCounts++;
                        }
                        else
                        {
                            roleEdge = new Edge(entitySetNodes[role.EntitySet], rsNode) { Length = l, LineWidth = lw };
                            order.Add(role, true);
                            inCounts++;
                        }
                    }
                    else
                    {
                        var map = maps[0];
                        if (map.MaxCardinalityOfImage == -1)
                        {
                            roleEdge = new Edge(entitySetNodes[role.EntitySet], rsNode) { Length = l, LineWidth = lw };
                            order.Add(role, true);
                            inCounts += 1;
                        }
                        else
                        {
                            roleEdge = new Edge(rsNode, entitySetNodes[role.EntitySet]) { Length = l, LineWidth = lw };
                            order.Add(role, false);
                            outCounts += 1;
                        }
                    }

                    graph.Edges.Add(roleEdge);
                    roleEdges.Add(role, roleEdge);
                }
                Console.WriteLine($"{rs.Name}: in {inCounts}, out {outCounts}");
            }

            var settings1 = new Microsoft.Msagl.Layout.Layered.SugiyamaLayoutSettings();
            var settings2 = new Microsoft.Msagl.Layout.Incremental.FastIncrementalLayoutSettings();
            //settings1.ScaleX = 1;
            //settings1.ScaleY = 1;
            settings1.NodeSeparation = 10 * (10 / Math.Sqrt(schema.EntitySets.Count + schema.RelationshipSets.Count));

            settings1.EdgeRoutingSettings.EdgeRoutingMode = EdgeRoutingMode.StraightLine;
            settings1.EdgeRoutingSettings.Padding = 10 * (10 / Math.Sqrt(schema.EntitySets.Count + schema.RelationshipSets.Count));
            settings2.EdgeRoutingSettings.EdgeRoutingMode = EdgeRoutingMode.StraightLine;
            settings2.RouteEdges = true;
            settings2.AvoidOverlaps = true;
            settings2.NodeSeparation = 20;
            settings2.RespectEdgePorts = true;
            settings2.EdgeRoutingSettings.Padding = 40;

            //settings2.LayerSeparation = 1;
            //settings2.PackingMethod = PackingMethod.Compact;
            settings2.LiftCrossEdges = true;

            LayoutHelpers.CalculateLayout(graph, settings1, null);
            //LayoutHelpers.CalculateLayout(graph, settings2, null);
            //MainWindow.MsaglGraph = graph;

            return graph;
        }

        private static GeometryGraph LayoutMsaglGraph(GeometryGraph graph, Rectangle clientRect)
        {
            graph.UpdateBoundingBox();
            graph.Translate(new P(-graph.Left, -graph.Bottom));

            var cr = clientRect;
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
            // MainWindow.DiagramPanel.ClientRectangle;

            PlaneTransformation matrix = new(s, 0, dx, 0, -s, dy);

            graph.Transform(matrix);
            graph.UpdateBoundingBox();

            return graph;
        }

        public static ErDiagram GenerateDiagram(ErSchema schema, Rectangle clientRect)
        {
            Dictionary<ErEntitySet, Node> entitySetNodes;
            Dictionary<ErRelationshipSet, Node> relationshipSetNodes;
            Dictionary<ErRole, bool> order;
            Dictionary<ErRole, Edge> roleEdges;

            var graph = GetMsaglGraph(schema, out entitySetNodes, out relationshipSetNodes, out order, out roleEdges);
            graph = LayoutMsaglGraph(graph, clientRect);

            ErDiagram diagram = schema.Diagrams.Add("Новая диаграмма");
            foreach (Node node in graph.Nodes)
            {
                var bRect = node.BoundingBox;
                Point point = new((int)bRect.Left, (int)bRect.Top);

                ErEntitySet? entitySet = entitySetNodes.FirstOrDefault(x => x.Value == node).Key;
                if (entitySet != null)
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
            foreach (Edge edge in graph.Edges)
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
            return diagram;
        }

        public static System.Drawing.Point MsaglPointToDrawingPoint(P point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        public static ErSchemaRegistry? GetRegistry(ErSchema schema)
        {
            foreach (var filedata in openSchemas)
            {
                if(filedata.Schema == schema)
                {
                    return filedata.SchemaRegistry;
                }
            }
            return null;
        }
    }
}
