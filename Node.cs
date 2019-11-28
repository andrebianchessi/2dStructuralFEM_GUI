﻿using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;

namespace _2dStructuralFEM_GUI {
    class Node {
        // list of all instances
        public static List<Node> all = new List<Node>();

        public double x; // x coordinate
        public double y; // y coordinate

        public int number; // Node number

        public int u_index; // x global index
        public int v_index; // y global index
        public int w_index; // z global index

        public static int u_index_local = 0; // x local index
        public static int v_index_local = 1; // y local index
        public static int w_index_local = 2; // z local index

        // list of elements attached to this node 
        //public List<Element> elements;

        public Node(double x, double y) {
            this.x = x;
            this.y = y;
            this.number = Node.all.Count + 1;

            this.u_index = 3 * this.number - 3;  // node 1 -> u_index = 0, v_index = 1, w_index = 2
            this.v_index = 3 * this.number -2;   // node 2 -> u_index = 3, v_index = 4, w_index = 5
            this.w_index = 3 * this.number - 1;

            Node.all.Add(this);
        }

        /// <summary>
        /// If node already exists at (x,y), return node
        /// Else, create and return node at (xy)
        /// </summary>
        /// <returns></returns>
        public static Node getNode(double x, double y) {
            foreach (Node n in Node.all) {
                if (n.x == x && n.y == y) {
                    return n;
                }
            }
            return new Node(x, y);
        }

        public string str() {
            return "Node " + this.number.ToString() + " at ("+this.x.ToString()+","+ this.y.ToString()+")";
        }

        public static void printResults(Vector<double> results, string keyWord) {
            foreach (Node n in Node.all) {
                Console.WriteLine(n.str());
                Console.WriteLine("\tX direction "+keyWord+": " + results[n.u_index]);
                Console.WriteLine("\tY direction "+keyWord+": " + results[n.v_index]);
                if (keyWord == "displacement") {
                    Console.WriteLine("\tZ direction rotation: " + results[n.w_index] + "\n");
                }
                if (keyWord == "force") {
                    Console.WriteLine("\tZ direction torque: " + results[n.w_index] + "\n");
                }
            }
        }

        public static void printLocalResults(Vector<double> forces, Element element) {
            Console.WriteLine("Element "+element.number+" between " + element.node1.str() +" and "+ element.node2.str());
            Console.WriteLine("Z Direction: Leaving the plane");
            Console.WriteLine("X Direction: From "+element.node1.str()+" to "+ element.node2.str());
            Console.WriteLine("Y Direction: z^x=y");

            foreach (Node n in element.nodes) {
                Console.WriteLine("\t"+n.str());
                if(n == element.node1) {
                    Console.WriteLine("\t\tLocal X direction force: " + forces[0]);
                    Console.WriteLine("\t\tLocal Y direction force: " + forces[1]);
                    Console.WriteLine("\t\tZ direction torque: " + forces[2] + "\n");
                }
                if (n == element.node2) {
                    Console.WriteLine("\t\tLocal X direction force: " + forces[3]);
                    Console.WriteLine("\t\tLocal Y direction force: " + forces[4]);
                    Console.WriteLine("\t\tZ direction torque: " + forces[5] + "\n");
                }
            }
        }

    }
}