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
        IVisitor<ObjectUpdatedNotification<TErElement>>,
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
        public void Visit(ObjectUpdatedNotification<TErElement> notification)
        {
            observers.Notify(notification);
        }
        public override void Visit(ObjectDeletedNotification<TErElement> notif)
        {
            base.Visit(notif);
            notif.Object.Unsubscribe(notificationProcessor);
        }
    }

    public abstract class ErElementWithAttributesWatcher<TErElement> : ErElementWatcher<TErElement>,
        IVisitor<ObjectAddedNotification<ErElementWithAttributes, ErAttribute>>,
        IVisitor<ObjectUpdatedNotification<ErAttribute>>,
        IVisitor<ObjectDeletedNotification<ErAttribute>>
        
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
        public void Visit(ObjectUpdatedNotification<ErAttribute> notification)
        {
            observers.Notify(notification);
        }
        public void Visit(ObjectDeletedNotification<ErAttribute> notification)
        {
            observers.Notify(notification);
            notification.Object.Unsubscribe(this);
        }
    }
    public class ErEntitySetWatcher : ErElementWithAttributesWatcher<ErEntitySet> { }
    public class ErRelationshipSetWatcher : 
        ErElementWithAttributesWatcher<ErRelationshipSet>,
        IVisitor<ObjectAddedNotification<ErRelationshipSet, ErRole>>,
        IVisitor<ObjectAddedNotification<ErRelationshipSet, ErMapping>>,
        IVisitor<ObjectUpdatedNotification<ErRole>>,
        IVisitor<ObjectUpdatedNotification<ErMapping>>,
        IVisitor<ObjectDeletedNotification<ErRole>>,
        IVisitor<ObjectDeletedNotification<ErMapping>>
    {
        public void Visit(ObjectAddedNotification<ErRelationshipSet, ErRole> notif)
        {
            notif.ObjectAdded.Subscribe(this);
            observers.Notify(notif);
        }
        public void Visit(ObjectAddedNotification<ErRelationshipSet, ErMapping> notif)
        {
            notif.ObjectAdded.Subscribe(this);
            observers.Notify(notif);
        }
        public void Visit(ObjectUpdatedNotification<ErRole> notification)
        {
            observers.Notify(notification);
        }
        public void Visit(ObjectUpdatedNotification<ErMapping> notif)
        {
            observers.Notify(notif);
        }
        public void Visit(ObjectDeletedNotification<ErRole> notification)
        {
            observers.Notify(notification);
            notification.Object.Unsubscribe(this);
        }
        public void Visit(ObjectDeletedNotification<ErMapping> notification)
        {
            observers.Notify(notification);
            notification.Object.Unsubscribe(this);
        }
    }
    public class ErValueSetWatcher : ErElementWatcher<ErValueSet> { }
    public class ErDiagramWatcher 
        : ErElementWatcher<ErDiagram>,
        IVisitor<ObjectAddedNotification<ErDiagram, ErDiagramPrimitive>>,
        IVisitor<ObjectUpdatedNotification<ErDiagramPrimitive>>,
        IVisitor<ObjectDeletedNotification<ErDiagramPrimitive>>
    {
        public void Visit(ObjectAddedNotification<ErDiagram, ErDiagramPrimitive> notif)
        {
            notif.ObjectAdded.Subscribe(this);
            observers.Notify(notif);
        }
        public void Visit(ObjectUpdatedNotification<ErDiagramPrimitive> notification)
        {
            observers.Notify(notification);
        }
        public void Visit(ObjectDeletedNotification<ErDiagramPrimitive> notif)
        {
            notif.Object.Unsubscribe(this);
            observers.Notify(notif);
        }
    }
}
