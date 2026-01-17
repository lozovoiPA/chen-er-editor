using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.DbSchemaClasses
{

    // Используется для передачи данных из БД один раз. По сути, результат запроса к БД для получения всех элементов схемы.
    public class DbSchema
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

        private void AddRange<TDbEntity> (List<TDbEntity> entities, IEnumerable<TDbEntity> range)
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
}
