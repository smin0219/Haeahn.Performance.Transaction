using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Haeahn.Performance.Transaction.Model;
using Haeahn.Performance.Transaction.View;
using Haeahn.Performance.Transaction.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Haeahn.Performance.Transaction
{
    /**
            모든 커맨드 호출하기에 앞서 사용자 인증 절차 거침

            1. 사용자 PC 레지스트리에 등록된 UUID가 있는지 확인

            2. UUID가 있을 경우 > POST /api/uuidlogin 호출
            2.1 호출 결과 resultCode == 0 (성공) > 반환된 인사정보 연동, 애드인 변수에 저장 
            2.2 호출 결과 resultCode != 0 (실패) > 3번으로 이동

            3. UUID가 없을 경우 > POST /api/loginsso SSO 로그인 API 호출(RegistrationWindow 띄움)
            3.1 EMP_NO 반환되면 POST /api/huserlogin 호출 (AutoDesk ID가 있으면 입력, 없으면 생략)
            4. UUID 반환되면 레지스트리에 등록하고 POST /api/uuidlogin 호출
            4.1 호출 결과 resultCode == 0 (성공) > 반환된 인사정보 연동, 애드인 변수에 저장 
            4.2 호출 결과 resultCode != 0 (실패) > RegistrationWindow에서 계속 사용자가 로그인 시도..

            애드인에서 이미 버튼을 한 번 이상 클릭한 상태라면 CurrentUser에 이미 UUID가 할당돼 있으므로 미리 체크
            할당돼 있으면 바로 상속받은 각 커맨드 실행
        **/

    internal class Authentication
    {
        protected UIApplication _uiApp;
        protected UIDocument _uiDoc;
        protected Document _doc;

        Employee employee;

        protected CommandType CommandType { get; set; }

        private const string REGISTRY_KEY = @"Software\PROC";
        private const string REGISTRY_KEY_UUID = "UUID";
        
        public Tuple<string, Employee> Login()
        {
            try
            {
                // 애드인에서 이미 버튼을 한 번 이상 클릭한 상태라면 CurrentUser에 이미 UUID가 할당돼 있으므로 미리 체크
                // 할당돼 있으면 바로 상속받은 각 커맨드 실행
                if (!string.IsNullOrWhiteSpace(CurrentUser.Instance.Uuid))
                {
                    employee = new Employee(CurrentUser.Instance.UserName, CurrentUser.Instance.EmpNo, CurrentUser.Instance.DeptName);
                    return Tuple.Create("succeeded", employee);
                }

                //1.사용자 PC 레지스트리에 등록된 UUID가 있는지 확인
                ReadUuidFromWindowsRegistry();

                LoginResult loginResult;

                // 2. UUID가 있을 경우 > POST /api/uuidlogin 호출
                if (!string.IsNullOrWhiteSpace(CurrentUser.Instance.Uuid)) 
                
                {
                    // 2.1 호출 결과 resultCode == 0 (성공) > 반환된 인사정보 연동, 애드인 변수에 저장
                    if (LoginResult.UuidLoginSucceeded == CallUuidLoginApi()) // 2.1 호출 결과 resultCode  == 0 (성공)이면 바로 본 커맨드(Execute()) 실행
                    {
                        loginResult = LoginResult.UuidLoginSucceeded;
                    }
                    // 2.2 호출 결과 resultCode != 0 (실패) > 3번으로 이동
                    else 
                    {
                        // 3. UUID가 없을 경우 > POST /api/loginsso SSO 로그인 API 호출(RegistrationWindow 띄움)
                        loginResult = LaunchRegistrationWindow();
                    }
                }
                // 3. UUID가 없을 경우 > POST /api/loginsso SSO 로그인 API 호출
                else
                {
                    loginResult = LaunchRegistrationWindow();

                }

                // 로그인 성공하면 상속받은 각 커맨드 실행
                switch (loginResult)
                {
                    case LoginResult.UuidLoginSucceeded:
                        employee = new Employee(CurrentUser.Instance.UserName, CurrentUser.Instance.EmpNo, CurrentUser.Instance.DeptName);
                        return Tuple.Create("succeeded", employee);
                    case LoginResult.LoginSsoSucceeded:
                        employee = new Employee(CurrentUser.Instance.UserName, CurrentUser.Instance.EmpNo, CurrentUser.Instance.DeptName);
                        return Tuple.Create("succeeded", employee);
                    case LoginResult.LoginSsoFailed:
                        return Tuple.Create("failed", new Employee());
                    case LoginResult.Cancelled:
                        return Tuple.Create("cancelled", new Employee());
                    default:
                        throw new Exception(loginResult.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Tuple.Create("error", new Employee());
            }
        }

        #region 윈도우 레지스트리 읽기/쓰기
        public void ReadUuidFromWindowsRegistry()
        {
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, RegistryKeyPermissionCheck.ReadSubTree);
            if (registryKey != null)
            {
                object value64 = registryKey.GetValue(REGISTRY_KEY_UUID);
                if (value64 != null)
                {
                    registryKey.Close();
                    CurrentUser.Instance.Uuid = value64.ToString();
                    return;
                }
                registryKey.Close();
            }
        }

        public void WriteUuidToWindowsRegistry(string uuid)
        {
            RegistryKey baseRegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
            if (registryKey == null)
            {
                RegistrySecurity registrySecurity = new RegistrySecurity();
                string user = Environment.UserDomainName + "\\" + Environment.UserName;

                registrySecurity.AddAccessRule(new RegistryAccessRule(user,
                            RegistryRights.WriteKey | RegistryRights.ReadKey | RegistryRights.Delete,
                            InheritanceFlags.None,
                            PropagationFlags.None,
                            AccessControlType.Allow));

                registryKey = Registry.CurrentUser.CreateSubKey(REGISTRY_KEY,
                            RegistryKeyPermissionCheck.Default, registrySecurity);
            }

            registryKey.SetValue(REGISTRY_KEY_UUID, uuid);
            registryKey.Close();
        }
        #endregion

        #region POST /api/uuidlogin 호출
        private LoginResult CallUuidLoginApi()
        {
            // 호출 결과 resultCode == 0 (성공) > 반환된 인사정보 연동, 애드인 변수에 저장
            var task = Task.Run(() => WebAPIClient.UuidLogin());

            // 아래에서 성공하면 반환된 인사정보 CurrentUser.Instance에 저장
            // 실패하면 RegistrationWindow에서 계속 사용자가 로그인 시도..
            task.Wait();

            return task.Result;
        }
        #endregion

        #region POST /api/loginsso SSO 로그인 API 호출
        private LoginResult LaunchRegistrationWindow()
        {
            // 3. UUID가 없을 경우 > POST /api/loginsso SSO 로그인 API 호출(RegistrationWindow 띄움)
            // RegistrationWindow 띄워서 사용자 ID, PW 받음
            var viewModel = new RegistrationViewModel();
            var registrationWindow = new RegistrationWindow() { DataContext = viewModel };
            var loginSsoResult = registrationWindow.ShowDialog();
            if (viewModel._loginSsoResult == LoginResult.LoginSsoSucceeded) // RegistrationWindow에서 OkCommand 실행('Connect' 버튼 클릭)
            {
                // 3.1 EMP_NO 반환되면 POST /api/huserlogin 호출 (AutoDesk ID가 있으면 입력, 없으면 생략)
                if (!string.IsNullOrWhiteSpace(CurrentUser.Instance.EmpNo))
                {
                    //string autodeskUserId = _uiApp.Application.LoginUserId;
                    //CurrentUser.Instance.AutodeskUserId = autodeskUserId;

                    // 사용자 사번과 오토데스트 Id를 해안 DB에 등록하고(오토데스트 Id는 등록만 하고 사용은 안 함, 추후 사용을 위해)
                    // 사용자 사번으로 UUID를 반환 받음
                    LoginResult huserLoginResult = LoginResult.HUserLoginFailed;
                    var firstTask = Task.Factory.StartNew(() => huserLoginResult = WebAPIClient.HUserLogin().Result);

                    var continuationTask = firstTask.ContinueWith((antecedent) =>
                    {
                        if (LoginResult.HUserLoginSucceeded == huserLoginResult)
                        {
                            // 4. UUID 반환되면 레지스트리에 등록하고 POST /api/uuidlogin 호출
                            if (!string.IsNullOrWhiteSpace(CurrentUser.Instance.Uuid))
                            {
                                WriteUuidToWindowsRegistry(CurrentUser.Instance.Uuid);

                                return CallUuidLoginApi();
                            }
                            else // UUID 반환 안 되면 false 반환
                            {
                                return LoginResult.HUserLoginFailed;
                            }
                        }
                        else
                        {
                            return LoginResult.HUserLoginFailed;
                        }
                    });

                    continuationTask.Wait();

                    return continuationTask.Result;
                }
                else // EMP_NO 반환 안 되면 false 반환
                {
                    return LoginResult.LoginSsoFailed;
                }
            }
            else if(viewModel._loginSsoResult == LoginResult.LoginSsoFailed)
            {
                return LoginResult.LoginSsoFailed;
            }
            else
            {
                return LoginResult.Cancelled;
            }
        }
        #endregion
    }
}
