using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    internal class View
    {
        internal View()
        {
        }

        internal ViewType GetViewType(Autodesk.Revit.DB.View activeView)
        {
            if (activeView is Autodesk.Revit.DB.View3D)
            {
                return ViewType.View3D;
            }
            else if (activeView is Autodesk.Revit.DB.ViewSection)
            {
                return ViewType.Section;
            }
            else if (activeView is Autodesk.Revit.DB.ViewSheet)
            {
                return ViewType.Sheet;
            }
            else if (activeView is Autodesk.Revit.DB.ViewDrafting)
            {
                return ViewType.Drafting;
            }
            else
            {
                return ViewType.Normal;
            }
        }
    }
}
