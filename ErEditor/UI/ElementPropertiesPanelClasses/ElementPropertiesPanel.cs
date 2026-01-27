using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.ElementPropertiesPanelClasses
{
    public partial class ElementPropertiesPanel : Panel
    {
        private RoleView roleView = new();
        private MappingView mappingView = new MappingView();

        private ElementView? activeView;

        public ElementPropertiesPanel()
        {
            roleView.Visible = false;
            mappingView.Visible = false;
            roleView.Dock = DockStyle.Fill;
            mappingView.Dock = DockStyle.Fill;
            Controls.Add(roleView);
            Controls.Add(mappingView);

            this.DoubleBuffered = true;
        }

        // Этот объект и MainWindow не интересует конкретный тип TErElement. Они с ним не работают, они его перенапрявлют туда, куда надо.
        public void OpenProperties<TErElement>(ErSchema schema, TErElement element)
        {
            CloseProperties();
            ElementView<TErElement>? elementView = null;

            switch (element)
            {
                case ErRole role:
                    elementView = roleView as ElementView<TErElement>;
                    roleView.Open(schema, role);
                    break;
                case ErMapping mapping:
                    elementView = mappingView as ElementView<TErElement>;
                    mappingView.Open(mapping);
                    break;
            }

            if (elementView != null)
            {
                activeView = elementView;
                activeView.Visible = true;
            }
        }
        public void CloseProperties()
        {
            if(activeView != null)
            {
                activeView.CloseAndSave();
                activeView.Visible = false;
                activeView = null;
            }
        }
        public void CloseAndDiscardProperties()
        {
            if (activeView != null)
            {
                activeView.CloseAndDiscard();
                activeView.Visible = false;
                activeView = null;
            }
        }
    }
}
