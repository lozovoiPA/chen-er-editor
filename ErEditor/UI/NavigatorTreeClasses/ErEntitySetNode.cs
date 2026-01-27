using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
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
        public class ErEntitySetNode : 
            NavigatorErNode<ErEntitySet>, 
            IVisitor<ObjectDeletedNotification<ErAttribute>>
        {
            private ExtTreeNodeCollection<IExtTreeNode> nodes;
            private ErEntitySet entitySet;
            private NavigatorTreeView parentTree;

            private ExtTreeNodeTyped<object, ErAttributeNode> attributeFolder = new("Атрибуты");

            public ErEntitySetNode(ErSchema schema, ErEntitySet entitySet, NavigatorTreeView parentTree) : base(schema)
            {
                nodes = new(TreeNode.Nodes); // или this.TreeNodes
                this.entitySet = entitySet;
                base.Name = entitySet.Name;
                this.parentTree = parentTree;

                Initialize();

                // notificationParser = new(this); we can actually do this for nonabstract hierarchies of observers
                entitySet.Subscribe(this);
            }

            public override string Name
            {
                get { return base.Name; }
                set { entitySet.Name = value; }
            }
            public override ErEntitySet Data
            {
                get { return entitySet; }
                set { entitySet = value; }
            }
            public override ExtTreeNodeCollection<IExtTreeNode> Nodes
            {
                get { return nodes; }
            }

            private void Initialize()
            {
                ImageIndex = 3;
                SelectedImageIndex = 3;
                UIHelper.AddContextMenu(
                    this,
                    new Dictionary<string, EventHandler>() {
                    { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) },
                    { "Удалить", new EventHandler(DeleteEntitySet) }
                    });

                UIHelper.AddContextMenu(attributeFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(AddAttribute) } });
                nodes.Add(attributeFolder);
                attributeFolder.ImageIndex = 1;
                attributeFolder.SelectedImageIndex = 1;

                foreach (var attr in entitySet.Attributes)
                {
                    AddAttributeNode(attr);
                }
            }
            private void DeleteEntitySet(object? sender, EventArgs e)
            {
                parentSchema.EntitySets.Remove(entitySet);
            }
            private ErAttributeNode AddAttributeNode(ErAttribute attribute)
            {
                var newNode = new ErAttributeNode(parentSchema, entitySet, attribute, parentTree);
                attributeFolder.Nodes.Add(newNode);
                return newNode;
            }
            private void AddAttribute(object? sender, EventArgs e)
            {
                var newAttribute = entitySet.AddAttribute();
                var newNode = AddAttributeNode(newAttribute);

                attributeFolder.Expand();
                parentTree.RenameNode(newNode);
            }

            public override void Recieve(Notification notification)
            {
                notificationParser.Recieve(notification);
            }
            public override void Visit(ObjectNameChangedNotification notification)
            {
                DisplayName = notification.NewName;
            }
            public void Visit(ObjectDeletedNotification<ErAttribute> notif)
            {
                DeleteChildNode(notif.Object, attributeFolder);
            }
        }
    }
}
