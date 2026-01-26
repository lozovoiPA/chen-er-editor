using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using ErEditor.UI.ExtTreeClasses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace ErEditor.UI.NavigatorTreeClasses
{
    partial class NavigatorTreeView
    {
        public class ErMappingNode 
            : NavigatorErNode<ErMapping>,
            IObserver,
            IVisitor<ObjectNameChangedNotification>,
            IVisitor<ObjectUpdatedNotification<ErMapping>>
        {
            private ExtTreeNodeCollection<IExtTreeNode> nodes;
            private ErMapping mapping;
            private NavigatorTreeView parentTree;

            private ObserverBase notificationParser;

            public ErMappingNode(ErSchema schema, ErMapping mapping, NavigatorTreeView parentTree) : base(schema)
            {
                nodes = new(TreeNodes);
                this.mapping = mapping;
                base.Name = mapping.Name;
                this.parentTree = parentTree;

                notificationParser = new(this);
                mapping.Subscribe(this);
                Initialize();
            }

            public override string Name
            {
                get { return base.Name; }
                set { base.Name = value; mapping.Name = value; }
            }
            public string DisplayName
            {
                get { return base.Name; }
                set { base.Name = value; }
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
                ImageIndex = 9;
                SelectedImageIndex = 9;
                UIHelper.AddContextMenu(
                    this,
                    new Dictionary<string, EventHandler>() {
                        { "Переименовать", new EventHandler(parentTree.RenameSelectedNode) }
                    });
            }

            public void Recieve(Notification notification)
            {
                //notificationParser.Recieve(notification);
                if(notification is ObjectNameChangedNotification || notification is ObjectUpdatedNotification<ErMapping>)
                {
                    this.DisplayName = (mapping.Name == "") ? mapping.DefaultName : mapping.Name;
                }
            }
            public void Visit(ObjectNameChangedNotification notification)
            {
                
            }
            public void Visit(ObjectUpdatedNotification<ErMapping> notification)
            {

            }
        }
    }
}
