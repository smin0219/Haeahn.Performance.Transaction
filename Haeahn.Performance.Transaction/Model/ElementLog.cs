using Haeahn.Performance.Transaction.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction.Model
{
    //Model과 Annotation 객체 정보를 위한 모델
    public class ElementLog
    {
        public ElementLog() { }
        internal ElementLog(Project project, Employee employee, ViewType viewType, Haeahn.Performance.Transaction.EventType eventType, Element element = null)
        {
            this.ProjectCode = project.Code;
            this.ProjectName = project.Name;
            this.ProjectType = project.Type;
            this.ElementId = (element.Id != null) ? element.Id.ToString() : null;
            this.ElementName = (element.Name != null) ? element.Name.ToString() : null;
            this.CategoryType = (element.CategoryType != null) ? element.CategoryType : null;
            this.CategoryName = (element.CategoryName != null) ? element.CategoryName : null;
            this.FamilyName = (element.FamilyName != null) ? element.FamilyName : null;
            this.TypeName = (element.TypeName != null) ? element.TypeName : null;
            this.ViewType = viewType.ToString();
            this.EmployeeId = employee.Id;
            this.EventType = eventType.ToString();
            this.OccurredOn = DateTime.Now.ToString("yyyyMMdd HH:mm:ss tt", CultureInfo.CreateSpecificCulture("en-US"));
            this.IsCentral = project.IsCentral;
            this.FilePath = project.FilePath;
        }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string ProjectType { get; set; }
        public string ElementId { get; set; }
        public string ElementName { get; set; }
        public string CategoryType { get; set; }
        public string CategoryName { get; set; }
        public string ViewType { get; set; }
        public string FamilyName { get; set; }
        public string TypeName { get; set; }
        public string TransactionName { get; set; }
        public string EmployeeId { get; set; }
        public string EventType { get; set; }
        public string OccurredOn { get; set; }
        public bool IsCentral { get; set; }
        public string FilePath { get; set; }  
    }
}
