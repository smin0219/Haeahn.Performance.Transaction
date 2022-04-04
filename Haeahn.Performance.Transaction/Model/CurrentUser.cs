using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction.Model
{
    internal class CurrentUser
    {
        #region singleton
        private static CurrentUser instance = null;

        private CurrentUser()
        {
        }

        public static CurrentUser Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CurrentUser();
                }
                return instance;
            }
        }
        #endregion

        public string Uuid { get; set; }
        public string AutodeskUserId { get; set; }
        public string EmpNo { get; set; }
        public string UserName { get; set; }
        public string DeptName { get; set; }
        public string TitleName { get; set; }
        public string Email { get; set; }
    }
}
