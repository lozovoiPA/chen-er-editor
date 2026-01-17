using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ErEditor.UI
{
    // По сути этот же нод ответственен и за своих четырех детей. Нет смысла плодить дополнительные классы если они неразрывно связаны со схемой и ее коллекциями.
    public class ErSchemaNode : ExtTreeNodeBase<ErSchema>, IObserver
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes; // если такая коллекция значит в нодах храним разные объекты
        private ErSchema schema;
        private NavigatorTreeView parentTree;

        private ExtTreeNodeTyped<object, ErEntitySetNode> entitySetFolder = new("Множества сущностей");
        private ExtTreeNodeTyped<object, ErRelationshipSetNode> relationshipSetFolder = new("Множества связей");
        private ExtTreeNodeTyped<object, ErValueSetNode> valueSetFolder = new("Множества значений");
        private ExtTreeNodeTyped<object, ErDiagramNode> diagramFolder = new("ER-диаграммы");

        private bool acceptNotifications = true;
        public ErSchemaNode(ErSchema schema, NavigatorTreeView parentTree)
        {
            nodes = new(this.TreeNodes);
            this.schema = schema;
            base.Name = schema.Name;
            this.parentTree = parentTree;

            this.Initialize();

            schema.Subscribe(this);
        }

        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; schema.Name = value; }
        }
        public override ErSchema Data
        {
            get { return schema; }
            set { schema = value; }
        }
        public override ExtTreeNodeCollection<IExtTreeNode> Nodes
        {
            get { return nodes; }
        }

        private void Initialize()
        {
            this.ImageIndex = 0;
            this.SelectedImageIndex = 0;
            UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });

            UIHelper.AddContextMenu(entitySetFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddEntitySet) } });
            UIHelper.AddContextMenu(relationshipSetFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddRelationshipSet) } });
            UIHelper.AddContextMenu(valueSetFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddValueSet) } });
            UIHelper.AddContextMenu(diagramFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddDiagram) } });

            this.nodes.Add(entitySetFolder);
            this.nodes.Add(relationshipSetFolder);
            this.nodes.Add(valueSetFolder);
            this.nodes.Add(diagramFolder);

            entitySetFolder.ImageIndex = 1;
            entitySetFolder.SelectedImageIndex = 1;
            relationshipSetFolder.ImageIndex = 1;
            relationshipSetFolder.SelectedImageIndex = 1;
            valueSetFolder.ImageIndex = 1;
            valueSetFolder.SelectedImageIndex = 1;
            diagramFolder.ImageIndex = 0;
            diagramFolder.SelectedImageIndex = 0;

            foreach (var el in this.schema.EntitySets)
            {
                AddEntitySetNode(el);
            }
            foreach (var el in this.schema.RelationshipSets)
            {
                AddRelationshipSetNode(el);
            }
            foreach (var el in this.schema.ValueSets)
            {
                AddValueSetNode(el);
            }
            foreach (var el in this.schema.Diagrams)
            {
                AddDiagramNode(el);
            }
            entitySetFolder.Expand();
            relationshipSetFolder.Expand();
            valueSetFolder.Expand();
            diagramFolder.Expand();

            parentTree.NodeMouseClick += Tree_NodeMouseClick;
        }

        private ErEntitySetNode AddEntitySetNode(ErEntitySet es)
        {
            var newNode = new ErEntitySetNode(es, parentTree);
            entitySetFolder.Nodes.Add(newNode);
            return newNode;
        }
        private ErRelationshipSetNode AddRelationshipSetNode(ErRelationshipSet rs)
        {
            var newNode = new ErRelationshipSetNode(rs, parentTree);
            relationshipSetFolder.Nodes.Add(newNode);
            return newNode;
        }
        private ErValueSetNode AddValueSetNode(ErValueSet vs)
        {
            var newNode = new ErValueSetNode(vs, parentTree);
            valueSetFolder.Nodes.Add(newNode);
            return newNode;
        }
        private ErDiagramNode AddDiagramNode(ErDiagram dgr)
        {
            var newNode = new ErDiagramNode(dgr, parentTree);
            diagramFolder.Nodes.Add(newNode);
            return newNode;
        }
        public void Tree_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            foreach (ErDiagramNode node in diagramFolder.Nodes)
            {
                if(e.Node == node)
                {
                    ConsoleLog.Log("Clicked diagram node. The diagram will be assigned to diagram panel.", node.Data.Name);
                    DialogManager.OpenDiagram(node.Data);
                }
            }
        }

        private void AddEntitySet(object? sender, EventArgs e)
        {
            ConsoleLog.Log("Adding new entity set in the navigator", this, "INFO");
            acceptNotifications = false;

            var newEl = schema.AddEntitySet();
            var newNode = AddEntitySetNode(newEl);

            entitySetFolder.Expand();
            parentTree.RenameNode(newNode);
            acceptNotifications = true;
        }
        private void AddRelationshipSet(object? sender, EventArgs e)
        {
            ConsoleLog.Log("Adding new relationship set in the navigator", this, "INFO");
            acceptNotifications = false;

            var newEl = schema.AddRelationshipSet();
            var newNode = AddRelationshipSetNode(newEl);

            relationshipSetFolder.Expand();
            parentTree.RenameNode(newNode);
            acceptNotifications = true;
        }
        private void AddValueSet(object? sender, EventArgs e)
        {
            ConsoleLog.Log("Adding new value set in the navigator", this, "INFO");
            var newEl = schema.AddValueSet();
            var newNode = AddValueSetNode(newEl);

            valueSetFolder.Expand();
            parentTree.RenameNode(newNode);
        }
        private void AddDiagram(object? sender, EventArgs e)
        {
            ConsoleLog.Log("Adding new diagram in the navigator", this, "INFO");
            var newEl = schema.AddDiagram();
            var newNode = AddDiagramNode(newEl);

            diagramFolder.Expand();
            parentTree.RenameNode(newNode);
        }

        public void Recieve(Notification notification)
        {
            if (acceptNotifications)
            {
                if (notification is ObjectCreatedNotification<ErEntitySet>)
                {
                    ConsoleLog.Log($"Schema node {this.Name} received notification that new entity set was added ({((ObjectCreatedNotification<ErEntitySet>)notification).Object})");
                    this.AddEntitySetNode(((ObjectCreatedNotification<ErEntitySet>)notification).Object);
                }
                else if (notification is ObjectCreatedNotification<ErRelationshipSet>)
                {
                    ConsoleLog.Log($"Schema node {this.Name} received notification that new relationship set was added ({((ObjectCreatedNotification<ErRelationshipSet>)notification).Object})");
                    this.AddRelationshipSetNode(((ObjectCreatedNotification<ErRelationshipSet>)notification).Object);
                }
            }
        }
    }

    public class ErEntitySetNode : ExtTreeNodeBase<ErEntitySet>, IObserver, IVisitor<ObjectNameChangedNotification>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErEntitySet entitySet;
        private NavigatorTreeView parentTree;

        private ExtTreeNodeTyped<object, ErAttributeNode> attributeFolder = new("Атрибуты");

        private Visitor visitorLogic;

        public ErEntitySetNode(ErEntitySet entitySet, NavigatorTreeView parentTree)
        {
            nodes = new(this.TreeNode.Nodes); // или this.TreeNodes
            this.entitySet = entitySet;
            base.Name = entitySet.Name;
            this.parentTree = parentTree;

            this.Initialize();

            visitorLogic = new(this);
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
            this.ImageIndex = 3;
            this.SelectedImageIndex = 3;
            UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });
            
            UIHelper.AddContextMenu(attributeFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddAttribute) } });
            this.nodes.Add(attributeFolder);
            attributeFolder.ImageIndex = 1;
            attributeFolder.SelectedImageIndex = 1;

            foreach(var attr in entitySet.attributes)
            {
                this.AddAttributeNode(attr);
            }
        }
        private ErAttributeNode AddAttributeNode(ErAttribute attribute)
        {
            var newNode = new ErAttributeNode(attribute, parentTree);
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

        public void Recieve(Notification notification)
        {
            notification.Accept(visitorLogic);
        }
        public void Visit(ObjectNameChangedNotification notif)
        {
            base.Name = notif.NewName;
        }
    }

    public class ErRelationshipSetNode : ExtTreeNodeBase<ErRelationshipSet>, IObserver, IVisitor<ObjectNameChangedNotification>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErRelationshipSet relationshipSet;
        private NavigatorTreeView parentTree;

        private ExtTreeNodeTyped<object, ErAttributeNode> attributeFolder = new("Атрибуты");
        private ExtTreeNodeTyped<object, ErRoleNode> roleFolder = new("Роли");
        private ExtTreeNodeTyped<object, ErMappingNode> mappingFolder = new("Отображения");

        private Visitor visitorLogic; // через визитеров МОЖНО обновляться по нотификациям. По сути ООП версия if else if ... по типам Notification

        public ErRelationshipSetNode(ErRelationshipSet relationshipSet, NavigatorTreeView parentTree)
        {
            nodes = new(this.TreeNodes);
            this.relationshipSet = relationshipSet;
            base.Name = relationshipSet.Name;
            this.parentTree = parentTree;

            this.Initialize();

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
            this.ImageIndex = 4;
            this.SelectedImageIndex = 4;
            UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });

            attributeFolder = new("Атрибуты");
            UIHelper.AddContextMenu(attributeFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddAttribute) } });
            this.nodes.Add(attributeFolder);
            attributeFolder.ImageIndex = 1;
            attributeFolder.SelectedImageIndex = 1;

            roleFolder = new("Роли");
            UIHelper.AddContextMenu(roleFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddRole) } });
            this.nodes.Add(roleFolder);
            roleFolder.ImageIndex = 1;
            roleFolder.SelectedImageIndex = 1;

            mappingFolder = new("Отображения");
            UIHelper.AddContextMenu(mappingFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddMapping) } });
            this.nodes.Add(mappingFolder);
            mappingFolder.ImageIndex = 1;
            mappingFolder.SelectedImageIndex = 1;
        }

        private ErAttributeNode CreateAttributeNode(ErAttribute attribute)
        {
            var newNode = new ErAttributeNode(attribute, parentTree);
            attributeFolder.Nodes.Add(newNode);
            return newNode;

        }
        private void AddAttribute(object? sender, EventArgs e)
        {
            var newAttribute = relationshipSet.AddAttribute();
            var newNode = CreateAttributeNode(newAttribute);

            attributeFolder.Expand();
            parentTree.RenameNode(newNode);
        }
        private void AddRole(object? sender, EventArgs e)
        {
            var newRole = relationshipSet.AddRole();
            var newNode = new ErRoleNode(newRole, parentTree);
            roleFolder.Nodes.Add(newNode);

            roleFolder.Expand();
            parentTree.RenameNode(newNode);
        }
        private void AddMapping(object? sender, EventArgs e)
        {
            var newMap = relationshipSet.AddMapping();
            var newNode = new ErMappingNode(newMap, parentTree);
            mappingFolder.Nodes.Add(newNode);

            mappingFolder.Expand();
            parentTree.RenameNode(newNode);
        }

        public void Recieve(Notification notification)
        {
            notification.Accept(visitorLogic);
        }
        public void Visit(ObjectNameChangedNotification notif)
        {
            base.Name = notif.NewName;
        }
    }

    public class ErValueSetNode : ExtTreeNodeBase<ErValueSet>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErValueSet valueSet;
        private NavigatorTreeView parentTree;

        public ErValueSetNode(ErValueSet valueSet, NavigatorTreeView parentTree)
        {
            nodes = new(this.TreeNodes);
            this.valueSet = valueSet;
            base.Name = valueSet.Name;
            this.parentTree = parentTree;

            this.Initialize();
        }

        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; valueSet.Name = value; }
        }
        public override ErValueSet Data
        {
            get { return valueSet; }
            set { valueSet = value; }
        }
        public override ExtTreeNodeCollection<IExtTreeNode> Nodes
        {
            get { return nodes; }
        }

        private void Initialize()
        {
            this.ImageIndex = 5;
            this.SelectedImageIndex = 5;
            UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });
        }
    }

    public class ErDiagramNode : ExtTreeNodeBase<ErDiagram>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErDiagram diagram;
        private NavigatorTreeView parentTree;

        public ErDiagramNode(ErDiagram diagram, NavigatorTreeView parentTree)
        {
            nodes = new(this.TreeNodes);
            this.diagram = diagram;
            base.Name = diagram.Name;
            this.parentTree = parentTree;

            this.Initialize();
        }

        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; diagram.Name = value; }
        }
        public override ErDiagram Data
        {
            get { return diagram; }
            set { diagram = value; }
        }
        public override ExtTreeNodeCollection<IExtTreeNode> Nodes
        {
            get { return nodes; }
        }

        private void Initialize()
        {
            this.ImageIndex = 0;
            this.SelectedImageIndex = 0;
            UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });
        }
    }

    public class ErAttributeNode : ExtTreeNodeBase<ErAttribute>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErAttribute attribute;
        private NavigatorTreeView parentTree;

        public ErAttributeNode(ErAttribute attribute, NavigatorTreeView parentTree)
        {
            nodes = new(this.TreeNodes);
            this.attribute = attribute;
            base.Name = attribute.Name;
            this.parentTree = parentTree;

            this.Initialize();
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
            this.ImageIndex = 6;
            this.SelectedImageIndex = 6;
            UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });
        }
    }
    public class ErRoleNode : ExtTreeNodeBase<ErRole>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErRole role;
        private NavigatorTreeView parentTree;

        public ErRoleNode(ErRole role, NavigatorTreeView parentTree)
        {
            nodes = new(this.TreeNodes);
            this.role = role;
            base.Name = role.Name;
            this.parentTree = parentTree;

            this.Initialize();
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
            this.ImageIndex = 7;
            this.SelectedImageIndex = 7;
            UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });
        }
    }
    public class ErMappingNode : ExtTreeNodeBase<ErMapping>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErMapping mapping;
        private NavigatorTreeView parentTree;

        public ErMappingNode(ErMapping mapping, NavigatorTreeView parentTree)
        {
            nodes = new(this.TreeNodes);
            this.mapping = mapping;
            base.Name = mapping.Name;
            this.parentTree = parentTree;

            this.Initialize();
        }

        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; mapping.Name = value; }
        }
        public override ErMapping Data
        {
            get { return mapping; }
            set { mapping = value; }
        }
        public override ExtTreeNodeCollection<IExtTreeNode> Nodes
        {
            get { return nodes; }
        }

        private void Initialize()
        {
            this.ImageIndex = 9;
            this.SelectedImageIndex = 9;
            UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) } });
        }
    }
}
