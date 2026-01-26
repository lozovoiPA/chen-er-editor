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
        public class ErValueSetNode : NavigatorErNode<ErValueSet>
        {
            private ExtTreeNodeCollection<IExtTreeNode> nodes;
            private ErValueSet valueSet;
            private NavigatorTreeView parentTree;

            public ErValueSetNode(ErSchema schema, ErValueSet valueSet, NavigatorTreeView parentTree) : base(schema)
            {
                nodes = new(TreeNodes);
                this.valueSet = valueSet;
                base.Name = valueSet.Name;
                this.parentTree = parentTree;

                Initialize();
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
                ImageIndex = 5;
                SelectedImageIndex = 5;
                UIHelper.AddContextMenu(
                    this, 
                    new Dictionary<string, EventHandler>() { 
                        { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) },
                        { "Удалить", new EventHandler(DeleteValueSet) }
                    });
            }

            private void DeleteValueSet(object? sender, EventArgs e)
            {
                ParentSchema.ValueSets.Remove(valueSet);
            }
        }
    }
}
