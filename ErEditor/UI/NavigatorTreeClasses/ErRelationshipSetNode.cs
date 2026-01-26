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
        public class ErRelationshipSetNode :
            NavigatorErNode<ErRelationshipSet>,
            IObserver,
            IVisitor<ObjectNameChangedNotification>,
            IVisitor<ObjectAddedNotification<ErRelationshipSet, ErAttribute>>,
            IVisitor<ObjectAddedNotification<ErRelationshipSet, ErRole>>,
            IVisitor<ObjectAddedNotification<ErRelationshipSet, ErMapping>>,
            IVisitor<ObjectDeletedNotification<ErAttribute>>,
            IVisitor<ObjectDeletedNotification<ErRole>>,
            IVisitor<ObjectDeletedNotification<ErMapping>>
        {
            private ExtTreeNodeCollection<IExtTreeNode> nodes;
            private ErRelationshipSet relationshipSet;
            private NavigatorTreeView parentTree;

            private ExtTreeNodeTyped<object, ErAttributeNode> attributeFolder = new("Атрибуты");
            private ExtTreeNodeTyped<object, ErRoleNode> roleFolder = new("Роли");
            private ExtTreeNodeTyped<object, ErMappingNode> mappingFolder = new("Отображения");

            private NotificationVisitor visitorLogic;

            public ErRelationshipSetNode(ErSchema schema, ErRelationshipSet relationshipSet, NavigatorTreeView parentTree) : base(schema)
            {
                nodes = new(TreeNodes);
                this.relationshipSet = relationshipSet;
                base.Name = relationshipSet.Name;
                this.parentTree = parentTree;

                Initialize();

                visitorLogic = new(this);
                relationshipSet.Subscribe(this);
            }

            public override string Name
            {
                get { return base.Name; }
                set { relationshipSet.Name = value; }
            }
            public override ErRelationshipSet Data
            {
                get { return relationshipSet; }
                set { relationshipSet = value; }
            }
            public override ExtTreeNodeCollection<IExtTreeNode> Nodes
            {
                get { return nodes; }
            }

            private void Initialize()
            {
                ImageIndex = 4;
                SelectedImageIndex = 4;
                UIHelper.AddContextMenu(
                    this,
                    new Dictionary<string, EventHandler>() {
                        { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) },
                        { "Удалить", new EventHandler(DeleteRelationshipSet) }
                    });

                UIHelper.AddContextMenu(attributeFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(AddAttribute) } });
                nodes.Add(attributeFolder);
                attributeFolder.ImageIndex = 1;
                attributeFolder.SelectedImageIndex = 1;

                UIHelper.AddContextMenu(roleFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(AddRole) } });
                nodes.Add(roleFolder);
                roleFolder.ImageIndex = 1;
                roleFolder.SelectedImageIndex = 1;

                nodes.Add(mappingFolder);
                mappingFolder.ImageIndex = 1;
                mappingFolder.SelectedImageIndex = 1;

                foreach (var attr in relationshipSet.Attributes)
                {
                    AddAttributeNode(attr);
                }
                foreach (var role in relationshipSet.Roles)
                {
                    AddRoleNode(role);
                }
                foreach (var mapping in relationshipSet.Mappings)
                {
                    AddMappingNode(mapping);
                }
            }

            private void DeleteRelationshipSet(object? sender, EventArgs e)
            {
                ParentSchema.RelationshipSets.Remove(relationshipSet);
            }
            private ErAttributeNode AddAttributeNode(ErAttribute attribute)
            {
                var newNode = new ErAttributeNode(ParentSchema, attribute, parentTree);
                attributeFolder.Nodes.Add(newNode);
                return newNode;
            }
            private void AddAttribute(object? sender, EventArgs e)
            {
                var newAttribute = relationshipSet.AddAttribute();
                var newNode = AddAttributeNode(newAttribute);

                attributeFolder.Expand();
                parentTree.RenameNode(newNode);
            }
            private ErRoleNode AddRoleNode(ErRole role)
            {
                var newNode = new ErRoleNode(ParentSchema, relationshipSet, role, parentTree);
                roleFolder.Nodes.Add(newNode);
                role.Subscribe(this);

                return newNode;
            }
            private void AddRole(object? sender, EventArgs e)
            {
                var es = parentTree.GetNodeData<ErEntitySet>(Parent.Parent.Nodes[0].Nodes[0]);
                if (Parent.Parent.Nodes[0].Nodes.Count > 0 && es != null)
                {
                    var newRole = relationshipSet.AddRole(es, "", true);
                    var newNode = roleFolder.Nodes[roleFolder.Nodes.Count - 1];

                    roleFolder.Expand();
                    parentTree.RenameNode(newNode);
                }
            }
            private ErMappingNode AddMappingNode(ErMapping mapping)
            {
                var newNode = new ErMappingNode(ParentSchema, mapping, parentTree);
                if (mapping.Name == "")
                {
                    newNode.DisplayName = mapping.DefaultName;
                }
                mappingFolder.Nodes.Add(newNode);

                return newNode;
            }

            public void Recieve(Notification notification)
            {
                notification.Accept(visitorLogic);
            }
            public void Visit(ObjectNameChangedNotification notif)
            {
                switch (notif.Object)
                {
                    case ErRelationshipSet rs:
                        base.Name = notif.NewName;
                        break;
                    case ErRole role:
                        foreach (var node in mappingFolder.Nodes)
                        {
                            if (node.Data.Name == "")
                            {
                                node.DisplayName = node.Data.DefaultName;
                            }
                        }
                        break;
                }

            }
            public void Visit(ObjectAddedNotification<ErRelationshipSet, ErAttribute> notification)
            {
                AddAttributeNode(notification.ObjectAdded);
            }
            public void Visit(ObjectAddedNotification<ErRelationshipSet, ErRole> notification)
            {
                AddRoleNode(notification.ObjectAdded);
            }
            public void Visit(ObjectAddedNotification<ErRelationshipSet, ErMapping> notification)
            {
                AddMappingNode(notification.ObjectAdded);
            }
            public void Visit(ObjectDeletedNotification<ErAttribute> notif)
            {
                DeleteChildNode(notif.Object, attributeFolder);
            }
            public void Visit(ObjectDeletedNotification<ErRole> notif)
            {
                DeleteChildNode(notif.Object, roleFolder);
            }
            public void Visit(ObjectDeletedNotification<ErMapping> notif)
            {
                DeleteChildNode(notif.Object, mappingFolder);
            }
        }
    }
}
