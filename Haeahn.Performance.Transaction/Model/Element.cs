using Autodesk.Revit.DB;
using Haeahn.Performance.Transaction.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction.Model
{
    public class Element
    {
        internal Element() { }
        internal Element(Autodesk.Revit.DB.Element rvt_element)
        {
            try
            {
                var rvt_doc = PerformanceApplication.rvt_doc;
                var projectInformation = rvt_doc.ProjectInformation;

                this.Id = rvt_element.Id.ToString();
                this.Name = rvt_element.Name;
                this.ProjectName = (projectInformation == null) ? null : projectInformation.Name;
                this.ProjectCode = (projectInformation == null) ? null : projectInformation.Number.ToString();
                this.CategoryName = (rvt_element.Category == null) ? string.Empty : rvt_element.Category.Name;
                this.CategoryType = (rvt_element.Category == null) ? string.Empty : rvt_element.Category.CategoryType.ToString();

                FamilyInstance familyInstance = rvt_element as FamilyInstance;
                ElementType type = rvt_doc.GetElement(rvt_element.GetTypeId()) as ElementType;

                if (type != null)
                {
                    this.FamilyName = type.FamilyName;
                    this.TypeName = type.Name;
                }
            }
            catch (Exception ex)
            {
                DAO dao = new DAO();
                var errorMsg = ex.Message + '\n' + ex.StackTrace;
                dao.InsertErrorLog(new ErrorLog(PerformanceApplication.project.Name, PerformanceApplication.employee.Id, errorMsg, DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"))));
            }
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        
    }
}
