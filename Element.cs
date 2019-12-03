using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace _2dStructuralFEM_GUI {
    class Element {
        public static List<Element> all = new List<Element>();

        public int number;   // element number
        public Node node1;   // element left node
        public Node node2;   // element right node
        public double alpha; // element's angle in radians with global x axis

        public double E;   // young modulus
        public double A;   // c.s. area
        public double I;   // c.s. moment of inertia
        public double l;   // length

        public List<Node> nodes; // list of nodes that make this element
        public Vector<double> distributedLoadsVector;

        public string type;
        public Element(string type, Node node1, Node node2, double E, double A, double I) {
            this.type = type;

            this.node1 = node1;
            this.node2 = node2;

            this.E = E;
            this.A = A;
            this.I = I;
            this.l = Math.Sqrt(Math.Pow((node2.y - node1.y), 2) + Math.Pow((node2.x - node1.x), 2));

            this.alpha = Math.Atan((node2.y - node1.y) / (node2.x - node1.x));

            Element.all.Add(this);
            this.number = Element.all.Count;

            this.nodes = new List<Node>() { node1, node2 };

            this.distributedLoadsVector = Vector<double>.Build.Dense(6);
        }

        /// <summary>
        /// Return element rotation matrix
        /// </summary>
        public Matrix<double> getRot() {
            var r = Matrix<double>.Build.Dense(6, 6);
            double c = Math.Cos(this.alpha);
            double s = Math.Sin(this.alpha);

            r[0, 0] = c;
            r[0, 1] = s;
            r[1, 0] = -s;
            r[1, 1] = c;
            r[2, 2] = 1;
            r[3, 3] = c;
            r[3, 4] = s;
            r[4, 3] = -s;
            r[4, 4] = c;
            r[5, 5] = 1;

            return r;
        }
        /// <summary>
        /// Returns list of global indexes so that:
        /// K_global[i,j]=K_local[getIndexes()[i],getIndexes()[j]]
        /// </summary>
        public List<int> getIndexes() {
            return new List<int>() {this.node1.u_index, this.node1.v_index, this.node1.w_index,
                                    this.node2.u_index, this.node2.v_index, this.node2.w_index};
        }
        /// <summary>
        /// Returns local stiffness matrix
        /// </summary>
        public Matrix<double> getLocalK() {
            var k = Matrix<double>.Build.Dense(6, 6);

            if (this.type == "bar") {
                double eal1 = this.E * this.A / this.l;  // E*A/l
                double eil1 = this.E * this.I / this.l;  // E*I/l
                double eil2 = eil1 / this.l;             // E*I/l^2
                double eil3 = eil2 / this.l;             // E*I/l^3

                k[0, 0] = eal1;
                k[0, 3] = -eal1;
                k[1, 1] = 12 * eil3;
                k[1, 2] = 6 * eil2;
                k[1, 4] = -12 * eil3;
                k[1, 5] = 6 * eil2;
                k[2, 2] = 4 * eil1;
                k[2, 4] = -6 * eil2;
                k[2, 5] = 2 * eil1;
                k[3, 3] = eal1;
                k[4, 4] = 12 * eil3;
                k[4, 5] = -6 * eil2;
                k[5, 5] = 4 * eil1;

                // Apply symmetry
                for (int j = 0; j < 6; j++) {
                    for (int i = j + 1; i < 6; i++) {
                        k[i, j] = k[j, i];
                    }
                }

            }
            
            return k;
        }

        /// <summary>
        /// Transform local vector coord. to global coord.
        /// </summary>
        public Vector<double> getGlobal(Vector<double> localVector) {
            return this.getRot().Transpose() * localVector;
        }
        public Vector<double> getLocal(Vector<double> globalVector) {
            return this.getRot() * globalVector;
        }

        public Matrix<double> getGlobalK() {
            return this.getRot().Transpose() * this.getLocalK() * this.getRot();
        }

        public static Element getElement(double x1, double y1, double x2, double y2) {
            foreach (Element el in Element.all) {
                if (el.nodes.Contains(Node.getNode(x1, y1)) && el.nodes.Contains(Node.getNode(x2, y2))) {
                    return el;
                }
            }
            return null;
        }

        // returns forces on point on element
        // getForces()[0] -> sigma
        // getForces()[1] -> V
        // getForces()[2] -> M
        // using local coordinate system
        //      positive sigma  : from node1 to node 2
        //      positive moment : z perendicular, leaving the plane
        //      positive shear  : z^x=y
        // 0<=x_adim<=1 (adimensional x)
        public List<double> getForces(Solution s, double x_adim) {
            List<double> r = new List<double>();
            r.Add(0.0);
            r.Add(0.0);
            r.Add(0.0);

            double sigmaNode1 = s.getElementLocalForce(this, "x1");
            double vNode1 = s.getElementLocalForce(this, "y1");
            double vNode2 = s.getElementLocalForce(this, "y2");
            double mNode1 = s.getElementLocalForce(this, "z1");

            r[0] = -sigmaNode1;
            r[1] = vNode1 + x_adim * (vNode2-vNode1);
            r[2] = mNode1 + x_adim*vNode1+x_adim*x_adim/2.0*(vNode2-vNode1);

            return r;
        }

    }
}
