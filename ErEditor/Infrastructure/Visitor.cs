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
        public void Visit(T concreteObject);
    }

    public interface IVisitor
    {
        public void Visit<T>(T concreteObject);
    }

    public class Visitor : IVisitor
    {
        private readonly object concreteVisitor;

        public Visitor(object concreteVisitor)
        {
            this.concreteVisitor = concreteVisitor;
        }

        public void Visit<T>(T concreteObject)
        {
            ConsoleLog.Log($"Received object: {concreteObject}, trying to pass it to visitor {concreteVisitor}");
            IVisitor<T>? concreteVisitorCasted = concreteVisitor as IVisitor<T>;
            concreteVisitorCasted?.Visit(concreteObject);

            if(concreteVisitorCasted == null)
            {
                ConsoleLog.Log("Visitor couldn't be casted to the type of the object");
            }
        }
    }

    public interface IVisitable
    {
        public void Accept(IVisitor visitor);
    }
}
