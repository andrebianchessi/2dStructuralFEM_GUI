using System;
using System.Collections.Generic;
using System.Text;
using MathNet.Numerics.LinearAlgebra;
using System.Linq;

namespace _2dStructuralFEM_GUI {
    class Load {
        // list of all instances
        public static List<Load> all = new List<Load>();

        public Node node;                  // node where load is applied
        public List<Element> elements;     // list of elements where load is applied

        public double magnitude; // load magnitude
        public double? alpha = null;     // angle c.c.-wise to global x axis

        public double x; // magnitude on each global direction
        public double y;
        public double z;



        /// <summary>
        /// Create force with global direction given by alpha
        /// </summary>
        public Load(Node node, double magnitude, double alpha, bool radians=false, bool addToAll=true) {
            if (!radians) {
                alpha = misc.toRadians(alpha);
            }
            this.node = node;
            this.alpha = alpha;
            this.magnitude = magnitude;

            this.x = magnitude*Math.Cos(alpha);
            this.y = magnitude * Math.Sin(alpha);
            this.z = 0;

            this.elements = new List<Element>();

            if (addToAll) {
                Load.all.Add(this);
            }

            foreach(Element e in Element.all) {
                if( e.nodes.Contains(node) ){
                    this.elements.Add(e);
                }
            }
        }
        /// <summary>
        /// Create moment
        /// </summary>
        public Load(Node node, double magnitude, bool addToAll = true) {
            this.node = node;
            this.magnitude = magnitude;
            this.alpha = null;

            this.x = 0;
            this.y = 0;
            this.z = magnitude;

            this.elements = new List<Element>();

            if (addToAll) {
                Load.all.Add(this);
            }

            foreach (Element e in Element.all) {
                if (e.nodes.Contains(node)) {
                    this.elements.Add(e);
                }
            }
        }


    }
}
