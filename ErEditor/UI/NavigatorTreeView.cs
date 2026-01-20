using ErEditor.DbSchemaClasses;
using ErEditor.ErSchemaClasses;
using ErEditor.Infrastructure;
using FontAwesome.Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ErEditor.UI
{
    public class NavigatorTreeView : ExtTreeViewBase
    {
        private ExtTreeNodeCollection<ErSchemaNode> nodes;

        public NavigatorTreeView()
        {
            nodes = new(this.TreeNodes);
            this.Initialize();
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
        }

        public void OpenSchema(ErSchema schema)
        {
            ErSchemaNode node = new(schema, this);
            nodes.Add(node);

            node.TreeNode.Expand();
        }
    }
}
