using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{ 
    public enum EventType
    {
        Opened,
        Closed,
        Created,
        Added,
        Deleted,
        Modified
    }
    public enum ViewType
    {
        View3D,
        Section,
        Sheet,
        Drafting,
        Normal
    }
}
