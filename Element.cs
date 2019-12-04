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
        // 0<=x_adim<=1 (adimensional x)
        public List<double> getForces_Displacement(Solution s, double x_adim) {
            List<double> r = new List<double>();
            r.Add(0.0);
            r.Add(0.0);
            r.Add(0.0);

            Vector<double> nodalDisplacements = Vector<double>.Build.Dense(6);
            Vector<double> displacement = Vector<double>.Build.Dense(6);
            nodalDisplacements[0] = s.getNodeGlobalDisplacement(this.node1, 'x');
            nodalDisplacements[1] = s.getNodeGlobalDisplacement(this.node1, 'y');
            nodalDisplacements[2] = s.getNodeGlobalDisplacement(this.node1, 'z');
            nodalDisplacements[3] = s.getNodeGlobalDisplacement(this.node2, 'x');
            nodalDisplacements[4] = s.getNodeGlobalDisplacement(this.node2, 'y');
            nodalDisplacements[5] = s.getNodeGlobalDisplacement(this.node2, 'z');
            nodalDisplacements = this.getLocal(nodalDisplacements);

            // TO-DO : Add distributed load correction
            // local displacements at x_adim
            double u;
            u = (1 - x_adim) * nodalDisplacements[0] + x_adim * nodalDisplacements[3];

            // sigma
            double length = x_adim * this.l; // distance from node1 to x_adim
            r[0] = this.E *this.A* (u- nodalDisplacements[0]) / length;

            // M
            r[2] = this.E * this.I / (this.l * this.l) * ((-6 + 12 * x_adim)* -nodalDisplacements[1] +
                                                (-4*this.l+6*this.l*x_adim)* -nodalDisplacements[2] +
                                                (6 - 12 * x_adim) * -nodalDisplacements[4] +
                                                (-2*this.l + 6 * this.l * x_adim) * -nodalDisplacements[5]  );

            // V
            r[1] = this.E * this.I / (this.l * this.l * this.l) * ((12) * -nodalDisplacements[1] +
                                                (6 * this.l) * -nodalDisplacements[2] +
                                                (-12) * -nodalDisplacements[4] +
                                                (6 * this.l) * -nodalDisplacements[5]);

            return r;
        }
        public List<double> getForces(Problem p, double x_adim) {
            List<double> r = new List<double>();
            r.Add(0.0);
            r.Add(0.0);
            r.Add(0.0);


            Vector<double> localDistributedForces = Vector<double>.Build.Dense(6);
            Vector<double> inputGlobalDistributedForces = Vector<double>.Build.Dense(6);

            // get local distributed forces
            for (int i=0; i<p.inputDistributedLoads.Count; i++) {
                if (p.inputDistributedLoads[i][0].node == this.node1 && p.inputDistributedLoads[i][1].node == this.node2) {
                    inputGlobalDistributedForces[0] = p.inputDistributedLoads[i][0].x;
                    inputGlobalDistributedForces[1] = p.inputDistributedLoads[i][0].y;
                    inputGlobalDistributedForces[2] = p.inputDistributedLoads[i][0].z;
                    inputGlobalDistributedForces[3] = p.inputDistributedLoads[i][1].x;
                    inputGlobalDistributedForces[4] = p.inputDistributedLoads[i][1].y;
                    inputGlobalDistributedForces[5] = p.inputDistributedLoads[i][1].z;
                    localDistributedForces += this.getLocal(inputGlobalDistributedForces);
                }
                if (p.inputDistributedLoads[i][0].node == this.node2 && p.inputDistributedLoads[i][1].node == this.node1) {
                    inputGlobalDistributedForces[0] = p.inputDistributedLoads[i][1].x;
                    inputGlobalDistributedForces[1] = p.inputDistributedLoads[i][1].y;
                    inputGlobalDistributedForces[2] = p.inputDistributedLoads[i][1].z;
                    inputGlobalDistributedForces[3] = p.inputDistributedLoads[i][0].x;
                    inputGlobalDistributedForces[4] = p.inputDistributedLoads[i][0].y;
                    inputGlobalDistributedForces[5] = p.inputDistributedLoads[i][0].z;
                    localDistributedForces += this.getLocal(inputGlobalDistributedForces);
                }
            }
            // normal force
            double n1 = localDistributedForces[0];
            double n2 = localDistributedForces[3];
            double nx = n1 + (n2 - n1) * x_adim;
            double ne = (nx + n1) * x_adim / 2 * this.l;
            r[0] = -1*(p.solution.getElementLocalForce(this,"x1")+ne);

            // shear force
            double v1 = localDistributedForces[1];
            double v2 = localDistributedForces[4];
            double vx = v1 + (v2 - v1) * x_adim;
            double ve = (vx + v1) * x_adim / 2 * this.l;

            double xg_adim = x_adim * (v1 + (vx - v1) * 2 / 3.0) / (v1 + vx); //distrib load centroid

            r[1] = p.solution.getElementLocalForce(this, "y1") + ve;

            // moment
            double m1 = localDistributedForces[2];
            double m2 = localDistributedForces[5];
            double mx = m1 + (m2 - m1) * x_adim;
            double me = (mx + m1) * x_adim / 2 * this.l;

            if (ve == 0) {
                xg_adim = 0;
            }
            r[2] = -(p.solution.getElementLocalForce(this, "z1") + me + ve*xg_adim*this.l - r[1]*x_adim*this.l);


            return r;
        }
    }
}
