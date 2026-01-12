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

namespace ErEditor.DbSchema
{
    public interface IDbEntry
    {
        public int Id { get; set; }
    }

    // Technically the Registry corresponds to one table, and keeps track of all its Ids
    // It is only used to map Ids to object in memory and vice versa, nothing else.

    // In some cases if one table corresponds to two or more object types, separate registries can be created
    // However, DO NOT create a single registry for two or more table! Even if objects there have one hierarchy.
    public class Registry<TObject>
        : IObserver,
        IVisitor<ObjectAddedNotification<TObject>>,
        IVisitor<ObjectUpdatedNotification<TObject>>,
        IVisitor<ObjectDeletedNotification<TObject>>
        where TObject : notnull
    {
        protected Dictionary<int, TObject> retrievedIdMap = new();

        // Must only contain entries that exist in retrievedIdMap
        protected List<TObject> updated = new();
        protected List<TObject> deleted = new();
        protected List<TObject> created = new();

        private ObserverBase observerLogic;

        public Registry(IObservable observableObject)
        {
            observableObject.Subscribe(this);
            observerLogic = new(this);
        }

        public ReadOnlyCollection<TObject> Updated
        {
            get { return updated.AsReadOnly(); }
        }
        public ReadOnlyCollection<TObject> Created
        {
            get { return created.AsReadOnly(); }
        }
        public ReadOnlyCollection<TObject> Deleted
        {
            get { return deleted.AsReadOnly(); }
        }

        public TObject? FindById(int id)
        {
            return retrievedIdMap.ContainsKey(id) ? retrievedIdMap[id] : default;
        }
        public int? FindId(TObject entry)
        {
            if (!retrievedIdMap.ContainsValue(entry))
            {
                return null;
            }
            var entries = retrievedIdMap.Where(x => x.Value?.Equals(entry) ?? false).ToList();
            if(entries.Count > 1)
            {
                ConsoleLog.Log("Registry contains several of the same value. This shouldn't be the case.", this, "ERROR");
                return null;
            }
            return entries[0].Key;
        }

        private bool AddRetrieved(int id, TObject entry)
        {
            if (retrievedIdMap.ContainsKey(id))
            {
                ConsoleLog.Log($"Entry with id {id} already exists in the registry. It will not be added.", this, "WARNING");
                return false;
            }
            retrievedIdMap.Add(id, entry);
            return true;
        }
        private bool RemoveRetrieved(TObject entry)
        {
            int? id = FindId(entry);
            if (id == null)
            {
                ConsoleLog.Log($"Entry {entry} doesn't exist in the registry.", this, "WARNING");
                return false;
            }
            retrievedIdMap.Remove((int)id);
            return true;
        }

        public bool AddCreated(TObject entry)
        {
            if (this.deleted.Contains(entry))
            {
                deleted.Remove(entry);
            }
            if (this.updated.Contains(entry))
            {
                updated.Remove(entry);
            }
            if (this.created.Contains(entry))
            {
                return false;
            }
            if (this.FindId(entry) != null)
            {
                ConsoleLog.Log($"Skipped adding entry {entry} already retrieved", this, "INFO");
                return false;
            }
            ConsoleLog.Log($"Adding new {entry} to the created list.", this, "INFO");
            created.Add(entry);
            return true;
        }
        public bool AddUpdated(TObject entry)
        {
            if (this.deleted.Contains(entry) || this.created.Contains(entry) || this.updated.Contains(entry))
            {
                return false;
            }
            if (this.FindId(entry) == null)
            {
                ConsoleLog.Log("Aborted trying to update entity not in registry.", this, "ERROR");
                return false;
            }
            ConsoleLog.Log($"Adding updated {entry} to the updated list.", this, "INFO");
            this.updated.Add(entry);
            return true;
        }
        public bool AddDeleted(TObject entry)
        {
            if (this.updated.Contains(entry))
            {
                updated.Remove(entry);
            }
            if (this.created.Contains(entry))
            {
                created.Remove(entry);
                return false;
            }
            if (this.deleted.Contains(entry))
            {
                return false;
            }
            if (this.FindId(entry) == null)
            {
                ConsoleLog.Log("Aborted trying to delete entity not in registry.", this, "ERROR");
                return false;
            }
            ConsoleLog.Log($"Adding deleted {entry} to the deleted list.", this, "INFO");
            deleted.Add(entry);
            return true;
        }


        // Maps a DB entry to this Registry entry (object entry) and adds it to retrieved entries
        public TObject RetrieveDbEntry<TDbObject>(TDbObject dbEntry, Func<TDbObject, TObject> mapFunc) where TDbObject: IDbEntry
        {
            TObject? retrievedEl = this.FindById(dbEntry.Id);
            if (retrievedEl != null)
            {
                return retrievedEl;
            }
            retrievedEl = mapFunc(dbEntry);
            this.AddRetrieved(dbEntry.Id, retrievedEl);
            return retrievedEl;
        }
        public List<TObject> RetrieveDbSet<TDbObject>(List<TDbObject> dbList, Func<TDbObject, TObject> mapFunc) where TDbObject : class, IDbEntry
        {
            List<TObject> objectList = new();
            dbList.ForEach(dbEl =>
            {
                objectList.Add(RetrieveDbEntry(dbEl, mapFunc));
            });
            return objectList;
        }
        /*
        public TDbEntry CreateDbEntry<TDbEntry>(TObjEntry objEntry, Func<TObjEntry, TDbEntry> mapFunc) where TDbEntry: IDbEntry
        {
            TDbEntry dbEntry = mapFunc(objEntry);
            int? id = this.FindInRetrieved(objEntry);
            if (id != null)
            {
                dbEntry.Id = (int)id;
            }
            return dbEntry;
        }*/
        public void Flush(Dictionary<TObject, int> createdIds)
        {
            foreach (var entry in deleted)
            {
                this.RemoveRetrieved(entry);
            }
            foreach (var entryKeyPair in createdIds)
            {
                if (created.Contains(entryKeyPair.Key))
                {
                    this.AddRetrieved(entryKeyPair.Value, entryKeyPair.Key);
                }
            }

            created.Clear();
            updated.Clear();
            deleted.Clear();
        }

        public string PrintState()
        {
            string res = "";

            res += "Retrieved:\n";
            foreach (var entry in retrievedIdMap)
            {
                res += $"\t{entry.Value.ToString()}: {entry.Key}\n";
            }
            res += "\nUpdated:\n";
            foreach(var entry in updated)
            {
                res += $"\t{entry.ToString()}\n";
            }
            res += "\nCreated:\n";
            foreach (var entry in created)
            {
                res += $"\t{entry.ToString()}\n";
            }
            res += "\nDeleted:\n";
            foreach (var entry in deleted)
            {
                res += $"\t{entry.ToString()}\n";
            }
            return res;
        }

        public void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
        public void Visit(ObjectAddedNotification<TObject> concreteObject)
        {
            this.AddCreated(concreteObject.Object);
        }
        public void Visit(ObjectUpdatedNotification<TObject> concreteObject)
        {
            this.AddUpdated(concreteObject.Object);
        }
        public void Visit(ObjectDeletedNotification<TObject> concreteObject)
        {
            this.AddDeleted(concreteObject.Object);
        }
    }

