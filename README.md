# 2dStructuralFEM_GUI
2d Structural Finite Element Solver with Graphical Interface
Problem is described by input .txt file


# Tutorial
1) Launch /bin/Release/2dStructuralFEM_GUI.exe
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
  
  To-do
  
