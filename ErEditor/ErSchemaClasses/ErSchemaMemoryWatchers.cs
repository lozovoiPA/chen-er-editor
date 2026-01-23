using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.ErSchemaClasses
{
    public abstract class ErElementWatcher<TErElement> : CollectionWatcher<TErElement>,
        IVisitor<ObjectNameChangedNotification>,
        IVisitor<ObjectCreatedNotification<TErElement>>,
        IVisitor<ObjectDeletedNotification<TErElement>>

        where TErElement : class, IObservable
    {
        public virtual void Visit(ObjectNameChangedNotification notif)
        {
            ConsoleLog.Log($"Name of {notif.Object.GetType()} was changed", this, "INFO");
            TErElement? castedEl = notif.Object as TErElement;
            if (castedEl != null)
            {
                observers.Notify(new ObjectUpdatedNotification<TErElement>(castedEl));
                return;
            }
        }
        public override void Visit(ObjectCreatedNotification<TErElement> notif)
        {
            base.Visit(notif);
            notif.Object.Subscribe(notificationProcessor);
        }
        public override void Visit(ObjectDeletedNotification<TErElement> notif)
        {
            base.Visit(notif);
            notif.Object.Unsubscribe(notificationProcessor);
        }
    }

    public abstract class ErElementWithAttributesWatcher<TErElement> : ErElementWatcher<TErElement>,
        IVisitor<ObjectAddedNotification<ErElementWithAttributes, ErAttribute>>
        
        where TErElement : ErElementWithAttributes
    {
        public override void Visit(ObjectNameChangedNotification notif)
        {
            ConsoleLog.Log($"Name of {notif.Object.GetType()} was changed", this, "INFO");
            TErElement? castedEl = notif.Object as TErElement;
            if(castedEl != null)
            {
                observers.Notify(new ObjectUpdatedNotification<TErElement>(castedEl));
                return;
            }
            ErAttribute? castedAttr = notif.Object as ErAttribute;
            if (castedAttr != null)
            {
                observers.Notify(new ObjectUpdatedNotification<ErAttribute>(castedAttr));
                return;
            }
        }
        public virtual void Visit(ObjectAddedNotification<ErElementWithAttributes, ErAttribute> notif)
        {
            ConsoleLog.Log($"New attribute was added to the element", this, "INFO");
            notif.ObjectAdded.Subscribe(this);

            observers.Notify(notif);
        }
    }
    public class ErEntitySetWatcher : ErElementWithAttributesWatcher<ErEntitySet> { }
    public class ErRelationshipSetWatcher : 
        ErElementWithAttributesWatcher<ErRelationshipSet>,
        IVisitor<ObjectAddedNotification<ErRelationshipSet, ErRole>>
    {
        public void Visit(ObjectAddedNotification<ErRelationshipSet, ErRole> notif)
        {
            notif.ObjectAdded.Subscribe(this);
            observers.Notify(notif);
        }
    }
    public class ErValueSetWatcher : ErElementWatcher<ErValueSet> { }
    public class ErDiagramWatcher 
        : ErElementWatcher<ErDiagram>,
        IVisitor<ObjectAddedNotification<ErDiagram, ErDiagramPrimitive>>
    {
        public void Visit(ObjectAddedNotification<ErDiagram, ErDiagramPrimitive> notif)
        {
            notif.ObjectAdded.Subscribe(this);
            observers.Notify(notif);
        }
        public void Visit(ObjectDeletedNotification<ErDiagramPrimitive> notif)
        {
            notif.Object.Unsubscribe(this);
            observers.Notify(notif);
        }
    }
}
