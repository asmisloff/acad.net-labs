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

using System;
using System.Windows.Forms;
using System.Collections; // For ArrayList

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Colors;

using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

using Autodesk.AutoCAD.Windows;

[assembly: ExtensionApplication(typeof(Lab7.AsdkClass1))]

namespace Lab7
{
	/// <summary>
	/// Summary description for AsdkClass1.
	/// </summary>
	public class AsdkClass1 : IExtensionApplication
	{
		public static string sDivisionDefault = "Sales";
		public static string sDivisionManager = "Fiona Q. Farnsby";

		public void Initialize() 
		{
			AddContextMenu();
			EmployeeOptions.AddTabDialog();
		}	
		
		public void Terminate() 
		{
		}

		ContextMenuExtension m_ContextMenu;

		void CallbackOnClick(object Sender, EventArgs e)
		{
			DocumentLock docLock = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.LockDocument();
			Create();
			docLock.Dispose();
		}

		void AddContextMenu()
		{
			try
			{
				m_ContextMenu = new ContextMenuExtension();
				m_ContextMenu.Title = "Acme Employee Menu";
				Autodesk.AutoCAD.Windows.MenuItem mi;
				mi = new Autodesk.AutoCAD.Windows.MenuItem("Create Employee");
				mi.Click +=  new EventHandler(CallbackOnClick);
				m_ContextMenu.MenuItems.Add(mi);
            
				Autodesk.AutoCAD.ApplicationServices.Application.AddDefaultContextMenuExtension(m_ContextMenu);
			}
			catch
			{
			}
		}

		void RemoveContextMenu()
		{
			try
			{
				if( m_ContextMenu != null )
				{
					Autodesk.AutoCAD.ApplicationServices.Application.RemoveDefaultContextMenuExtension(m_ContextMenu);
					m_ContextMenu = null;
				}
			}
			catch
			{
			}
		}

		//This function returns the ObjectId for the BlockTableRecord called "EmployeeBlock",
		//creating it if necessary.  The block contains three entities - circle, text 
		//and ellipse.
		public static ObjectId CreateEmployeeDefinition()
		{
			ObjectId newBtrId = ObjectId.Null;	//The return value for this function
			Database db = HostApplicationServices.WorkingDatabase; //save some space
			Transaction trans = db.TransactionManager.StartTransaction(); // begin the transaction
			try
			{
				//Now, drill into the database and obtain a reference to the BlockTable
				BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForWrite);
				if (bt.Has("EmployeeBlock") )
				{
					newBtrId = bt["EmployeeBlock"]; //Already there...no need to recreate it!
				}
				else 
				{
					Point3d center = new Point3d(0, 0, 0); //convenient declaration...

					//Declare and define the entities we want to add:
					//Circle:
					Circle circle = new Circle(center, Vector3d.ZAxis, 2.0);

					//Attribute
					AttributeDefinition attDef = new AttributeDefinition(center, "NoName", "Name:", "Enter Name", db.Textstyle);

					//Ellipse:
					Ellipse ellipse = new Ellipse(center, Vector3d.ZAxis, new Vector3d(3, 0, 0), 0.5, 0.0, 0.0);

					//Next, create a layer with the helper function, and assign
					//the layer to our entities.
					ObjectId empId = CreateLayer();
					circle.LayerId = empId;
					ellipse.LayerId = empId;
					//Set the color for each entity irrespective of the layer//s color.
					attDef.ColorIndex = 2;
					circle.ColorIndex = 1;
					ellipse.ColorIndex = 3;

					//Create a new block definition called EmployeeBlock
					BlockTableRecord newBtr = new BlockTableRecord();
					newBtr.Name = "EmployeeBlock";
					newBtrId = bt.Add(newBtr); //Add the block, and set the id as the return value of our function
					trans.AddNewlyCreatedDBObject(newBtr, true); //Let the transaction know about any object/entity you add to the database!

					newBtr.AppendEntity(circle); //Append our entities...
					newBtr.AppendEntity(attDef);
					newBtr.AppendEntity(ellipse);
					trans.AddNewlyCreatedDBObject(circle, true); //Again, let the transaction know about our newly added entities.
					trans.AddNewlyCreatedDBObject(attDef, true);
					trans.AddNewlyCreatedDBObject(ellipse, true);
				}
				trans.Commit(); //All done, no errors?  Go ahead and commit!
			}
			catch 
			{
				MessageBox.Show("Error Creating Employee Block"); //Uh oh!
			}
			
