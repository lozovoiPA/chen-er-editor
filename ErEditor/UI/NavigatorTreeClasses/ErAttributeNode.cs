using ErEditor.ErSchemaClasses;
using ErEditor.UI.ExtTreeClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.NavigatorTreeClasses
{
    partial class NavigatorTreeView
    {
        public class ErAttributeNode : NavigatorErNode<ErAttribute>
        {
            private ExtTreeNodeCollection<IExtTreeNode> nodes;
            private ErAttribute attribute;
            private NavigatorTreeView parentTree;

            public ErAttributeNode(ErSchema schema, ErAttribute attribute, NavigatorTreeView parentTree) : base(schema)
            {
                nodes = new(TreeNodes);
                this.attribute = attribute;
                base.Name = attribute.Name;
                this.parentTree = parentTree;

                Initialize();
            }

            public override string Name
            {
                get { return base.Name; }
                set { base.Name = value; attribute.Name = value; }
            }
            public override ErAttribute Data
            {
                get { return attribute; }
                set { attribute = value; }
            }
            public override ExtTreeNodeCollection<IExtTreeNode> Nodes
            {
                get { return nodes; }
            }

            private void Initialize()
            {
                ImageIndex = 6;
                SelectedImageIndex = 6;
                UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });
            }
        }
    }
}
