using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace _2dStructuralFEM_GUI {
    class inputTxtReader {

        public inputTxtReader(string fileName, Problem p) {
            string[] lines = System.IO.File.ReadAllLines(fileName);
            string line;
            string[] pars;

            for (int i = 0; i<lines.Length;i++) {
                line = lines[i];

                // ignore blank lines
                if (line == "") {
                    line = "#\n";
                }

                // ignore lines starting with #
                if (line[0] != '#') {
                    line = line.Substring(0, line.Length - 1).Replace(" ", string.Empty); //remove last ( and spaces

                    // debug mode
                    if (line.Substring(0, 6) == "debug(") {
                        line = line.Substring(6);

                        if (line == "false") {
                            p.debug = false;
                        }
                        else {
                            p.debug = true;
                        }
                        line = "###############";
                    }

                    // add element
                    if (line.Substring(0, 11) == "addElement(") {
                        line = line.Substring(11);
                        pars = line.Split(',');
                        if (pars[0] == "frame") {
                            p.addElement(pars[0], double.Parse(pars[1], CultureInfo.InvariantCulture), double.Parse(pars[2], CultureInfo.InvariantCulture),
                            double.Parse(pars[3], CultureInfo.InvariantCulture), double.Parse(pars[4], CultureInfo.InvariantCulture), double.Parse(pars[5], CultureInfo.InvariantCulture),
                            double.Parse(pars[6], CultureInfo.InvariantCulture), double.Parse(pars[7], CultureInfo.InvariantCulture));
                        }
                        if (pars[0] == "truss") {
                            p.addElement(pars[0], double.Parse(pars[1], CultureInfo.InvariantCulture), double.Parse(pars[2], CultureInfo.InvariantCulture),
                            double.Parse(pars[3], CultureInfo.InvariantCulture), double.Parse(pars[4], CultureInfo.InvariantCulture), double.Parse(pars[5], CultureInfo.InvariantCulture),
                            double.Parse(pars[6], CultureInfo.InvariantCulture));
                        }
                        line = "###############";
                    }

                    // add BC
                    if (line.Substring(0, 6) == "addBC(") {
                        line = line.Substring(6);
                        pars = line.Split(',');
                        bcType t = bcType.fix;
                        double value = 0.0;
                        if (pars[2] == "fix") { t = bcType.fix; }
                        if (pars[2] == "rollerX") { t = bcType.rollerX; }
                        if (pars[2] == "rollerY") { t = bcType.rollerY; }
                        if (pars[2] == "pin") { t = bcType.pin; }
                        if (pars[2] == "xDisplacement") { t = bcType.xDisplacement; value = double.Parse(pars[3], CultureInfo.InvariantCulture); }
                        if (pars[2] == "yDisplacement") { t = bcType.yDisplacement; value = double.Parse(pars[3], CultureInfo.InvariantCulture); }
                        if (pars[2] == "zDisplacement") { t = bcType.zDisplacement; value = double.Parse(pars[3], CultureInfo.InvariantCulture); }
                        if (pars.Length == 3) {
                            p.addBC(double.Parse(pars[0], CultureInfo.InvariantCulture), double.Parse(pars[1], CultureInfo.InvariantCulture), t);
                        }
                        if (pars.Length == 4) {
                            p.addBC(double.Parse(pars[0], CultureInfo.InvariantCulture), double.Parse(pars[1], CultureInfo.InvariantCulture), t, value);
                        }
                        line = "###############";
                    }

                    // add forces
                    if (line.Substring(0, 9) == "addForce(") {
                        line = line.Substring(9);
                        pars = line.Split(',');

                        bool rad = false;
                        if (pars[pars.Length - 1] == "false") {
                            rad = false;
                        }
                        else {
                            rad = true;
                        }

                        if (pars.Length == 5) { // concentrated force
                            p.addForce(double.Parse(pars[0], CultureInfo.InvariantCulture), double.Parse(pars[1], CultureInfo.InvariantCulture), double.Parse(pars[2], CultureInfo.InvariantCulture), double.Parse(pars[3], CultureInfo.InvariantCulture), rad);
                        }
                        if (pars.Length == 9) { // distributed force
                            p.addForce(double.Parse(pars[0], CultureInfo.InvariantCulture), double.Parse(pars[1], CultureInfo.InvariantCulture), double.Parse(pars[2], CultureInfo.InvariantCulture),
                                       double.Parse(pars[3], CultureInfo.InvariantCulture), double.Parse(pars[4], CultureInfo.InvariantCulture), double.Parse(pars[5], CultureInfo.InvariantCulture),
                                       double.Parse(pars[6], CultureInfo.InvariantCulture), double.Parse(pars[7], CultureInfo.InvariantCulture), rad);
                        }
                        line = "###############";
                    }

                    if (line.Substring(0, 10) == "addMoment(") {
                        line = line.Substring(10);
                        pars = line.Split(',');
                        p.addMoment(double.Parse(pars[0], CultureInfo.InvariantCulture), double.Parse(pars[1], CultureInfo.InvariantCulture), double.Parse(pars[2], CultureInfo.InvariantCulture));
                        line = "###############";
                    }
                    
                }
            }
        }


    }
}