			//CreateDivision() //Create the Employee Division dictionaries.
			return(newBtrId);
		} // end of CreateEmployeeDefinition
		

		[CommandMethod("CREATE")] 
		public void Create()
		{
			Database db = HostApplicationServices.WorkingDatabase;
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
			using(Transaction trans = db.TransactionManager.StartTransaction())
			{
				//Get employee name
				PromptStringOptions prName = new PromptStringOptions("Enter Employee Name");
				prName.AllowSpaces = true;
				PromptResult prNameRes = ed.GetString(prName);
				if( prNameRes.Status != PromptStatus.OK )
				{
					trans.Abort();
					return;
				}

				//Get employee division
				PromptStringOptions prDiv = new PromptStringOptions("Enter Employee Division");
				prDiv.AllowSpaces = true;
				PromptResult prDivRes = ed.GetString(prDiv);
				if( prDivRes.Status != PromptStatus.OK )
				{
					trans.Abort();
					return;
				}

				//Get employee salary
				PromptDoubleOptions prSal = new PromptDoubleOptions("Enter Employee Salary");
				prSal.AllowNegative = false;
				prSal.AllowNone = false;
				prSal.AllowZero = false;
				PromptDoubleResult prSalRes = ed.GetDouble(prSal);
				if( prSalRes.Status != PromptStatus.OK )
				{
					trans.Abort();
					return;
				}

				//Get employee//s coordinates in the drawing
				PromptPointOptions prPos = new PromptPointOptions("Enter Employee Position");
				prPos.AllowNone = false;
				PromptPointResult prPosRes = ed.GetPoint(prPos);
				if( prPosRes.Status != PromptStatus.OK )
				{
					trans.Abort();
					return;
				}

				//Create the employee block reference
				CreateEmployee(prNameRes.StringResult, prDivRes.StringResult, prSalRes.Value, prPosRes.Value);

				String manager = null;

				//Now create the division 
				//Pass an empty string for manager to check if it already exists
				Xrecord depMgrXRec;
				ObjectId xRecId;
				xRecId = CreateDivision(prDivRes.StringResult, manager);

				//Open the department manager XRecord
				depMgrXRec = (Xrecord)trans.GetObject(xRecId, OpenMode.ForRead);

				foreach( TypedValue val in depMgrXRec.Data.AsArray() )
				{
					string str;
					str = (string)val.Value;
					if( str.Equals("") )
					{
						// Manager was not set, now set it
						// Prompt for  manager name first
						PromptStringOptions prManagerName = new PromptStringOptions("\n" + "No Manager Set. Enter Manager Name");
						prManagerName.AllowSpaces = true;
						PromptResult prManagerNameRes = ed.GetString(prManagerName);
						if( prManagerNameRes.Status != PromptStatus.OK )
						{
							trans.Abort();
							return;
						}

						//Set a manager name
						depMgrXRec.Data = new ResultBuffer(new TypedValue((int)DxfCode.Text, prManagerName.Message));
					}
				}

				trans.Commit();
			}
		}		
 
