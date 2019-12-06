using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

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
                            p.addElement(pars[0], Convert.ToDouble(pars[1]), Convert.ToDouble(pars[2]),
                            Convert.ToDouble(pars[3]), Convert.ToDouble(pars[4]), Convert.ToDouble(pars[5]),
                            Convert.ToDouble(pars[6]), Convert.ToDouble(pars[7]));
                        }
                        if (pars[0] == "truss") {
                            p.addElement(pars[0], Convert.ToDouble(pars[1]), Convert.ToDouble(pars[2]),
                            Convert.ToDouble(pars[3]), Convert.ToDouble(pars[4]), Convert.ToDouble(pars[5]),
                            Convert.ToDouble(pars[6]));
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
                        if (pars[2] == "xDisplacement") { t = bcType.xDisplacement; value = Convert.ToDouble(pars[3]); }
                        if (pars[2] == "yDisplacement") { t = bcType.yDisplacement; value = Convert.ToDouble(pars[3]); }
                        if (pars[2] == "zDisplacement") { t = bcType.zDisplacement; value = Convert.ToDouble(pars[3]); }
                        if (pars.Length == 3) {
                            p.addBC(Convert.ToDouble(pars[0]), Convert.ToDouble(pars[1]), t);
                        }
                        if (pars.Length == 4) {
                            p.addBC(Convert.ToDouble(pars[0]), Convert.ToDouble(pars[1]), t, value);
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
                            p.addForce(Convert.ToDouble(pars[0]), Convert.ToDouble(pars[1]), Convert.ToDouble(pars[2]), Convert.ToDouble(pars[3]), rad);
                        }
                        if (pars.Length == 9) { // distributed force
                            p.addForce(Convert.ToDouble(pars[0]), Convert.ToDouble(pars[1]), Convert.ToDouble(pars[2]),
                                       Convert.ToDouble(pars[3]), Convert.ToDouble(pars[4]), Convert.ToDouble(pars[5]),
                                       Convert.ToDouble(pars[6]), Convert.ToDouble(pars[7]), rad);
                        }
                        line = "###############";
                    }

                    if (line.Substring(0, 10) == "addMoment(") {
                        line = line.Substring(10);
                        pars = line.Split(',');
                        p.addMoment(Convert.ToDouble(pars[0]), Convert.ToDouble(pars[1]), Convert.ToDouble(pars[2]));
                        line = "###############";
                    }
                    
                }
            }
        }


    }
}