    public class ErSchemaRegistry
    {
        public readonly ErSchema Schema;

        public readonly Registry<ErEntitySet> EntitySetRegistry;
        public readonly Registry<ErRelationshipSet> RelationshipSetRegistry;
        public readonly Registry<ErValueSet> ValueSetRegistry;
        public readonly Registry<ErDiagram> DiagramRegistry;
        public readonly Registry<ErAttribute> AttributeRegistry;
        public readonly Registry<ErRole> RoleRegistry;
        public readonly Registry<ErMapping> MappingRegistry;
        public readonly Registry<DiagramPrimitive> PrimitiveRegistry;

        private Dictionary<ErEntitySet, DbEntitySet> entitySetsNotRetrieved = new();
        private Dictionary<ErRelationshipSet, DbRelationshipSet> relationshipSetsNotRetrieved = new();
        private Dictionary<ErValueSet, DbValueSet> valueSetsNotRetrieved = new();
        private Dictionary<ErDiagram, DbDiagram> diagramsNotRetrieved = new();
        private Dictionary<ErAttribute, DbAttribute> attributesNotRetrieved = new();
        private Dictionary<ErRole, DbRole> rolesNotRetrieved = new();
        private Dictionary<ErMapping, DbMapping> mappingsNotRetrieved = new();
        private Dictionary<DiagramPrimitive, DbPrimitive> primitivesNotRetrieved = new();

