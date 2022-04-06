using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Haeahn.Performance.Transaction.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class RegistrationViewModel : BaseViewModel
    {
        public RelayCommand CmdRun { get; set; }
        public RelayCommand CmdClose { get; set; }

        internal LoginResult _loginSsoResult;

        #region 프로퍼티
        public string UserId { get; set; }
        public string Password { get; set; }

        //public bool CanExecuteApplyCommand(string fileName)
        //{
        //    return !string.IsNullOrEmpty(fileName);
        //}
        #endregion
        #region 커맨드


        #endregion

        public RegistrationViewModel()
        {
            CmdRun = new RelayCommand(Run);
        }

        private void Run(object obj)
        {
            _loginSsoResult = LoginResult.LoginSsoFailed;
            var task = Task.Run(() => _loginSsoResult = WebAPIClient.LoginSso(UserId, Password).Result);
            task.Wait();

            if (LoginResult.LoginSsoSucceeded == _loginSsoResult)
            {
            }
            else
            {
                ShowErrorMessage("잘못된 사용자 이름 또는 비밀번호 입니다.\n다시 입력해주세요.");
            }
        }



        //#region 윈도우 닫기 명령 오버라이드, 로그인 성공하기 전까지 창 안 닫고 계속 시도할 수 있도록 처리
        //public override void Close(Window window, bool? result)
        //{
        //    try
        //    {
        //        if (true == result) // OkCommand에서 호출
        //        {
        //            ExecuteApply();

        //            if (LoginResult.LoginSsoSucceeded == _loginSsoResult)
        //            {
        //                // 윈도우 닫기
        //                window.DialogResult = true; // 
        //            }
        //        }
        //        else// CancelCommand에서 호출
        //        {
        //            _loginSsoResult = LoginResult.Cancelled;
        //            // 윈도우 닫기
        //            window.DialogResult = result;
        //        }
        //    }
        //    catch (InvalidOperationException)
        //    {
        //        window.Close();
        //    }
        //}
        //#endregion
    }
}
