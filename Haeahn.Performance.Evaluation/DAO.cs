using Haeahn.Performance.Transaction;
using Haeahn.Performance.Transaction.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Evaluation
{
    public class DAO
    {
        internal string GetConnectionString()
        {
            return String.Format("Data Source=.;Initial Catalog=Performance;Integrated Security=True;");
        }
        internal List<ElementLog> SelectTransactionLogs(string employeeId, DateTime from = default, DateTime to = default)
         {
            try
            {
                List<ElementLog> transactionLogs = new List<ElementLog>();  

                using (SqlConnection connection = new SqlConnection())
                {
                    var where = "WHERE employee_id = " + employeeId;

                    if(from != default || to != default)
                    {
                        where += "AND occurred_on >= from AND occurred_on <= to";
                    }

                    var query = "SELECT * FROM transaction_log " + where + " ORDER BY occurred_on DESC";
                    SqlCommand command = new SqlCommand(query, connection);

                    connection.ConnectionString = GetConnectionString();
                    connection.Open();

                    var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        ElementLog transactionLog = new ElementLog();
                        transactionLog.ProjectCode = reader["project_code"].ToString();
                        transactionLog.ProjectName = reader["project_name"].ToString();
                        transactionLog.ProjectType = reader["project_type"].ToString();
                        transactionLog.ElementId = reader["element_id"].ToString();
                        transactionLog.ElementName = reader["element_name"].ToString();
                        transactionLog.CategoryType = reader["category_type"].ToString();
                        transactionLog.CategoryName = reader["category_name"].ToString();
                        transactionLog.ViewType = reader["view_type"].ToString();
                        transactionLog.FamilyName = reader["family_name"].ToString();
                        transactionLog.TypeName = reader["type_name"].ToString();
                        transactionLog.TransactionName = reader["transaction_name"].ToString();
                        transactionLog.EmployeeId = reader["employee_id"].ToString();
                        transactionLog.EventType = reader["event_type"].ToString();
                        transactionLog.OccurredOn = reader["occurred_on"].ToString();

                        transactionLogs.Add(transactionLog);
                    }
                }
                return transactionLogs;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        
    }
}