        public ErSchemaRegistry(ErSchema schema)
        {
            EntitySetRegistry = new(schema);
            RelationshipSetRegistry = new(schema);
            ValueSetRegistry = new(schema);
            DiagramRegistry = new(schema);
            AttributeRegistry = new(schema);
            RoleRegistry = new(schema);
            MappingRegistry = new(schema);
            PrimitiveRegistry = new(schema);

            Schema = schema;
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
        private ErEntitySet MapToEntitySet(DbEntitySet dbEs)
        {
            ErEntitySet entitySet = new ErEntitySet(dbEs.Name ?? string.Empty);
            var attributes = this.AttributeRegistry.RetrieveDbSet(dbEs.Attributes.ToList(), MapToAttribute);
            entitySet.attributes.AddRange(attributes);
            return entitySet;
        }
        private ErRelationshipSet MapToRelationshipSet(DbRelationshipSet dbRs)
        {
            ErRelationshipSet relationshipSet = new ErRelationshipSet(dbRs.Name ?? string.Empty);
            var attributes = this.AttributeRegistry.RetrieveDbSet(dbRs.Attributes.ToList(), MapToAttribute);
            relationshipSet.attributes.AddRange(attributes);
            //MapDbSetToErSet(dbRs.Attributes.ToList(), relationshipSet.attributes, MapToAttribute);
            //MapDbSetToErSet(dbRs.EntitySets.ToList(), relationshipSet.roles, MapToRole);
            return relationshipSet;
        }
        private ErValueSet MapToValueSet(DbValueSet dbVs)
        {
            ErValueSet valueSet = new ErValueSet(dbVs.Name == null ? "" : dbVs.Name);
            return valueSet;
        }
        private ErDiagram MapToDiagram(DbDiagram dbDgr)
        {
            ErDiagram diagram = new ErDiagram(this.Schema, dbDgr.Name ?? string.Empty);
            return diagram;
        }
        private ErAttribute MapToAttribute(DbAttribute dbAttr)
        {
            ErAttribute attr = new ErAttribute(dbAttr.Name == null ? "" : dbAttr.Name);
            attr.minValue = dbAttr.MinValue;
            attr.maxValue = dbAttr.MaxValue;
            attr.allowedValues = dbAttr.AllowedValues;
            attr.isKey = dbAttr.IsKey;

            foreach (DbValueSet dbVs in dbAttr.ValueSets)
            {
                ErValueSet vs = RetrieveValueSet(dbVs);
                attr.valueSets.Add(vs);
            }
            return attr;
        }

        // Это обертки вокруг методов выше, которые учитывают был ли уже получен объект из БД или нет.
        // Слабые сущности таких не имеют. Только маппинги.
        public ErEntitySet RetrieveEntitySet(DbEntitySet dbEs)
        {
            return this.EntitySetRegistry.RetrieveDbEntry(dbEs, MapToEntitySet);
        }
        public List<ErEntitySet> RetrieveEntitySetRange(List<DbEntitySet> dbEsList)
        {
            return this.EntitySetRegistry.RetrieveDbSet(dbEsList, this.MapToEntitySet);
        }
        public ErRelationshipSet RetrieveRelationshipSet(DbRelationshipSet dbRs)
        {
            return this.RelationshipSetRegistry.RetrieveDbEntry(dbRs, this.MapToRelationshipSet);
        }
        public List<ErRelationshipSet> RetrieveRelationshipSetRange(List<DbRelationshipSet> dbRsList)
        {
            return this.RelationshipSetRegistry.RetrieveDbSet(dbRsList, this.MapToRelationshipSet);
        }
        public ErValueSet RetrieveValueSet(DbValueSet dbVs)
        {
            return this.ValueSetRegistry.RetrieveDbEntry(dbVs, this.MapToValueSet);
        }
        public List<ErValueSet> RetrieveValueSetRange(List<DbValueSet> dbVsList)
        {
            return this.ValueSetRegistry.RetrieveDbSet(dbVsList, this.MapToValueSet);
        }
        public ErDiagram RetrieveDiagram(DbDiagram dbDgr)
        {
            return this.DiagramRegistry.RetrieveDbEntry(dbDgr, MapToDiagram);
        }
        public List<ErDiagram> RetrieveDiagramRange(List<DbDiagram> dbDgrList)
        {
            return this.DiagramRegistry.RetrieveDbSet(dbDgrList, this.MapToDiagram);
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

            foreach (var attr in es.attributes)
            {
                var dbAttr = MakeDbAttribute(attr);
                dbEs.Attributes.Add(dbAttr);
            }
            return dbEs;
        }
        public DbRelationshipSet MakeDbRelationshipSet(ErRelationshipSet rs)
        {
            DbRelationshipSet dbRs = new(rs.Name == "" ? null : rs.Name);
            TryToGetId(RelationshipSetRegistry, relationshipSetsNotRetrieved, rs, dbRs);

            foreach (var attr in rs.attributes)
            {
                var dbAttr = MakeDbAttribute(attr);
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
        public DbAttribute MakeDbAttribute(ErAttribute attr)
        {
            DbAttribute dbAttr = new DbAttribute(attr.Name == "" ? null : attr.Name);
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
    }
}
