﻿using Haeahn.Performance.Revit;
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


namespace Haeahn.Performance.Revit
{ 
    class DAO
    {
        internal string GetConnectionString()
        {
            return String.Format("Data Source=.;Initial Catalog=Performance;Integrated Security=True;");
        }

        #region INSERT
        internal void InsertProjectIntoDB(Project project)
        {
            var currentDateTime = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
            EmployeeController employeeController = new EmployeeController();
            Employee employee = employeeController.GetEmployee("20210916");

            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    SqlCommand command = new SqlCommand("INSERT INTO project VALUES (@code, @name, @created_on, @created_by)", connection);
                    command.Parameters.Add("@code", SqlDbType.NVarChar).Value = (object)project.Code ?? DBNull.Value;
                    command.Parameters.Add("@name", SqlDbType.NVarChar).Value = (object)project.Name ?? DBNull.Value;
                    command.Parameters.Add("@created_on", SqlDbType.NVarChar).Value = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
                    command.Parameters.Add("@created_by", SqlDbType.NVarChar).Value = (object)employee.Name ?? DBNull.Value;

                    connection.ConnectionString = GetConnectionString();
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
        internal void InsertElementsIntoDB(IEnumerable<Element> elements)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    SqlCommand command = new SqlCommand("INSERT INTO element VALUES" +
                        "(@id, @name, @project_code, @project_name, @category_name, @family_name, @type_name, " +
                        "@location, @geometry, @verticies, @bounding_box, @instance_parameter, @type_parameter)", connection);

                    command.Parameters.Add("@id", SqlDbType.NVarChar);
                    command.Parameters.Add("@name", SqlDbType.NVarChar);
                    command.Parameters.Add("@project_code", SqlDbType.NVarChar);
                    command.Parameters.Add("@project_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@category_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@family_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@type_name", SqlDbType.NVarChar);
                    command.Parameters.Add("@location", SqlDbType.NVarChar);
                    command.Parameters.Add("@geometry", SqlDbType.NVarChar);
                    command.Parameters.Add("@verticies", SqlDbType.NVarChar);
                    command.Parameters.Add("@bounding_box", SqlDbType.NVarChar);
                    command.Parameters.Add("@instance_parameter", SqlDbType.NVarChar);
                    command.Parameters.Add("@type_parameter", SqlDbType.NVarChar);

                    connection.ConnectionString = GetConnectionString();
                    connection.Open();

                    foreach (var element in elements)
                    {
                        command.Parameters["@id"].Value = (object)element.Id ?? DBNull.Value;
                        command.Parameters["@name"].Value = (object)element.Name ?? DBNull.Value;
                        command.Parameters["@project_code"].Value = (object)element.ProjectCode ?? DBNull.Value;
                        command.Parameters["@project_name"].Value = (object)element.ProjectName ?? DBNull.Value;
                        command.Parameters["@category_name"].Value = (object)element.CategoryName ?? DBNull.Value;
                        command.Parameters["@family_name"].Value = (object)element.FamilyName ?? DBNull.Value;
                        command.Parameters["@type_name"].Value = (object)element.TypeName ?? DBNull.Value;
                        command.Parameters["@location"].Value = (object)element.Location ?? DBNull.Value;
                        command.Parameters["@geometry"].Value = (object)element.Geometry ?? DBNull.Value;
                        command.Parameters["@verticies"].Value = (object)element.Verticies ?? DBNull.Value;
                        command.Parameters["@bounding_box"].Value = (object)element.BoundingBox ?? DBNull.Value;
                        command.Parameters["@instance_parameter"].Value = (object)element.InstanceParameter ?? DBNull.Value;
                        command.Parameters["@type_parameter"].Value = (object)element.TypeParameter ?? DBNull.Value;
                        command.ExecuteNonQuery();
                    }

                    
                }
            }
            catch(Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
            }
        }
        internal void InsertTransactionsIntoDB(IEnumerable<Transaction> transactions, EventType eventType)
        {

        }
        internal void InsertSession(Session session)
        {

        }
        #endregion

        #region SELECT
        internal string SelectProjectCodeFromDB(Project project)
        {
            try
            { 
                using(SqlConnection connection = new SqlConnection())
                {
                    SqlCommand command = new SqlCommand("SELECT * FROM project WHERE code = @code", connection);
                    command.Parameters.Add("@code", SqlDbType.NVarChar).Value = (object)project.Code ?? DBNull.Value;
                    
                    connection.ConnectionString = GetConnectionString();
                    connection.Open();

                    var reader = command.ExecuteReader();
                    string projectCode = null;

                    while (reader.Read())
                    {
                        projectCode = reader["code"].ToString();
                    }
                    return projectCode;
                }
            }
            catch(Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return null;
            }
        }
        internal IEnumerable<Element> SelectAllElementsFromDB(Project project)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection())
                {
                    SqlCommand command = new SqlCommand("SELECT * FROM element WHERE project_code = @project_code", connection);
                    command.Parameters.Add("@project_code", SqlDbType.NVarChar).Value = (object)project.Code ?? DBNull.Value;

                    connection.ConnectionString = GetConnectionString();
                    connection.Open();

                    var reader = command.ExecuteReader();

                    List<Element> elements = new List<Element>();

                    while (reader.Read())
                    {
                        Element element = new Element();
                        element.Id = reader["id"].ToString();
                        element.Name = reader["name"].ToString();
                        element.ProjectCode = reader["project_code"].ToString();
                        element.ProjectName = reader["project_name"].ToString();
                        element.CategoryName = reader["category_name"].ToString();
                        element.FamilyName = reader["family_name"].ToString();
                        element.TypeName = reader["type_name"].ToString();
                        element.Location = reader["location"].ToString();
                        element.Geometry = reader["geometry"].ToString();
                        element.Verticies = reader["verticies"].ToString();
                        element.BoundingBox = reader["bounding_box"].ToString();
                        element.InstanceParameter = reader["instance_parameter"].ToString();
                        element.TypeParameter = reader["type_parameter"].ToString();
                        elements.Add(element);
                    }

                    return elements;
                }
            }
            catch (Exception ex)
            {
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
                return null;
            }
        }
        #endregion

    }
}
