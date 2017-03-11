// (C) Copyright 2002-2005 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//


using Autodesk.AutoCAD.Runtime; 
using Autodesk.AutoCAD.ApplicationServices; 
using Autodesk.AutoCAD.EditorInput; 

[assembly: CommandClass(typeof(ClassLibrary.Class))]
namespace ClassLibrary
{
	/// <summary>
	/// Summary description for Class.
	/// </summary>
	public class Class
	{
		public Class()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		// Define Command "AsdkCmd1"
		[CommandMethod("selectPoint")]
		static public void selectpoint() // This method can have any name
		{
			// Put your command code here
			PromptPointOptions prPointOptions = new PromptPointOptions("Select a point"); 
			PromptPointResult prPointRes; 
			Editor ed = Application.DocumentManager.MdiActiveDocument.Editor; 
			prPointRes = ed.GetPoint(prPointOptions); 
			if (prPointRes.Status != PromptStatus.OK) 
			{ 
				ed.WriteMessage("Error\n");
			} 
			else
			{
				ed.WriteMessage("You selected point " + prPointRes.Value.ToString()); 
			}
		}

		// Define Command "getdistance"
		[CommandMethod("getdistance")]
		static public void getdistance()// This method can have any name
		{
			Editor ed = Application.DocumentManager.MdiActiveDocument.Editor; 
			PromptDistanceOptions prDistOptions = new PromptDistanceOptions("Find distance, select first point:"); 
			PromptDoubleResult prDistRes; 
			prDistRes = ed.GetDistance(prDistOptions); 
			if (prDistRes.Status != PromptStatus.OK) 
			{ 
				ed.WriteMessage("Error\n");
			} 
			else
			{
				ed.WriteMessage("The distance is: " + prDistRes.Value.ToString());
			}

		}

	}
}