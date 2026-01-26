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
        public class ErDiagramNode : NavigatorErNode<ErDiagram>
        {
            private ExtTreeNodeCollection<IExtTreeNode> nodes;
            private ErDiagram diagram;
            private NavigatorTreeView parentTree;

            public ErDiagramNode(ErSchema schema, ErDiagram diagram, NavigatorTreeView parentTree) : base(schema)
            {
                nodes = new(TreeNodes);
                this.diagram = diagram;
                base.Name = diagram.Name;
                this.parentTree = parentTree;

                Initialize();
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
                ImageIndex = 0;
                SelectedImageIndex = 0;
                UIHelper.AddContextMenu(
                    this,
                    new Dictionary<string, EventHandler>() {
                        { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) },
                        { "Удалить", new EventHandler(DeleteDiagram) }
                    });
            }

            private void DeleteDiagram(object? sender, EventArgs e)
            {
                ParentSchema.Diagrams.Remove(diagram);
            }
            public override void DoubleClick(object? sender, MouseEventArgs e)
            {
                MainWindow.OpenDiagram(ParentSchema, diagram);
            }
        }
    }
}
