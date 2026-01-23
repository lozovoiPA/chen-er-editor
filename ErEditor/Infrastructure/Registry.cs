using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.Infrastructure
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
        IVisitor<ObjectCreatedNotification<TObject>>,
        IVisitor<ObjectUpdatedNotification<TObject>>,
        IVisitor<ObjectDeletedNotification<TObject>>
        where TObject : notnull
    {
        protected Dictionary<int, TObject> retrievedIdMap = new();

        // Must only contain entries that exist in retrievedIdMap
        protected List<TObject> updated = new();
        protected List<TObject> deleted = new();

        protected Dictionary<TObject, IDbEntry?> created = new(); // these entries will be used to get ids when flushing

        protected ObserverBase observerLogic;

        public Registry()
        {
            observerLogic = new(this);
        }

        public ReadOnlyCollection<TObject> Updated
        {
            get { return updated.AsReadOnly(); }
        }
        public ReadOnlyCollection<TObject> Created
        {
            get { return created.Keys.ToList().AsReadOnly(); }
        }
        public ReadOnlyDictionary<TObject, IDbEntry?> CreatedDbEntries
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
            if (entries.Count > 1)
            {
                ConsoleLog.Log("Registry contains several of the same value. This shouldn't be the case.", this, "ERROR");
                return null;
            }
            return entries[0].Key;
        }
        public bool RetrievedContains(TObject entry)
        {
            return retrievedIdMap.ContainsValue(entry);
        }
        public EntityState? GetState(TObject entry)
        {
            if (RetrievedContains(entry))
            {
                return EntityState.Unchanged;
            }
            if (created.ContainsKey(entry))
            {
                return EntityState.Added;
            }
            if (updated.Contains(entry))
            {
                return EntityState.Modified;
            }
            if (deleted.Contains(entry))
            {
                return EntityState.Deleted;
            }
            return null;
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

        public bool AddCreated(TObject entry, IDbEntry? dbentry = null)
        {
            if (this.deleted.Contains(entry))
            {
                deleted.Remove(entry);
            }
            if (this.updated.Contains(entry))
            {
                updated.Remove(entry);
            }
            if (this.created.ContainsKey(entry))
            {
                return false;
            }
            if (this.FindId(entry) != null)
            {
                ConsoleLog.Log($"Skipped adding entry {entry} already retrieved", this, "INFO");
                return false;
            }
            ConsoleLog.Log($"Adding new {entry} to the created list.", this, "INFO");
            created.Add(entry, dbentry);
            return true;
        }
        public void AddCreatedDbEntry(TObject entry, IDbEntry dbentry)
        {
            if (created.ContainsKey(entry))
            {
                created[entry] = dbentry;
            }
        }
        public bool AddUpdated(TObject entry)
        {
            if (this.deleted.Contains(entry) || this.created.ContainsKey(entry) || this.updated.Contains(entry))
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
            if (this.created.ContainsKey(entry))
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
        public TObject? RetrieveDbEntry<TDbObject>(TDbObject dbEntry, Func<TDbObject, TObject?> mapFunc) where TDbObject : IDbEntry
        {
            TObject? retrievedEl = this.FindById(dbEntry.Id);
            if (retrievedEl != null)
            {
                return retrievedEl;
            }
            retrievedEl = mapFunc(dbEntry);
            if(retrievedEl != null)
            {
                this.AddRetrieved(dbEntry.Id, retrievedEl);
            }
            return retrievedEl;
        }
        public List<TObject> RetrieveDbEntryList<TDbObject>(IEnumerable<TDbObject> dbList, Func<TDbObject, TObject?> mapFunc) where TDbObject : class, IDbEntry
        {
            List<TObject> objectList = new();
            foreach(var dbEl in dbList)
            {
                var retrievedEl = RetrieveDbEntry(dbEl, mapFunc);
                if(retrievedEl != null)
                {
                    objectList.Add(retrievedEl);
                }
            }
            return objectList;
        }

        public void Flush()
        {
            foreach (var entry in deleted)
            {
                this.RemoveRetrieved(entry);
            }
            foreach (var entryKeyPair in created)
            {
                if (entryKeyPair.Value != null)
                {
                    this.AddRetrieved(entryKeyPair.Value.Id, entryKeyPair.Key);
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
            foreach (var entry in updated)
            {
                res += $"\t{entry.ToString()}\n";
            }
            res += "\nCreated:\n";
            foreach (var entry in created.Keys)
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

        public virtual void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
        public virtual void Visit(ObjectCreatedNotification<TObject> concreteObject)
        {
            this.AddCreated(concreteObject.Object);
        }
        public virtual void Visit(ObjectUpdatedNotification<TObject> concreteObject)
        {
            this.AddUpdated(concreteObject.Object);
        }
        public virtual void Visit(ObjectDeletedNotification<TObject> concreteObject)
        {
            this.AddDeleted(concreteObject.Object);
        }
    }

}