		//This function creates a new BlockReference to the "EmployeeBlock" object,
		//and adds it to ModelSpace.
		public static ObjectId CreateEmployee(String name, String division, Double salary, Point3d pos)
		{
			Database db = HostApplicationServices.WorkingDatabase;
			Transaction trans = db.TransactionManager.StartTransaction();
			try 
			{
				BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForWrite);
				BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
				//Create the block reference...use the return from CreateEmployeeDefinition directly!
				BlockReference br = new BlockReference(pos, CreateEmployeeDefinition());
				// create a new attribute reference
				AttributeReference attRef = new AttributeReference();

				//Iterate the employee block and find the attribute definition
				BlockTableRecord empBtr = (BlockTableRecord)trans.GetObject(bt["EmployeeBlock"], OpenMode.ForRead);

				foreach(ObjectId id in empBtr )
				{
					Entity ent = (Entity)trans.GetObject(id, OpenMode.ForRead, false); //Use it to open the current object! 
					if( ent.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.AttributeDefinition")  )  //We use .NET//s RTTI to establish type.
					{
						//Set the properties from the attribute definition on our attribute reference
						AttributeDefinition attDef = (AttributeDefinition)ent;
						attRef.SetPropertiesFrom(attDef);
						attRef.Position = new Point3d(attDef.Position.X + br.Position.X, 
						attDef.Position.Y + br.Position.Y, 
						attDef.Position.Z + br.Position.Z);

						attRef.Height = attDef.Height;
						attRef.Rotation = attDef.Rotation;
						attRef.Tag = attDef.Tag;
						attRef.TextString = name;
					}
				}
				btr.AppendEntity(br); //Add the reference to ModelSpace

				//Add the attribute reference to the block reference
				br.AttributeCollection.AppendAttribute(attRef);

				//let the transaction know
				trans.AddNewlyCreatedDBObject(attRef, true);
				trans.AddNewlyCreatedDBObject(br, true); //Let the transaction know about it

				//Create the custom per-employee data
				Xrecord xRec = new Xrecord();
				//We want to add //Name//, //Salary// and //Division// information.  Here is how:
				xRec.Data = new ResultBuffer( 
					new TypedValue((int)DxfCode.Text, name), 
					new TypedValue((int)DxfCode.Real, salary), 
					new TypedValue((int)DxfCode.Text, division));

				//Next, we need to add this data to the //Extension Dictionary// of the employee.
				br.CreateExtensionDictionary();
				DBDictionary brExtDict = (DBDictionary)trans.GetObject(br.ExtensionDictionary, OpenMode.ForWrite, false);
				brExtDict.SetAt("EmployeeData", xRec); //Set our XRecord in the dictionary at //EmployeeData//.
				trans.AddNewlyCreatedDBObject(xRec, true);

				trans.Commit();

				//return the objectId of the employee block reference
				return br.ObjectId;
			}
			finally
			{
				trans.Dispose();
			}
		}




		//This function returns the objectId for the "EmployeeLayer", creating it if necessary.
		static ObjectId CreateLayer()
		{
			ObjectId layerId; //the return value for this function
			Database db = HostApplicationServices.WorkingDatabase;
			Transaction trans = db.TransactionManager.StartTransaction();
			//Get the layer table first...
			LayerTable lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForWrite);

