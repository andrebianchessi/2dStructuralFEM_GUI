using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace _2dStructuralFEM_GUI {
    class Solution {

        public Vector<double> displacements;
        public Vector<double> externalForces;
        public List<List<double>> elementForces;

        public double max_absolute_x_nodal_displacement;
        public double max_absolute_y_nodal_displacement;
        public double max_absolute_z_nodal_displacement;

        public Node max_absolute_x_nodal_displacement_node;
        public Node max_absolute_y_nodal_displacement_node;
        public Node max_absolute_z_nodal_displacement_node;

        public double maxNormal;
        public double maxNormal_x_adim;
        public Element maxNormal_element;

        public double minNormal;
        public double minNormal_x_adim;
        public Element minNormal_element;

        public double maxShear;
        public double maxShear_x_adim;
        public Element maxShear_element;

        public double minShear;
        public double minShear_x_adim;
        public Element minShear_element;

        public double maxMoment;
        public double maxMoment_x_adim;
        public Element maxMoment_element;

        public double minMoment;
        public double minMoment_x_adim;
        public Element minMoment_element;




        public Solution() {
            this.elementForces = new List<List<double>>();
        
            this.max_absolute_x_nodal_displacement=0;
            this.max_absolute_y_nodal_displacement=0;
            this.max_absolute_z_nodal_displacement=0;

            this.maxNormal = float.NegativeInfinity;
            this.minNormal = float.PositiveInfinity;
            this.maxShear = float.NegativeInfinity;
            this.minShear = float.PositiveInfinity;
            this.maxMoment = float.NegativeInfinity;
            this.minMoment = float.PositiveInfinity;

    }


        public double getNodeGlobalDisplacement(Node n, char d) {
            if (d == 'x') {
                return this.displacements[n.u_index];
            }
            if (d == 'y') {
                return this.displacements[n.v_index];
            }
            if (d == 'z') {
                return this.displacements[n.w_index];
            }
            return 0.0;
        }

        public double getNodeExternalForce(Node n, char d) {
            if (d == 'x') {
                return this.externalForces[n.u_index];
            }
            if (d == 'y') {
                return this.externalForces[n.v_index];
            }
            if (d == 'z') {
                return this.externalForces[n.w_index];
            }
            return 0.0;
        }

        public double getElementLocalForce(Element e, string d) {
            if (d == "x1") {
                return this.elementForces[e.number-1][0];
            }
            if (d == "y1") {
                return this.elementForces[e.number - 1][1];
            }
            if (d == "z1") {
                return this.elementForces[e.number - 1][2];
            }
            if (d == "x2") {
                return this.elementForces[e.number - 1][3];
            }
            if (d == "y2") {
                return this.elementForces[e.number - 1][4];
            }
            if (d == "z2") {
                return this.elementForces[e.number - 1][5];
            }
            return 0.0;
        }

        public static string maxForce(Element element, string label, double magnitude, double x_adim) {
            string s = "";

            s += "Element " + element.number + " between " +element.node1.str() + ", where x_adim = 0, " +
                " and "+element.node1.str()+ ", where x_adim = 1, " + "\n";
            s += "x_adim = "+x_adim+" -> " + label + "= " + magnitude + "\n\n";

            return s;
        }
    }
}
