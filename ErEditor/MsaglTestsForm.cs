using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Core.Layout;
using Microsoft.Msagl.Core.Routing;
using Microsoft.Msagl.Miscellaneous;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using P = Microsoft.Msagl.Core.Geometry.Point;

namespace ErEditor
{
    public partial class MsaglTestsForm : Form
    {
        GeometryGraph gleeGraph;
        public MsaglTestsForm()
        {
            InitializeComponent();
            this.SizeChanged += new EventHandler(Form1_SizeChanged);
        }

        void Form1_SizeChanged(object sender, EventArgs e)
        {
            this.Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {

            base.OnPaint(e);
            if (gleeGraph == null)
                gleeGraph = CreateAndLayoutGraph();



            DrawFromGraph(e.Graphics);
        }

        private void DrawFromGraph(Graphics graphics)
        {
            SetGraphicsTransform(graphics);
            Pen pen = new Pen(Brushes.Black);
            DrawNodes(pen, graphics);
            DrawEdges(pen, graphics);
        }

        private void SetGraphicsTransform(Graphics graphics)
        {
            RectangleF r = this.ClientRectangle;
            var gr = this.gleeGraph.BoundingBox;
            if (r.Height > 1 && r.Width > 1)
            {
                float scale = Math.Min(r.Width / (float)gr.Width, r.Height / (float)gr.Height);
                float g0 = (float)(gr.Left + gr.Right) / 2;
                float g1 = (float)(gr.Top + gr.Bottom) / 2;

                float c0 = (r.Left + r.Right) / 2;
                float c1 = (r.Top + r.Bottom) / 2;
                float dx = c0 - scale * g0;
                float dy = c1 + scale * g1;
                graphics.Transform = new System.Drawing.Drawing2D.Matrix(scale, 0, 0, -scale, dx, dy);
            }
        }

        private void DrawEdges(Pen pen, Graphics graphics)
        {
            foreach (Edge e in gleeGraph.Edges)
                DrawEdge(e, pen, graphics);
        }

        private void DrawEdge(Edge e, Pen pen, Graphics graphics)
        {
            ICurve curve = e.Curve;
            Curve c = curve as Curve;
            if (c != null)
            {
                foreach (ICurve s in c.Segments)
                {
                    LineSegment l = s as LineSegment;
                    if (l != null)
                        graphics.DrawLine(pen, MsaglPointToDrawingPoint(l.Start), MsaglPointToDrawingPoint(l.End));
                    CubicBezierSegment cs = s as CubicBezierSegment;
                    if (cs != null)
                        graphics.DrawBezier(pen, MsaglPointToDrawingPoint(cs.B(0)), MsaglPointToDrawingPoint(cs.B(1)), MsaglPointToDrawingPoint(cs.B(2)), MsaglPointToDrawingPoint(cs.B(3)));

                }
                if (e.ArrowheadAtSource)
                    DrawArrow(e, pen, graphics, e.Curve.Start, e.EdgeGeometry.SourceArrowhead.TipPosition);
                if (e.ArrowheadAtTarget)
                    DrawArrow(e, pen, graphics, e.Curve.End, e.EdgeGeometry.TargetArrowhead.TipPosition);
            }
            else
            {
                var l = curve as LineSegment;
                if (l != null)
                    graphics.DrawLine(pen, MsaglPointToDrawingPoint(l.Start), MsaglPointToDrawingPoint(l.End));
            }
        }

        private void DrawArrow(Edge e, Pen pen, Graphics graphics, P start, P end)
        {
            PointF[] points;
            float arrowAngle = 30;

            P dir = end - start;
            P h = dir;
            dir /= dir.Length;

            P s = new P(-dir.Y, dir.X);

            s *= h.Length * ((float)Math.Tan(arrowAngle * 0.5f * (Math.PI / 180.0)));

            points = new PointF[] { MsaglPointToDrawingPoint(start + s), MsaglPointToDrawingPoint(end), MsaglPointToDrawingPoint(start - s) };

            graphics.FillPolygon(pen.Brush, points);
        }


        private void DrawNodes(Pen pen, Graphics graphics)
        {
            foreach (Node n in gleeGraph.Nodes)
                DrawNode(n, pen, graphics);
        }

        private void DrawNode(Node n, Pen pen, Graphics graphics)
        {
            ICurve curve = n.BoundaryCurve;
            Ellipse el = curve as Ellipse;
            if (el != null)
            {
                graphics.DrawEllipse(pen, new RectangleF((float)el.BoundingBox.Left, (float)el.BoundingBox.Bottom,
                    (float)el.BoundingBox.Width, (float)el.BoundingBox.Height));
            }
            else
            {
                Curve c = curve as Curve;
                foreach (ICurve seg in c.Segments)
                {
                    LineSegment l = seg as LineSegment;
                    if (l != null)
                        graphics.DrawLine(pen, MsaglPointToDrawingPoint(l.Start), MsaglPointToDrawingPoint(l.End));
                }
            }
        }

        private System.Drawing.Point MsaglPointToDrawingPoint(P point)
        {
            return new System.Drawing.Point((int)point.X, (int)point.Y);
        }

        static internal GeometryGraph CreateAndLayoutGraph()
        {
            if (MainWindow.MsaglGraph != null) return MainWindow.MsaglGraph;
            double w = 30;
            double h = 20;
            GeometryGraph graph = new GeometryGraph();
            Node hospitalDoctor = new Node(CurveFactory.CreateDiamond(w, h, new P()));
            Node hospitalLab = new Node(CurveFactory.CreateDiamond(w, h, new P()));
            Node hospitalRoom = new Node(CurveFactory.CreateDiamond(w, h, new P()));
            Node roomPersonnel = new Node(CurveFactory.CreateDiamond(w, h, new P()));
            Node roomPatient = new Node(CurveFactory.CreateDiamond(w, h, new P()));
            Node doctorPatient = new Node(CurveFactory.CreateDiamond(w, h, new P()));
            Node testLab = new Node(CurveFactory.CreateDiamond(w, h, new P()));
            Node testPatient = new Node(CurveFactory.CreateDiamond(w, h, new P()));
            Node patientDiagnosis = new Node(CurveFactory.CreateDiamond(w, h, new P()));

            Node hospital = new Node(CurveFactory.CreateRectangle(w, h, new P()));
            Node doctor = new Node(CurveFactory.CreateRectangle(w, h, new P()));
            Node lab = new Node(CurveFactory.CreateRectangle(w, h, new P()));
            Node room = new Node(CurveFactory.CreateRectangle(w, h, new P()));
            Node personnel = new Node(CurveFactory.CreateRectangle(w, h, new P()));
            Node patient = new Node(CurveFactory.CreateRectangle(w, h, new P()));
            Node test = new Node(CurveFactory.CreateRectangle(w, h, new P()));
            Node diagnosis = new Node(CurveFactory.CreateRectangle(w, h, new P()));

            graph.Nodes.Add(hospitalDoctor);
            graph.Nodes.Add(hospitalLab);
            graph.Nodes.Add(hospitalRoom);
            graph.Nodes.Add(roomPersonnel);
            graph.Nodes.Add(roomPatient);
            graph.Nodes.Add(doctorPatient);
            graph.Nodes.Add(testLab);
            graph.Nodes.Add(testPatient);
            graph.Nodes.Add(patientDiagnosis);

            graph.Nodes.Add(hospital);
            graph.Nodes.Add(doctor);
            graph.Nodes.Add(lab);
            graph.Nodes.Add(room);
            graph.Nodes.Add(personnel);
            graph.Nodes.Add(patient);
            graph.Nodes.Add(test);
            graph.Nodes.Add(diagnosis);

            graph.Edges.Add(new Edge(hospital, hospitalDoctor) { Length = 3 });
            graph.Edges.Add(new Edge(hospital, hospitalRoom) { Length = 3 });
            graph.Edges.Add(new Edge(hospital, hospitalLab) { Length = 3 });

            graph.Edges.Add(new Edge(hospitalDoctor, doctor) { Length = 3 });
            graph.Edges.Add(new Edge(doctor, doctorPatient) { Length = 3 });

            graph.Edges.Add(new Edge(hospitalRoom, room) { Length = 3 });
            graph.Edges.Add(new Edge(room, roomPatient) { Length = 3 });
            graph.Edges.Add(new Edge(room, roomPersonnel) { Length = 3 });

            graph.Edges.Add(new Edge(roomPersonnel, personnel) { Length = 3 });

            graph.Edges.Add(new Edge(hospitalLab, lab) { Length = 3 });
            graph.Edges.Add(new Edge(lab, testLab) { Length = 3 });

            graph.Edges.Add(new Edge(testLab, test) { Length = 3 });
            graph.Edges.Add(new Edge(testPatient, test) { Length = 3 });

            graph.Edges.Add(new Edge(patient, testPatient) { Length = 3 });
            graph.Edges.Add(new Edge(doctorPatient, patient) { Length = 3 });
            graph.Edges.Add(new Edge(roomPatient, patient) { Length = 3 });
            graph.Edges.Add(new Edge(patient, patientDiagnosis) { Length = 3 });

            graph.Edges.Add(new Edge(patientDiagnosis, diagnosis) { Length = 3 });

            //var settings1 = new Microsoft.Msagl.Layout.MDS.MdsLayoutSettings();
            var settings2 = new Microsoft.Msagl.Layout.Incremental.FastIncrementalLayoutSettings();
            settings2.EdgeRoutingSettings.EdgeRoutingMode = EdgeRoutingMode.StraightLine;

            //LayoutHelpers.CalculateLayout(graph, settings1, null);
            LayoutHelpers.CalculateLayout(graph, settings2, null);

            return graph;
        }
    }
}
