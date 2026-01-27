using ErEditor.DbSchemaClasses;
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
        public class ErRoleNode : NavigatorErNode<ErRole>
        {
            private ExtTreeNodeCollection<IExtTreeNode> nodes;
            private ErRole role;
            private NavigatorTreeView parentTree;
            private ErRelationshipSet parentRelationshipSet;

            public ErRoleNode(ErSchema schema, ErRelationshipSet parent, ErRole role, NavigatorTreeView parentTree) : base(schema)
            {
                nodes = new(TreeNodes);
                this.role = role;
                base.Name = role.Name;
                this.parentTree = parentTree;
                this.parentRelationshipSet = parent;

                Initialize();
                role.Subscribe(this);
            }

            public override string Name
            {
                get { return base.Name; }
                set { base.Name = value; role.Name = value; }
            }
            public override ErRole Data
            {
                get { return role; }
                set { role = value; }
            }
            public override ExtTreeNodeCollection<IExtTreeNode> Nodes
            {
                get { return nodes; }
            }

            private void Initialize()
            {
                ImageIndex = 7;
                SelectedImageIndex = 7;
                UIHelper.AddContextMenu(
                    this,
                    new Dictionary<string, EventHandler>() {
                        { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) },
                        { "Удалить", new EventHandler(DeleteRole) }
                    });
            }

            private void DeleteRole(object? sender, EventArgs e)
            {
                parentRelationshipSet.RemoveRole(role);
            }
        }
    }
}
