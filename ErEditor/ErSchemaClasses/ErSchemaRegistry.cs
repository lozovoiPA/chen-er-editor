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
using System.Windows.Media.Media3D;

namespace ErEditor.ErSchemaClasses
{
    public class ErSchemaRegistry : IObserver, 
        IVisitor<ObjectAddedNotification<ErElementWithAttributes, ErAttribute>>,
        IVisitor<ObjectAddedNotification<ErRelationshipSet, ErRole>>
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
                el =>
                {
                    var attr = CreateAttributeOnSchema(entitySet, el);
                    return attr;
                });

            return entitySet;
        }
        private ErRelationshipSet CreateRelationshipSetOnSchema(DbRelationshipSet dbRs)
        {
            ErRelationshipSet relationshipSet = Schema.RelationshipSets.Add(dbRs.Name ?? string.Empty);

            var attributes = AttributeRegistry.RetrieveDbEntryList(
                dbRs.Attributes.ToList(),
                el =>
                {
                    var attr = CreateAttributeOnSchema(relationshipSet, el);
                    return attr;
                });

            var roles = RoleRegistry.RetrieveDbEntryList(
                dbRs.Roles.ToList(),
                el =>
                {
                    var role = CreateRoleOnSchema(relationshipSet, el);
                    return role;
                });
            return relationshipSet;
        }
        private ErValueSet CreateValueSetOnSchema(DbValueSet dbVs)
        {
            ErValueSet valueSet = Schema.ValueSets.Add(dbVs.Name == null ? "" : dbVs.Name);
            return valueSet;
        }
        private ErDiagram CreateDiagramOnSchema(DbDiagram dbDgr)
        {
            ErDiagram diagram = Schema.Diagrams.Add(dbDgr.Name ?? string.Empty);
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
        public DbValueSet MakeDbValueSet(ErValueSet vs)
        {
            DbValueSet dbVs = new(vs.Name == "" ? null : vs.Name);
            dbVs.Id = TryToAssignId(vs, ValueSetRegistry);

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
            DbAttribute dbAttr = new DbAttribute(attr.Name == "" ? null : attr.Name);

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

            return dbAttr;
        }
        private DbRole? MakeDbRole(ErRole role)
        {
            var es = role.EntitySet;
            if (es == null)
            {
                ConsoleLog.Log("Role doesn't have an entity set assigned to it. It will not be saved in the database.");
                return null;
            }

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

        public void GetSchemaFromDb(DbSchema dbSchema)
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

            created.AddRange(MakeCreatedDbEntriesList(EntitySetRegistry, MakeDbEntitySet));
            created.AddRange(MakeCreatedDbEntriesList(RelationshipSetRegistry, MakeDbRelationshipSet));
            created.AddRange(MakeCreatedDbEntriesList(ValueSetRegistry, MakeDbValueSet));
            created.AddRange(MakeCreatedDbEntriesList(DiagramRegistry, MakeDbDiagram));
            created.AddRange(MakeCreatedDbEntriesList(AttributeRegistry, MakeDbAttribute));
            created.AddRange(MakeCreatedDbEntriesList(RoleRegistry, MakeDbRole));

            updated.AddRange(MakeUpdatedDbEntriesList(EntitySetRegistry, MakeDbEntitySet));
            updated.AddRange(MakeUpdatedDbEntriesList(RelationshipSetRegistry, MakeDbRelationshipSet));
            updated.AddRange(MakeUpdatedDbEntriesList(ValueSetRegistry, MakeDbValueSet));
            updated.AddRange(MakeUpdatedDbEntriesList(DiagramRegistry, MakeDbDiagram));
            updated.AddRange(MakeUpdatedDbEntriesList(AttributeRegistry, MakeDbAttribute));
            updated.AddRange(MakeUpdatedDbEntriesList(RoleRegistry, MakeDbRole));

            deleted.AddRange(MakeDeletedDbEntriesList(EntitySetRegistry, MakeDbEntitySet));
            deleted.AddRange(MakeDeletedDbEntriesList(RelationshipSetRegistry, MakeDbRelationshipSet));
            deleted.AddRange(MakeDeletedDbEntriesList(ValueSetRegistry, MakeDbValueSet));
            deleted.AddRange(MakeDeletedDbEntriesList(DiagramRegistry, MakeDbDiagram));
            deleted.AddRange(MakeDeletedDbEntriesList(AttributeRegistry, MakeDbAttribute));
            deleted.AddRange(MakeDeletedDbEntriesList(RoleRegistry, MakeDbRole));

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
        }

        // Распаковщик уведомлений от вотчеров к общим уведомлениям реестров (чтобы реестры не менять и у них у всех
        // ожидаемое и одинаковое поведение)
        // (опять же, его можно было бы поменять и через наследование, но здесь я сделал по-другому. масштабируемо!)
        public void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
        public void Visit(ObjectAddedNotification<ErElementWithAttributes, ErAttribute> notif)
        {
            AttributeRegistry.Recieve(new ObjectCreatedNotification<ErAttribute>(notif.ObjectAdded));
            attributeForeignKeys.Add(notif.ObjectAdded, notif.ObjectAddedTo);
        }
        public void Visit(ObjectAddedNotification<ErRelationshipSet, ErRole> notif)
        {
            RoleRegistry.Recieve(new ObjectCreatedNotification<ErRole>(notif.ObjectAdded));
            roleForeignKeys.Add(notif.ObjectAdded, notif.ObjectAddedTo);
        }
    }
}
