using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using ErEditor.UI.ExtTreeClasses;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI.NavigatorTreeClasses
{
    public partial class NavigatorTreeView : ExtTreeViewBase
    {
        // По сути поддерживаю ограничение внешнего ключа схема -> элемент в навигаторе.
        // Логично потому что это пока единственное место, где может быть открыто несколько схем (не считая инфраструктурных
        // классов которые работают только со схемой, и не работают с ее элементами, поэтому им это ограничение безразлично)
        public abstract class NavigatorErNode<TData> : ExtTreeNodeWithNotNullableData<TData>
        {
            protected readonly ErSchema ParentSchema;

            public NavigatorErNode(ErSchema parentSchema)
            {
                ParentSchema = parentSchema;
            }

            protected static void DeleteChildNode<TEntity, TNode>(TEntity entity, ExtTreeNodeTyped<object, TNode> folder)
                where TNode : ExtTreeNodeWithNotNullableData<TEntity>
            {
                ConsoleLog.Log($"{folder.Nodes.Count}");
                TNode? delNode = default;
                foreach (var node in folder.Nodes)
                {
                    if (node.Data.Equals(entity))
                    {
                        delNode = node;
                        break;
                    }
                }
                if (delNode != null)
                {
                    folder.Nodes.Remove(delNode);
                }
            }
            public override void Click(object? sender, MouseEventArgs e)
            {
                MainWindow.OpenProperties(ParentSchema, Data);
            }
        }

        private ExtTreeNodeCollection<ErSchemaNode> nodes;

        public NavigatorTreeView()
        {
            nodes = new(TreeNodes);
            Initialize();
        }

        public override ExtTreeNodeCollection<ErSchemaNode> Nodes
        {
            get { return nodes; }
        }
        public List<ErSchema> Schemas
        {
            get
            {
                List<ErSchema> schemas = new();
                foreach (var node in nodes)
                {
                    schemas.Add(node.Data);
                }
                return schemas;
            }
        }

        public void Initialize()
        {
            var imageList = new ImageList();

            imageList.AddIcon(IconChar.WindowMaximize);         // 0
            imageList.AddIcon(IconChar.Folder, Color.Orange);   // 1
            imageList.AddIcon(IconChar.CodeCompare);            // 2
            imageList.AddIcon(IconChar.E);                      // 3
            imageList.AddIcon(IconChar.R);                      // 4
            imageList.AddIcon(IconChar.V);                      // 5
            imageList.AddIcon(IconChar.Wrench);                 // 6
            imageList.AddIcon(IconChar.UserPlus);               // 7
            imageList.AddIcon(IconChar.ArrowLeft);              // 8
            imageList.AddIcon(IconChar.Exchange);               // 9

            ImageList = imageList;

            NodeMouseClick += NavigatorTreeView_NodeMouseClick;
            NodeMouseDoubleClick += NavigatorTreeView_NodeMouseDoubleClick;
        }

        // this requires all nodes to have access to the mediator.
        // alternatively, received node could be casted to specific types and actions can be performed depending on the type.
        private void NavigatorTreeView_NodeMouseClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            var args = new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta);
            Nodes[e.Node]?.Click(sender, e);
        }
        private void NavigatorTreeView_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            var args = new MouseEventArgs(e.Button, e.Clicks, e.X, e.Y, e.Delta);
            Nodes[e.Node]?.DoubleClick(sender, e);
        }

        public void OpenSchema(ErSchema schema)
        {
            ErSchemaNode node = new(schema, this);
            nodes.Add(node);

            node.TreeNode.Expand();
        }
    }
}
