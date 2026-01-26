using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.ExtTreeClasses
{
    public interface ITreeNodeCollection : IList
    {
        public new IExtTreeNode this[int index] { get; set; }
        public IExtTreeNode? this[TreeNode treeNode] { get; }
    }
    // TO-DO: this class lacks many actual method implementations from IList and TreeNodeCollection except those I needed
    public class ExtTreeNodeCollection<TExtTreeNode> : ITreeNodeCollection where TExtTreeNode : IExtTreeNode
    {
        private TreeNodeCollection treeNodeCollection;
        private List<TExtTreeNode> extTreeNodeCollection = new();

        public bool IsFixedSize => ((IList)treeNodeCollection).IsFixedSize;
        public bool IsReadOnly => ((IList)treeNodeCollection).IsReadOnly;
        public int Count => ((ICollection)treeNodeCollection).Count;
        public bool IsSynchronized => ((ICollection)treeNodeCollection).IsSynchronized;
        public object SyncRoot => ((ICollection)treeNodeCollection).SyncRoot;
        object? IList.this[int index] { get => ((IList)treeNodeCollection)[index]; set => ((IList)treeNodeCollection)[index] = value; }
        IExtTreeNode ITreeNodeCollection.this[int index] { get => ((ITreeNodeCollection)treeNodeCollection)[index]; set => ((ITreeNodeCollection)treeNodeCollection)[index] = value; }

        public TExtTreeNode this[int index]
        {
            get
            {
                return extTreeNodeCollection[index];
            }
            set
            {
                extTreeNodeCollection[index] = value;
            }
        }
        public IExtTreeNode? this[TreeNode treeNode] // медленно, надо хешами делать. использовать Name которые у TreeNode ключи
        {
            get
            {
                IExtTreeNode? foundNode = null;
                for (int i = 0; i < treeNodeCollection.Count; i++)
                {
                    if (treeNodeCollection[i] == treeNode)
                    {
                        return extTreeNodeCollection[i];
                    }
                    foundNode = extTreeNodeCollection[i].Nodes[treeNode];
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }
                return null;
            }
        }

        public ExtTreeNodeCollection(TreeNodeCollection treeNodeCollection)
        {
            this.treeNodeCollection = treeNodeCollection; 
        }


        public int Add(TExtTreeNode extTreeNode)
        {
            int index = treeNodeCollection.Add(extTreeNode.TreeNode);
            Console.WriteLine($"ADDING NODE {extTreeNode.Name}, INDEX: {index}");
            extTreeNodeCollection.Add(extTreeNode);
            return index;
        }

        public void Add(TreeNode[] treeNodes)
        {
            treeNodeCollection.AddRange(treeNodes);
        }
        public void AddRange(TreeNode[] treeNodes) // only used for TreeView compatibility
        {
            Add(treeNodes);
        }

        int IList.Add(object? value) => ((IList)treeNodeCollection).Add(value);

        public void Clear()
        {
            ((IList)treeNodeCollection).Clear();
        }

        public bool Contains(object? value)
        {
            return ((IList)treeNodeCollection).Contains(value);
        }

        public int IndexOf(object? value)
        {
            return ((IList)treeNodeCollection).IndexOf(value);
        }

        public void Insert(int index, object? value)
        {
            ((IList)treeNodeCollection).Insert(index, value);
        }

        public void Remove(IExtTreeNode value)
        {
            ((IList)treeNodeCollection).Remove(value.TreeNode);
            ((IList)extTreeNodeCollection).Remove(value);
        }
        void IList.Remove(object? value)
        {
            if(value is IExtTreeNode)
            {
                this.Remove(value as IExtTreeNode);
            }
        }

        public void RemoveAt(int index)
        {
            ((IList)treeNodeCollection).RemoveAt(index);
        }

        public void CopyTo(Array array, int index)
        {
            ((ICollection)treeNodeCollection).CopyTo(array, index);
        }

        public List<TExtTreeNode>.Enumerator GetEnumerator()
        {
            return extTreeNodeCollection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
