using ErEditor.ErSchemaClasses;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI
{
    public static class DialogManager
    {

        // method shouldn't be called several times to open new window. Save window in DialogManager as a field and redirect
        // requests to Open it by focusing it?
        public static DiagramPanel? DiagramPanel;

        public static ErSchema CreateNewErSchemaWindow()
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

                        // разделение обязанностей: DialogManager должен выдать созданную в окне схему, его не должно
                        // интересовать что дальше с ней будет происходить и где.
                        // Другими словами: медиатор в основном окне само это окно (оно и так знает все элементы. Зачем их дублировать?)
                        return newSchema;
                    }

                default:
                    {
                        ErSchema newSchema = new();
                        return newSchema;
                    }
            }
        }

        public static ErSchema OpenSchemaWindow()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            DialogResult dr = ofd.ShowDialog();
            ErSchema schema = new();
            switch (dr)
            {
                case DialogResult.OK:
                    schema = ErSchemaFileManager.OpenErSchema(ofd.FileName);
                    //cur_path = Path.GetDirectoryName(ofd.FileName);
                    //erEditor.ActivateControls();
                    break;
            }
            return schema;
        }

        public static void OpenDiagram(ErDiagram diagram)
        {
            if(DiagramPanel != null)
            {
                DiagramPanel.Diagram = diagram;
            }
        }
    }
}
