using ErEditor.ErSchemaClasses;
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

namespace ErEditor.DbSchemaClasses
{
    public class ErSchemaRegistry : IObserver, 
        IVisitor<ObjectAddedToCompositeObject<ErAttribute, ErElementWithAttributes>>
    {
        public readonly ErSchema Schema;

        public readonly Registry<ErEntitySet> EntitySetRegistry = new();
        public readonly Registry<ErRelationshipSet> RelationshipSetRegistry = new();
        public readonly Registry<ErValueSet> ValueSetRegistry = new();
        public readonly Registry<ErDiagram> DiagramRegistry = new();
        public readonly Registry<ErAttribute> AttributeRegistry = new();
        public readonly Registry<ErRole> RoleRegistry = new();
        public readonly Registry<ErMapping> MappingRegistry = new();
        public readonly Registry<DiagramPrimitive> PrimitiveRegistry = new();

        private Dictionary<ErEntitySet, DbEntitySet> entitySetsNotRetrieved = new();
        private Dictionary<ErRelationshipSet, DbRelationshipSet> relationshipSetsNotRetrieved = new();
        private Dictionary<ErValueSet, DbValueSet> valueSetsNotRetrieved = new();
        private Dictionary<ErDiagram, DbDiagram> diagramsNotRetrieved = new();
        private Dictionary<ErAttribute, DbAttribute> attributesNotRetrieved = new();
        private Dictionary<ErRole, DbRole> rolesNotRetrieved = new();
        private Dictionary<ErMapping, DbMapping> mappingsNotRetrieved = new();
        private Dictionary<DiagramPrimitive, DbPrimitive> primitivesNotRetrieved = new();

        public ObserverBase observerLogic;

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
            ErEntitySet entitySet = Schema.AddEntitySet(dbEs.Name ?? string.Empty);

            var attributes = this.AttributeRegistry.RetrieveDbEntryList(
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
            ErRelationshipSet relationshipSet = Schema.AddRelationshipSet(dbRs.Name ?? string.Empty);

            var attributes = this.AttributeRegistry.RetrieveDbEntryList(
                dbRs.Attributes.ToList(),
                el =>
                {
                    var attr = CreateAttributeOnSchema(relationshipSet, el);
                    return attr;
                });
            //MapDbSetToErSet(dbRs.EntitySets.ToList(), relationshipSet.roles, MapToRole);
            return relationshipSet;
        }
        private ErValueSet CreateValueSetOnSchema(DbValueSet dbVs)
        {
            ErValueSet valueSet = Schema.AddValueSet(dbVs.Name == null ? "" : dbVs.Name);
            return valueSet;
        }
        private ErDiagram CreateDiagramOnSchema(DbDiagram dbDgr)
        {
            ErDiagram diagram = Schema.AddDiagram(dbDgr.Name ?? string.Empty);
            return diagram;
        }
        private ErAttribute CreateAttributeOnSchema(ErElementWithAttributes element, DbAttribute dbAttr)
        {
            ErAttribute attr = element.AddAttribute(dbAttr.Name == null ? "" : dbAttr.Name);
            attr.minValue = dbAttr.MinValue;
            attr.maxValue = dbAttr.MaxValue;
            attr.allowedValues = dbAttr.AllowedValues;
            attr.isKey = dbAttr.IsKey;

            AttributeRegistry.AddForeignKey(attr, [dbAttr.ErElementWithAttributesId]);

            foreach (DbValueSet dbVs in dbAttr.ValueSets)
            {
                ErValueSet vs = ValueSetRegistry.RetrieveDbEntry(dbVs, CreateValueSetOnSchema);
                attr.valueSets.Add(vs);
            }
            return attr;
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
        public DbSchemaChanges MakeChangedDbEntries()
        {
            DbSchemaChanges changes = new();

            List<IDbEntry> created = new();
            List<IDbEntry> updated = new();
            List<IDbEntry> deleted = new();
            foreach (var el in EntitySetRegistry.Created)
            {
                created.Add(MakeDbEntitySet(el));
            }
            foreach (var el in RelationshipSetRegistry.Created)
            {
                created.Add(MakeDbRelationshipSet(el));
            }
            foreach (var el in ValueSetRegistry.Created)
            {
                created.Add(MakeDbValueSet(el));
            }
            foreach (var el in DiagramRegistry.Created)
            {
                created.Add(MakeDbDiagram(el));
            }
            foreach (var el in AttributeRegistry.Created)
            {
                var attr = MakeDbAttribute(el);
                if(attr != null)
                {
                    created.Add(attr);
                }
            }

            foreach (var el in EntitySetRegistry.Updated)
            {
                /*
                var dbEl = MakeUpdatedDbEntitySet(el);
                updated.Add(dbEl);

                foreach(var dbAttr in dbEl.Attributes)
                {
                    if(dbAttr.State == EntityState.Deleted)
                    {
                        deleted.Add(dbAttr);
                    }
                }*/
                updated.Add(MakeDbEntitySet(el));
            }
            foreach (var el in RelationshipSetRegistry.Updated)
            {
                updated.Add(MakeDbRelationshipSet(el));
            }
            foreach (var el in ValueSetRegistry.Updated)
            {
                updated.Add(MakeDbValueSet(el));
            }
            foreach (var el in DiagramRegistry.Updated)
            {
                updated.Add(MakeDbDiagram(el));
            }
            foreach (var el in AttributeRegistry.Updated)
            {
                var attr = MakeDbAttribute(el);
                if (attr != null)
                {
                    updated.Add(attr);
                }
            }

            foreach (var el in EntitySetRegistry.Deleted)
            {
                deleted.Add(MakeDbEntitySet(el));
            }
            foreach (var el in RelationshipSetRegistry.Deleted)
            {
                deleted.Add(MakeDbRelationshipSet(el));
            }
            foreach (var el in ValueSetRegistry.Deleted)
            {
                deleted.Add(MakeDbValueSet(el));
            }
            foreach (var el in DiagramRegistry.Deleted)
            {
                deleted.Add(MakeDbDiagram(el));
            }
            foreach (var el in AttributeRegistry.Deleted)
            {
                var attr = MakeDbAttribute(el);
                if (attr != null)
                {
                    deleted.Add(attr);
                }
            }

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

        private void TryToGetId<TElement, TDbEntry>(Registry<TElement> registry,
            Dictionary<TElement, TDbEntry> notRetrievedSet, TElement el, TDbEntry dbEl)
            where TElement : notnull
            where TDbEntry : IDbEntry
        {
            int? id = registry.FindId(el);
            if (id != null)
            {
                dbEl.Id = (int)id;
            }
            else
            {
                notRetrievedSet.Add(el, dbEl);
            }
        }
        // Это создание объектов Db из объектов в памяти. У них может не быть айдишников, как и у вложенных объектов, это просто маппинг.
        public DbEntitySet MakeDbEntitySet(ErEntitySet es)
        {
            DbEntitySet dbEs = new DbEntitySet(es.Name == "" ? null : es.Name);
            TryToGetId(EntitySetRegistry, entitySetsNotRetrieved, es, dbEs);

            /*
            foreach (var attr in es.attributes)
            {
                var dbAttr = MakeDbAttribute(dbEs, attr);
                dbEs.Attributes.Add(dbAttr);
            }*/
            return dbEs;
        }
        /*
        public DbEntitySet MakeUpdatedDbEntitySet(ErEntitySet es)
        {
            DbEntitySet dbEs = new DbEntitySet(es.Name == "" ? null : es.Name);
            TryToGetId(EntitySetRegistry, entitySetsNotRetrieved, es, dbEs);

            foreach (var attr in es.attributes)
            {
                var attrState = AttributeRegistry.GetState(attr);
                if(attrState != null && attrState != EntityState.Unchanged)
                {
                    var dbAttr = MakeDbAttribute(dbEs, attr);
                    dbAttr.State = (EntityState)attrState;

                    dbEs.Attributes.Add(dbAttr);
                }
            }
            return dbEs;
        }*/
        public DbRelationshipSet MakeDbRelationshipSet(ErRelationshipSet rs)
        {
            DbRelationshipSet dbRs = new(rs.Name == "" ? null : rs.Name);
            TryToGetId(RelationshipSetRegistry, relationshipSetsNotRetrieved, rs, dbRs);

            /*
            foreach (var attr in rs.attributes)
            {
                var dbAttr = MakeDbAttribute(dbRs, attr);
                dbRs.Attributes.Add(dbAttr);
            }
            /*
            foreach (var item in rs.roles)
            {
                var dbrole = MapFromRole(item);
                dbRs.EntitySets.Add(dbrole);
                dbcontext.SaveChanges();
                roleIdMap[dbrole.Id] = item;
            }*/
            return dbRs;
        }
        /*
        public DbRelationshipSet MakeUpdatedDbRelationshipSet(ErRelationshipSet rs)
        {
            DbRelationshipSet dbRs = new(rs.Name == "" ? null : rs.Name);
            TryToGetId(RelationshipSetRegistry, relationshipSetsNotRetrieved, rs, dbRs);

            foreach (var attr in rs.attributes)
            {
                var attrState = AttributeRegistry.GetState(attr);
                if (attrState != null && attrState != EntityState.Unchanged)
                {
                    var dbAttr = MakeDbAttribute(dbRs, attr);
                    dbAttr.State = (EntityState)attrState;

                    dbRs.Attributes.Add(dbAttr);
                }
            }
            /*
            foreach (var item in rs.roles)
            {
                var dbrole = MapFromRole(item);
                dbRs.EntitySets.Add(dbrole);
                dbcontext.SaveChanges();
                roleIdMap[dbrole.Id] = item;
            }
            return dbRs;
        }*/
        public DbValueSet MakeDbValueSet(ErValueSet vs)
        {
            DbValueSet dbVs = new(vs.Name == "" ? null : vs.Name);
            TryToGetId(ValueSetRegistry, valueSetsNotRetrieved, vs, dbVs);

            return dbVs;
        }
        public DbDiagram MakeDbDiagram(ErDiagram dgr)
        {
            DbDiagram dbDgr = new DbDiagram(dgr.Name == "" ? null : dgr.Name);
            TryToGetId(DiagramRegistry, diagramsNotRetrieved, dgr, dbDgr);

            return dbDgr;
        }
        private DbAttribute? MakeDbAttribute(ErAttribute attr)
        {
            DbAttribute dbAttr = new DbAttribute(attr.Name == "" ? null : attr.Name);
            var fk = AttributeRegistry.FindForeignKey(attr);
            ConsoleLog.Log($"{fk} {fk?.Count} {fk?[0]}");
            if (fk != null && fk[0] != null)
            {
                dbAttr.ErElementWithAttributesId = (int)fk[0];
            }
            else
            {
                ConsoleLog.Log("TRYING TO CREATE AN ATTRIBUTE ENTRY THAT REQUIRES A FOREIGN KEY BUT DOESN'T HAVE IT", this);
                return null;
            }
            dbAttr.MinValue = attr.minValue;
            dbAttr.MaxValue = attr.maxValue;
            dbAttr.AllowedValues = attr.allowedValues;
            
            TryToGetId(AttributeRegistry, attributesNotRetrieved, attr, dbAttr);

            return dbAttr;
        }

        private Dictionary<TObject, int> MakeObjectIdDict<TObject, TDbEntry>(Dictionary<TObject, TDbEntry> dict) 
            where TObject : notnull 
            where TDbEntry : IDbEntry
        {
            Dictionary<TObject, int> newDict = new();
            foreach(var objectDbEntryPair in dict)
            {
                newDict.Add(objectDbEntryPair.Key, objectDbEntryPair.Value.Id);
            }
            return newDict;
        }
        public void FlushRegistries()
        {
            EntitySetRegistry.Flush(MakeObjectIdDict(entitySetsNotRetrieved));
            RelationshipSetRegistry.Flush(MakeObjectIdDict(relationshipSetsNotRetrieved));
            ValueSetRegistry.Flush(MakeObjectIdDict(valueSetsNotRetrieved));
            DiagramRegistry.Flush(MakeObjectIdDict(diagramsNotRetrieved));
            AttributeRegistry.Flush(MakeObjectIdDict(attributesNotRetrieved));
            RoleRegistry.Flush(MakeObjectIdDict(rolesNotRetrieved));
            MappingRegistry.Flush(MakeObjectIdDict(mappingsNotRetrieved));
            PrimitiveRegistry.Flush(MakeObjectIdDict(primitivesNotRetrieved));
        }

        public void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }

        public void Visit(ObjectAddedToCompositeObject<ErAttribute, ErElementWithAttributes> notif)
        {
            int? id = null;
            var es = notif.CompositeObject as ErEntitySet;
            if(es != null)
            {
                id = EntitySetRegistry.FindId(es);
            }
            var rs = notif.CompositeObject as ErRelationshipSet;
            if (rs != null)
            {
                id = RelationshipSetRegistry.FindId(rs);
            }

            if(id != null)
            {
                AttributeRegistry.Recieve(new ObjectCreatedNotification<ErAttribute>(notif.Object));
                AttributeRegistry.AddForeignKey(notif.Object, [id]);
            }
        }
    }
}
