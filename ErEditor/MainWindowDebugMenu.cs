using ErEditor.ErSchemaClasses;
using ErEditor.ExportClasses;
using ErEditor.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace ErEditor
{
    partial class MainWindow
    {
        private void InitializeDebugMenu()
        {
            ToolStripMenuItem debugDropDown = new("Дебаг");

            ToolStripMenuItem showSchemaObjectItem = new("Показать схему в оперативной памяти");
            showSchemaObjectItem.Click += Debug_ShowSchema;
            ToolStripMenuItem showRegistryStateItem = new("Показать состояние реестра схемы");
            showRegistryStateItem.Click += Debug_ShowRegistry;
            ToolStripMenuItem showMsaglGraph = new("Генерация графа (MSAGL)");
            showMsaglGraph.Click += Debug_ShowMsaglGraph;
            ToolStripMenuItem executeSQLCreateQuery = new("Выполнить создание тестовой БД");
            executeSQLCreateQuery.Click += Debug_ExecuteSQLCreateQuery;
            
            debugDropDown.DropDownItems.AddRange(
                [
                showSchemaObjectItem,
                showRegistryStateItem,
                showMsaglGraph,
                executeSQLCreateQuery
                ]);
            menuStrip1.Items.Add(debugDropDown);
        }

        private void Debug_ShowSchema(object? sender, EventArgs e)
        {
            var schemas = navigatorTreeView1.Schemas;
            if (schemas.Count > 0)
            {
                Console.WriteLine(schemas[0].PrintState());
            }
        }
        private void Debug_ShowRegistry(object? sender, EventArgs e)
        {
            ErSchemaRegistry? registry = null;
            var schemas = navigatorTreeView1.Schemas;
            if (schemas.Count > 0)
            {
                registry = ErSchemaFileManager.GetRegistry(schemas[0]);
            }
            if (registry != null)
            {
                Console.WriteLine(registry.PrintState());
            }
        }
        private void Debug_ShowMsaglGraph(object? sender, EventArgs e)
        {
            MsaglTestsForm msaglTestsForm = new();
            msaglTestsForm.ShowDialog();
        }
        private string BuildQuery(ExportTable table)
        {
            string sqlQuery = $"CREATE TABLE \"{table.Source.Name}\"(";
            if(table.Columns.Count > 0 || table.ForeignKeys.Count == 0)
            {
                sqlQuery += "id INT PRIMARY KEY NOT NULL,";
            }

            foreach (var column in table.Columns)
            {
                sqlQuery += $"\"{column.Name}\"";
                // Type и Constraints задаются некоторым форматом самой ExportTable,
                // а провайдеры БД из этих абстрактных типов их уже резолвят в конкретные
                switch (column.Type)
                {
                    case "bool":
                    case "int":
                        sqlQuery += "INT";
                        break;
                    case "float":
                        sqlQuery += "REAL";
                        break;
                    case "text":
                        sqlQuery += "TEXT";
                        break;
                }
                sqlQuery += ",";
            }
            foreach (var foreignKey in table.ForeignKeys)
            {
                sqlQuery += $"\"{foreignKey.Name}\" INT";
                sqlQuery += ",";
            }
            foreach (var foreignKey in table.ForeignKeys)
            {
                sqlQuery += $"FOREIGN KEY(\"{foreignKey.Name}\") REFERENCES \"{foreignKey.LinkedTable.Name}\"(id),";
            }
            sqlQuery = sqlQuery.Remove(sqlQuery.Length - 1);
            sqlQuery += ");";
            return sqlQuery;
        }

        private void Debug_ExecuteSQLCreateQuery(object? sender, EventArgs e)
        {
            if(MainWindow.Navigator.Schemas.Count > 0)
            {
                ErSchema schema = navigatorTreeView1.Schemas[0];

                // Db creation
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\test_db2\\test_SQL.db";
                ConsoleLog.Log($"[1/2] Initializing ExportDbContext instance... (path to DB: {path})", this);
                ExportDbContext dbcontext = new(path);
                dbcontext.Database.EnsureDeleted();
                dbcontext.Database.EnsureCreated();
                dbcontext.Dispose();

                // connecting to Db
                string connectionPath = $"Data Source={path}";
                using (SqliteConnection connection = new SqliteConnection(connectionPath))
                {
                    connection.Open();

                    ExportSchema exportSchema = new();
                    exportSchema.BuildFrom(schema);

                    string sqlQuery = "SELECT 1";
                    using (SqliteCommand command = new SqliteCommand(sqlQuery, connection))
                    {
                        foreach (var table in exportSchema.tables)
                        {
                            command.CommandText = BuildQuery(table);
                            using (SqliteDataReader reader = command.ExecuteReader())
                            {

                            }
                            ConsoleLog.Log(command.CommandText, this);
                        }

                    }

                        
                }
                ConsoleLog.Log($"[2/2] Query executed successfully.", this);
            }
        }
    }
}
