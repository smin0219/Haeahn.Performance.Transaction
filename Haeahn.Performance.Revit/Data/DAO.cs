using Haeahn.Performance.Revit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace Haeahn.Performance.Revit
{ 
    class DAO
    {
        internal SqlConnection GetConnectionString()
        {
            return new SqlConnection(ConfigurationManager.ConnectionStrings["dev"].ConnectionString);
        }

        internal void Insert(IEnumerable<ElementState> elementStates)
        {
        }

        internal void Insert(IEnumerable<Transaction> transactions, EventType eventType)
        {

        }

        internal void Insert(Session session)
        {

        }

        internal void Insert(Project project)
        {

        }
    }
}
