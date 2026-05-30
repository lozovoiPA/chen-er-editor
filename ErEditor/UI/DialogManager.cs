using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.ExportClasses;
using ErEditor.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace ErEditor.UI
{
    public static class DialogManager
    {


        // method shouldn't be called several times to open new window. Save window in DialogManager as a field and redirect
        // requests to Open it by focusing it?
        public static ErSchema? CreateNewErSchemaWindow()
        {
            NewErSchemaWindow newErSchemaWindow = new();
            switch (newErSchemaWindow.ShowDialog())
            {
                case DialogResult.OK:
                    {
                        ErSchema newSchema = ErSchemaFileManager.NewErSchema(
                            newErSchemaWindow.ErSchemaName,
                            newErSchemaWindow.ErSchemaFileName,
                            newErSchemaWindow.ErSchemaFolderPath
                        );
                        newErSchemaWindow.Dispose();
                        return newSchema;
                    }

                default:
                    {
                        newErSchemaWindow.Dispose();
                        return null;
                    }
            }
        }
        public static ErSchema? OpenSchemaWindow()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult dr = ofd.ShowDialog();
            switch (dr)
            {
                case DialogResult.OK:
                    var schema = ErSchemaFileManager.OpenErSchema(ofd.FileName);
                    return schema;

                default:
                    return null;
            }
            
        }

        public static ErDiagram GenerateDiagram(ErSchema schema)
        {
            ErDiagram diagram = ErSchemaFileManager.GenerateDiagram(schema, MainWindow.DiagramPanel.ClientRectangle);
            MainWindow.OpenDiagram(schema, diagram);
            return diagram;
        }

        private static string BuildQuery(ExportTable table)
        {
            string sqlQuery = $"CREATE TABLE \"{table.Source.Name}\"(";
            if (table.Columns.Count > 0 || table.ForeignKeys.Count == 0)
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

        public static void TranslateSchema(ErSchema schema)
        {
            SaveFileDialog fbd = new();
            fbd.Filter = "Database files (*.db)|*.db";

            DialogResult dr = fbd.ShowDialog();
            string? path = null;
            switch (dr)
            {
                case DialogResult.OK:
                    path = fbd.FileName;
                break;

                default:
                    MessageBox.Show("Путь к экспортируемой БД не выбран");
                break;
            }

            if (path != null)
            {
                ConsoleLog.Log($"[1/2] Initializing ExportDbContext instance... (path to DB: {path})");
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
                            ConsoleLog.Log(command.CommandText);
                        }

                    }
                }
                ConsoleLog.Log($"[2/2] Query executed successfully.");
            }
            
        }

        public static bool ExportDiagram(ErDiagram diagram)
        {
            SaveFileDialog sfd = new();
            sfd.AddExtension = true;
            sfd.Filter = "PNG Image|*.png|JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
            sfd.DefaultExt = "png";
            DialogResult dr = sfd.ShowDialog();
            switch (dr)
            {
                case DialogResult.OK:
                    var path = sfd.FileName;
                    var size = diagram.GetSize();
                    Console.WriteLine($"{size.X}, {size.Y}, {size.Width}, {size.Height}");
                    Bitmap bitmap = new Bitmap(size.Width + 20, size.Height + 20);

                    Graphics g = Graphics.FromImage(bitmap);
                    g.TranslateTransform(-size.X + 10, -size.Y + 10);

                    g.Clear(Color.White);
                    diagram.Draw(g);
                    g.Flush();
                    bitmap.Save(path);

                    return true;
                default:
                    return false;
            }
        }
    }
}
