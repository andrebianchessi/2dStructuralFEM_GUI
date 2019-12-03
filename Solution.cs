using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace _2dStructuralFEM_GUI {
    class Solution {

        public Vector<double> displacements;
        public Vector<double> externalForces;
        public List<List<double>> elementForces;

        public double maxAbsoluteDisplacement;

        public Solution() {
            this.elementForces = new List<List<double>>();
        }

        public void calculateMax(Problem p) {
            this.maxAbsoluteDisplacement = Math.Max(Math.Abs(p.displacementVector.Maximum()), Math.Abs(p.displacementVector.Minimum()));
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

    }
}
