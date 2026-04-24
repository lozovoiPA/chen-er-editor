using ErEditor.DbSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ErEditor.ErSchemaClasses
{
    public class ErSchemaRegistry : IObserver, 
        IVisitor<ObjectAddedNotification<ErElementWithAttributes, ErAttribute>>,
        IVisitor<ObjectAddedNotification<ErRelationshipSet, ErRole>>,
        IVisitor<ObjectAddedNotification<ErRelationshipSet, ErMapping>>,
        IVisitor<ObjectAddedNotification<ErDiagram, ErDiagramPrimitive>>,
        IVisitor<ObjectDeletedNotification<ErEntitySet>>,
        IVisitor<ObjectDeletedNotification<ErRelationshipSet>>,
        IVisitor<ObjectDeletedNotification<ErDiagram>>
    {
        public readonly ErSchema Schema;

        public readonly Registry<ErEntitySet> EntitySetRegistry = new();
        public readonly Registry<ErRelationshipSet> RelationshipSetRegistry = new();
        public readonly Registry<ErValueSet> ValueSetRegistry = new();
        public readonly Registry<ErDiagram> DiagramRegistry = new();
        public readonly Registry<ErAttribute> AttributeRegistry = new();
        public readonly Registry<ErRole> RoleRegistry = new();
        public readonly Registry<ErMapping> MappingRegistry = new();
        public readonly Registry<ErDiagramPrimitive> PrimitiveRegistry = new();

        public ObserverBase observerLogic;

        private Dictionary<ErAttribute, ErElementWithAttributes> attributeForeignKeys = new();
        private Dictionary<ErRole, ErRelationshipSet> roleForeignKeys = new();
        private Dictionary<ErMapping, ErRelationshipSet> mappingForeignKeys = new();
        private Dictionary<ErDiagramPrimitive, ErDiagram> primitiveForeignKeys = new();

        public ErSchemaRegistry(ErSchema schema)
        {
            Schema = schema;

            observerLogic = new(this);
            ObserveOn();
        }
        public string PrintState()
        {
            string res = "";
            res += "Entity Set registry:\n====================\n" + EntitySetRegistry.PrintState();
            res += "\nRelationship Set registry:\n====================\n" + RelationshipSetRegistry.PrintState();
            res += "\nValue Set registry:\n====================\n" + ValueSetRegistry.PrintState();
            res += "\nDiagram registry:\n====================\n" + DiagramRegistry.PrintState();

            res += "\nAttributes registry:\n====================\n" + AttributeRegistry.PrintState();
            res += "\nRole registry:\n====================\n" + RoleRegistry.PrintState();
            res += "\nMappings registry:\n====================\n" + MappingRegistry.PrintState();
            res += "\nPrimitives registry:\n====================\n" + PrimitiveRegistry.PrintState();

            return res;
        }

        // MapTo - создание объекта из Dbобъекта
        private ErEntitySet CreateEntitySetOnSchema(DbEntitySet dbEs)
        {
            ErEntitySet entitySet = Schema.EntitySets.Add(dbEs.Name ?? string.Empty);

            var attributes = AttributeRegistry.RetrieveDbEntryList(
                dbEs.Attributes.ToList(), 
                el => CreateAttributeOnSchema(entitySet, el));

            return entitySet;
        }
        private ErRelationshipSet CreateRelationshipSetOnSchema(DbRelationshipSet dbRs)
        {
            ErRelationshipSet relationshipSet = Schema.RelationshipSets.Add(dbRs.Name ?? string.Empty);

            var attributes = AttributeRegistry.RetrieveDbEntryList(
                dbRs.Attributes.ToList(),
                el => CreateAttributeOnSchema(relationshipSet, el));

            var roles = RoleRegistry.RetrieveDbEntryList(
                dbRs.Roles.ToList(),
                el => CreateRoleOnSchema(relationshipSet, el));

            var mappings = MappingRegistry.RetrieveDbEntryList(
                dbRs.Mappings.ToList(),
                el => CreateMappingOnSchema(relationshipSet, el));

            return relationshipSet;
        }
        private ErValueSet CreateValueSetOnSchema(DbValueSet dbVs)
        {
            ErValueSet valueSet = Schema.ValueSets.Add(dbVs.Name == null ? "" : dbVs.Name);
            valueSet.BaseValueType = dbVs.BaseType;
            return valueSet;
        }
        private ErDiagram CreateDiagramOnSchema(DbDiagram dbDgr)
        {
            ErDiagram diagram = Schema.Diagrams.Add(dbDgr.Name ?? string.Empty);
            var primitives = PrimitiveRegistry.RetrieveDbEntryList(
                dbDgr.Primitives.Where(x => x.Type != "Edge").ToList().Concat(dbDgr.Primitives.Where(x => x.Type == "Edge").ToList()).ToList(),
                dbPrimitive => CreatePrimitiveOnDiagram(diagram, dbPrimitive));
            return diagram;
        }
        private ErAttribute CreateAttributeOnSchema(ErElementWithAttributes element, DbAttribute dbAttr)
        {
            ErAttribute attr = element.AddAttribute(dbAttr.Name == null ? "" : dbAttr.Name);
            attr.minValue = dbAttr.MinValue;
            attr.maxValue = dbAttr.MaxValue;
            attr.allowedValues = dbAttr.AllowedValues;
            attr.isKey = dbAttr.IsKey;

            attributeForeignKeys.Add(attr, element);

            foreach (DbValueSet dbVs in dbAttr.ValueSets)
            {
                ErValueSet vs = ValueSetRegistry.RetrieveDbEntry(dbVs, CreateValueSetOnSchema);
                attr.valueSets.Add(vs);
            }
            attr.valueSets.Reverse();
            return attr;
        }
        private ErRole CreateRoleOnSchema(ErRelationshipSet rs, DbRole dbRole)
        {   
            ErRole role = rs.AddRole(dbRole.Name ?? string.Empty);
            role.IsKey = dbRole.IsKeyEntitySet;
            role.IsIdDependency = dbRole.IsIdDependant;

            roleForeignKeys.Add(role, rs);

            var es = EntitySetRegistry.RetrieveDbEntry(dbRole.EntitySet, CreateEntitySetOnSchema);
            role.EntitySet = es;
            
            return role;
        }
        private ErDiagramPrimitive? CreatePrimitiveOnDiagram(ErDiagram diagram, DbPrimitive dbPrimitive)
        {
            ErDiagramPrimitive? primitive = null;
            switch (dbPrimitive)
            {
                case DbRectangle dbRect:
                    var entitySet = EntitySetRegistry.RetrieveDbEntry(dbRect.ElementWithAttributes, CreateEntitySetOnSchema);
                    primitive = diagram.AddRectangle(entitySet, dbPrimitive.X, dbPrimitive.Y, dbPrimitive.width, dbPrimitive.height);
                    break;
                case DbDiamond dbDiamond:
                    var relationshipSet = RelationshipSetRegistry.RetrieveDbEntry(dbDiamond.ElementWithAttributes, CreateRelationshipSetOnSchema);
                    primitive = diagram.AddDiamond(relationshipSet, dbPrimitive.X, dbPrimitive.Y, dbPrimitive.width, dbPrimitive.height);
                    break;
                case DbEdge dbEdge:
                    var roleRelationshipSet = RelationshipSetRegistry.RetrieveDbEntry(dbEdge.Role.RelationshipSet, CreateRelationshipSetOnSchema);
                    var role = RoleRegistry.RetrieveDbEntry(dbEdge.Role, 
                        dbRole => 
                        {
                            return CreateRoleOnSchema(roleRelationshipSet, dbRole);
                        });
                    primitive = diagram.AddEdge(role, roleRelationshipSet,
                        new Point(dbPrimitive.X, dbPrimitive.Y), 
                        new Point(dbPrimitive.width, dbPrimitive.height));
                    break;
            }
            if(primitive != null)
            {
                primitiveForeignKeys.Add(primitive, diagram);
            }
            Console.WriteLine($"Primitive: {primitive}, DbPrimitive: {dbPrimitive}");
            return primitive;
        }
        private ErMapping CreateMappingOnSchema(ErRelationshipSet rs, DbMapping dbMapping)
        {
            ErMapping mapping = rs.AddMapping(dbMapping.Name ?? string.Empty);
            mapping.MinCardinalityOfPreimage = (int)dbMapping.MinCardinalityOfPreImage;
            mapping.MaxCardinalityOfPreimage = (int)dbMapping.MaxCardinalityOfPreImage;
            mapping.MinCardinalityOfImage = (int)dbMapping.MinCardinalityOfImage;
            mapping.MaxCardinalityOfImage = (int)dbMapping.MaxCardinalityOfImage;

            mappingForeignKeys.Add(mapping, rs);

            var dbMappingRoles = dbMapping.MappingRoles;
            foreach(var dbMappingRole in dbMappingRoles)
            {
                var role = RoleRegistry.RetrieveDbEntry(dbMappingRole.Role, role => CreateRoleOnSchema(rs, role));
                if(dbMappingRole.Type == "image")
                {
                    mapping.AddToImage(role);
                }
                else{
                    mapping.AddToPreImage(role);
                }
            }

            return mapping;
        }

        private int TryToAssignId<TObject>(TObject el, Registry<TObject> registry)
            where TObject : notnull
        {
            return registry.FindId(el) ?? default;
        }
        // Старые попытки как-то хранить ограничения внешних ключей... Ничего не понимаю, как их адекватно поддерживать.
        // Пока использую методы выше целиком для маппинга всего вместе. 
        public DbEntitySet MakeDbEntitySet(ErEntitySet es)
        {
            DbEntitySet dbEs = new DbEntitySet(es.Name == "" ? null : es.Name);
            dbEs.Id = TryToAssignId(es, EntitySetRegistry);

            return dbEs;
        }
        public DbRelationshipSet MakeDbRelationshipSet(ErRelationshipSet rs)
        {
            DbRelationshipSet dbRs = new(rs.Name == "" ? null : rs.Name);
            dbRs.Id = TryToAssignId(rs, RelationshipSetRegistry);

            return dbRs;
        }
        public Registry<DbAttribute> dbAttrRegistryTemp = new();
        public Registry<DbValueSet> dbVsRegistryTemp = new(); 
        public DbValueSet MakeDbValueSet(ErValueSet vs)
        {
            int id = TryToAssignId(vs, ValueSetRegistry);
            DbValueSet? dbVs = null;
            if (id != default)
            {
                dbVs = dbVsRegistryTemp.FindById(id);
            }
            if(dbVs == null)
            {
                dbVs = new(vs.Name == "" ? null : vs.Name);
                dbVs.BaseType = vs.BaseValueType;
                dbVs.Id = id;

                if(id != default)
                {
                    dbVsRegistryTemp.AddRetrieved(dbVs.Id, dbVs);
                }
            }
            else
            {
                dbVs.Name = vs.Name == "" ? null : vs.Name;
                dbVs.BaseType = vs.BaseValueType;
            }
            return dbVs;
        }
        public DbDiagram MakeDbDiagram(ErDiagram dgr)
        {
            DbDiagram dbDgr = new DbDiagram(dgr.Name == "" ? null : dgr.Name);
            dbDgr.Id = TryToAssignId(dgr, DiagramRegistry);

            return dbDgr;
        }
        private DbAttribute? MakeDbAttribute(ErAttribute attr)
        {
            int attrid = TryToAssignId(attr, AttributeRegistry);
            DbAttribute? dbAttr = dbAttrRegistryTemp.FindById(attrid);

            if(dbAttr == null)
            {
                Console.WriteLine("Attr not found");
                dbAttr = new DbAttribute(attr.Name == "" ? null : attr.Name);
            }
            else
            {
                Console.WriteLine("Attr found");

                //foreach(var vs in dbAttr.)
            }

                int? id = null;
            DbErElementWithAttributes? dbEl = null;
            if (!attributeForeignKeys.ContainsKey(attr))
            {
                ConsoleLog.Log("Attribute foreign key entity couldn't be found in the foreign keys. Entity will not be saved in the database.");
                return null;
            }
            var el = attributeForeignKeys[attr];
            ErEntitySet? es = el as ErEntitySet;
            if(es != null)
            {
                id = EntitySetRegistry.FindId(es);
                if(id == null)
                {
                    if (!EntitySetRegistry.CreatedDbEntries.ContainsKey(es))
                    {
                        ConsoleLog.Log("Attribute foreign key entity couldn't be found in the registry. Entity will not be saved in the database.");
                        return null;
                    }
                    dbEl = (DbErElementWithAttributes?)EntitySetRegistry.CreatedDbEntries[es];
                }
                else if(EntitySetRegistry.Deleted.Contains(es))
                {
                    ConsoleLog.Log("Attribute foreign key entity set is marked for deletion. Attribute will not be saved in the database.");
                    return null;
                }
            }
            else
            {
                ErRelationshipSet? rs = el as ErRelationshipSet;
                if(rs != null)
                {
                    id = RelationshipSetRegistry.FindId(rs);
                    if (id == null)
                    {
                        if (!RelationshipSetRegistry.CreatedDbEntries.ContainsKey(rs))
                        {
                            ConsoleLog.Log("Attribute foreign key entity couldn't be found in the registry. Entity will not be saved in the database.");
                            return null;
                        }
                        dbEl = (DbErElementWithAttributes?)RelationshipSetRegistry.CreatedDbEntries[rs];
                    }
                    else if (RelationshipSetRegistry.Deleted.Contains(rs))
                    {
                        ConsoleLog.Log("Attribute foreign key relationship set is marked for deletion. Attribute will not be saved in the database.");
                        return null;
                    }
                }
            }

            if(id != null)
            {
                dbAttr.ErElementWithAttributesId = (int)id;
            }
            else if(dbEl != null)
            {
                dbAttr.ErElementWithAttributes = dbEl;
            }
            else
            {
                ConsoleLog.Log("Attribute foreign key ID couldn't be found. Entity will not be saved in the database.");
                return null;
            }

            dbAttr.MinValue = attr.minValue;
            dbAttr.MaxValue = attr.maxValue;
            dbAttr.AllowedValues = attr.allowedValues;
            dbAttr.Id = TryToAssignId(attr, AttributeRegistry);

            
            foreach (var valueSet in attr.valueSets)
            {
                //DbAttributeDbValueSet pair = new();
                (int? valueSetId, DbValueSet? dbValueSet) = FindForeignKeyConstraint<DbValueSet, ErValueSet>(valueSet, ValueSetRegistry);

                if (valueSetId != null && dbValueSet == null)
                {
                    dbValueSet = MakeDbValueSet(valueSet);
                }
                if (dbValueSet != null && !dbAttr.ValueSets.Contains(dbValueSet))
                {
                    dbAttr.ValueSets.Add(dbValueSet);
                }
                break;
            }

            return dbAttr;
        }
        private DbRole? MakeDbRole(ErRole role)
        {
            var es = role.EntitySet;

            DbRole dbRole = new DbRole(role.Name == "" ? null : role.Name);

            int? esId = null, rsId = null;
            DbEntitySet? dbEs = null; DbRelationshipSet? dbRs = null; 
            if (!roleForeignKeys.ContainsKey(role))
            {
                ConsoleLog.Log("Role foreign key entity couldn't be found in the foreign keys. Entity will not be saved in the database.");
                return null;
            }
            var rs = roleForeignKeys[role];
            rsId = RelationshipSetRegistry.FindId(rs);
            if (rsId == null)
            {
                if (!RelationshipSetRegistry.CreatedDbEntries.ContainsKey(rs))
                {
                    ConsoleLog.Log("Role foreign key relationship set couldn't be found in the registry. Entity will not be saved in the database.");
                    return null;
                }
                dbRs = (DbRelationshipSet?)RelationshipSetRegistry.CreatedDbEntries[rs];
            }

            esId = EntitySetRegistry.FindId(es);
            if(esId == null)
            {
                if (!EntitySetRegistry.CreatedDbEntries.ContainsKey(es))
                {
                    ConsoleLog.Log("Role foreign key entity set couldn't be found in the registry. Entity will not be saved in the database.");
                    return null;
                }
                dbEs = (DbEntitySet?)EntitySetRegistry.CreatedDbEntries[es];
            }

            if (rsId != null)
            {
                dbRole.RelationshipSetId = (int)rsId;
            }
            else if (dbRs != null)
            {
                dbRole.RelationshipSet = dbRs;
            }
            else
            {
                ConsoleLog.Log("Role foreign key ID couldn't be found. Entity will not be saved in the database.");
                return null;
            }

            if (esId != null)
            {
                dbRole.EntitySetId = (int)esId;
            }
            else if (dbEs != null)
            {
                dbRole.EntitySet = dbEs;
            }
            else
            {
                ConsoleLog.Log("Role foreign key ID couldn't be found. Entity will not be saved in the database.");
                return null;
            }

            dbRole.IsIdDependant = role.IsIdDependency;
            dbRole.IsKeyEntitySet = role.IsKey;

            dbRole.Id = TryToAssignId(role, RoleRegistry);

            return dbRole;
        }

        private (int? EntryId, TDbRow? EntryCandidate) FindForeignKeyConstraint<TDbRow, TElement>(TElement element, Registry<TElement> registry)
            where TElement: notnull
            where TDbRow : class, IDbEntry
        {
            // Find id or row candidate for creation
            int? elementId = registry.FindId(element);
            TDbRow? dbElement = null;

            //dbElement = (TDbRow?)registry.CreatedDbEntries[element] ?? (TDbRow?)registry.UpdatedDbEntries[element] ?? (TDbRow?)registry.DeletedDbEntries[element];
            if (registry.CreatedDbEntries.ContainsKey(element))
            {
                dbElement = (TDbRow?)registry.CreatedDbEntries[element];
            }
            else if (registry.UpdatedDbEntries.ContainsKey(element))
            {
                dbElement = (TDbRow?)registry.UpdatedDbEntries[element];
            }
            else if (registry.DeletedDbEntries.ContainsKey(element))
            {
                dbElement = (TDbRow?)registry.DeletedDbEntries[element];
            }
            if (elementId == null && dbElement == null)
            {
                ConsoleLog.Log("Foreign key constraint couldn't be found in the registry. " +
                    "Entity will not be saved in the database.", this);
            }
            return (elementId, dbElement);
        }
        
        private DbMapping? MakeDbMapping(ErMapping mapping)
        {
            if (!mappingForeignKeys.ContainsKey(mapping))
            {
                ConsoleLog.Log("Mapping foreign key entity couldn't be found in the foreign keys. Entity will not be saved in the database.");
                return null;
            }

            ErRelationshipSet rs = mappingForeignKeys[mapping];
            (int? rsId, DbRelationshipSet? dbRs) = FindForeignKeyConstraint<DbRelationshipSet, ErRelationshipSet>(rs, RelationshipSetRegistry);
            if (rsId == null && dbRs == null)
            {
                ConsoleLog.Log("can't find rs for map");
                return null;
            }

            DbMapping dbMap = new(mapping.Name);

            if (rsId != null)
            {
                dbMap.RelationshipSetId = (int)rsId;
            }
            else
            {
                dbMap.RelationshipSet = dbRs!;
            }

            dbMap.MaxCardinalityOfPreImage = mapping.MaxCardinalityOfPreimage;
            dbMap.MinCardinalityOfPreImage = mapping.MinCardinalityOfPreimage;
            dbMap.MaxCardinalityOfImage = mapping.MaxCardinalityOfImage;
            dbMap.MinCardinalityOfImage = mapping.MinCardinalityOfImage;

            dbMap.Id = TryToAssignId(mapping, MappingRegistry);


            foreach (var role in mapping.Image.Concat(mapping.PreImage))
            {
                DbMappingDbRole pair = new();
                (int? roleId, DbRole? dbRole) = FindForeignKeyConstraint<DbRole, ErRole>(role, RoleRegistry);

                if (mapping.Image.Contains(role))
                {
                    pair.Type = "image";
                }
                else
                {
                    pair.Type = "preimage";
                }

                if (roleId != null)
                {
                    pair.MappingId = dbMap.Id;
                    pair.RoleId = (int)roleId;
                    dbMap.MappingRoles.Add(pair);
                }
                else if(dbRole != null)
                {
                    pair.MappingId = dbMap.Id;
                    pair.Role = dbRole;
                    dbMap.MappingRoles.Add(pair);
                }

            }
            ConsoleLog.Log($"\tMapping Id: {dbMap.Id}");

            return dbMap;
        }
        private DbPrimitive? MakeDbPrimitive(ErDiagramPrimitive primitive)
        {
            // Find diagram foreign key constraint
            if (!primitiveForeignKeys.ContainsKey(primitive))
            {
                ConsoleLog.Log("Primitive foreign key entity couldn't be found in the foreign keys. Entity will not be saved in the database.");
                return null;
            }

            ErDiagram diagram = primitiveForeignKeys[primitive];
            (int? diagramId, DbDiagram? dbDiagram) = FindForeignKeyConstraint<DbDiagram, ErDiagram>(diagram, DiagramRegistry);
            if(diagramId == null && dbDiagram == null)
            {
                ConsoleLog.Log("can't find diagram for primitive");
                return null;
            }

            DbPrimitive? dbPrimitive = null;
            switch (primitive)
            {
                case ErDiagramRectangle rect:
                    (int? entitySetId, DbEntitySet? dbEntitySet) 
                        = FindForeignKeyConstraint<DbEntitySet, ErEntitySet>(rect.ErElement, EntitySetRegistry);
                    if(entitySetId == null && dbEntitySet == null)
                    {
                        ConsoleLog.Log("can't find entity set for primitive");
                        return null;
                    }
                    DbRectangle dbRect = new DbRectangle();
                    if(entitySetId != null)
                    {
                        dbRect.ElementWithAttributesId = (int)entitySetId;
                    }
                    else
                    {
                        dbRect.ElementWithAttributes = dbEntitySet!;
                    }
                    dbPrimitive = dbRect;
                    dbPrimitive.Type = "Rectangle";
                    break;
                case ErDiagramDiamond diamond:
                    (int? relationshipSetId, DbRelationshipSet? dbRelationshipSet)
                        = FindForeignKeyConstraint<DbRelationshipSet, ErRelationshipSet>(diamond.ErElement, RelationshipSetRegistry);
                    if (relationshipSetId == null && dbRelationshipSet == null)
                    {
                        ConsoleLog.Log("can't find rel set for primitive");
                        return null;
                    }
                    DbDiamond dbDiamond = new DbDiamond();
                    if (relationshipSetId != null)
                    {
                        dbDiamond.ElementWithAttributesId = (int)relationshipSetId;
                    }
                    else
                    {
                        dbDiamond.ElementWithAttributes = dbRelationshipSet!;
                    }
                    dbPrimitive = dbDiamond;
                    dbPrimitive.Type = "Diamond";
                    break;
                case ErDiagramEdge edge:
                    (int? roleId, DbRole? dbRole)
                        = FindForeignKeyConstraint<DbRole, ErRole>(edge.ErElement, RoleRegistry);
                    if (roleId == null && dbRole == null)
                    {
                        ConsoleLog.Log("can't find role for primitive");
                        return null;
                    }
                    DbEdge dbEdge = new DbEdge();
                    if (roleId != null)
                    {
                        dbEdge.RoleId = (int)roleId;
                    }
                    else
                    {
                        dbEdge.Role = dbRole!;
                    }
                    dbPrimitive = dbEdge;
                    dbPrimitive.Type = "Edge";
                    break;

            }
            if(dbPrimitive == null)
            {
                return null;
            }

            // через рефлексию такое можно закодить... ну или хотя бы через макрос
            if(diagramId != null)
            {
                dbPrimitive.DiagramId = (int)diagramId;
            }
            else
            {
                dbPrimitive.Diagram = dbDiagram!;
            }

            dbPrimitive.X = primitive.X;
            dbPrimitive.Y = primitive.Y;
            dbPrimitive.width = primitive.width;
            dbPrimitive.height = primitive.height;

            return dbPrimitive;
        }

        public void GetSchemaFromDb(ErSchemaDbObjects dbSchema)
        {
            ObserveOff();

            EntitySetRegistry.RetrieveDbEntryList(dbSchema.DbEntitySets, CreateEntitySetOnSchema);
            RelationshipSetRegistry.RetrieveDbEntryList(dbSchema.DbRelationshipSets, CreateRelationshipSetOnSchema);
            ValueSetRegistry.RetrieveDbEntryList(dbSchema.DbValueSets, CreateValueSetOnSchema);
            DiagramRegistry.RetrieveDbEntryList(dbSchema.DbDiagrams, CreateDiagramOnSchema);

            ObserveOn();
        }

        private List<IDbEntry> MakeCreatedDbEntriesList<TObject, TDbEntry>(Registry<TObject> registry, Func<TObject, TDbEntry?> mapFunc)
            where TObject : notnull
            where TDbEntry : IDbEntry
        {
            List<IDbEntry> created = new();
            foreach (var el in registry.Created)
            {
                var dbEl = mapFunc(el);
                if (dbEl != null)
                {
                    registry.AddCreatedDbEntry(el, dbEl);
                    created.Add(dbEl);
                }
            }
            return created;
        }
        private List<IDbEntry> MakeUpdatedDbEntriesList<TObject, TDbEntry>(Registry<TObject> registry, Func<TObject, TDbEntry?> mapFunc)
            where TObject : notnull
            where TDbEntry : IDbEntry
        {
            List<IDbEntry> updated = new();
            foreach (var el in registry.Updated)
            {
                var dbEl = mapFunc(el);
                if (dbEl != null)
                {
                    registry.AddUpdatedDbEntry(el, dbEl);
                    updated.Add(dbEl);
                }
            }
            return updated;
        }
        private List<IDbEntry> MakeDeletedDbEntriesList<TObject, TDbEntry>(Registry<TObject> registry, Func<TObject, TDbEntry?> mapFunc)
            where TObject : notnull
            where TDbEntry : IDbEntry
        {
            List<IDbEntry> deleted = new();
            foreach (var el in registry.Deleted)
            {
                var dbEl = mapFunc(el);
                if (dbEl != null)
                {
                    registry.AddDeletedDbEntry(el, dbEl);
                    deleted.Add(dbEl);
                }
            }
            return deleted;
        }
        public DbSchemaChanges MakeChangedDbEntries()
        {
            DbSchemaChanges changes = new();

            List<IDbEntry> created = new();
            List<IDbEntry> updated = new();
            List<IDbEntry> deleted = new();

            ConsoleLog.Log("CREATING");

            created.AddRange(MakeCreatedDbEntriesList(EntitySetRegistry, MakeDbEntitySet));
            created.AddRange(MakeCreatedDbEntriesList(RelationshipSetRegistry, MakeDbRelationshipSet));
            created.AddRange(MakeCreatedDbEntriesList(ValueSetRegistry, MakeDbValueSet));
            created.AddRange(MakeCreatedDbEntriesList(DiagramRegistry, MakeDbDiagram));
            created.AddRange(MakeCreatedDbEntriesList(AttributeRegistry, MakeDbAttribute));
            created.AddRange(MakeCreatedDbEntriesList(RoleRegistry, MakeDbRole));
            created.AddRange(MakeCreatedDbEntriesList(MappingRegistry, MakeDbMapping));
            created.AddRange(MakeCreatedDbEntriesList(PrimitiveRegistry, MakeDbPrimitive));

            ConsoleLog.Log("UPDATING");

            updated.AddRange(MakeUpdatedDbEntriesList(EntitySetRegistry, MakeDbEntitySet));
            updated.AddRange(MakeUpdatedDbEntriesList(RelationshipSetRegistry, MakeDbRelationshipSet));
            updated.AddRange(MakeUpdatedDbEntriesList(ValueSetRegistry, MakeDbValueSet));
            updated.AddRange(MakeUpdatedDbEntriesList(DiagramRegistry, MakeDbDiagram));
            updated.AddRange(MakeUpdatedDbEntriesList(AttributeRegistry, MakeDbAttribute));
            updated.AddRange(MakeUpdatedDbEntriesList(RoleRegistry, MakeDbRole));
            updated.AddRange(MakeUpdatedDbEntriesList(MappingRegistry, MakeDbMapping));
            updated.AddRange(MakeUpdatedDbEntriesList(PrimitiveRegistry, MakeDbPrimitive));

            ConsoleLog.Log("DELETING");

            deleted.AddRange(MakeDeletedDbEntriesList(EntitySetRegistry, MakeDbEntitySet));
            deleted.AddRange(MakeDeletedDbEntriesList(RelationshipSetRegistry, MakeDbRelationshipSet));
            deleted.AddRange(MakeDeletedDbEntriesList(ValueSetRegistry, MakeDbValueSet));
            deleted.AddRange(MakeDeletedDbEntriesList(DiagramRegistry, MakeDbDiagram));
            deleted.AddRange(MakeDeletedDbEntriesList(AttributeRegistry, MakeDbAttribute));
            deleted.AddRange(MakeDeletedDbEntriesList(RoleRegistry, MakeDbRole));
            deleted.AddRange(MakeDeletedDbEntriesList(MappingRegistry, MakeDbMapping));
            deleted.AddRange(MakeDeletedDbEntriesList(PrimitiveRegistry, MakeDbPrimitive));

            changes.AddCreatedRange(created);
            changes.AddUpdatedRange(updated);
            changes.AddDeletedRange(deleted);

            return changes;
        }
        private void ObserveOff()
        {
            Schema.EntitySetWatcher.Unsubscribe(EntitySetRegistry);
            Schema.RelationshipSetWatcher.Unsubscribe(RelationshipSetRegistry);
            Schema.ValueSetWatcher.Unsubscribe(ValueSetRegistry);
            Schema.DiagramWatcher.Unsubscribe(DiagramRegistry);

            Schema.EntitySetWatcher.Unsubscribe(AttributeRegistry);
            Schema.RelationshipSetWatcher.Unsubscribe(AttributeRegistry);
            Schema.EntitySetWatcher.Unsubscribe(this);
            Schema.RelationshipSetWatcher.Unsubscribe(this);

            Schema.RelationshipSetWatcher.Unsubscribe(RoleRegistry);
            Schema.RelationshipSetWatcher.Unsubscribe(MappingRegistry);
            Schema.DiagramWatcher.Unsubscribe(PrimitiveRegistry);
            Schema.DiagramWatcher.Unsubscribe(this);
        }
        private void ObserveOn()
        {
            Schema.EntitySetWatcher.Subscribe(EntitySetRegistry);
            Schema.RelationshipSetWatcher.Subscribe(RelationshipSetRegistry);
            Schema.ValueSetWatcher.Subscribe(ValueSetRegistry);
            Schema.DiagramWatcher.Subscribe(DiagramRegistry);

            Schema.EntitySetWatcher.Subscribe(AttributeRegistry);
            Schema.RelationshipSetWatcher.Subscribe(AttributeRegistry);
            Schema.EntitySetWatcher.Subscribe(this);
            Schema.RelationshipSetWatcher.Subscribe(this);

            Schema.RelationshipSetWatcher.Subscribe(RoleRegistry);
            Schema.RelationshipSetWatcher.Subscribe(MappingRegistry);
            Schema.DiagramWatcher.Subscribe(PrimitiveRegistry);
            Schema.DiagramWatcher.Subscribe(this);
        }

        public void FlushRegistries()
        {
            EntitySetRegistry.Flush();
            RelationshipSetRegistry.Flush();
            ValueSetRegistry.Flush();
            DiagramRegistry.Flush();
            AttributeRegistry.Flush();
            RoleRegistry.Flush();
            MappingRegistry.Flush();
            PrimitiveRegistry.Flush();

            dbAttrRegistryTemp = new();
            dbVsRegistryTemp = new();
        }

        // Распаковщик уведомлений от вотчеров к общим уведомлениям реестров (чтобы реестры не менять и у них у всех
        // ожидаемое и одинаковое поведение)
        // (опять же, его можно было бы поменять и через наследование, но здесь я сделал по-другому. масштабируемо!)
        // Это надо для сохранения внешних ключей (которые иначе надо было бы представлять обратной связью от дочерних объектов к родителям)
        public void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
        public void Visit(ObjectAddedNotification<ErElementWithAttributes, ErAttribute> notif)
        {
            AttributeRegistry.Visit(new ObjectCreatedNotification<ErAttribute>(notif.ObjectAdded));
            attributeForeignKeys.Add(notif.ObjectAdded, notif.ObjectAddedTo);
        }
        public void Visit(ObjectAddedNotification<ErRelationshipSet, ErRole> notif)
        {
            RoleRegistry.Visit(new ObjectCreatedNotification<ErRole>(notif.ObjectAdded));
            roleForeignKeys.Add(notif.ObjectAdded, notif.ObjectAddedTo);
        }
        public void Visit(ObjectAddedNotification<ErDiagram, ErDiagramPrimitive> notif)
        {
            PrimitiveRegistry.Visit(new ObjectCreatedNotification<ErDiagramPrimitive>(notif.ObjectAdded));
            primitiveForeignKeys.Add(notif.ObjectAdded, notif.ObjectAddedTo);
        }
        public void Visit(ObjectAddedNotification<ErRelationshipSet, ErMapping> notif)
        {
            MappingRegistry.Visit(new ObjectCreatedNotification<ErMapping>(notif.ObjectAdded));
            mappingForeignKeys.Add(notif.ObjectAdded, notif.ObjectAddedTo);
        }

        // might not be useful when implementing Ctrl+Z stuff of restoring
        public void Visit(ObjectDeletedNotification<ErEntitySet> notification)
        {
            foreach(var attribute in notification.Object.Attributes)
            {
                AttributeRegistry.RemoveRetrieved(attribute);
            }
        }
        public void Visit(ObjectDeletedNotification<ErRelationshipSet> notification)
        {
            foreach (var attribute in notification.Object.Attributes)
            {
                AttributeRegistry.RemoveRetrieved(attribute);
            }
            foreach (var role in notification.Object.Roles)
            {
                RoleRegistry.RemoveRetrieved(role);
            }
            foreach (var mapping in notification.Object.Mappings)
            {
                MappingRegistry.RemoveRetrieved(mapping);
            }
        }
        public void Visit(ObjectDeletedNotification<ErDiagram> notification)
        {
            foreach (var primitive in notification.Object)
            {
                PrimitiveRegistry.RemoveRetrieved(primitive);
            }
        }
    }
}
