using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using ErEditor.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor
{
    partial class MainWindow
    {
        private static MainWindow? instance;
        private static MainWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    ConsoleLog.Log("Main window mediator is not set. Static operations cannot be performed.");
                    instance = new();
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }
        private static NavigatorTreeView Navigator
        {
            get
            {
                return Instance.navigatorTreeView1;
            }
        }
        private static ElementPropertiesPanel PropertiesPanel
        {
            get
            {
                return Instance.elementPropertiesPanel1;
            }
        }

        public static void OpenSchema(ErSchema schema)
        {
            Navigator.OpenSchema(schema);
        }
        public static void OpenProperties<TErElement>(ErSchema schema, TErElement entitySet)
        {
            PropertiesPanel.OpenProperties(schema, entitySet);
        }
        public static void CloseProperties()
        {
            PropertiesPanel.CloseProperties();
        }
    }
}
