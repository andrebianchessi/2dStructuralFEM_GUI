// To-do: forces in elements with distributed load applied


using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace _2dStructuralFEM_GUI {
    class Problem {
        public Vector<double> loadVector;
        public Matrix<double> stiffnessMatrix;
        public Vector<double> displacementVector;

        public Solution solution;

        public List<Load> inputConcentratedLoads;
        public List<List<Load>> inputDistributedLoads;

        public string outputText;


        public Matrix<double> stiffnessMatrix_BC; // stiff. matrix with BC applied

        public bool debug;

        public Problem() {
            this.solution = new Solution();
            this.outputText = "";
            this.debug = false;
            this.inputConcentratedLoads = new List<Load>();
            this.inputDistributedLoads = new List<List<Load>>();

            Node.all.Clear();
            Element.all.Clear();
            BoundaryCondition.all.Clear();
            Load.all.Clear();


        }

        // create element
        public void addElement(string type, double x1, double y1, double x2, double y2, double E, double A, double I) {
            Node n1 = Node.getNode(x1, y1);
            Node n2 = Node.getNode(x2, y2);
            new Element(type, n1, n2, E, A, I);
        }

        // create concentrated force
        public void addForce(double x, double y, double magnitude, double alpha, bool radians = false) {
            if (!radians) {
                alpha = misc.toRadians(alpha);
            }
            Node n = Node.getNode(x, y);
            new Load(n, magnitude, alpha, radians:true);

            this.inputConcentratedLoads.Add(new Load(n, magnitude, alpha, radians : true, addToAll:false));
        }

        // create concentrated moment
        public void addMoment(double x, double y, double magnitude) {
            Node n = Node.getNode(x, y);
            new Load(n, magnitude);
            this.inputConcentratedLoads.Add(new Load(n, magnitude, addToAll:false));
        }

        // create distributed force
        public void addForce(double x1, double y1, double magnitude1, double alpha1,
                             double x2, double y2, double magnitude2, double alpha2, bool radians = false) {
            if (!radians) {
                alpha1 = misc.toRadians(alpha1);
                alpha2 = misc.toRadians(alpha2);
            }

            Node n1 = Node.getNode(x1, y1);
            Node n2 = Node.getNode(x2, y2);

            List<Load> distributedLoadList = new List<Load>();
            distributedLoadList.Add( new Load(n1,magnitude1,alpha1,radians:true,addToAll:false));
            distributedLoadList.Add( new Load(n2, magnitude2, alpha2, radians: true, addToAll: false));
            this.inputDistributedLoads.Add(distributedLoadList);

            Element e = Element.getElement(x1, y1, x2, y2);
            double l = e.l;
            double alpha = e.alpha;

            double fx1 = magnitude1 * Math.Cos(alpha1);
            double fy1 = magnitude1 * Math.Sin(alpha1);
            double fx2 = magnitude2 * Math.Cos(alpha2);
            double fy2 = magnitude2 * Math.Sin(alpha2);

            double t1 = fx1 * Math.Cos(alpha) + fy1*Math.Sin(alpha);
            double t2 = fx2 * Math.Cos(alpha) + fy2 * Math.Sin(alpha);

            double g1 = fy1 * Math.Cos(alpha) - fx1 * Math.Sin(alpha);
            double g2 = fy2 * Math.Cos(alpha) - fx2 * Math.Sin(alpha);

            Vector<double> localForces = Vector<double>.Build.Dense(6);
            localForces[0] = (1 / 3.0 * t1 + 1 / 6.0 * t2) * l;
            localForces[1] = (7 / 20.0 * g1 + 3 / 20.0 * g2) * l;
            localForces[2] = (1 / 20.0 * g1 + 1 / 30.0 * g2) * l * l;
            localForces[3] = (1 / 6.0 * t1 + 1 / 3.0 * t2) * l;
            localForces[4] = (3 / 20.0 * g1 + 7 / 20.0 * g2) * l;
            localForces[5] = (-1 / 30.0 * g1 - 1 / 20.0 * g2) * l * l;

            Vector<double> globalForces = e.getGlobal(localForces);

            new Load(n1, globalForces[0], 0, radians=false);
            new Load(n1, globalForces[1], 90, radians = false);
            new Load(n1, globalForces[2]);
            new Load(n2, globalForces[3], 0, radians = false);
            new Load(n2, globalForces[4], 90, radians = false);
            new Load(n2, globalForces[5]);

            for(int i=0; i<6; i++) {
                e.distributedLoadsVector[i] += globalForces[i];
            }

        }

            // create boundary conditions
        public void addBC(double x, double y, bcType t, double displacement) {
            Node n = Node.getNode(x, y);
            new BoundaryCondition(n, t, displacement);
        }
        public void addBC(double x, double y, bcType t) {
            Node n = Node.getNode(x, y);
            if (t == bcType.fix) {
                new BoundaryCondition(n, bcType.xDisplacement, 0);
                new BoundaryCondition(n, bcType.yDisplacement, 0);
                new BoundaryCondition(n, bcType.zDisplacement, 0);
            }
            if (t == bcType.rollerX) {
                new BoundaryCondition(n, bcType.yDisplacement, 0);
            }
            if (t == bcType.rollerY) {
                new BoundaryCondition(n, bcType.xDisplacement, 0);
            }
            if (t == bcType.pin) {
                new BoundaryCondition(n, bcType.xDisplacement, 0);
                new BoundaryCondition(n, bcType.yDisplacement, 0);
            }

        }

        public void buildProblem() {
            // Build Stiffness Matrix
            this.stiffnessMatrix = Matrix<double>.Build.Sparse(Node.all.Count * 3, Node.all.Count * 3);
            foreach (Element e in Element.all) {
                Matrix<double> k = e.getGlobalK();
                if (this.debug) {
                    Console.WriteLine("Element " + e.number + " global stiffness matrix:");
                    Console.WriteLine(k);
                    outputText += "Element " + e.number.ToString() + " global stiffness matrix:\n";
                    outputText += k.ToString()+"\n";
                }
                for (int i=0; i<6; i++) {
                    for (int j=0; j<6; j++) {
                        this.stiffnessMatrix[e.getIndexes()[i], e.getIndexes()[j]]+=k[i,j];
                    }
                }
            }
            if (this.debug) {
                Console.WriteLine("Stiffness matrix:");
                Console.WriteLine(this.stiffnessMatrix);
                outputText += "Stiffness matrix:\n";
                outputText += this.stiffnessMatrix.ToString() + "\n";
            }

            // Build Load vector
            this.loadVector = Vector<double>.Build.Dense(Node.all.Count * 3);
            foreach (Load l in Load.all) {
                loadVector[l.node.u_index] += l.x;
                loadVector[l.node.v_index] += l.y;
                loadVector[l.node.w_index] += l.z;
            }

            if (this.debug) {
                Console.WriteLine("Load vector:");
                Console.WriteLine(this.loadVector);
                outputText += "Load vector:\n";
                outputText += this.loadVector.ToString() + "\n";
            }

            // Apply BC's
            this.stiffnessMatrix_BC = Matrix<double>.Build.Sparse(Node.all.Count * 3, Node.all.Count * 3);
            this.stiffnessMatrix.CopyTo(this.stiffnessMatrix_BC);
            foreach(BoundaryCondition bc in BoundaryCondition.all) {
                for (int i=0; i<Node.all.Count*3; i++) {
                    this.stiffnessMatrix_BC[bc.index,i] = 0;
                    this.stiffnessMatrix_BC[i,bc.index] = 0;
                }
                this.stiffnessMatrix_BC[bc.index, bc.index] = 1;
                this.loadVector[bc.index] = bc.displacement;
            }

            if (this.debug) {
                Console.WriteLine("Stiffness matrix with Boundary Conditions");
                Console.WriteLine(this.stiffnessMatrix_BC);
                Console.WriteLine("Load vector with Boundary Conditions");
                Console.WriteLine(this.loadVector);
                Console.WriteLine("\n\n\n\n\n\n");

                outputText += "Stiffness matrix with Boundary Conditions:\n";
                outputText += this.stiffnessMatrix_BC.ToString() + "\n";
                outputText += "Load vector with Boundary Conditions:\n";
                outputText += this.loadVector.ToString() + "\n\n\n\n";
            }
        }
        public void solve() {
            this.displacementVector = Vector<double>.Build.Dense(Node.all.Count * 3);
            this.stiffnessMatrix_BC.Solve(this.loadVector).CopyTo(this.displacementVector);
            Console.WriteLine("############################################ Displacements: ############################################");
            outputText += "############################################ Displacements: ############################################\n";
            outputText+=Node.printResults(this.displacementVector, "displacement");
            this.solution.displacements=displacementVector;
        }

        public void postProcess() {
            Console.WriteLine("######################################### External Nodal Forces #########################################");
            outputText += "######################################### External Nodal Forces ##########################################\n";
            Vector<double> externalForces =this.stiffnessMatrix* displacementVector;
            outputText+=Node.printResults(externalForces, "force");
            this.solution.externalForces=externalForces;

            Console.WriteLine("############################################ Element Forces ############################################");
            outputText += "############################################ Element Forces ############################################\n";
            Vector<double> localDisplacementVector = Vector<double>.Build.Dense(6);
            List<int> indexes;
            Vector<double> localForces=null;
            foreach (Element e in Element.all) {
                indexes = e.getIndexes();
                for (int i = 0; i < 6; i++) {
                    localDisplacementVector[i] = this.displacementVector[indexes[i]];
                }
                localDisplacementVector = e.getLocal(localDisplacementVector);

                // E = del*K - f -> Slide is wrong (Logan Finite Element Book, pdf pg 217
                localForces = e.getLocalK() * localDisplacementVector - e.getLocal(e.distributedLoadsVector);
                outputText+=Node.printLocalResults(localForces, e);
            }

            this.solution.elementForces=localForces;

        }

    }
}
