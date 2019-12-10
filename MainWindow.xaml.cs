using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using System.Windows.Threading;


namespace _2dStructuralFEM_GUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        Problem p;
        PlotModel plotModel;
        PlotController customController;
        List<string> BC_list_type = new List<string>();
        List<BoundaryCondition> BC_list=new List<BoundaryCondition>();
        List<Node> BC_list_nodes = new List<Node>();
        double l; // reference length which is max{grid length, grid height}

    public MainWindow() {
            InitializeComponent();

            this.plot.Model = new PlotModel(){ PlotType = PlotType.Cartesian }; ;
            this.plotModel = this.plot.Model;

            this.customController = new PlotController();
            this.plot.Controller = this.customController;
            customController.UnbindMouseDown(OxyMouseButton.Left);
            customController.BindMouseEnter(PlotCommands.HoverPointsOnlyTrack);

            plotModel.Axes.Add(new LinearAxis() {
                Position = AxisPosition.Bottom,
                //IsAxisVisible = false
            });

            plotModel.Axes.Add(new LinearAxis() {
                Position = AxisPosition.Left,
                //IsAxisVisible = false
            });
        }

        public void addLine(MainWindow window, double x1, double y1, double x2, double y2, OxyColor color) {
            LineSeries series1 = new LineSeries { Color = color };
            series1.Points.Add(new DataPoint(x1, y1));
            series1.Points.Add(new DataPoint(x2, y2));
            series1.TrackerKey = "InvisibleTracker";
            window.plotModel.Series.Add(series1);

        }

        public void addNode(MainWindow window, double x, double y, string s, double markerSize, OxyColor color) {
            LineSeries series = new LineSeries();
            series.MarkerType = OxyPlot.MarkerType.Circle;
            series.Color = color;
            series.MarkerSize = markerSize;
            series.Points.Add(new DataPoint(x, y));
            series.TrackerFormatString = s;
            window.plotModel.Series.Add(series);
        }

        void addBCs(MainWindow window, double circleSize) {
            bool found;
            int i_aux=0;
            for (int j=0; j<BoundaryCondition.all.Count; j++) {
                found = false;

                for (int i = 0; i < this.BC_list.Count; i++) {
                    if (BoundaryCondition.all[j].node == this.BC_list[i].node) {
                        found = true;
                        i_aux = i;
                    }
                }

                if (!found) {   
                    this.BC_list.Add(BoundaryCondition.all[j]);
                    this.BC_list_nodes.Add(BoundaryCondition.all[j].node);
                    this.BC_list_type.Add(BoundaryCondition.all[j].type);
                }
                if (found) {
                    this.BC_list_type[i_aux] = this.BC_list_type[i_aux] + "\n" + BoundaryCondition.all[j].type;
                }
            }
            
            for(int i=0; i<this.BC_list.Count; i++) {
                addNode(window,this.BC_list[i].node.x, this.BC_list[i].node.y, "Node " + this.BC_list[i].node.number +"\n"+ this.BC_list_type[i]+"\n", circleSize, OxyColors.Blue);
            }
        }

        // returns arrow end point x1,y1
        List<double> addLoad(MainWindow window, Load l, OxyColor color, double circleSize = 0, double maxLoadMagnitude = 1, double arrowUnitLength = 1) {
            if (l.alpha == null) { // moment
                string s="Node " + l.node.number+"\n";
                if (this.BC_list_nodes.Contains(l.node)) {
                    s += this.BC_list_type[this.BC_list_nodes.IndexOf(l.node)] + "\n";
                }
                s += "Moment (z axis)=" + l.magnitude;
                addNode(window, l.node.x, l.node.y, s, circleSize*0.7, color);
                List<double> r = new List<double>();
                r.Add(l.node.x);
                r.Add(l.node.y);
                return r;

            }
            else {
                ArrowAnnotation arrow = new ArrowAnnotation() { };
                arrow.HeadLength = window.l * 0.1;
                arrow.StartPoint = new DataPoint(l.node.x, l.node.y);

                arrow.EndPoint = new DataPoint(l.node.x+Math.Cos(Convert.ToDouble(l.alpha))*l.magnitude/maxLoadMagnitude*arrowUnitLength,
                                               l.node.y + Math.Sin(Convert.ToDouble(l.alpha))*l.magnitude/maxLoadMagnitude*arrowUnitLength);
                arrow.Text = "fx: " + l.x.ToString("0.##") + "\nfy: " + l.y.ToString("0.##");

                arrow.Color = color;

                double xOffset, yOffset;
                xOffset = Math.Cos(Convert.ToDouble(l.alpha)) * l.magnitude / maxLoadMagnitude * arrowUnitLength + 0.03 * window.l;
                yOffset = Math.Sin(Convert.ToDouble(l.alpha)) * l.magnitude / maxLoadMagnitude * arrowUnitLength + 0.03 * window.l;

                if (l.alpha < 0) {
                    l.alpha = 360 + l.alpha;
                }

                double a = Convert.ToDouble(l.alpha);
                if (misc.toRadians(90) < a && a < misc.toRadians(270)) {
                    xOffset += -2*0.03 * window.l;
                }
                if (misc.toRadians(180) < a && a <= misc.toRadians(360)) {
                    yOffset += -2 * 0.03 * window.l;
                }

                arrow.TextPosition = new DataPoint(l.node.x+xOffset,l.node.y+yOffset);

                window.plotModel.Annotations.Add(arrow);

                List<double> r = new List<double>();
                r.Add(arrow.EndPoint.X);
                r.Add(arrow.EndPoint.Y);
                return r;

            }
        }

        void addDistributedLoad(MainWindow window, List<Load> loadList, OxyColor color, double circleSize = 0, double maxLoadMagnitude = 1, double arrowUnitLength = 1) {
            Load load1 = loadList[0];
            Load load2 = loadList[1];

            List<double> p1 = new List<double>();
            List<double> p2 = new List<double>();
            p1=addLoad(window, load1, color, circleSize: circleSize, maxLoadMagnitude: maxLoadMagnitude, arrowUnitLength: arrowUnitLength);
            p2=addLoad(window, load2, color, circleSize: circleSize, maxLoadMagnitude: maxLoadMagnitude, arrowUnitLength: arrowUnitLength);

            addLine(window, p1[0], p1[1], p2[0], p2[1], color);
        }
        
        // Load input file
        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            // Create new problem
            this.p = new Problem();
            this.BC_list.Clear();
            this.BC_list_nodes.Clear();
            this.BC_list_type.Clear();

            // Clear plot
            this.plotModel.Series.Clear();
            this.plotModel.Annotations.Clear();
            this.plotModel.InvalidatePlot(true);

            // Import input
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            string fileName;
            fileName = openFileDialog.FileName;
            if(fileName == "") {
                this.plotModel.Annotations.Clear();
                this.plotModel.Series.Clear();
                this.plotModel.InvalidatePlot(true);
                return;
            }
            new inputTxtReader(fileName, this.p);

            // show and hide components plot
            this.plot.Visibility = Visibility.Visible;
            this.button.Visibility = Visibility.Visible;
            this.calculating_text.Visibility = Visibility.Visible;
            this.calculating_text.Text = "Hover mouse on blue nodes to view boundary condition\n";

            // determine max X Y
            double xmin, xmax, ymin, ymax;
            xmin = Node.all[0].x;
            xmax = Node.all[0].x;
            ymin = Node.all[0].y;
            ymax = Node.all[0].y;

            for(int i=0; i<Node.all.Count; i++) {
                if (Node.all[i].x < xmin) {
                    xmin = Node.all[i].x;
                }
                if (Node.all[i].x > xmax) {
                    xmax = Node.all[i].x;
                }
                if (Node.all[i].y < ymin) {
                    ymin = Node.all[i].y;
                }
                if (Node.all[i].y > ymax) {
                    ymax = Node.all[i].y;
                }
            }

            this.l = Math.Max(xmax - xmin, ymax - ymin);

            
            // determine maxLoadMagnitude and arrowUnitLenght to scale loads
            double maxLoadMagnitude = 0;
            double arrowUnitLength = 3.5;
            for (int i = 0; i < p.inputConcentratedLoads.Count; i++) {
                if (p.inputConcentratedLoads[i].magnitude > maxLoadMagnitude && p.inputConcentratedLoads[i].alpha != null) {
                    maxLoadMagnitude = p.inputConcentratedLoads[i].magnitude;
                }
            }
            for (int i=0; i<p.inputDistributedLoads.Count; i++) {
                if (p.inputDistributedLoads[i][0].magnitude > maxLoadMagnitude && p.inputDistributedLoads[i][0].alpha != null) {
                    maxLoadMagnitude = p.inputDistributedLoads[i][0].magnitude;
                }
                if (p.inputDistributedLoads[i][1].magnitude > maxLoadMagnitude && p.inputDistributedLoads[i][1].alpha != null) {
                    maxLoadMagnitude = p.inputDistributedLoads[i][1].magnitude;
                }
            }

            this.plotModel.Axes[0].Reset();
            this.plotModel.Axes[1].Reset();

            if (maxLoadMagnitude == 0) { maxLoadMagnitude = 1; }

            this.plotModel.Axes[0].Maximum = (xmax + arrowUnitLength) * 1.05;
            this.plotModel.Axes[0].Minimum = (xmin - arrowUnitLength) * 1.05;
            this.plotModel.Axes[1].Maximum = (ymax + arrowUnitLength) * 1.05;
            this.plotModel.Axes[1].Minimum = (ymin - arrowUnitLength) * 1.05;

            // add lines
            double d = this.l * 0.014;
            Node lowX;
            Node highX;
            Node lowY;
            Node highY;
            for (int i = 0; i < Element.all.Count; i++) {
                if(Element.all[i].node1.x< Element.all[i].node2.x) {
                    lowX = Element.all[i].node1;
                    highX = Element.all[i].node2;
                }
                else {
                    lowX = Element.all[i].node2;
                    highX = Element.all[i].node1;
                }
                if (Element.all[i].node1.y < Element.all[i].node2.y) {
                    lowY = Element.all[i].node1;
                    highY = Element.all[i].node2;
                }
                else {
                    lowY = Element.all[i].node2;
                    highY = Element.all[i].node1;
                }

                addLine(this, lowX.x + d * Math.Cos(Element.all[i].alpha),
                    lowY.y + d * Math.Sin(Element.all[i].alpha),
                    highX.x - d * Math.Cos(Element.all[i].alpha),
                    highY.y - d * Math.Sin(Element.all[i].alpha), OxyColors.Black);
                addNode(this, Element.all[i].node1.x, Element.all[i].node1.y, "Node " + Element.all[i].node1.number + "\n",
                    5, OxyColors.Black);
            }

            // add BCs
            addBCs(this,5);

            // add loads
            for (int i=0; i<p.inputConcentratedLoads.Count; i++) {
                addLoad(this, p.inputConcentratedLoads[i], OxyColors.Aqua, circleSize : 5,
                    maxLoadMagnitude:maxLoadMagnitude,arrowUnitLength: arrowUnitLength);
            }
            for (int i=0; i<p.inputDistributedLoads.Count; i++) {
                addDistributedLoad(this, p.inputDistributedLoads[i], OxyColors.DeepSkyBlue,
                    circleSize: 5, maxLoadMagnitude: maxLoadMagnitude, arrowUnitLength: arrowUnitLength);
            }

            this.plotModel.InvalidatePlot(true); // refresh
        }   

        // Solve
        private void button_Click(object sender, RoutedEventArgs e) {
            // Build and solve problem
            this.calculating_text.Text = "Calculating ...";
            this.button.Visibility = Visibility.Hidden;
            this.calculating_text.Visibility = Visibility.Visible;

            this.InvalidateVisual();
            ForceUIToUpdate();



            this.p.buildProblem();
            this.p.solve();
            this.p.postProcess();

            this.plotModel.Series.Clear();
            this.plotModel.Annotations.Clear();

            double maxElementLengh = 0;
            for (int i=0; i<Element.all.Count; i++) {
                if (Element.all[i].l > maxElementLengh) {
                    maxElementLengh = Element.all[i].l;
                }
            }

            double maxDisp = Math.Max(this.p.solution.max_absolute_x_nodal_displacement, this.p.solution.max_absolute_y_nodal_displacement);
            double f = Math.Abs(Math.Round(0.05*maxElementLengh/maxDisp,2));// increase displacement factor
            if (f > 10000) {
                f = 10000;
            }
            
            double d=this.l*0.014; 
            // add lines
            for (int i = 0; i < Element.all.Count; i++) {
                addLine(this,Element.all[i].node1.x + p.solution.getNodeGlobalDisplacement(Element.all[i].node1, 'x') * f + d*Math.Cos(Element.all[i].alpha),
                        Element.all[i].node1.y + p.solution.getNodeGlobalDisplacement(Element.all[i].node1, 'y') * f + d * Math.Sin(Element.all[i].alpha),
                        Element.all[i].node2.x + p.solution.getNodeGlobalDisplacement(Element.all[i].node2, 'x') * f - d * Math.Cos(Element.all[i].alpha),
                        Element.all[i].node2.y + p.solution.getNodeGlobalDisplacement(Element.all[i].node2, 'y') * f - d * Math.Sin(Element.all[i].alpha),
                        OxyColors.Red);

                if (Element.all[i].node1.label == "") {
                    Element.all[i].node1.label = "dx="+p.solution.getNodeGlobalDisplacement(Element.all[i].node1, 'x') + "\n" +
                                             "dy=" + p.solution.getNodeGlobalDisplacement(Element.all[i].node1, 'y') + "\n" +
                                             "dz=" + p.solution.getNodeGlobalDisplacement(Element.all[i].node1, 'z') + "\n" +
                                             "External force x = " + p.solution.getNodeExternalForce(Element.all[i].node1, 'x') + "\n" +
                                             "External force y = " + p.solution.getNodeExternalForce(Element.all[i].node1, 'y') + "\n" +
                                             "External moment = " + p.solution.getNodeExternalForce(Element.all[i].node1, 'z') + "\n";
                }
                if (Element.all[i].node2.label == "") {
                    Element.all[i].node2.label = "dx = "+p.solution.getNodeGlobalDisplacement(Element.all[i].node2, 'x') + "\n" +
                                             "dy = " + p.solution.getNodeGlobalDisplacement(Element.all[i].node2, 'y') + "\n" +
                                             "dz = " + p.solution.getNodeGlobalDisplacement(Element.all[i].node2, 'z') + "\n" +
                                             "External force x = " + p.solution.getNodeExternalForce(Element.all[i].node2, 'x') + "\n" +
                                             "External force y = " + p.solution.getNodeExternalForce(Element.all[i].node2, 'y') + "\n" +
                                             "External moment = " + p.solution.getNodeExternalForce(Element.all[i].node2, 'z') + "\n";
                }


            }

            // add nodes
            for (int i = 0; i < Node.all.Count; i++) {
                addNode(this,Node.all[i].x + p.solution.getNodeGlobalDisplacement(Node.all[i], 'x') * f,
                        Node.all[i].y + p.solution.getNodeGlobalDisplacement(Node.all[i], 'y') * f,
                        "Node "+Node.all[i].number+"\n"+Node.all[i].label, 5, OxyColors.Red);
            }

            this.plotModel.InvalidatePlot(true); // refresh

            this.button.Visibility = Visibility.Collapsed;
            this.calculating_text.Visibility = Visibility.Visible;
            this.calculating_text.Text = "Done!\nFinal structure with X and Y displacements multiplied by "+f+"\nHover mouse on red nodes to view results\n";

            // verify forces sum
            if (true) {
                double xSum = 0;
                double ySum = 0;
                double zSum = 0;
                foreach(Node n in Node.all) {
                    xSum += p.solution.getNodeExternalForce(n, 'x');
                    zSum += p.solution.getNodeExternalForce(n, 'y');
                    zSum += p.solution.getNodeExternalForce(n, 'x');
                }
                p.outputText = "External nodal loads sum: \n X: " + xSum + "     Y: " + ySum + "     Z: " + zSum + "\n\n"+p.outputText;
            }

            // display text
            resultsWindow resultsWindowObj = new resultsWindow();
            resultsWindowObj.TextBox.Text = p.outputText;
            resultsWindowObj.Show();


        }
        public static void ForceUIToUpdate() {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);
            Dispatcher.PushFrame(frame);
        }
    }
}
