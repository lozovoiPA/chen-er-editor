using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.Infrastructure
{
    // Посетитель в конкретный объект - тип объекта T - абстрактный 
    public interface IVisitor<T>
    {
        public void Visit(T notification);
    }

    public interface IVisitor
    {
        public void Visit<T>(T concreteObject);
    }

    // alternatively this is abstract-to-concrete type resolver
    public class NotificationVisitor : IVisitor
    {
        private readonly object concreteVisitor;
        private readonly string concreteVisitorName = "[parent class not specified]";

        public NotificationVisitor(object concreteVisitor)
        {
            this.concreteVisitor = concreteVisitor;
            concreteVisitorName = ConsoleLog.GetShortTypeName(concreteVisitor);
        }

        public void Visit<T>(T notification)
        {
            //ConsoleLog.Log($"Sending notif {notification.GetType()} to {concreteVisitor.GetType()}");
            IVisitor<T>? concreteVisitorCasted = concreteVisitor as IVisitor<T>;
            concreteVisitorCasted?.Visit(notification);
        }
    }

    public interface IVisitable
    {
        public void Accept(IVisitor visitor);
    }
}
