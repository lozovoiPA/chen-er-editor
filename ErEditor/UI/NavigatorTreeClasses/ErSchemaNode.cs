using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using ErEditor.UI.ExtTreeClasses;


namespace ErEditor.UI.NavigatorTreeClasses
{
    partial class NavigatorTreeView
    {
        // По сути этот же нод ответственен и за своих четырех детей. Нет смысла плодить дополнительные классы если они неразрывно связаны со схемой и ее коллекциями.
        public class ErSchemaNode :
            NavigatorErNode<ErSchema>, 
            IObserver,
            IVisitor<ObjectDeletedNotification<ErEntitySet>>,
            IVisitor<ObjectDeletedNotification<ErRelationshipSet>>,
            IVisitor<ObjectDeletedNotification<ErValueSet>>,
            IVisitor<ObjectCreatedNotification<ErDiagram>>,
            IVisitor<ObjectDeletedNotification<ErDiagram>>
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
                nodes = new(TreeNodes);
                this.schema = schema;
                base.Name = schema.Name;
                this.parentTree = parentTree;

                Initialize();

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
                ImageIndex = 0;
                SelectedImageIndex = 0;
                UIHelper.AddContextMenu(this, new Dictionary<string, EventHandler>() { 
                    { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) },
                    { "Транслировать", new EventHandler(TranslateSchema_Handler) }
                });

                UIHelper.AddContextMenu(entitySetFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(AddEntitySet) } });
                UIHelper.AddContextMenu(relationshipSetFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(AddRelationshipSet) } });
                UIHelper.AddContextMenu(valueSetFolder, new Dictionary<string, EventHandler>() { { "Создать", new EventHandler(AddValueSet) } });
                UIHelper.AddContextMenu(diagramFolder, new Dictionary<string, EventHandler>() { 
                    { "Создать", new EventHandler(AddDiagram) },
                    { "Сгенерировать", new EventHandler(GenerateDiagram_Handler) }
                });

                nodes.Add(entitySetFolder);
                nodes.Add(relationshipSetFolder);
                nodes.Add(valueSetFolder);
                nodes.Add(diagramFolder);

                entitySetFolder.ImageIndex = 1;
                entitySetFolder.SelectedImageIndex = 1;
                relationshipSetFolder.ImageIndex = 1;
                relationshipSetFolder.SelectedImageIndex = 1;
                valueSetFolder.ImageIndex = 1;
                valueSetFolder.SelectedImageIndex = 1;
                diagramFolder.ImageIndex = 0;
                diagramFolder.SelectedImageIndex = 0;

                foreach (var el in schema.EntitySets)
                {
                    AddEntitySetNode(el);
                }
                foreach (var el in schema.RelationshipSets)
                {
                    AddRelationshipSetNode(el);
                }
                foreach (var el in schema.ValueSets)
                {
                    AddValueSetNode(el);
                }
                foreach (var el in schema.Diagrams)
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
                var newNode = new ErEntitySetNode(parentSchema, es, parentTree);
                entitySetFolder.Nodes.Add(newNode);
                return newNode;
            }
            private ErRelationshipSetNode AddRelationshipSetNode(ErRelationshipSet rs)
            {
                var newNode = new ErRelationshipSetNode(parentSchema, rs, parentTree);
                relationshipSetFolder.Nodes.Add(newNode);
                return newNode;
            }
            private ErValueSetNode AddValueSetNode(ErValueSet vs)
            {
                var newNode = new ErValueSetNode(parentSchema, vs, parentTree);
                valueSetFolder.Nodes.Add(newNode);
                return newNode;
            }
            private ErDiagramNode AddDiagramNode(ErDiagram dgr)
            {
                var newNode = new ErDiagramNode(parentSchema, dgr, parentTree);
                newNode.DisplayName = dgr.Name;
                diagramFolder.Nodes.Add(newNode);
                return newNode;
            }

            private void GenerateDiagram_Handler(object? sender, EventArgs e)
            {
                DialogManager.GenerateDiagram(schema);
            }
            private void TranslateSchema_Handler(object? sender, EventArgs e)
            {
                DialogManager.TranslateSchema(schema);
            }
            private void AddEntitySet(object? sender, EventArgs e)
            {
                ConsoleLog.Log("Adding new entity set in the navigator", this);
                acceptNotifications = false;

                var newEl = schema.EntitySets.Add();
                var newNode = AddEntitySetNode(newEl);

                entitySetFolder.Expand();
                parentTree.RenameNode(newNode);
                acceptNotifications = true;
            }
            private void AddRelationshipSet(object? sender, EventArgs e)
            {
                ConsoleLog.Log("Adding new relationship set in the navigator", this);
                acceptNotifications = false;

                var newEl = schema.RelationshipSets.Add();
                var newNode = AddRelationshipSetNode(newEl);

                relationshipSetFolder.Expand();
                parentTree.RenameNode(newNode);
                acceptNotifications = true;
            }
            private void AddValueSet(object? sender, EventArgs e)
            {
                ConsoleLog.Log("Adding new value set in the navigator", this);
                var newEl = schema.ValueSets.Add();
                var newNode = AddValueSetNode(newEl);

                valueSetFolder.Expand();
                parentTree.RenameNode(newNode);
            }
            private void AddDiagram(object? sender, EventArgs e)
            {
                acceptNotifications = false;
                ConsoleLog.Log("Adding new diagram in the navigator", this);
                var newEl = schema.Diagrams.Add();
                var newNode = AddDiagramNode(newEl);

                diagramFolder.Expand();
                parentTree.RenameNode(newNode);
                acceptNotifications = true;
            }

            public override void Recieve(Notification notification)
            {
                
                if (acceptNotifications)
                {
                    observerLogic.Recieve(notification);
                    if (notification is ObjectCreatedNotification<ErEntitySet>)
                    {
                        ConsoleLog.Log($"Schema node {Name} received notification that new entity set was added ({((ObjectCreatedNotification<ErEntitySet>)notification).Object})");
                        AddEntitySetNode(((ObjectCreatedNotification<ErEntitySet>)notification).Object);
                    }
                    else if (notification is ObjectCreatedNotification<ErRelationshipSet>)
                    {
                        ConsoleLog.Log($"Schema node {Name} received notification that new relationship set was added ({((ObjectCreatedNotification<ErRelationshipSet>)notification).Object})");
                        AddRelationshipSetNode(((ObjectCreatedNotification<ErRelationshipSet>)notification).Object);
                    }
                }
            }

            public void Visit(ObjectDeletedNotification<ErEntitySet> notif)
            {
                DeleteChildNode(notif.Object, entitySetFolder);
            }
            public void Visit(ObjectDeletedNotification<ErRelationshipSet> notif)
            {
                DeleteChildNode(notif.Object, relationshipSetFolder);
            }
            public void Visit(ObjectDeletedNotification<ErValueSet> notif)
            {
                DeleteChildNode(notif.Object, valueSetFolder);
            }
            public void Visit(ObjectCreatedNotification<ErDiagram> notif)
            {
                var node = AddDiagramNode(notif.Object);
                diagramFolder.Expand();
                parentTree.RenameNode(node);
            }
            public void Visit(ObjectDeletedNotification<ErDiagram> notif)
            {
                DeleteChildNode(notif.Object, diagramFolder);
            }
        }
    }

    

    

    

    

    
    
    
}