			//Check if EmployeeLayer exists...
			if( lt.Has("EmployeeLayer") )
			{
				layerId = lt["EmployeeLayer"];
			}
			else 
			{
				//if not, create the layer here.
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
			 

		public static ObjectId	CreateDivision(String division, String manager)  
		{
			Database db = HostApplicationServices.WorkingDatabase;
			Transaction trans = db.TransactionManager.StartTransaction();
			//Define a corporate level dictionary
			DBDictionary acmeDict;			
			//First, get the NOD...
			using(	DBDictionary NOD =	(DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead, false))
			{
				try
				{
					//if it	exists,	just get it
					acmeDict = (DBDictionary)trans.GetObject(NOD.GetAt("ACME_DIVISION"), OpenMode.ForRead);
				}
				catch
				{
					//Doesn//t exist, so create	one
					NOD.UpgradeOpen();
					acmeDict = new DBDictionary();
					NOD.SetAt("ACME_DIVISION", acmeDict);
					trans.AddNewlyCreatedDBObject(acmeDict,	true);
				}
			}

			//Now get the division we want from	acmeDict
			DBDictionary divDict;
			try
			{
				divDict	= (DBDictionary)trans.GetObject(acmeDict.GetAt(division),	OpenMode.ForWrite);
			}
			catch
			{
				acmeDict.UpgradeOpen();
				divDict	= new DBDictionary(); //Division	doesn//t exist,	create one
				acmeDict.SetAt(division, divDict);
				trans.AddNewlyCreatedDBObject(divDict, true);
			}

			//Now get the manager info from	the	division
			//We need to add the name of the division supervisor.  We//ll do this with another XRecord.
			Xrecord mgrXRec;
			try
			{
				mgrXRec	= (Xrecord)trans.GetObject(divDict.GetAt("Department	Manager"), OpenMode.ForWrite);
			}
			catch
			{
				mgrXRec	= new Xrecord();
				mgrXRec.Data = new ResultBuffer(new	TypedValue((int)DxfCode.Text, manager));
				divDict.SetAt("Department Manager",	mgrXRec);
				trans.AddNewlyCreatedDBObject(mgrXRec, true);
			}

			trans.Commit();

			//return the department	manager	XRecord
			return mgrXRec.ObjectId;
		}	


		public void	EmployeeCount(ObjectId[] idArray)
		{
			Database db = HostApplicationServices.WorkingDatabase;
			using( Transaction trans = db.TransactionManager.StartTransaction()) //Start the transaction.
			{
				int nEmployeeCount = 0;
				try
				{
					//Now, we need to be able to print to the commandline.	Here is	an object which	will help us:
					Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

					//Now, here	is the fun part.  This is where	we iterate through ModelSpace:
					foreach( ObjectId id in idArray )
					{
						Entity ent = (Entity)trans.GetObject(id, OpenMode.ForRead,	false); //Use it	to open	the	current	object!
						if(ent.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.BlockReference"))	 //We use .NET//s RTTI to establish	type.
						{
							BlockReference br = (BlockReference)ent;

							//Test whether it is an	employee block

							//open its extension dictionary
							DBDictionary brExtDict;
							try
							{
								brExtDict = (DBDictionary)trans.GetObject(br.ExtensionDictionary, OpenMode.ForRead);
								brExtDict.GetAt("EmployeeData");
								//successfully got "EmployeeData" so br	is employee	block ref
								//increment	employee count
								nEmployeeCount += 1;

								//print	name of	the	employee here
								ed.WriteMessage(br.AttributeCollection[0].ToString() +	"\n");
							}
							catch
							{
								//Some problem.	Verifies the object	is not an employee block ref
							}
						}
					}
	                
					ed.WriteMessage(("Employees	Found: {0}"	+ "\n"),	nEmployeeCount.ToString());
					trans.Commit();
				}
				catch
				{
				}
			}
		}
	
		[CommandMethod("LISTEMPLOYEES")]
		public void List()
		{
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

			try
			{
				PromptSelectionOptions Opts =  new PromptSelectionOptions();

				//Build	a filter list so that only block references	are	selected
				TypedValue[] filList =	{new TypedValue((int)DxfCode.Start, "INSERT")};

				SelectionFilter filter = new SelectionFilter(filList);
				PromptSelectionResult res = ed.GetSelection(Opts, filter);

				//Do nothing if	selection is unsuccessful
				if( res.Status != PromptStatus.OK )
					return;
			
				Autodesk.AutoCAD.EditorInput.SelectionSet SS = res.Value;
				ObjectId[] idArray = SS.GetObjectIds();

				//List the selection set
				EmployeeCount(idArray);
			}
			catch
			{
			}

		}

		//We want a	command	which will go through and list all the relevant	employee data.
		public static void ListEmployee(ObjectId employeeId, ArrayList saEmployeeList)
		{
			int nEmployeeDataCount = 0;
			Database db = HostApplicationServices.WorkingDatabase;
			using(Transaction trans = db.TransactionManager.StartTransaction()) //Start the transaction.
			{
				using(Entity ent = (Entity)trans.GetObject(employeeId, OpenMode.ForRead,false)) //Use it	to open	the	current	object!
				{
					if( ent.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.BlockReference") ) //We use .NET//s RTTI to establish	type.
					{
						//Not all BlockReferences will have	our	employee data, so we must make sure	we can handle failure
						Boolean bHasOurDict = true;
						Xrecord EmployeeXRec = null;
						try
						{
							BlockReference br = (BlockReference)ent;
							DBDictionary extDict =	(DBDictionary)trans.GetObject(br.ExtensionDictionary, OpenMode.ForRead,	false);
							EmployeeXRec = (Xrecord)trans.GetObject(extDict.GetAt("EmployeeData"), OpenMode.ForRead,	false);
						}
						catch
						{
							bHasOurDict	= false;	//Something	bad	happened...our dictionary and/or XRecord is	not	accessible
						}
						if( bHasOurDict ) 	//if obtaining the Extension Dictionary, and our XRecord is	successful...
						{
							//Stretch the employee list	to fit three more entries...
							//Add Employee Name
							TypedValue resBuf = EmployeeXRec.Data.AsArray()[0];

							saEmployeeList.Add(String.Format("{0}"	+ "\n", resBuf.Value));
							nEmployeeDataCount += 1;

							//Add the Employee Salary
							resBuf = EmployeeXRec.Data.AsArray()[1];
							saEmployeeList.Add(String.Format("{0}"	+ "\n", resBuf.Value));
							nEmployeeDataCount += 1;

							//Add the Employee Division
							resBuf = EmployeeXRec.Data.AsArray()[2];
							String str	= (String)resBuf.Value;
							saEmployeeList.Add(String.Format("{0}"	+ "\n", resBuf.Value));
							nEmployeeDataCount += 1;


							//Now, we get the Boss// name from the corporate dictionary...
							//Dig into the NOD and get it.
							DBDictionary NOD =	(DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead, false);
							DBDictionary acmeDict = (DBDictionary)trans.GetObject(NOD.GetAt("ACME_DIVISION"), OpenMode.ForRead);
							//Notice we	use	the	XRecord	data directly...
							DBDictionary salesDict = (DBDictionary)trans.GetObject(acmeDict.GetAt(EmployeeXRec.Data.AsArray()[2].Value.ToString()), OpenMode.ForRead);
							Xrecord salesXRec = (Xrecord)trans.GetObject(salesDict.GetAt("Department Manager"),	OpenMode.ForRead);
							//Finally, write the employee//s supervisor	to the commandline
							resBuf = salesXRec.Data.AsArray()[0];
							saEmployeeList.Add(String.Format("{0}"	+ "\n", resBuf.Value));
							nEmployeeDataCount += 1;
						}
						
					}
				}
			}
		}

		[CommandMethod("PRINTOUTEMPLOYEE")]
		public void	PrintoutEmployee()
		{
			Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;

			//int nEmployeeCount= 0;

			//int nEmployeeDataCount;
			//Declare the tool//s we//ll use throughout...
			Database db = HostApplicationServices.WorkingDatabase;
			Transaction trans = db.TransactionManager.StartTransaction(); //Start the transaction.
			try
			{
				//First, get at	the	BlockTable,	and	the	ModelSpace BlockTableRecord
				BlockTable bt = (BlockTable)trans.GetObject(HostApplicationServices.WorkingDatabase.BlockTableId, OpenMode.ForRead);
				BlockTableRecord btr =	(BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace],	OpenMode.ForRead);
				//Now, we need to be able to print to the commandline.	Here is	an object which	will help us:

				//Now, here	is the fun part.  This is where	we iterate through ModelSpace:
				foreach( ObjectId id in btr )
				{
					Entity ent	= (Entity)trans.GetObject(id, OpenMode.ForRead,	false); //Use it	to open	the	current	object!
					if( ent.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.BlockReference") )	 //We use .NET//s RTTI to establish	type.
					{
						ArrayList saEmployeeList = new ArrayList(); // We use ArrayList which can have dynamic size in C#
						ListEmployee(id, saEmployeeList);
						//String sEmployeeData;

						if (saEmployeeList.Count ==	4) 
						{
							ed.WriteMessage("Employee Name:	{0}", saEmployeeList[0]);
							ed.WriteMessage("Employee Salary: {0}",	saEmployeeList[1]);
							ed.WriteMessage("Employee Division:	{0}", saEmployeeList[2]);
							ed.WriteMessage("Division Manager: {0}", saEmployeeList[3]);
						}
					}
				}
			}
			catch
			{
			}
		}


	} // class
} // namespace
