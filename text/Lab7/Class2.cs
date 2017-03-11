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
//Lab 7 code begins here

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;

namespace Lab7
{
	public class AsdkClass2
	{
		//Global variables
		bool bEditCommand;
		bool bDoRepositioning;  
		ObjectIdCollection changedObjects = new ObjectIdCollection();
		Point3dCollection employeePositions = new Point3dCollection();

		[CommandMethod("AddEvents")]
		public void plantDbEvents()
		{
			Database db;  
			Document doc;  
			//To avoid ambiguity, we have to use the full type here.
			doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
			db = HostApplicationServices.WorkingDatabase;
			db.ObjectOpenedForModify += new ObjectEventHandler(objOpenedForMod);
			doc.CommandWillStart += new CommandEventHandler(cmdWillStart);
			doc.CommandEnded += new CommandEventHandler(cmdEnded);
			bEditCommand = false;
			bDoRepositioning = false;
		}

		[CommandMethod("RemoveEvents")]
		public void removeDbEvents()
		{
			Database db ; 
			Document doc;  
			doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
			db = HostApplicationServices.WorkingDatabase;
			db.ObjectOpenedForModify -= new ObjectEventHandler(objOpenedForMod);
			doc.CommandEnded -= new CommandEventHandler(cmdEnded);
			doc.CommandWillStart -= new CommandEventHandler(cmdWillStart);
			bEditCommand = false;
			bDoRepositioning = false;
		}

		public void objOpenedForMod(object o, ObjectEventArgs e)
		{
			if ( bEditCommand == false ) 
			{
				return;
			}

			if ( bDoRepositioning == true ) 
			{
				return;
			}

			ObjectId objId; 
			objId = e.DBObject.ObjectId;

			Transaction  trans;  
			Database  db;  
			db = HostApplicationServices.WorkingDatabase;

			trans = db.TransactionManager.StartTransaction();

			using(Entity ent  = (Entity)trans.GetObject(objId, OpenMode.ForRead, false))
			{
				if ( ent.GetType().FullName.Equals( "Autodesk.AutoCAD.DatabaseServices.BlockReference" ) )
				{ //We use .NET//s RTTI to establish type.
					BlockReference br   = (BlockReference)ent;
					//Test whether it is an employee block
					//open its extension dictionary
					if ( br.ExtensionDictionary.IsValid ) 
					{
						using(DBDictionary brExtDict  = (DBDictionary)trans.GetObject(br.ExtensionDictionary, OpenMode.ForRead))
						{
							if ( brExtDict.GetAt("EmployeeData").IsValid ) 
							{
								//successfully got "EmployeeData" so br is employee block ref

								//Store the objectID and the position
								changedObjects.Add(objId);
								employeePositions.Add(br.Position);
								//Get the attribute references,if any
								AttributeCollection atts;  
								atts = br.AttributeCollection;
								if ( atts.Count > 0 ) 
								{
									foreach(ObjectId attId in atts )
									{
										AttributeReference att; 
										using(att = (AttributeReference)trans.GetObject(attId, OpenMode.ForRead, false))
										{
											changedObjects.Add(attId);
											employeePositions.Add(att.Position);
										}
									}
								}
							}
						}
					}
				}
			}
			trans.Commit();
		}

		public void cmdWillStart( object o  , CommandEventArgs e )
		{
			if ( e.GlobalCommandName == "MOVE" ) 
			{
				//Set the global variables
				bEditCommand = true;
				bDoRepositioning = false;
				//Delete all stored information
				changedObjects.Clear();
				employeePositions.Clear();
			}
		}

		public void cmdEnded(object o  , CommandEventArgs e)
		{
			//Was our monitored command active?
			if ( bEditCommand == false ) 
			{
				return;
			}

			bEditCommand = false;

			//Set flag to bypass OpenedForModify handler
			bDoRepositioning = true;

			Database db   = HostApplicationServices.WorkingDatabase;
			Transaction trans ; 
			BlockTable bt;  
			Point3d oldpos;  
			Point3d newpos;  
			int i ;
			for ( i = 0; i< changedObjects.Count; i++)
			{
				trans = db.TransactionManager.StartTransaction();
				using(	bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead) )
				{
					using(Entity ent   = (Entity)trans.GetObject(changedObjects[i], OpenMode.ForWrite))
					{
						if ( ent.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.BlockReference") ) 
						{ //We use .NET//s RTTI to establish type.
							BlockReference br = (BlockReference)ent;
							newpos = br.Position;
							oldpos = employeePositions[i];

							//Reset blockref position
							if ( !oldpos.Equals(newpos) ) 
							{
								using( trans.GetObject(br.ObjectId, OpenMode.ForWrite) )
								{
									br.Position = oldpos;
								}
							}
						}
						else if ( ent.GetType().FullName.Equals("Autodesk.AutoCAD.DatabaseServices.AttributeReference") ) 
						{
							AttributeReference att = (AttributeReference)ent;
							newpos = att.Position;
							oldpos = employeePositions[i];

							//Reset attref position
							if ( !oldpos.Equals(newpos) ) 
							{
								using( trans.GetObject(att.ObjectId, OpenMode.ForWrite))
								{
									att.Position = oldpos;
								}
							}
						}
					}
				}
				trans.Commit();
			}
		}
	} 
}
//End of Lab7 code