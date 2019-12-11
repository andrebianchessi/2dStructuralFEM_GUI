# 2dStructuralFEM_GUI
2d Structural Finite Element Solver with Graphical Interface

Suports:
-Frame (beam + truss) and truss elements
-Distributed and concentrated loads

Problem is described by a .txt input file, and graphical interface displays the structure with the loads and boundary conditions applied.

Outputs nodal displacements, maximum forces and moments, and forces diagram data for each element (as .csv format text) .



# Tutorial
1) To use the software, download this repository as .zip. Copy everything inside /bin/Release/ to a folder where you want to use the software. Launch /bin/Release/2dStructuralFEM_GUI.exe
2) Click "File"
3) Select one of "input_example ... .txt" file
- Structure will be displayed.
- Light blue arrows indicate concentrated loads.
- 2 Light blue arrows connected indicate distributed load.
- Dark blue nodes represent boundary condition. Hover mouse on them to view type of boundary condition.
- Light blue circles on nodes indicate concentrated moments. Hover mouse on them to view magnitude and direction.
4) Click "Solve"
Deformed structure will be displayed and text output will pop-up containing:
- Sum of external forces and moments in each direction (they should all be zero).
- Maximum displacements and forces and their corresponding nodes and elements.
- Resultant forces and moments at each node.
- Normal force, shear force and moments diagram for each element, in csv format.

# Input file format
  
Blank lines and lines starting with '#' are ignored

Lines end with 'new line'

Add frame element:

addElement(frame, x1, y1, x2, y2, E, A, I)

Add truss element:

addElement(frame, x1, y1, x2, y2, E, A)

Add Boundary condition:

addBC(x, y, type, displacement)

addBC(x, y, type)

type: fix, rollerX, rollerY, pin, xDisplacement, yDisplacement, zDisplacement

Add loads:

Concentrated force  -> addForce(x, y, magnitude, alpha, radians)

Distributed force   -> addForce(x1, y1, magnitude1, alpha1, x2, y2, magnitude2, alpha2, radians)

Concentrated moment -> addMoment(double x, double y, double magnitude)

    radians = true -> alpha in radians
    
    radians = false -> alpha in degrees

Debug mode:

debug(true)
