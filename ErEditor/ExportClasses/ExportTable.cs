using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ExportClasses
{
    public class ExportSchema
    {
        public List<ExportTable> tables = new();

        private void AddTable(ExportTable table)
        {
            int counter = 0;
            foreach (var existingTables in tables)
            {
                if (existingTables.Name == table.Name)
                {
                    counter += 1;
                }
            }
            if (counter != 0)
            {
                table.Name += $" ({counter})";
            }
            tables.Add(table);
        }

        public void BuildFrom(ErSchema schema)
        {
            foreach (var entitySet in schema.EntitySets)
            {
                var table = BuildTableFrom(entitySet);
                AddTable(table);
            }

            foreach(var relationshipSet in schema.RelationshipSets)
            {
                var table = BuildTableFrom(relationshipSet);
                if(table.ForeignKeys.Count > 1 || table.Columns.Count > 0)
                {
                    AddTable(table);
                }
            }
        }

        private ExportTable BuildTableAttributeBase(ErElementWithAttributes element)
        {
            ExportTable table = new(element.Name, element);
            foreach (var attribute in element.Attributes)
            {
                if (attribute.allowsManyValues)
                {
                    var attributeTable = BuildTableFrom(element, attribute);
                    table.AddForeignKey(new($"{attribute.Name}_id", attributeTable));
                    AddTable(attributeTable);
                }
                else
                {
                    if(attribute.valueSets.Count > 1)
                    {
                        foreach (var valueSet in attribute.valueSets)
                        {
                            table.AddColumn(new($"{attribute.Name}_{valueSet.Name}", valueSet.BaseValueType, null));
                        }
                    }
                    else
                    {
                        table.AddColumn(new($"{attribute.Name}", attribute.valueSets[0].BaseValueType, null));
                    }
                    
                }
            }
            return table;
        }
        private ExportTable BuildTableFrom(ErEntitySet entitySet)
        {
            var table = BuildTableAttributeBase(entitySet);
            return table;
        }
        private ExportTable BuildTableFrom(ErRelationshipSet relationshipSet)
        {
            ExportTable table = new(relationshipSet.Name, relationshipSet);
            if(relationshipSet.Attributes.Count > 0)
            {
                table = BuildTableAttributeBase(relationshipSet);
            }
            if(relationshipSet.Mappings.Count > 1)
            {
                foreach (var mapping in relationshipSet.Mappings)
                {
                    var entitySetTable = this.tables.Find(x => x.Source == mapping.PreImage[0].EntitySet);
                    if (entitySetTable != null)
                    {
                        table.AddForeignKey(new($"{entitySetTable.Source.Name}_id", entitySetTable));
                    }
                }
            }
            else
            {
                var mapping = relationshipSet.Mappings[0];
                var entitySetTable1 = this.tables.Find(x => x.Source == mapping.PreImage[0].EntitySet);
                var entitySetTable2 = this.tables.Find(x => x.Source == mapping.Image[0].EntitySet);
                if(entitySetTable1 != null && entitySetTable2 != null)
                {
                    if (mapping.MaxCardinalityOfImage == mapping.MaxCardinalityOfPreimage)
                    {
                        if (mapping.MaxCardinalityOfPreimage < 0) // M : M
                        {
                            table.AddForeignKey(new($"{entitySetTable1.Source.Name}_id", entitySetTable1));
                            table.AddForeignKey(new($"{entitySetTable2.Source.Name}_id", entitySetTable2));
                        }
                        else // 1 : 1
                        {
                            entitySetTable1.AddForeignKey(new($"{entitySetTable2.Source.Name}_id", entitySetTable2));
                        }
                    }
                    else if(mapping.MaxCardinalityOfImage == -1 && mapping.MaxCardinalityOfPreimage == 1)
                    {
                        entitySetTable2.AddForeignKey(new($"{entitySetTable1.Source.Name}_id", entitySetTable1));
                    }
                    else if (mapping.MaxCardinalityOfPreimage == -1 && mapping.MaxCardinalityOfImage == 1)
                    {
                        entitySetTable1.AddForeignKey(new($"{entitySetTable2.Source.Name}_id", entitySetTable2));
                    }
                }
            }
            return table;
        }
        private ExportTable BuildTableFrom(ErElementWithAttributes parent, ErAttribute attribute)
        {
            ExportTable table = new(attribute.Name, attribute);
            foreach (var valueSet in attribute.valueSets)
            {
                table.AddColumn(new($"{attribute.Name}_{valueSet.Name}", valueSet.BaseValueType, null));
            }
            return table;
        }
    }
    public partial class ExportTable
    {
        public string Name;
        public readonly ErElement Source;
        public readonly List<ExportTableColumn> Columns = new();
        public readonly List<ExportTableForeignKey> ForeignKeys = new();
        
        public ExportTable(string name, ErElement source) 
        {
            this.Name = name;
            this.Source = source;
        }

        public void AddColumn(ExportTableColumn column)
        {
            int counter = 0;
            foreach(var existingColumn in Columns)
            {
                if(existingColumn.Name == column.Name)
                {
                    counter += 1;
                }
            }
            if(counter != 0)
            {
                column.Name += $" ({counter})";
            }
            Columns.Add(column);
        }
        public void AddForeignKey(ExportTableForeignKey foreignKey)
        {
            int counter = 0;
            foreach (var existingForeignKey in ForeignKeys)
            {
                if (existingForeignKey.Name == foreignKey.Name)
                {
                    counter += 1;
                }
            }
            if(counter != 0)
            {
                foreignKey.Name += $" ({counter})";
            }
            ForeignKeys.Add(foreignKey);
        }


        public class ExportTableColumn
        {
            public string Name;
            public string Type;
            public string? Constraints;

            public ExportTableColumn(string name, string type, string? constraints)
            {
                Name = name;
                Type = type;
                Constraints = constraints;
            }
        }
        public class ExportTableForeignKey
        {
            public string Name;
            public ExportTable LinkedTable;

            public ExportTableForeignKey(string name, ExportTable linkedTable)
            {
                this.Name = name;
                this.LinkedTable = linkedTable;
            }
        }
    }
}
