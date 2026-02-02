using ErEditor.DbSchemaClasses;
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
        private ValueSetView valueSetView = new();
        private AttributeView attributeView = new();
        private RoleView roleView = new();
        private MappingView mappingView = new();


        private ElementView? activeView;

        public ElementPropertiesPanel()
        {
            Controls.Add(valueSetView);
            Controls.Add(attributeView);
            Controls.Add(roleView);
            Controls.Add(mappingView);

            this.DoubleBuffered = true;
        }

        // Этот объект и MainWindow не интересует конкретный тип TErElement. Они с ним не работают, они его перенапрявлют туда, куда надо.
        public void OpenProperties<TErElement>(ErSchema schema, TErElement element)
            where TErElement : class, IObservable
        {
            CloseProperties();
            ElementView<TErElement>? elementView = null;

            switch (element)
            {
                case ErValueSet valueSet:
                    elementView = valueSetView as ElementView<TErElement>;
                    break;
                case ErAttribute attribute:
                    elementView = attributeView as ElementView<TErElement>;
                    break;
                case ErRole role:
                    elementView = roleView as ElementView<TErElement>;
                    break;
                case ErMapping mapping:
                    elementView = mappingView as ElementView<TErElement>;
                    break;
            }

            if (elementView != null)
            {
                elementView.Open(schema, element);
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
