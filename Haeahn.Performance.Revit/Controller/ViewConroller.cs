using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    class ViewConroller
    {
        internal string GetViewType(Autodesk.Revit.DB.View activeView)
        {
            string viewType = default;

            if (activeView is Autodesk.Revit.DB.View3D)
            {
                viewType = "3D View";
            }
            else if (activeView is Autodesk.Revit.DB.ViewSection)
            {
                viewType = "Section View";
            }
            else if (activeView is Autodesk.Revit.DB.ViewSheet)
            {
                viewType = "Sheet View";
            }
            else if (activeView is Autodesk.Revit.DB.ViewDrafting)
            {
                viewType = "Drafting View";
            }
            else
            {
                viewType = "Normal View";
            }

            return viewType;
        }
    }
}
