using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.DbSchemaClasses
{
    /*
    public class ErSchemaDbMapper
    {
        private ErSchema schema;
        private ErSchemaRegistry schemaRegistry;
        private ErDbContext dbcontext;

        public ErSchemaDbMapper(ErSchema schema, ErSchemaRegistry schemaRegistry, ErDbContext dbcontext) 
        {
            this.schema = schema;
            this.schemaRegistry = schemaRegistry;
            this.dbcontext = dbcontext;
        }

        // this split (between retrieving into the registry and adding retrieved elements to schema) could be useful to delay 
        // adding elements to schema (but they'll still be in the registry, creating intergity of elements whenever we save to the database).
        public ErSchema MapFromDatabase()
        {
            schemaRegistry.GetSchemaFromDb(dbcontext);

            return schema;
        }
        public void MapToDatabase()
        {
            ConsoleLog.Log("[0/3] Initiating mapping to database.", this);
            MapErSetToDbSet(this.schemaRegistry.EntitySetRegistry, schemaRegistry.MakeDbEntitySet);
            MapErSetToDbSet(this.schemaRegistry.RelationshipSetRegistry, schemaRegistry.MakeDbRelationshipSet);
            MapErSetToDbSet(this.schemaRegistry.ValueSetRegistry, schemaRegistry.MakeDbValueSet);
            MapErSetToDbSet(this.schemaRegistry.DiagramRegistry, schemaRegistry.MakeDbDiagram);

            ConsoleLog.Log("[1/3] Entries in registry were created for database.", this);

            dbcontext.SaveChanges();
            ConsoleLog.Log("[2/3] Schema was saved to database.", this);
            ConsoleLog.Log("[2/3] Entity Ids will be collected to flush the registries.", this);
            this.schemaRegistry.FlushRegistries();
            ConsoleLog.Log("[3/3] Registries have been flushed.", this);
            ConsoleLog.Log("[3/3] Schema was fully saved, you can continue working.", this);
        }
        private void MapErSetToDbSet<TDbEntry, TErElement>(Registry<TErElement> erRegistry, Func<TErElement, TDbEntry> mapFunc) 
            where TDbEntry : notnull, IDbEntry
            where TErElement : notnull
        {
            MapChanges(erRegistry.Updated, mapFunc, dbEl => { this.dbcontext.Update(dbEl); });
            MapChanges(erRegistry.Created, mapFunc, dbEl => { this.dbcontext.Add(dbEl); });
            MapChanges(erRegistry.Deleted, mapFunc, dbEl => { this.dbcontext.Remove(dbEl); });
        }
        private void MapChanges<TDbEntry, TObjEntry>(IEnumerable<TObjEntry> erSet, Func<TObjEntry, TDbEntry> mapFunc, Action<TDbEntry> changeFunc) 
            where TDbEntry : notnull
            where TObjEntry : notnull
        {
            TDbEntry dbEl;
            foreach (TObjEntry erEl in erSet)
            {
                dbEl = mapFunc(erEl);
                changeFunc(dbEl);
            }
        }

        /*
        private ErRole MapToRole(DbRole dbRole)
        {
            ErRole role;
            var inMemoryIdMap = roleIdMap;

            if (inMemoryIdMap.ContainsKey(dbRole.Id))
            {
                role = inMemoryIdMap[dbRole.Id];
            }
            else
            {
                role = new ErRole(dbRole.Name == null ? "" : dbRole.Name, RetrieveEntitySet(dbRole.EntitySet));
                role.isKey = dbRole.IsKeyEntitySet;
                role.isIdDependency = dbRole.IsIdDependant;
                inMemoryIdMap[dbRole.Id] = role;
            }
            return role;
        }
        private DbRole MapFromRole(ErRole role)
        {
            DbRole dbRole = new DbRole();
            dbRole.Name = role.Name;
            var inMemoryIdMap = roleIdMap;

            if (role.entitySet != null)
            {
                int key = (entitySetIdMap.Where(y => y.Value == role.entitySet).ToList()[0].Key);
                var dbes = dbcontext!.EntitySets.Where(x => x.Id == key).ToList()[0];
                dbRole.EntitySet = dbes;
            }
            dbRole.IsKeyEntitySet = role.isKey;
            if (inMemoryIdMap.ContainsValue(role))
            {
                dbRole.Id = inMemoryIdMap.Where(x => x.Value == role).ToList()[0].Key;
                dbcontext!.Update(dbRole);
            }
            else
            {
                dbcontext!.Add(dbRole);
            }
            return dbRole;
        }
    }*/
}
