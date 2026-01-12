using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI
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
    }
    public abstract class ExtTreeNodeBase<T> : TreeNode, IExtTreeNode
    {
        public TreeNode TreeNode
        {
            get { return this; }    
        }
        public new virtual string Name
        {
            get { return base.Text; }
            set { base.Name = value; base.Text = value; }
        }
        public abstract T? Data { get; set; }
        public new abstract ITreeNodeCollection Nodes { get; }
        protected TreeNodeCollection TreeNodes { get { return base.Nodes; } }
    }

    // ExtTreeNode подходит для любых типов объектов и любых целей, когда не нужны особые классы узлов
    public class ExtTreeNode : ExtTreeNodeBase<object>
    {
        private object? data;
        private ExtTreeNodeCollection<IExtTreeNode> nodes;

        public ExtTreeNode(string name = "", object? data = null)
        {
            nodes = new(this.TreeNode.Nodes);
            this.Name = name;
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
    public class ExtTreeNodeTyped<T, U> : ExtTreeNodeBase<T> where U: IExtTreeNode
    {
        private T? data;
        private ExtTreeNodeCollection<U> nodes;

        public ExtTreeNodeTyped(string name = "")
        {
            nodes = new(this.TreeNode.Nodes);
            this.Name = name;
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
