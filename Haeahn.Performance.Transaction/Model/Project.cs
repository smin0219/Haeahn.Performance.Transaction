using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Haeahn.Performance.Transaction.Model
{
    internal class Project
    {
        internal Project() {}
        internal Project(string name, string code, string type, bool isCentral, string centralFilePath) 
        {
            this.Name = name;  
            this.Code = code;
            this.Type = type;
            this.IsCentral = isCentral;
            this.CentralFilePath = centralFilePath;
        }
        internal Project(Autodesk.Revit.DB.ProjectInfo projectInfo, bool isCentral, string centralFilePath)
        {
            this.Name = projectInfo.Name;
            this.Code = projectInfo.Number.ToString();
            this.Type = "기획설계";
            this.IsCentral = isCentral;
            this.CentralFilePath = centralFilePath;
        }
        internal Project(Autodesk.Revit.DB.Document rvt_doc, bool isCentral, string centralFilePath)
        {
            if (rvt_doc.IsFamilyDocument)
            {
                this.Name = "RFA";
                this.Code = "RFA";
                this.Type = "RFA";
                this.IsCentral = isCentral;
                this.CentralFilePath = centralFilePath;
            }
            //프로젝트 파일
            else
            {
                ProjectInfo projectInformation = rvt_doc.ProjectInformation;
                if (projectInformation != null)
                {
                    this.Name = projectInformation.Name;
                    this.Code = projectInformation.Number;
                    this.Type = "기획설계";
                    this.IsCentral = isCentral;
                    this.CentralFilePath = centralFilePath;
                }
                else
                {
                    this.Name = "RFA";
                    this.Code = "RFA";
                    this.Type = "RFA";
                    this.IsCentral = isCentral;
                    this.CentralFilePath = centralFilePath;
                }
            }
        }
        internal string Name { get; set; }
        internal string Code { get; set; }
        internal string Type { get; set; }
        internal bool IsCentral { get; set; }
        internal string CentralFilePath { get; set; }
    }
}
