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

using System ;
using Autodesk.AutoCAD.Runtime ;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;
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
		//This function returns the ObjectId for the BlockTableRecord called "EmployeeBlock",
		//creating it if necessary.  The block contains three entities - circle, text 
		//and ellipse.
		public ObjectId CreateEmployeeDefinition() 
		{ 
			ObjectId newBtrId = new ObjectId(); //The return value for this function
			Database db = HostApplicationServices.WorkingDatabase; //save some space
			Transaction trans = db.TransactionManager.StartTransaction(); //begin the transaction
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor; 
			
			try 
			{ 
				//Now, drill into the database and obtain a reference to the BlockTable
				BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForWrite); 
				if ((bt.Has("EmployeeBlock"))) 
				{ 
					newBtrId =bt["EmployeeBlock"];
				} 
				else 
				{ 
					Point3d center = new Point3d(10, 10, 0); // convenient declaration...
					//  Declare and define the entities we want to add:
					//Circle:
					Circle circle = new Circle(center, Vector3d.ZAxis, 2); 
					//Text:
					MText text = new MText(); 
					text.Contents = "Earnest Shackleton"; 
					text.Location = center; 
					//Ellipse:
					Ellipse ellipse = new Ellipse(center, Vector3d.ZAxis, new Vector3d(3, 0, 0), 0.5, 0, 0); 

					//Next, create a layer with the helper function, and assign
					//the layer to our entities.
					ObjectId empId = CreateLayer(); 
					text.LayerId = empId; 
					circle.LayerId = empId; 
					ellipse.LayerId = empId; 
					//Set the color for each entity irrespective of the layer's color.
					text.ColorIndex = 2; 
					circle.ColorIndex = 1; 
					ellipse.ColorIndex = 3; 

					//Create a new block definition called EmployeeBlock
					BlockTableRecord newBtr = new BlockTableRecord(); 
					newBtr.Name = "EmployeeBlock"; 
					newBtrId = bt.Add(newBtr); //Add the block, and set the id as the return value of our function
					trans.AddNewlyCreatedDBObject(newBtr, true); //Let the transaction know about any object/entity you add to the database!
					
					newBtr.AppendEntity(circle); //Append our entities...
					newBtr.AppendEntity(text); 
					newBtr.AppendEntity(ellipse); 
					trans.AddNewlyCreatedDBObject(circle, true); //Again, let the transaction know about our newly added entities.
					trans.AddNewlyCreatedDBObject(text, true); 
					trans.AddNewlyCreatedDBObject(ellipse, true); 
										
				} 
				trans.Commit(); //All done, no errors?  Go ahead and commit!
			} 
			catch 
			{ 				
				ed.WriteMessage("Error Creating Employee Block");
			} 
			finally 
			{ 
				trans.Dispose(); 
			} 
			CreateDivision(); //Create the Employee Division dictionaries.
			return newBtrId; 
		}
		

		//This function creates a new BlockReference to the "EmployeeBlock" object,
		//and adds it to ModelSpace.
		[CommandMethod("CREATE")] 
		public void CreateEmployee() 
		{ 
			Database db = HostApplicationServices.WorkingDatabase; 
			Transaction trans = db.TransactionManager.StartTransaction(); 
			try 
			{ 

				BlockTable bt = (BlockTable)(trans.GetObject(db.BlockTableId, OpenMode.ForWrite)); 
				BlockTableRecord btr =(BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
				//Create the block reference...use the return from CreateEmployeeDefinition directly!
				BlockReference br = new BlockReference(new Point3d(10, 10, 0), CreateEmployeeDefinition()); 
				btr.AppendEntity(br); //Add the reference to ModelSpace
				trans.AddNewlyCreatedDBObject(br, true); //Let the transaction know about it

				//Create the custom per-employee data
				Xrecord xRec =  new Xrecord();
				//We want to add 'Name', 'Salary' and 'Division' information.  Here is how:
				xRec.Data = new ResultBuffer( 
					new TypedValue((int)DxfCode.Text, "Earnest Shackleton"), 
					new TypedValue((int)DxfCode.Real, 72000), 
					new TypedValue((int)DxfCode.Text, "Sales"));

				//Next, we need to add this data to the 'Extension Dictionary' of the employee.
				br.CreateExtensionDictionary();
				DBDictionary brExtDict  = (DBDictionary)trans.GetObject(br.ExtensionDictionary, OpenMode.ForWrite, false);
				brExtDict.SetAt("EmployeeData", xRec); //Set our XRecord in the dictionary at 'EmployeeData'.
				trans.AddNewlyCreatedDBObject(xRec, true);

				trans.Commit(); 
			} 
			finally 
			{ 
				trans.Dispose(); 
			} 
		}
		 
		//This function returns the objectId for the "EmployeeLayer", creating it if necessary.
		public ObjectId CreateLayer() 
		{ 
			ObjectId layerId; 
			Database db = HostApplicationServices.WorkingDatabase; 
			Transaction trans = db.TransactionManager.StartTransaction(); 
			//Get the layer table first...
			LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite); 
			//Check if EmployeeLayer exists...
			if (lt.Has("EmployeeLayer")) 
			{ 
				layerId = lt["EmployeeLayer"];
			} 
			else 
			{ 
				//If not, create the layer here.
				LayerTableRecord ltr = new LayerTableRecord(); 
				ltr.Name = "EmployeeLayer"; // Set the layer name
				ltr.Color = Color.FromColorIndex(ColorMethod.ByAci, 2); 
				layerId = lt.Add(ltr); 
				trans.AddNewlyCreatedDBObject(ltr, true); 
			} 
			trans.Commit(); 
			trans.Dispose(); 
			return layerId; 
		}

		public void CreateDivision()
		{
			Database db = HostApplicationServices.WorkingDatabase;
			Transaction trans = db.TransactionManager.StartTransaction(); 

			try
			{
				//First, get the NOD...
				DBDictionary NOD = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);
				//Define a corporate level dictionary
				DBDictionary acmeDict;
				try
				{
					//Just throw if it doesn't exist...do nothing else
					acmeDict = (DBDictionary)trans.GetObject(NOD.GetAt("ACME_DIVISION"), OpenMode.ForRead);
				}
				catch
				{
					//Doesn't exist, so create one, and set it in the NOD�
					acmeDict = new DBDictionary();
					NOD.SetAt("ACME_DIVISION", acmeDict);
					trans.AddNewlyCreatedDBObject(acmeDict, true);
				}

				//Now get the division we want from acmeDict
				DBDictionary divDict;
				try
				{
					divDict = (DBDictionary)trans.GetObject(acmeDict.GetAt("Sales"), OpenMode.ForWrite);
				}
				catch
				{
					divDict = new DBDictionary();
					//Division doesn't exist, create one
					acmeDict.UpgradeOpen();
					acmeDict.SetAt("Sales", divDict);
					trans.AddNewlyCreatedDBObject(divDict, true);
				}

				//Now get the manager info from the division
				//We need to add the name of the division supervisor.  We'll do this with another XRecord.
				Xrecord mgrXRec;
				try
				{
					mgrXRec = (Xrecord)trans.GetObject(divDict.GetAt("Department Manager"), OpenMode.ForWrite);
				}
				catch
				{
					mgrXRec = new Xrecord();
					mgrXRec.Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, "Randolph P. Brokwell"));
					divDict.SetAt("Department Manager", mgrXRec);
					trans.AddNewlyCreatedDBObject(mgrXRec, true);
				}

				trans.Commit();
			}
			finally
			{
				trans.Dispose();
			}
		}

		[CommandMethod("EMPLOYEECOUNT")]
		public void EmployeeCount()
		{
			Database db = HostApplicationServices.WorkingDatabase;
			Transaction trans = db.TransactionManager.StartTransaction();  //Start the transaction.
			int nEmployeeCount  = 0;
			try
			{
				//First, get at the BlockTable, and the ModelSpace BlockTableRecord
				BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
				BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
				//Now, we need to be able to print to the commandline.  Here is an object which will help us:
				Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

				//Now, here is the fun part.  This is where we iterate through ModelSpace:
				foreach (ObjectId id in btr)
				{
					Entity ent = (Entity)trans.GetObject(id, OpenMode.ForRead, false);  //Use it to open the current object!
					if (ent.GetType() == typeof(BlockReference)) //We use .NET's RTTI to establish type.
					{
						nEmployeeCount += 1;
					}
				}

				ed.WriteMessage("Employees Found: " + nEmployeeCount.ToString());
				trans.Commit();
			}
			finally
			{
				trans.Dispose();
			}
		}

		//We want a command which will go through and list all the relevant employee data.
		public static void ListEmployee(ObjectId employeeId, ref string[] saEmployeeList) 
		{ 
			int nEmployeeDataCount = 0; 
			Database db = HostApplicationServices.WorkingDatabase; 
			Transaction trans = db.TransactionManager.StartTransaction(); //Start the transaction
			try 
			{ 
				Entity ent = (Entity)trans.GetObject(employeeId, OpenMode.ForRead, false); //Use it to open the current object!
				if (ent.GetType() == typeof(BlockReference)) //We use .NET's RTTI to establish type.
				{ 
					//Not all BlockReferences will have our employee data, so we must make sure we can handle failure
					bool bHasOurDict = true; 
					Xrecord EmployeeXRec = null; 
					try 
					{ 
						BlockReference br = (BlockReference)ent; 
						DBDictionary extDict = (DBDictionary)trans.GetObject(br.ExtensionDictionary, OpenMode.ForRead, false); 
						EmployeeXRec = (Xrecord)trans.GetObject(extDict.GetAt("EmployeeData"), OpenMode.ForRead, false); 
					} 
					catch 
					{ 
						bHasOurDict = false; //Something bad happened...our dictionary and/or XRecord is not accessible
					} 

					if (bHasOurDict) //If obtaining the Extension Dictionary, and our XRecord is successful...
					{ 

						// allocate memory for the list
						saEmployeeList = new String[4];

						TypedValue resBuf = EmployeeXRec.Data.AsArray()[0]; 
						saEmployeeList.SetValue(string.Format("{0}\n", resBuf.Value), nEmployeeDataCount); 
						nEmployeeDataCount += 1; 
						resBuf = EmployeeXRec.Data.AsArray()[1]; 
						saEmployeeList.SetValue(string.Format("{0}\n", resBuf.Value), nEmployeeDataCount); 
						nEmployeeDataCount += 1; 
						resBuf = EmployeeXRec.Data.AsArray()[2]; 
						string str = (string)resBuf.Value; 
						saEmployeeList.SetValue(string.Format("{0}\n", resBuf.Value), nEmployeeDataCount); 
						nEmployeeDataCount += 1; 
						DBDictionary NOD = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead, false); 
						DBDictionary acmeDict = (DBDictionary)trans.GetObject(NOD.GetAt("ACME_DIVISION"), OpenMode.ForRead); 
						DBDictionary salesDict = (DBDictionary)trans.GetObject(acmeDict.GetAt((string)EmployeeXRec.Data.AsArray()[2].Value), OpenMode.ForRead); 
						Xrecord salesXRec = (Xrecord)trans.GetObject(salesDict.GetAt("Department Manager"), OpenMode.ForRead); 
						resBuf = salesXRec.Data.AsArray()[0]; 
						saEmployeeList.SetValue(string.Format("{0}\n", resBuf.Value), nEmployeeDataCount); 
						nEmployeeDataCount += 1; 
					} 
				} 
				trans.Commit(); 
			} 
			finally 
			{ 
				trans.Dispose(); 
			} 
		} 

	
		[CommandMethod("PRINTOUTEMPLOYEE")] 
		public static  void PrintoutEmployee() 
		{ 
			Editor ed = Application.DocumentManager.MdiActiveDocument.Editor; 
			Database db = HostApplicationServices.WorkingDatabase; 
			Transaction trans = db.TransactionManager.StartTransaction(); 
			try 
			{ 
				BlockTable bt = (BlockTable)trans.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead); 
				BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead); 
				foreach (ObjectId id in btr) 
				{ 
					Entity ent = (Entity)trans.GetObject(id, OpenMode.ForRead, false); 
					if (ent is BlockReference) 
					{ 
						string[] saEmployeeList = null;
				
						ListEmployee(id, ref saEmployeeList); 
						if ((saEmployeeList.Length == 4)) 
						{ 
							ed.WriteMessage("Employee Name: {0}", saEmployeeList[0]); 
							ed.WriteMessage("Employee Salary: {0}", saEmployeeList[1]); 
							ed.WriteMessage("Employee Division: {0}", saEmployeeList[2]); 
							ed.WriteMessage("Division Manager: {0}", saEmployeeList[3]); 
						} 
					} 
				} 
			} 
			finally 
			{ 
			} 	
		}
	}
}