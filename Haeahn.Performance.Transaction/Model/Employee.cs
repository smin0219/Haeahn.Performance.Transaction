using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    class Employee
    {
        internal Employee() { }
        internal Employee(string employeeId)
        {
            this.Name = "sj.min";
            this.Id = employeeId;
            this.Department = "IT연구실";
        }
        //EmployeeId = 사원번호
        internal string Id { get; set; }
        //EmployeeName = 사원이름
        internal string Name { get; set; }
        //Department = 부서
        internal string Department { get; set; }

        internal Employee GetEmployee(string employeeId)
        {
            // 모든 커맨드 호출하기에 앞서 사용자 인증 절차 거침

            // 1. 사용자 PC 레지스트리에 등록된 UUID가 있는지 확인

            // 2. UUID가 있을 경우 > POST /api/uuidlogin 호출
            // 2.1 호출 결과 resultCode == 0 (성공) > 반환된 인사정보 연동, 애드인 변수에 저장 
            // 2.2 호출 결과 resultCode != 0 (실패) > 3번으로 이동

            // 3. UUID가 없을 경우 > POST /api/loginsso SSO 로그인 API 호출(RegistrationWindow 띄움)
            // 3.1 EMP_NO 반환되면 POST /api/huserlogin 호출 (AutoDesk ID가 있으면 입력, 없으면 생략)
            // 4. UUID 반환되면 레지스트리에 등록하고 POST /api/uuidlogin 호출
            // 4.1 호출 결과 resultCode == 0 (성공) > 반환된 인사정보 연동, 애드인 변수에 저장 
            // 4.2 호출 결과 resultCode != 0 (실패) > RegistrationWindow에서 계속 사용자가 로그인 시도..


            // 애드인에서 이미 버튼을 한 번 이상 클릭한 상태라면 CurrentUser에 이미 UUID가 할당돼 있으므로 미리 체크
            // 할당돼 있으면 바로 상속받은 각 커맨드 실행
            return null;
        }

        internal string GetUUID()
        {
            return null;
        }

        #region 윈도우 레지스트리 읽기/쓰기
        //public void ReadUuidFromWindowsRegistry()
        //{
        //    RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, RegistryKeyPermissionCheck.ReadSubTree);
        //    if (registryKey != null)
        //    {
        //        object value64 = registryKey.GetValue(REGISTRY_KEY_UUID);
        //        if (value64 != null)
        //        {
        //            registryKey.Close();
        //            CurrentUser.Instance.Uuid = value64.ToString();
        //            return;
        //        }
        //        registryKey.Close();
        //    }
        //}

        #endregion
    }
}
