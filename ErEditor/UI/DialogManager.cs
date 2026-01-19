using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI
{
    // Интерфейсы говорят, функции какие UI служебных классов может выполнять DialogMediator
    // Не могут реализовываться статическими классами, поэтому сделал синглтон, но с изюминкой
    public class DialogManager : IMainWindowMediator
    {
        private static readonly DialogManager instance = new();
        private static MainWindowMediator? mediator;

        private DialogManager() { }
        public DialogManager(MainWindowMediator mediator)
        {
            DialogManager.mediator = mediator;
        }

        public static DialogManager Instance { get { return instance; } }

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
                        return null;
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

        public void OpenSchema(ErSchema schema)
        {
            if (mediator == null)
            {
                ConsoleLog.Log("Trying to open a schema, but the mediator is not set.");
                return;
            }
            mediator.OpenSchema(schema);
        }

        public static void OpenDiagram(ErDiagram diagram)
        {
            /*
            if(DiagramPanel != null)
            {
                DiagramPanel.Diagram = diagram;
            }*/
        }

        public void OpenProperties(ErSchema schema, ErRole role)
        {
            if (mediator == null)
            {
                ConsoleLog.Log("Trying to open role properties, but the mediator is not set.");
                return;
            }
            mediator.OpenProperties(schema, role);
        }
    }

    public interface IMainWindowMediator
    {
        public void OpenSchema(ErSchema schema);
    }

    public class MainWindowMediator : IMainWindowMediator
    {
        private DiagramPanel diagramPanel;
        private ElementPropertiesPanel propertiesPanel;
        private NavigatorTreeView navigator;

        public MainWindowMediator(NavigatorTreeView navigator, ElementPropertiesPanel propertiesPanel, DiagramPanel diagramPanel)
        {
            this.navigator = navigator;
            this.propertiesPanel = propertiesPanel;
            this.diagramPanel = diagramPanel;
        }

        public void OpenSchema(ErSchema schema)
        {
            navigator.OpenSchema(schema);
        }
        public void OpenProperties(ErSchema schema, ErRole role)
        {
            propertiesPanel.OpenProperties(schema, role);
        }
    }
}
