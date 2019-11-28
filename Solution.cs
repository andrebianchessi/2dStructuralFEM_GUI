using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace _2dStructuralFEM_GUI {
    class Solution {

        public Vector<double> displacements;
        public Vector<double> externalForces;
        public Vector<double> elementForces;


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

        public double getElementForce(Element e, string d) {
            if (d == "x1") {
                return this.externalForces[e.node1.u_index];
            }
            if (d == "y1") {
                return this.externalForces[e.node1.v_index];
            }
            if (d == "z1") {
                return this.externalForces[e.node1.w_index];
            }
            if (d == "x2") {
                return this.externalForces[e.node2.u_index];
            }
            if (d == "y2") {
                return this.externalForces[e.node2.v_index];
            }
            if (d == "z2") {
                return this.externalForces[e.node2.w_index];
            }
            return 0.0;
        }

    }
}
