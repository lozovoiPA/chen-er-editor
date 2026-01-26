using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.ExtTreeClasses
{
    // Class that allows you to store any data in a TreeNode
    // Children must override Data and Nodes they store
    public interface IExtTreeNode
    {
        public TreeNode TreeNode { get; }
        public string Name { get; set; }
        public ITreeNodeCollection Nodes { get; }

        public void BeginEdit();
        public void EndEdit(bool cancel);

        public void Click(object? sender, MouseEventArgs e);
        public void DoubleClick(object? sender, MouseEventArgs e);
    }
    public abstract class ExtTreeNodeBase : TreeNode, IExtTreeNode
    {
        public TreeNode TreeNode
        {
            get { return this; }    
        }
        public new virtual string Name
        {
            get { return Text; }
            set { base.Name = value; Text = value; }
        }
        public new abstract ITreeNodeCollection Nodes { get; }
        protected TreeNodeCollection TreeNodes { get { return base.Nodes; } }

        // The base ExtTreeView implementation doesn't pass it's node-related handlers 
        // to child nodes. However you can derive your own implementation that does that.
        // Override these handlers if you need the node to react to the events.
        public virtual void Click(object? sender, MouseEventArgs e)
        {

        }
        public virtual void DoubleClick(object? sender, MouseEventArgs e)
        {

        }
    }
    public abstract class ExtTreeNodeWithNullableData<TData> : ExtTreeNodeBase
    {
        public abstract TData? Data { get; set; }
    }
    public abstract class ExtTreeNodeWithNotNullableData<TData> : ExtTreeNodeBase
    {
        public abstract TData Data { get; set; }
    }

    // ExtTreeNode подходит для любых типов объектов и любых целей, когда не нужны особые классы узлов
    public class ExtTreeNode : ExtTreeNodeWithNullableData<object>
    {
        private object? data;
        private ExtTreeNodeCollection<IExtTreeNode> nodes;

        public ExtTreeNode(string name = "", object? data = null)
        {
            nodes = new(TreeNode.Nodes);
            Name = name;
            this.data = data;
        }

        public override object? Data
        {
            get { return data; }
            set { data = value; }
        }
        public override ExtTreeNodeCollection<IExtTreeNode> Nodes
        {
            get { return nodes; }
        }
    }
    // ExtTreeNodeTyped нужен для создания узлов с конкретным типом хранимых данных или конкретным типом узлов в коллекции
    public class ExtTreeNodeTyped<T, U> : ExtTreeNodeWithNullableData<T> where U: IExtTreeNode
    {
        private T? data;
        private ExtTreeNodeCollection<U> nodes;

        public ExtTreeNodeTyped(string name = "")
        {
            nodes = new(TreeNode.Nodes);
            Name = name;
        }

        public override T? Data
        {
            get { return data; }
            set { data = value; }
        }
        public override ExtTreeNodeCollection<U> Nodes
        {
            get { return nodes; }
        }
    }
}
