using ErEditor.DbSchema;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    // Пока просто связка схемы с реестром и файлом бд
    public class ErSchemaFileData
    {
        private ErSchema schema;
        private ErSchemaRegistry schemaRegistry;
        private string filepath;

        public ErSchemaFileData(ErSchema schema, ErSchemaRegistry schemaRegistry, string filepath)
        {
            this.schema = schema;
            this.filepath = filepath;
            this.schemaRegistry = schemaRegistry;
        }

        public ErSchema Schema
        {
            get { return this.schema; }
        }
        public ErSchemaRegistry SchemaRegistry
        {
            get { return this.schemaRegistry; }
        }
        public string Filepath
        {
            get { return this.filepath; }
        }
    }

    public static class ErSchemaFileManager
    {
        private static List<ErSchemaFileData> openSchemas = new();
        public static ErSchema NewErSchema(string schemaName, string schemaFileName, string folderPath)
        {
            openSchemas.Clear();
            string fullPath = Path.Combine(folderPath, schemaFileName + ".db");
            if (File.Exists(fullPath))
            {
                ConsoleLog.Log("File already exists, it will be deleted and recreated", "ErSchemaFileManager", "WARNING");
            }

            ErDbContext dbcontext = new ErDbContext(fullPath);
            dbcontext.Database.EnsureDeleted();
            dbcontext.Database.EnsureCreated();
            dbcontext.Dispose();

            ErSchema schema = new(schemaName);
            ErSchemaRegistry schemaRegistry = new(schema);

            ErSchemaFileData newSchemaData = new(schema, schemaRegistry, fullPath);
            openSchemas.Add(newSchemaData);
            return newSchemaData.Schema;
        }
        public static bool SaveSchema(ErSchema schema)
        {
            // Check if filedata for this schema exists
            ErSchemaFileData? data = openSchemas.Find(x => x.Schema == schema);
            if (data == null)
            {
                ConsoleLog.Log($"You are trying to save schema that doesn't have a corresponding file data ({schema.Name})." +
                    $"This may be because schema wasn't created through the Schema File Manager." +
                    $"Schema won't be saved, you can save this schema by using a different overload of this method.", 
                    "ErSchemaFileManager", "ERROR");
                return false;
            }

            // Check if the file for this filedata exists
            if (!File.Exists(data.Filepath))
            {
                ConsoleLog.Log($"Saving schema failed because the file specified in this schema file data doesn't exist ({data.Filepath}).",
                    "ErSchemaFileManager", "ERROR");
                return false;
            }

            // Check if the database from the file can be connected to
            ErDbContext dbcontext = new ErDbContext(data.Filepath);
            if (!dbcontext.Database.CanConnect())
            {
                ConsoleLog.Log($"Saving schema {schema.Name} failed because the database connection couldn't be established." +
                    $"Please check whether there is another open connection.", 
                    "ErSchemaFileManager", "ERROR");
                return false;
            }

            // Save schema
            ErSchemaDbMapper mapper = new(data.Schema, data.SchemaRegistry, dbcontext);
            mapper.MapToDatabase();
            dbcontext.Dispose();

            return true;
        }
        public static ErSchema? OpenErSchema(string filepath)
        {
            openSchemas.Clear();
            string schemaName = Path.GetFileNameWithoutExtension(filepath);

            // Check if the file for this filedata exists
            if (!File.Exists(filepath))
            {
                ConsoleLog.Log($"Opening schema failed because the file specified doesn't exist ({filepath}).",
                    "ErSchemaFileManager", "ERROR");
                return null;
            }

            ErSchema schema = new(schemaName);
            ErSchemaRegistry schemaRegistry = new(schema);

            // Check if the database from the file can be connected to
            ErDbContext dbcontext = new ErDbContext(filepath);
            if (!dbcontext.Database.CanConnect())
            {
                ConsoleLog.Log($"Opening schema {schema.Name} failed because the database connection couldn't be established." +
                    $"Please check whether there is another open connection.",
                    "ErSchemaFileManager", "ERROR");
                return null;
            }

            ErSchemaDbMapper mapper = new(schema, schemaRegistry, dbcontext);
            schema = mapper.MapFromDatabase();
            dbcontext.Dispose();

            ErSchemaFileData data = new ErSchemaFileData(schema, schemaRegistry, filepath);
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
