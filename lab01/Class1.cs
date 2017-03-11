using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace lab01
{
    public class Lab01
    {
        [CommandMethod("HelloWorld")]
        public void Hello() {
            var ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage("Hellom World");
        }
    }
}
