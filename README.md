About
==============================================================================================
The Mixed-Reality Annotator is a tool, that shall allow people to inspect and annotate virtual
representations of physical objects everywhere through Mixed-Reality. 

Prerequisites
==============================================================================================
To Build the Mixed-Reality Annotator, the following prerequisites have to be installed:
- Unity 2007.1.0f3
- Export Plugin for UWP target
- Visual Studio 2015/2017
- Windows UWP SDK

Build
==============================================================================================
Import the Project into Unity and open the Holograms object in the Inspector. Put in your
credentials in the fields of CouchDBWrapper. After this open the build dialog under 
"File->Build Settings". Choose as the platform "Windows Universal Platform". Then choose in
the settings:

"Target Device" = Hololens
"Build Type" = D3D
"Unity C# Projects" = checked

After the Settings where made, start the build process by clicking on "Build" and choose or
create a folder, where the Project should by build. After the build process is finished, the
choosen folder opens. Then open the generated Visual Studio Solution File. Choose your target
device (HoloLens Device/Emulator) and start the debuggin process.

Functionality
==============================================================================================
The major part of the user interface is done by speech recognition. Below are the keywords, that
are recognized by the Mixed-Reality Annotator while focusing an object:

"Move Object" = Move the selected object
"Place Object" = When in move mode, this places the objects at the current position
"Rotate Object" = Rotate the selected object with the manipulation gesture
"Scale Object" = Scale the selected object with the manipulation gesture
"Hide Annotations" = Hide the annotations of the current selected object
"Show Annotations" = Show the annotations of the current selected object
"Create Annotation" = Creates a new Annotation at the current focused spot on the object
"Delete Annotation" = Deletes the current focused annotation
"Update Annotation" = Selects the current focused annotation and allows the user to change the annotation text
"Delete Object" = Deletes the current selected object from the scene

The following commands are recognized, without focusing an object
"Spawn <Object Name>" = Spawns a new Object with name <Object Name>
"Delete <Object Name>" = Deletes the Object with name <Object Name>
