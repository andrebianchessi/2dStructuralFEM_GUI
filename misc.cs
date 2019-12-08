using System;
using System.Collections.Generic;
using System.Text;

namespace _2dStructuralFEM_GUI {
    class misc {
        static public double toRadians(double alpha) {
            if (alpha == 0) {
                return 0;
            }
            if (alpha == 90) {
                return Math.PI / 2.0;
            }
            if (alpha == -90) {
                return -Math.PI / 2.0;
            }
            if (alpha == 180) {
                return Math.PI;
            }
            if (alpha == -180) {
                return -Math.PI;
            }

            return (Math.PI / 180.0) * alpha;
        }
    }
}
