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

namespace _2dStructuralFEM_GUI {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        Problem p;
        PlotModel plotModel;
        List<string> BC_list_type = new List<string>();
        List<BoundaryCondition> BC_list=new List<BoundaryCondition>();

    public MainWindow() {
            InitializeComponent();
            this.p = new Problem();
            this.plot.Model = new PlotModel(){ PlotType = PlotType.Cartesian }; ;
            this.plotModel = this.plot.Model;
        }

        public void addLine(double x1, double y1, double x2, double y2, OxyColor color) {
            LineSeries series1 = null;
            series1 = new LineSeries { Color = color };
            series1.Points.Add(new DataPoint(x1, y1));
            series1.Points.Add(new DataPoint(x2, y2));
            plotModel.Series.Add(series1);

        }

        void addBCs() {
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
                    this.BC_list_type.Add(BoundaryCondition.all[j].type);
                }
                if (found) {
                    this.BC_list_type[i_aux] = this.BC_list_type[i_aux] + "\n" + BoundaryCondition.all[j].type;
                }
            }
            
            for(int i=0; i<this.BC_list.Count; i++) {
                var textAnnotation = new TextAnnotation {
                    Text = this.BC_list_type[i],
                    TextPosition = new DataPoint(this.BC_list[i].node.x, this.BC_list[i].node.y)
                };
                this.plotModel.Annotations.Add(textAnnotation);
            }
        }
        

        private void MenuItem_Click(object sender, RoutedEventArgs e) {
            // Import input
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            string fileName;
            fileName = openFileDialog.FileName;
            new inputTxtReader(fileName, this.p);

            // show plot
            this.plot.Visibility = Visibility.Visible;
            this.button.Visibility = Visibility.Visible;

            // add lines
            for (int i=0; i<Element.all.Count; i++) {
                addLine(Element.all[i].node1.x, Element.all[i].node1.y, Element.all[i].node2.x, Element.all[i].node2.y, OxyColors.Black);
            }

            // add BC
            addBCs();

            this.plotModel.InvalidatePlot(true); // refresh
        }   

        private void button_Click(object sender, RoutedEventArgs e) {
            // Build and solve problem
            this.button.Visibility = Visibility.Hidden;
            this.calculating_text.Visibility = Visibility.Visible;
            this.p.buildProblem();
            this.p.solve();
            this.p.postProcess();

            // add lines
            int f = 1;// increase displacement factor
            for (int i = 0; i < Element.all.Count; i++) {
                addLine(Element.all[i].node1.x+p.solution.getNodeGlobalDisplacement(Element.all[i].node1,'x')*f,
                        Element.all[i].node1.y + p.solution.getNodeGlobalDisplacement(Element.all[i].node1, 'y') * f,
                        Element.all[i].node2.x + p.solution.getNodeGlobalDisplacement(Element.all[i].node2, 'x') * f,
                        Element.all[i].node2.y + p.solution.getNodeGlobalDisplacement(Element.all[i].node2, 'y') * f, OxyColors.Red);
            }
            this.plotModel.InvalidatePlot(true); // refresh

            this.calculating_text.Text = "Done!";

            resultsWindow resultsWindowObj = new resultsWindow();

            resultsWindowObj.TextBox.Text = p.outputText;
            resultsWindowObj.Show();

        }
    }
}
