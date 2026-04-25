using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
