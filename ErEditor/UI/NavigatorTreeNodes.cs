using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ErEditor.UI
{
    // По сути поддерживаю ограничение внешнего ключа схема -> элемент в навигаторе.
    // Логично потому что это пока единственное место, где может быть открыто несколько схем (не считая инфраструктурных
    // классов которые работают только со схемой, и не работают с ее элементами, поэтому им это ограничение безразлично)
    public abstract class NavigatorErNode<TData> : ExtTreeNodeBase<TData>
    {
        protected readonly ErSchema ParentSchema;

        public NavigatorErNode(ErSchema parentSchema){
            ParentSchema = parentSchema;
        }

        public override void Click(object? sender, MouseEventArgs e)
        {
            MainWindow.OpenProperties(ParentSchema, Data);
        }
    }

    // По сути этот же нод ответственен и за своих четырех детей. Нет смысла плодить дополнительные классы если они неразрывно связаны со схемой и ее коллекциями.
    public class ErSchemaNode : 
        NavigatorErNode<ErSchema>, IObserver,
        IVisitor<ObjectDeletedNotification<ErEntitySet>>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes; // если такая коллекция значит в нодах храним разные объекты
        private ErSchema schema;
        private NavigatorTreeView parentTree; // Через мой ПОТРЯСАЮЩИЙ статик-нестатик синглтон-несинглтон медиатор можно не хранить это

        private ExtTreeNodeTyped<object, ErEntitySetNode> entitySetFolder = new("Множества сущностей");
        private ExtTreeNodeTyped<object, ErRelationshipSetNode> relationshipSetFolder = new("Множества связей");
        private ExtTreeNodeTyped<object, ErValueSetNode> valueSetFolder = new("Множества значений");
        private ExtTreeNodeTyped<object, ErDiagramNode> diagramFolder = new("ER-диаграммы");

        private bool acceptNotifications = true;
        private ObserverBase observerLogic;
        public ErSchemaNode(ErSchema schema, NavigatorTreeView parentTree) : base(schema)
        {
            nodes = new(this.TreeNodes);
            this.schema = schema;
            base.Name = schema.Name;
            this.parentTree = parentTree;

            this.Initialize();

            observerLogic = new(this);

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
        }

        private ErEntitySetNode AddEntitySetNode(ErEntitySet es)
        {
            var newNode = new ErEntitySetNode(ParentSchema, es, parentTree);
            entitySetFolder.Nodes.Add(newNode);
            return newNode;
        }
        private ErRelationshipSetNode AddRelationshipSetNode(ErRelationshipSet rs)
        {
            var newNode = new ErRelationshipSetNode(ParentSchema, rs, parentTree);
            relationshipSetFolder.Nodes.Add(newNode);
            return newNode;
        }
        private ErValueSetNode AddValueSetNode(ErValueSet vs)
        {
            var newNode = new ErValueSetNode(ParentSchema, vs, parentTree);
            valueSetFolder.Nodes.Add(newNode);
            return newNode;
        }
        private ErDiagramNode AddDiagramNode(ErDiagram dgr)
        {
            var newNode = new ErDiagramNode(ParentSchema, dgr, parentTree);
            diagramFolder.Nodes.Add(newNode);
            return newNode;
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
            observerLogic.Recieve(notification);
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

        public void Visit(ObjectDeletedNotification<ErEntitySet> notif)
        {
            ErEntitySetNode? delNode = null;
            foreach(ErEntitySetNode node in entitySetFolder.Nodes)
            {
                if(node.Data == notif.Object)
                {
                    delNode = node;
                    break;
                }
            }
            if(delNode != null)
            {
                entitySetFolder.Nodes.Remove(delNode);
            }
        }
    }

    public class ErEntitySetNode : NavigatorErNode<ErEntitySet>, IObserver, IVisitor<ObjectNameChangedNotification>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErEntitySet entitySet;
        private NavigatorTreeView parentTree;

        private ExtTreeNodeTyped<object, ErAttributeNode> attributeFolder = new("Атрибуты");

        private Visitor visitorLogic;

        public ErEntitySetNode(ErSchema schema, ErEntitySet entitySet, NavigatorTreeView parentTree) : base(schema)
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
            UIHelper.AddContextMenu(
                this,
                new Dictionary<string, EventHandler>() {
                    { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) },
                    { "Удалить", new EventHandler(this.DeleteEntitySet) }
                });

            UIHelper.AddContextMenu(attributeFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddAttribute) } });
            this.nodes.Add(attributeFolder);
            attributeFolder.ImageIndex = 1;
            attributeFolder.SelectedImageIndex = 1;

            foreach(var attr in entitySet.Attributes)
            {
                this.AddAttributeNode(attr);
            }
        }

        private void DeleteEntitySet(object? sender, EventArgs e)
        {
            (parentTree.Nodes[this.Parent.Parent] as ErSchemaNode)?.Data.RemoveEntitySet(this.entitySet);
        }
        private ErAttributeNode AddAttributeNode(ErAttribute attribute)
        {
            var newNode = new ErAttributeNode(ParentSchema, attribute, parentTree);
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

    public class ErRelationshipSetNode : NavigatorErNode<ErRelationshipSet>, IObserver, IVisitor<ObjectNameChangedNotification>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErRelationshipSet relationshipSet;
        private NavigatorTreeView parentTree;

        private ExtTreeNodeTyped<object, ErAttributeNode> attributeFolder = new("Атрибуты");
        private ExtTreeNodeTyped<object, ErRoleNode> roleFolder = new("Роли");
        private ExtTreeNodeTyped<object, ErMappingNode> mappingFolder = new("Отображения");

        private Visitor visitorLogic;

        public ErRelationshipSetNode(ErSchema schema, ErRelationshipSet relationshipSet, NavigatorTreeView parentTree) : base(schema)
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

            UIHelper.AddContextMenu(attributeFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddAttribute) } });
            this.nodes.Add(attributeFolder);
            attributeFolder.ImageIndex = 1;
            attributeFolder.SelectedImageIndex = 1;

            UIHelper.AddContextMenu(roleFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddRole) } });
            this.nodes.Add(roleFolder);
            roleFolder.ImageIndex = 1;
            roleFolder.SelectedImageIndex = 1;

            UIHelper.AddContextMenu(mappingFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(this.AddMapping) } });
            this.nodes.Add(mappingFolder);
            mappingFolder.ImageIndex = 1;
            mappingFolder.SelectedImageIndex = 1;

            foreach (var attr in relationshipSet.Attributes)
            {
                this.AddAttributeNode(attr);
            }
            foreach (var role in relationshipSet.Roles)
            {
                this.AddRoleNode(role);
            }
            foreach (var mapping in relationshipSet.Mappings)
            {
                //this.AddMappingNode(mapping);
            }

            parentTree.NodeMouseClick += Tree_NodeMouseClick;
        }

        private void Tree_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            ConsoleLog.Log("Relationship Set Node nodeclick handler was triggered");
            foreach (ErRoleNode node in roleFolder.Nodes)
            {
                if (e.Node == node)
                {
                    ConsoleLog.Log("Found corresponding Role Node that was clicked.");
                    var schema = parentTree.GetNodeData<ErSchema>(this.Parent.Parent);
                    if(schema != null)
                    {
                        MainWindow.OpenProperties(schema, node.Data);
                    }
                }
            }
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
            var newNode = new ErRoleNode(ParentSchema, role, parentTree);
            roleFolder.Nodes.Add(newNode);

            return newNode;
        }
        private void AddRole(object? sender, EventArgs e)
        {
            var newRole = relationshipSet.AddRole();
            var newNode = AddRoleNode(newRole);

            roleFolder.Expand();
            parentTree.RenameNode(newNode);
        }
        private void AddMapping(object? sender, EventArgs e)
        {
            var newMap = relationshipSet.AddMapping();
            var newNode = new ErMappingNode(ParentSchema, newMap, parentTree);
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

    public class ErValueSetNode : NavigatorErNode<ErValueSet>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErValueSet valueSet;
        private NavigatorTreeView parentTree;

        public ErValueSetNode(ErSchema schema, ErValueSet valueSet, NavigatorTreeView parentTree) : base(schema)
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

    public class ErDiagramNode : NavigatorErNode<ErDiagram>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErDiagram diagram;
        private NavigatorTreeView parentTree;

        public ErDiagramNode(ErSchema schema, ErDiagram diagram, NavigatorTreeView parentTree) : base(schema)
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

        public override void DoubleClick(object? sender, MouseEventArgs e)
        {
            MainWindow.OpenDiagram(diagram);
        }
    }

    public class ErAttributeNode : NavigatorErNode<ErAttribute>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErAttribute attribute;
        private NavigatorTreeView parentTree;

        public ErAttributeNode(ErSchema schema, ErAttribute attribute, NavigatorTreeView parentTree) : base(schema)
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
    public class ErRoleNode : NavigatorErNode<ErRole>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErRole role;
        private NavigatorTreeView parentTree;

        public ErRoleNode(ErSchema schema, ErRole role, NavigatorTreeView parentTree) : base(schema)
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
    public class ErMappingNode : NavigatorErNode<ErMapping>
    {
        private ExtTreeNodeCollection<IExtTreeNode> nodes;
        private ErMapping mapping;
        private NavigatorTreeView parentTree;

        public ErMappingNode(ErSchema schema, ErMapping mapping, NavigatorTreeView parentTree) : base(schema)
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
