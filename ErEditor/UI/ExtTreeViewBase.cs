using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ErEditor.UI
{
    // Будет переопределять методы TreeView
    public abstract class ExtTreeViewBase : TreeView
    {
        protected IExtTreeNode? editingNode = null;

        public ExtTreeViewBase()
        {
            Initialize();
        }

        new public abstract ITreeNodeCollection Nodes { get; }
        protected TreeNodeCollection TreeNodes
        {
            get { return base.Nodes; }
        }
        new public IExtTreeNode? SelectedNode
        {
            get
            {
                return Nodes[base.SelectedNode]!;
            }
            set
            {
                if (value == null)
                {
                    base.SelectedNode = null;
                }
                else
                {
                    base.SelectedNode = value.TreeNode;
                }
            }
        }

        private void Initialize()
        {
            LabelEdit = false;
            SelectedNode = null;
            ImageList = new ImageList();
            ImageList.Images.Add(new Bitmap(1, 1));

            NodeMouseClick += new TreeNodeMouseClickEventHandler(ClickNode);
            AfterLabelEdit += new NodeLabelEditEventHandler(EndRenamingNode);
        }
        protected virtual void ClickNode(object? sender, TreeNodeMouseClickEventArgs e)
        {
            SelectedNode = Nodes[e.Node];
            Console.WriteLine($"Clicked: {e.Node.Name}");
        }
        protected virtual void EndRenamingNode(object? sender, NodeLabelEditEventArgs e)
        {
            if (editingNode != null)
            {
                editingNode.EndEdit(true);
                editingNode.Name = e.Label == null ? String.Empty : e.Label;

                SelectedNode = null;
                editingNode = null;
            }
            LabelEdit = false;
        }
        private void BeginRenamingNodeInner(IExtTreeNode node)
        {
            LabelEdit = true;
            if (node != null)
            {
                Console.WriteLine($"Begin editing node (inner): {node.Name}");
                SelectedNode = node;
                editingNode = node;
                editingNode.BeginEdit();
            }
        }
        public void RenameSelectedNode(object? sender, EventArgs e)
        {
            BeginRenamingNodeInner(this.SelectedNode);
        }
        public void RenameNode(IExtTreeNode node)
        {
            Console.WriteLine($"Begin editing node (Rename manually): {node.Name}");
            BeginRenamingNodeInner(node);
        }
    }
}
