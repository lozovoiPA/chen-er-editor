using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public partial class ErSchema :
        IVisitor<ObjectNameChangedNotification<ErElement>>,
        IVisitor<ObjectNameChangedNotification<ErEntitySet>>,
        IVisitor<ObjectNameChangedNotification<ErRelationshipSet>>,
        IVisitor<ObjectNameChangedNotification<ErValueSet>>,
        IVisitor<ObjectNameChangedNotification<ErDiagram>>
    {
        public void Visit(ObjectNameChangedNotification<ErElement> concreteObject)
        {
            // тут можно написать поиск объекта в коллекциях. Это если не хочется дженериками сыпать.
        }

        public void Visit(ObjectNameChangedNotification<ErEntitySet> concreteObject)
        {
            observableLogic.Notify(new ObjectUpdatedNotification<ErEntitySet>(concreteObject.Object));
        }
        public void Visit(ObjectNameChangedNotification<ErRelationshipSet> concreteObject)
        {
            observableLogic.Notify(new ObjectUpdatedNotification<ErRelationshipSet>(concreteObject.Object));
        }
        public void Visit(ObjectNameChangedNotification<ErValueSet> concreteObject)
        {
            observableLogic.Notify(new ObjectUpdatedNotification<ErValueSet>(concreteObject.Object));
        }
        public void Visit(ObjectNameChangedNotification<ErDiagram> concreteObject)
        {
            observableLogic.Notify(new ObjectUpdatedNotification<ErDiagram>(concreteObject.Object));
        }
    }
}
