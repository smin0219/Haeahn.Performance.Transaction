using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Revit
{
    public enum TaskType
    {
        Annotation,
        Model,
        Others
    }

    public enum EventType
    {
        Added,
        Deleted,
        Modified
    }
}
