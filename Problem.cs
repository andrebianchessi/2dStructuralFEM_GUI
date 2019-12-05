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

        public Matrix<double> stiffnessMatrix_BC; // stiff. matrix with BC applied

        public bool debug;

        public string outputText;

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
                
                outputText += "Stiffness matrix with Boundary Conditions:\n";
                outputText += this.stiffnessMatrix_BC.ToString() + "\n";
                outputText += "Load vector with Boundary Conditions:\n";
                outputText += this.loadVector.ToString() + "\n\n\n\n";
            }
        }
        public void solve() {
            this.displacementVector = Vector<double>.Build.Dense(Node.all.Count * 3);
            this.stiffnessMatrix_BC.Solve(this.loadVector).CopyTo(this.displacementVector);
            outputText += "############################################ Displacements ############################################\n";
            outputText+=Node.printResults(this.displacementVector, "displacement");
            this.solution.displacements=displacementVector;
        }

        public void postProcess() {
            StringBuilder builder = new StringBuilder();
            builder.Append("######################################### External Nodal Forces ##########################################\n");
            Vector<double> externalForces =this.stiffnessMatrix* displacementVector;
            builder.Append(Node.printResults(externalForces, "force"));
            this.solution.externalForces=externalForces;
            

            builder.Append("######################################## Element Forces Diagram ########################################\n");
            builder.Append("Coordinate x_adim is the adimensional x coordinate in each element ( x / element length)\n");
            Vector<double> localDisplacementVector = Vector<double>.Build.Dense(6);
            List<int> indexes;
            Vector<double> localForces=null;

            for (int i=0; i<Element.all.Count; i++) {
                List<double> l = new List<double>();
                l.Add(0.0);
                l.Add(0.0);
                l.Add(0.0);
                l.Add(0.0);
                l.Add(0.0);
                l.Add(0.0);
                this.solution.elementForces.Add(l);
            }

            foreach (Element e in Element.all) {
                indexes = e.getIndexes();
                for (int i = 0; i < 6; i++) {
                    localDisplacementVector[i] = this.displacementVector[indexes[i]];
                }
                localDisplacementVector = e.getLocal(localDisplacementVector);

                
                localForces = e.getLocalK() * localDisplacementVector - e.getLocal(e.distributedLoadsVector);
                

                for(int i=0; i<6; i++) {
                    this.solution.elementForces[e.number - 1][i] = localForces[i];
                }
                
            }

            // calculate local force distribution
            int divisions = 100;
            List<double> forces = new List<double>();
            List<double> x_adim = new List<double>();
            List<double> normal = new List<double>();
            List<double> shear = new List<double>();
            List<double> moment = new List<double>();
            for (int i=0; i<Element.all.Count; i++) {
                // check if max or min displacement
                if(Math.Abs(solution.getNodeGlobalDisplacement(Element.all[i].node1, 'x')) >
                            Math.Abs(this.solution.max_absolute_x_nodal_displacement)) {
                    this.solution.max_absolute_x_nodal_displacement = solution.getNodeGlobalDisplacement(Element.all[i].node1, 'x');
                    this.solution.max_absolute_x_nodal_displacement_node = Element.all[i].node1;
                }
                if (Math.Abs(solution.getNodeGlobalDisplacement(Element.all[i].node1, 'y')) >
                            Math.Abs(this.solution.max_absolute_y_nodal_displacement)) {
                    this.solution.max_absolute_y_nodal_displacement = solution.getNodeGlobalDisplacement(Element.all[i].node1, 'y');
                    this.solution.max_absolute_y_nodal_displacement_node = Element.all[i].node1;
                }
                if (Math.Abs(solution.getNodeGlobalDisplacement(Element.all[i].node1, 'z')) >
                            Math.Abs(this.solution.max_absolute_z_nodal_displacement)) {
                    this.solution.max_absolute_z_nodal_displacement = solution.getNodeGlobalDisplacement(Element.all[i].node1, 'z');
                    this.solution.max_absolute_z_nodal_displacement_node = Element.all[i].node1;
                }
                if (Math.Abs(solution.getNodeGlobalDisplacement(Element.all[i].node2, 'x')) >
                            Math.Abs(this.solution.max_absolute_x_nodal_displacement)) {
                    this.solution.max_absolute_x_nodal_displacement = solution.getNodeGlobalDisplacement(Element.all[i].node2, 'x');
                    this.solution.max_absolute_x_nodal_displacement_node = Element.all[i].node2;
                }
                if (Math.Abs(solution.getNodeGlobalDisplacement(Element.all[i].node2, 'y')) >
                            Math.Abs(this.solution.max_absolute_y_nodal_displacement)) {
                    this.solution.max_absolute_y_nodal_displacement = solution.getNodeGlobalDisplacement(Element.all[i].node2, 'y');
                    this.solution.max_absolute_y_nodal_displacement_node = Element.all[i].node2;
                }
                if (Math.Abs(solution.getNodeGlobalDisplacement(Element.all[i].node2, 'z')) >
                            Math.Abs(this.solution.max_absolute_z_nodal_displacement)) {
                    this.solution.max_absolute_z_nodal_displacement = solution.getNodeGlobalDisplacement(Element.all[i].node2, 'z');
                    this.solution.max_absolute_z_nodal_displacement_node = Element.all[i].node2;
                }

                builder.Append( "\n\n#### Element " + Element.all[i].number + " ####\n");
                builder.Append("Between " + Element.all[i].node1.str() + " and " + Element.all[i].node2.str() + "\n");
                builder.Append(Element.all[i].node1.str() + " -> x_adim = 0" + "\n");
                builder.Append(Element.all[i].node2.str() + " -> x_adim = 1" + "\n\n");

                // calculate forces at each point
                for (int j = 0; j <= divisions; j++) {
                    
                    x_adim.Add(j / (divisions+0.0));
                    forces = Element.all[i].getForces(this, j / (divisions+0.0));
                    
                    normal.Add(forces[0]);
                    shear.Add(forces[1]);
                    moment.Add(forces[2]);

                    // check if max or min
                    // normal
                    if (forces[0] > this.solution.maxNormal) {
                        this.solution.maxNormal = forces[0];
                        this.solution.maxNormal_element = Element.all[i];
                        this.solution.maxNormal_x_adim = j / (divisions + 0.0);
                    }
                    if (forces[0] < this.solution.minNormal) {
                        this.solution.minNormal = forces[0];
                        this.solution.minNormal_element = Element.all[i];
                        this.solution.minNormal_x_adim = j / (divisions + 0.0);
                    }

                    // shear
                    if (forces[1] > this.solution.maxShear) {
                        this.solution.maxShear = forces[1];
                        this.solution.maxShear_element = Element.all[i];
                        this.solution.maxShear_x_adim = j / (divisions + 0.0);
                    }
                    if (forces[1] < this.solution.minShear) {
                        this.solution.minShear = forces[1];
                        this.solution.minShear_element = Element.all[i];
                        this.solution.minShear_x_adim = j / (divisions + 0.0);
                    }

                    // moment
                    if (forces[2] > this.solution.maxMoment) {
                        this.solution.maxMoment = forces[2];
                        this.solution.maxMoment_element = Element.all[i];
                        this.solution.maxMoment_x_adim = j / (divisions + 0.0);
                    }
                    if (forces[2] < this.solution.minMoment) {
                        this.solution.minMoment = forces[2];
                        this.solution.minMoment_element = Element.all[i];
                        this.solution.minMoment_x_adim = j / (divisions + 0.0);
                    }

                }

                builder.Append("CSV  format forces diagram:\n");

                builder.Append("x_adim, Normal force, Shear force, Moment\n");
                for(int j = 0; j<normal.Count; j++) {
                    builder.Append(x_adim[j] + ", " + normal[j] + ", " + shear[j] + ", " +moment[j] +"\n");
                }

            


            }

            outputText += builder.ToString() + "\n\n\n";

            // max and min
            string s = "";
            s += "######################################### Max. and min. values ###########################################\n";
            s += "Coordinate x_adim is the adimensional x coordinate in each element(x / element length)\n\n";
            s += "Max displacements:\n";
            s += this.solution.max_absolute_x_nodal_displacement_node.str() + " dx: " + this.solution.max_absolute_x_nodal_displacement +"\n";
            s += this.solution.max_absolute_y_nodal_displacement_node.str() + " dy: " + this.solution.max_absolute_y_nodal_displacement + "\n";
            s += this.solution.max_absolute_z_nodal_displacement_node.str() + " dz: " + this.solution.max_absolute_x_nodal_displacement + "\n";

            s += "\n";
            s += "Max forces:\n";
            s += Solution.maxForce(this.solution.maxNormal_element, "Normal force",
                this.solution.maxNormal, this.solution.maxNormal_x_adim);
            s += Solution.maxForce(this.solution.minNormal_element, "Normal force",
                this.solution.minNormal, this.solution.minNormal_x_adim);

            s += Solution.maxForce(this.solution.maxShear_element, "Shear force",
                this.solution.maxShear, this.solution.maxShear_x_adim);
            s += Solution.maxForce(this.solution.minShear_element, "Shear force",
                this.solution.minShear, this.solution.minShear_x_adim);

            s += Solution.maxForce(this.solution.maxMoment_element, "Moment force",
                this.solution.maxMoment, this.solution.maxMoment_x_adim);
            s += Solution.maxForce(this.solution.minMoment_element, "Moment force",
                this.solution.minMoment, this.solution.minMoment_x_adim);

            outputText = s + "\n\n" +outputText;

        }

    }
}
