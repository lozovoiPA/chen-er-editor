using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public abstract class ErElementWatcher<TErElement> : RegistryMemoryWatcher,
        IVisitor<ObjectNameChangedNotification>,
        IVisitor<ObjectCreatedNotification<TErElement>>

        where TErElement : class, IErElement
    {
        public virtual void Visit(ObjectNameChangedNotification notif)
        {
            ConsoleLog.Log($"Name of {notif.Object.GetType()} was changed", this, "INFO");
            TErElement? castedEl = notif.Object as TErElement;
            if (castedEl != null)
            {
                observableLogic.Notify(new ObjectUpdatedNotification<TErElement>(castedEl));
                return;
            }
        }
        public virtual void Visit(ObjectCreatedNotification<TErElement> notif)
        {
            ConsoleLog.Log($"New {notif.Object.GetType()} was added", this, "INFO");
            notif.Object.Subscribe(observerLogic);
            observableLogic.Notify(notif);
        }
    }

    public abstract class ErElementWithAttributesWatcher<TErElement> : ErElementWatcher<TErElement>,
        IVisitor<ObjectAddedNotification<ErAttribute, ErElementWithAttributes>>
        
        where TErElement : ErElementWithAttributes
    {
        public override void Visit(ObjectNameChangedNotification notif)
        {
            ConsoleLog.Log($"Name of {notif.Object.GetType()} was changed", this, "INFO");
            TErElement? castedEl = notif.Object as TErElement;
            if(castedEl != null)
            {
                observableLogic.Notify(new ObjectUpdatedNotification<TErElement>(castedEl));
                return;
            }
            ErAttribute? castedAttr = notif.Object as ErAttribute;
            if (castedAttr != null)
            {
                observableLogic.Notify(new ObjectUpdatedNotification<ErAttribute>(castedAttr));
                return;
            }
        }
        public virtual void Visit(ObjectAddedNotification<ErAttribute, ErElementWithAttributes> notif)
        {
            ConsoleLog.Log($"New attribute was added to the element", this, "INFO");
            notif.Object.Subscribe(this);

            observableLogic.Notify(new ObjectAddedNotification<ErAttribute, ErElementWithAttributes>(notif.Object, notif.AddedTo));
        }
    }
    public class ErEntitySetWatcher : ErElementWithAttributesWatcher<ErEntitySet>
    {
        public override void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
    }
    public class ErRelationshipSetWatcher : ErElementWithAttributesWatcher<ErRelationshipSet>
    {
        public override void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
    }
    public class ErValueSetWatcher : ErElementWatcher<ErValueSet>
    {
        public override void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
    }
    public class ErDiagramWatcher : ErElementWatcher<ErDiagram>
    {
        public override void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
    }
    public class ErAttributeWatcher : ErElementWatcher<ErAttribute>
    {
        public override void Recieve(Notification notification)
        {
            observerLogic.Recieve(notification);
        }
    }
}
