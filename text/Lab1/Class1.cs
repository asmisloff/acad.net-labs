using Autodesk.AutoCAD.ApplicationServices; 
using Autodesk.AutoCAD.EditorInput; 
using Autodesk.AutoCAD.Runtime; 

public class Class1 
{ 

	[CommandMethod("HelloWorld")] 
	public void HelloWorld() 
	{ 
		Editor ed = Application.DocumentManager.MdiActiveDocument.Editor; 
		ed.WriteMessage("Hello World"); 
	
	} 
}