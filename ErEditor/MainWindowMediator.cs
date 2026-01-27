
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using ErEditor.UI;
using ErEditor.UI.ElementPropertiesPanelClasses;
using ErEditor.UI.NavigatorTreeClasses;

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
        private static DiagramPanel DiagramPanel
        {
            get
            {
                return Instance.diagramPanel1;
            }
        }

        public static void OpenSchema(ErSchema schema)
        {
            Navigator.OpenSchema(schema);
        }
        public static void OpenProperties<TErElement>(ErSchema schema, TErElement element)
            where TErElement : class, IObservable
        {
            PropertiesPanel.OpenProperties(schema, element);
        }
        public static void OpenDiagram(ErSchema schema, ErDiagram diagram)
        {
            DiagramPanel.BackColor = SystemColors.Window;
            Instance.groupBox2.Text = $"Диаграммер - {diagram.Name}";
            DiagramPanel.OpenDiagram(schema, diagram);
        }
        public static void CloseProperties()
        {
            PropertiesPanel.CloseProperties();
        }
        public static void CloseDiagram()
        {
            DiagramPanel.CloseDiagram();
            DiagramPanel.BackColor = SystemColors.ControlLight;
        }
    }
}
