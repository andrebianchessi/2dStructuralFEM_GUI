using System;
using System.Collections.Generic;
using System.Text;

namespace _2dStructuralFEM_GUI {
    enum bcType { fix, rollerX, rollerY, pin, xDisplacement, yDisplacement, zDisplacement };
    class BoundaryCondition {
        public static List<BoundaryCondition> all =new List<BoundaryCondition>();
        public double displacement;
        public int index;
        public string type;
        public Node node;
        public BoundaryCondition(Node n, bcType t, double displacement) {
            this.displacement = displacement;
            this.node = n;
            if (t == bcType.xDisplacement) {
                this.index = n.u_index;
                this.type = "dx="+this.displacement.ToString();
            }
            if (t == bcType.yDisplacement) {
                this.index = n.v_index;
                this.type = "dy="+this.displacement.ToString();
            }
            if (t == bcType.zDisplacement) {
                this.index = n.w_index;
                this.type = "dz="+this.displacement.ToString();
            }
            BoundaryCondition.all.Add(this);
        }


    }
}
