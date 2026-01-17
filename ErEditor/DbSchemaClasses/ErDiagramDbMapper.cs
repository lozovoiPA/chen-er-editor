using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.LinkLabel;

namespace ErEditor.DbSchemaClasses
{
    // use for one full transaction only
    /*
    public class DiagramDbMapper
    {
        private readonly ErDbContext dbcontext;
        private readonly SchemaRegistry schemaRegistry;

        DiagramDbMapper(ErDbContext dbcontext, SchemaRegistry schemaRegistry)
        {
            this.dbcontext = dbcontext;
            this.schemaRegistry = schemaRegistry;

            if (!dbcontext.Database.CanConnect())
            {
                ConsoleLog.Log("Cannot connect to the database. This mapper cannot be used.", this, "ERROR");
            }
        }
        
        private DiagramPrimitive? MapToPrimitive(DbPrimitive dbPrimitive)
        {
            // Check if the primitive is already in memory (for the future: this dbPrimitive might actually be an updated version)
            DiagramPrimitive? pr = this.schemaRegistry.FindPrimitive(dbPrimitive.Id);
            if(pr != null)
            {
                return pr;
            }

            // Check if the diagram and er element are mapped (for now we do not map primitives with unmapped diagram and element)
            ErDiagram? diagram = this.schemaRegistry.FindDiagram(dbPrimitive.DiagramId);
            if (diagram == null)
            {
                ConsoleLog.Log($"Cannot find diagram of dbPrimitive in memory (id = {dbPrimitive.Id}, diagram id = {dbPrimitive.DiagramId}). " +
                    $"This primitive will not be mapped.", this, "WARNING");
                return null;
            }
            int? elementIdNullable = (dbPrimitive as DbShape)?.ElementId;
            if(elementIdNullable == null)
            {
                ConsoleLog.Log($"Cannot access ElementId of dbPrimitive (id = {dbPrimitive.Id}, diagram id = {dbPrimitive.DiagramId})" +
                    $"This primitive will not be mapped.", this, "WARNING");
                return null;
            }

            // Start mapping the primitive if it's all okay
            int elementId = (int)elementIdNullable;
            switch (dbPrimitive.Type)
            {
                case "rect":
                    ErEntitySet? entitySet = this.schemaRegistry.FindEntitySet(elementId);
                    if (entitySet == null)
                    {
                        ConsoleLog.Log($"Cannot find er element of dbRectangle in memory (id = {dbPrimitive.Id}, diagram id = {dbPrimitive.DiagramId}, element id = {dbPrimitive.DiagramId}). " +
                            $"This primitive will not be mapped.", this, "WARNING");
                        return null;
                    }
                    pr = diagram.AddRectangle(entitySet, dbPrimitive.X, dbPrimitive.Y, dbPrimitive.width, dbPrimitive.height);
                    break;
            }
            if(pr != null)
            {
                this.schemaRegistry.AddRetrievedPrimitive(dbPrimitive.Id, pr);
            }
            return pr;
        }

        public Diagram GetDiagramFromFile(string _path)
        {
            path = _path;
            Diagram diagram = new Diagram();
            DiagramDbContext dbcontext = new DiagramDbContext(path);

            List<DbPrimitive> dbPrimitives = dbcontext.dbPrimitives.ToList();
            idMap.Clear();

            foreach (DbPrimitive dbPr in dbPrimitives)
            {
                if (idMap.ContainsKey(dbPr.Id)) continue;
                MapToPrimitive(diagram, dbcontext, dbPr);
            }
            dbcontext.Dispose();
            return diagram;
        }

        public Diagram CreateEmptyDiagram(string _path)
        {
            path = _path;
            Diagram diagram = new Diagram();
            DiagramDbContext dbcontext = new DiagramDbContext(path);
            dbcontext.Database.EnsureDeleted();
            dbcontext.Database.EnsureCreated();
            dbcontext.Dispose();
            return diagram;
        }

        private DbPrimitive StorePrimitive(DiagramDbContext dbcontext, CustomPrimitive primitive)
        {
            DbPrimitive dbPr = new DbPrimitive();
            dbPr.X = primitive.X;
            dbPr.Y = primitive.Y;
            dbPr.width = primitive.width;
            dbPr.height = primitive.height;

            dbPr.label = primitive.label;
            dbPr.type = primitive.GetCustomType();
            if (idMap.ContainsValue(primitive))
            {
                dbPr.Id = idMap.Where(x => x.Value == primitive).ToList()[0].Key;
                dbcontext.dbPrimitives.Update(dbPr);
                idMap.Remove(dbPr.Id);
            }
            else
            {
                dbcontext.dbPrimitives.Add(dbPr);
            }
            return dbPr;
        }

        // we can't update or delete links (they do so automatically) we only need to insert them manually
        private void InsertLink(DiagramDbContext dbcontext, CustomPrimitive linker, CustomPrimitive linkee)
        {
            DbLink dbLk = new DbLink();
            if (idMap.ContainsValue(linker) && idMap.ContainsValue(linkee))
            {
                dbLk.linkerId = idMap.Where(x => x.Value == linker).ToList()[0].Key;
                dbLk.linkeeId = idMap.Where(x => x.Value == linkee).ToList()[0].Key;
                dbcontext.dbLinks.Add(dbLk);
            }
        }

        private void DeletePrimitive(DiagramDbContext dbcontext, CustomPrimitive primitive)
        {
            DbPrimitive dbPr = new DbPrimitive();
            if (idMap.ContainsValue(primitive))
            {
                dbPr.Id = idMap.Where(x => x.Value == primitive).ToList()[0].Key;
                dbcontext.dbPrimitives.Remove(dbPr);
                idMap.Remove(dbPr.Id);
            }
        }

        public void SaveDiagram(Diagram diagram)
        {
            DiagramDbContext dbcontext = new DiagramDbContext(path);
            dbcontext.Database.EnsureCreated();

            Dictionary<DbPrimitive, CustomPrimitive> newIdMap = new Dictionary<DbPrimitive, CustomPrimitive>();
            Dictionary<CustomPrimitive, List<CustomPrimitive>> newLinks = new Dictionary<CustomPrimitive, List<CustomPrimitive>>();

            foreach (var item in diagram.shapes)
            {
                newIdMap.Add(StorePrimitive(dbcontext, item), item);
            }
            foreach (var item in diagram.edges) // this only stores primitive data. for connections:
                                                // 1. if edge exists in idMap and on this diagram, then both of its ends exist in both places too (because of Diagram logic), so corresponding connections exist and we don't update them
                                                // 2. if edge is deleted on diagram, then it will be deleted in idMap loop below and all connections will cascade-delete automatically after SaveChanges
                                                // 3. if edge didn't exist, only then we need to create a new connection. However we can't insert it immediately as edge can connect new rows that don't have keys until SaveChanges() is called,
                                                //      meaning we need to keep track of new connections that we need to add
            {
                if (!idMap.ContainsValue(item))
                {
                    List<CustomPrimitive> list = new List<CustomPrimitive>();
                    list.Add(item.pr1);
                    list.Add(item.pr2);
                    newLinks.Add(item, list);
                }

                newIdMap.Add(StorePrimitive(dbcontext, item), item);
            }
            foreach (var item in idMap) // if there is something left in old idMap, it means those primitives were deleted on the diagram (but not from db yet)
            {
                DeletePrimitive(dbcontext, item.Value);
            }
            dbcontext.SaveChanges();
            foreach (var item in newIdMap)
            {
                idMap.Add(item.Key.Id, item.Value);
            }
            foreach (var item in newLinks)
            {
                foreach (var item1 in item.Value)
                {
                    InsertLink(dbcontext, item.Key, item1);
                }
            }
            dbcontext.SaveChanges();
            dbcontext.Dispose();
        }
    }  
    */
}
