using ErEditor.ErSchemaClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace ErEditor.Infrastructure
{
    public abstract class Notification : IVisitable
    {
        public virtual void Accept(IVisitor visitor) { }
    }

    public class ObjectAddedNotification<TObject> : Notification
    {
        public readonly TObject Object;

        public ObjectAddedNotification(TObject @object)
        {
            Object = @object;
        }

        public override void Accept(IVisitor visitor) { visitor.Visit(this); } // if this is not overriden, then this Notification type cannot be visited
    }

    public class ObjectUpdatedNotification<TObject> : Notification
    {
        public readonly TObject Object;

        public ObjectUpdatedNotification(TObject @object)
        {
            Object = @object;
        }

        public override void Accept(IVisitor visitor) { visitor.Visit(this); }
    }

    public class ObjectDeletedNotification<TObject> : Notification
    {
        public readonly TObject Object;

        public ObjectDeletedNotification(TObject @object)
        {
            Object = @object;
        }

        public override void Accept(IVisitor visitor) { visitor.Visit(this); }
    }

    public class ObjectNameChangedNotification<TObject> : ObjectUpdatedNotification<TObject>
    {
        public readonly string NewName;

        public ObjectNameChangedNotification(TObject @object, string newName) : base(@object)
        {
            this.NewName = newName;
        }

        public override void Accept(IVisitor visitor) { visitor.Visit(this); }
    }

}
