using ErEditor.DbSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

        private void AddRange<TDbEntity>(List<TDbEntity> entities, IEnumerable<TDbEntity> range)
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

            dbcontext.EntitySets
                .Include(el => el.Attributes).ThenInclude(x => x.ValueSets);
            dbcontext.RelationshipSets
                .Include(el => el.Attributes).ThenInclude(x => x.ValueSets)
                .Include(el => el.Roles)
                .Include(el => el.Mappings).ThenInclude(x => x.MappingRoles);
            dbcontext.Diagrams.Include(el => el.Primitives);

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
            }
            foreach (var el in dbcontext.RelationshipSets.Include(el => el.Attributes))
            {
                foreach (var attr in el.Attributes)
                {
                    SchemaRegistry.dbAttrRegistryTemp.AddRetrieved(attr.Id, attr);
                    Console.WriteLine($"Added attr: {attr.Name}");
                }
            }

            ConsoleLog.Log("[0/3] Initiating mapping to database.", this);
            var changes = SchemaRegistry.MakeChangedDbEntries();
            ConsoleLog.Log("[1/3] Entries in registry were created for database.", this);

            dbcontext.RemoveRange(changes.Deleted);
            dbcontext.AddRange(changes.Created);
            dbcontext.UpdateRange(changes.Updated);

            dbcontext.SaveChanges();

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
