using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    class Element
    {
        internal Element() { }
        internal Element(Autodesk.Revit.DB.Element rvt_element)
        {
            try
            {
                var rvt_doc = ExternalApplication.rvt_doc;
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
                Debug.Assert(false, ex.ToString());
                Log.WriteToFile(ex.ToString());
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
        internal Autodesk.Revit.DB.ElementFilter GetElementFilterByCategoryTypes(List<Autodesk.Revit.DB.CategoryType> categoryTypes)
        {
            List<Autodesk.Revit.DB.ElementFilter> elementFilters = new List<Autodesk.Revit.DB.ElementFilter>();
            Autodesk.Revit.DB.Categories categories = ExternalApplication.rvt_doc.Settings.Categories;

            foreach (Autodesk.Revit.DB.Category category in categories)
            {
                if (categoryTypes.Contains(category.CategoryType))
                {
                    elementFilters.Add(new Autodesk.Revit.DB.ElementCategoryFilter(category.Id));
                }
            }
            return new Autodesk.Revit.DB.LogicalOrFilter(elementFilters);
        }
    }
}
