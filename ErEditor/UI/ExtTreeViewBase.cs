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
        }

        // rename node by handler (selected node)
        // this handler should be accessible for assignments for nodes and other stuff
        public void RenameSelectedNode(object? sender, EventArgs e)
        {
            if(SelectedNode != null)
            {
                RenameNode(SelectedNode);
            }
        }
        // rename node manually
        public void RenameNode(IExtTreeNode node)
        {
            LabelEdit = true;
            SelectedNode = node;
            editingNode = node;
            editingNode.BeginEdit();
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

        public TData? GetNodeData<TData>(TreeNode node)
        {
            if(Nodes[node] != null)
            {
                var extNode = (Nodes[node] as ExtTreeNodeWithNullableData<TData>);
                TData? data = default;
                if (extNode != null)
                {
                    data = extNode.Data;
                }
                return data;
            }
            return default;
        }
    }
}
