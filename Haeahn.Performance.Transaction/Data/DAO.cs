using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace Haeahn.Performance.Transaction
{
    class DAO
    {
        internal string GetConnectionString()
        {
            return String.Format("Data Source=.;Initial Catalog=Performance;Integrated Security=True;");
        }
        #region Insert
        internal void InsertTransactionLogs(IEnumerable<TransactionLog> transactionLogs)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    SqlCommand command = new SqlCommand("INSERT INTO transaction_log VALUES" +
                        "(@project_code, @project_name, @project_type, @element_id, @element_name, @category_type, @category_name, " +
                        "@view_type, @family_name, @type_name, @transaction_name, @employee_id, @employee_name, @department, @event_type, @occurred_on)", connection);

                    connection.ConnectionString = GetConnectionString();
                    connection.Open();

                    command.Parameters.Add("@project_code", SqlDbType.NVarChar);
                    command.Parameters.Add("@project_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@project_type", SqlDbType.NVarChar);
                    command.Parameters.Add("@element_id", SqlDbType.NVarChar);
                    command.Parameters.Add("@element_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@category_type", SqlDbType.NVarChar);
                    command.Parameters.Add("@category_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@view_type", SqlDbType.NVarChar);
                    command.Parameters.Add("@family_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@type_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@transaction_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@employee_id", SqlDbType.NVarChar);
                    command.Parameters.Add("@employee_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@department", SqlDbType.NVarChar);
                    command.Parameters.Add("@event_type", SqlDbType.NVarChar);
                    command.Parameters.Add("@occurred_on", SqlDbType.NVarChar);

                    foreach (var transactionLog in transactionLogs)
                    {
                        command.Parameters["@project_code"].Value = (object)transactionLog.ProjectCode ?? DBNull.Value;
                        command.Parameters["@project_name"].Value = (object)transactionLog.ProjectName ?? DBNull.Value;
                        command.Parameters["@project_type"].Value = (object)transactionLog.ProjectType ?? DBNull.Value;
                        command.Parameters["@element_id"].Value = (object)transactionLog.ElementId ?? DBNull.Value;
                        command.Parameters["@element_name"].Value = (object)transactionLog.ElementName ?? DBNull.Value;
                        command.Parameters["@category_type"].Value = (object)transactionLog.CategoryType ?? DBNull.Value;
                        command.Parameters["@category_name"].Value = (object)transactionLog.CategoryName ?? DBNull.Value;
                        command.Parameters["@view_type"].Value = (object)transactionLog.ViewType ?? DBNull.Value;
                        command.Parameters["@family_name"].Value = (object)transactionLog.FamilyName ?? DBNull.Value;
                        command.Parameters["@type_name"].Value = (object)transactionLog.TypeName ?? DBNull.Value;
                        command.Parameters["@transaction_name"].Value = (object)transactionLog.Transaction ?? DBNull.Value;
                        command.Parameters["@employee_id"].Value = (object)transactionLog.EmployeeId ?? DBNull.Value;
                        command.Parameters["@employee_name"].Value = (object)transactionLog.EmployeeName ?? DBNull.Value;
                        command.Parameters["@department"].Value = (object)transactionLog.Department ?? DBNull.Value;
                        command.Parameters["@event_type"].Value = (object)transactionLog.EventType ?? DBNull.Value;
                        command.Parameters["@occurred_on"].Value = (object)transactionLog.OccurredOn ?? DBNull.Value;

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
        #endregion
    }
}
