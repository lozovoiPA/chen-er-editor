using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ErEditor.Infrastructure;
using Microsoft.Msagl;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Miscellaneous;

namespace ErEditor.ErSchemaClasses
{
    public class MsaglTests
    {
        public MsaglTests()
        {
            Initialize();
        }

        public GeometryGraph Initialize()
        {
            GeometryGraph graph = new GeometryGraph();
            Node a = new Node();
            Node b = new Node();
            Node c = new Node();
            Node d = new Node();

            graph.Nodes.Add(a);
            graph.Nodes.Add(b);
            graph.Nodes.Add(c);
            graph.Nodes.Add(d);

            graph.Edges.Add(new Edge(a, b));
            graph.Edges.Add(new Edge(a, c));
            graph.Edges.Add(new Edge(a, d));
            graph.Edges.Add(new Edge(d, c));

            var settings = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings();
            settings.EdgeRoutingSettings.EdgeRoutingMode = EdgeRoutingMode.StraightLine;
            LayoutHelpers.CalculateLayout(graph, settings, null);

            return graph;
        }
    }
}
