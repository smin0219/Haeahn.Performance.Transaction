using Haeahn.Performance.Transaction.Controller;
using Haeahn.Performance.Transaction.Model;
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
using System.Windows;
using System.Xml;


namespace Haeahn.Performance.Transaction.Data
{
    class DAO
    {
        internal string GetConnectionString()
        {
            return String.Format("Server=192.168.40.145;Database=EKP_BIM;User Id=BimUser;Password=@summer2014;");
        }
        #region INSERT
        internal void InsertElementLogs(IEnumerable<ElementLog> elementLogs)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    SqlCommand command = new SqlCommand("INSERT INTO TB_PERFORMANCE_ELEMENT_LOG VALUES" +
                        "(@project_code, @project_name, @project_type, @element_id, @element_name, @category_type, @category_name, " +
                        "@view_type, @family_name, @type_name, @transaction_name, @employee_id, @event_type, @occurred_on, @is_central, @file_path)", connection);

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
                    command.Parameters.Add("@event_type", SqlDbType.NVarChar);
                    command.Parameters.Add("@occurred_on", SqlDbType.NVarChar);
                    command.Parameters.Add("@is_central", SqlDbType.Bit);
                    command.Parameters.Add("@file_path", SqlDbType.NVarChar);

                    foreach (var elementLog in elementLogs)
                    {
                        command.Parameters["@project_code"].Value = (object)elementLog.ProjectCode ?? DBNull.Value;
                        command.Parameters["@project_name"].Value = (object)elementLog.ProjectName ?? DBNull.Value;
                        command.Parameters["@project_type"].Value = (object)elementLog.ProjectType ?? DBNull.Value;
                        command.Parameters["@element_id"].Value = (object)elementLog.ElementId ?? DBNull.Value;
                        command.Parameters["@element_name"].Value = (object)elementLog.ElementName ?? DBNull.Value;
                        command.Parameters["@category_type"].Value = (object)elementLog.CategoryType ?? DBNull.Value;
                        command.Parameters["@category_name"].Value = (object)elementLog.CategoryName ?? DBNull.Value;
                        command.Parameters["@view_type"].Value = (object)elementLog.ViewType ?? DBNull.Value;
                        command.Parameters["@family_name"].Value = (object)elementLog.FamilyName ?? DBNull.Value;
                        command.Parameters["@type_name"].Value = (object)elementLog.TypeName ?? DBNull.Value;
                        command.Parameters["@transaction_name"].Value = (object)elementLog.TransactionName ?? DBNull.Value;
                        command.Parameters["@employee_id"].Value = (object)elementLog.EmployeeId ?? DBNull.Value;
                        command.Parameters["@event_type"].Value = (object)elementLog.EventType ?? DBNull.Value;
                        command.Parameters["@occurred_on"].Value = (object)elementLog.OccurredOn ?? DBNull.Value;
                        command.Parameters["@is_central"].Value = (object)elementLog.IsCentral ?? DBNull.Value;
                        command.Parameters["@file_path"].Value = (object)elementLog.FilePath ?? DBNull.Value;

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        internal void InsertTransactionLog(TransactionLog transactionLog)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    SqlCommand command = new SqlCommand("INSERT INTO TB_PERFORMANCE_TRANSACTION_LOG VALUES" +
                        "(@project_code, @employee_id, @transaction_name,@occurred_on)", connection);

                    connection.ConnectionString = GetConnectionString();
                    connection.Open();

                    command.Parameters.Add("@project_code", SqlDbType.NVarChar);
                    command.Parameters.Add("@employee_id", SqlDbType.NVarChar);
                    command.Parameters.Add("@transaction_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@occurred_on", SqlDbType.NVarChar);

                    command.Parameters["@project_code"].Value = (object)transactionLog.ProjectCode ?? DBNull.Value;
                    command.Parameters["@employee_id"].Value = (object)transactionLog.EmployeeId ?? DBNull.Value;
                    command.Parameters["@transaction_name"].Value = (object)transactionLog.TransactionName ?? DBNull.Value;
                    command.Parameters["@occurred_on"].Value = (object)transactionLog.OccurredOn ?? DBNull.Value;

                    command.ExecuteNonQuery();
                    
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        internal void InsertErrorLog(ErrorLog errorLog)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    SqlCommand command = new SqlCommand("INSERT INTO TB_PERFORMANCE_ERROR_LOG VALUES" +
                        "(@project_code, @employee_id, @description,@occurred_on)", connection);

                    connection.ConnectionString = GetConnectionString();
                    connection.Open();

                    command.Parameters.Add("@project_code", SqlDbType.NVarChar);
                    command.Parameters.Add("@description", SqlDbType.NVarChar);
                    command.Parameters.Add("@employee_id", SqlDbType.NVarChar);
                    command.Parameters.Add("@occurred_on", SqlDbType.NVarChar);

                   
                    command.Parameters["@project_code"].Value = (object)errorLog.ProjectCode ?? DBNull.Value;
                    command.Parameters["@description"].Value = (object)errorLog.Description ?? DBNull.Value;
                    command.Parameters["@employee_id"].Value = (object)errorLog.EmployeeId ?? DBNull.Value;
                    command.Parameters["@occurred_on"].Value = (object)errorLog.OccurredOn ?? DBNull.Value;

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("IT 연구소 [민성재: 7852]로 연락 부탁드립니다.");
            }
        }
        internal void InsertWarnings(IEnumerable<Warning> warnings)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                 {
                    connection.ConnectionString = GetConnectionString();
                    connection.Open();

                    string deleteQueryString = string.Format("DELETE FROM TB_PERFORMANCE_WARNING WHERE project_code='{0}'", PerformanceApplication.project.Code);
                    SqlCommand deleteCmd = new SqlCommand(deleteQueryString, connection);
                    deleteCmd.ExecuteNonQuery();

                    SqlCommand insertCmd = new SqlCommand("INSERT INTO TB_PERFORMANCE_WARNING VALUES" +
                        "(@project_code, @employee_id, @description, @severity, @element_ids, @occurred_on)",  connection);
                    insertCmd.Parameters.Add("@project_code", SqlDbType.NVarChar);
                    insertCmd.Parameters.Add("@employee_id", SqlDbType.NVarChar);
                    insertCmd.Parameters.Add("@description", SqlDbType.NVarChar);
                    insertCmd.Parameters.Add("@severity", SqlDbType.NVarChar);
                    insertCmd.Parameters.Add("@element_ids", SqlDbType.NVarChar);
                    insertCmd.Parameters.Add("@occurred_on", SqlDbType.NVarChar);

                    foreach (var warning in warnings)
                    {
                        insertCmd.Parameters["@project_code"].Value = (object)warning.ProjectCode ?? DBNull.Value;
                        insertCmd.Parameters["@employee_id"].Value = (object)warning.EmployeeId ?? DBNull.Value;
                        insertCmd.Parameters["@description"].Value = (object)warning.Description ?? DBNull.Value;
                        insertCmd.Parameters["@severity"].Value = (object)warning.Severity ?? DBNull.Value;
                        insertCmd.Parameters["@element_ids"].Value = (object)warning.ElementIds ?? DBNull.Value;
                        insertCmd.Parameters["@occurred_on"].Value = (object)warning.OccurredOn ?? DBNull.Value;

                        insertCmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        #endregion
        #region
        internal string GetLatestVersionNumber()
        {
            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = GetConnectionString();
                connection.Open();
                string selectQueryString = "SELECT version_number FROM TB_PERFORMANCE_VERSION WHERE is_update = 1";
                SqlCommand selectCmd = new SqlCommand(selectQueryString, connection);

                var reader = selectCmd.ExecuteReader();

                string versionNumber = "";

                while (reader.Read())
                {
                    versionNumber = reader[0].ToString();
                }

                return versionNumber;
            }
        }
        #endregion
    }
}
