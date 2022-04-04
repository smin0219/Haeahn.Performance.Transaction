using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Haeahn.Performance.Transaction.Model;
using Haeahn.Performance.Transaction.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace Haeahn.Performance.Transaction.ViewModel
{
    [POCOViewModel]
    public class BaseViewModel : INotifyPropertyChanged, ISupportServices
    {
        #region INotifyPropertyChanged 멤버
        public void RaisePropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region 멤버 변수
        protected UIApplication _uiApp;
        protected UIDocument _uiDoc;
        protected Document _doc;

        protected bool _runCommandExecuted;
        protected bool _runCommandExecutedOpt;

        protected delegate void ProgressBarDelegate();

        #endregion

        #region 프로퍼티
        protected System.Windows.Window _window;
        public virtual System.Windows.Window Window
        {
            get { return _window; }
            set
            {
                if (value != _window)
                {
                    _window = value;
                    RaisePropertyChanged();
                }
            }
        }

        public System.Windows.Controls.ProgressBar ProgressBar { get; set; }

        private double _maxProgress;
        public double MaxProgress
        {
            get { return _maxProgress; }
            set
            {
                if (value != _maxProgress)
                {
                    _maxProgress = value;
                    RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region 생성자
        protected BaseViewModel()
        {
            _runCommandExecuted = false;

            // _runCommandExecuted는 Apply 또는 Ok 버튼 클릭을 통해 RunCommand가 실행될 때 true로 변경되는 플래그
            // RunCommand가 실행된 이후더라도 뷰모델에 변화가 생겼을 때는 _runCommandExecuted 플래그 false로 다시 바꿔서 RunCommand 재실행되도록 처리
            PropertyChanged += new PropertyChangedEventHandler((o, e) => _runCommandExecuted = false);
        }

        protected BaseViewModel(UIApplication uiApp, UIDocument uiDoc, Document doc)
        {
            _uiApp = uiApp;
            _uiDoc = uiDoc;
            _doc = doc;

            MaxProgress = 1;
        }
        #endregion

        #region 윈도우 닫기 커맨드
        protected DelegateCommand<Window> _okCommand;
        public virtual DelegateCommand<Window> OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new DelegateCommand<Window>(
              x => Close(x as Window, true),
              x => CanExecuteApply()));
            }
        }

        protected DelegateCommand<Window> _createCommand;
        public virtual DelegateCommand<Window> CreateCommand
        {
            get
            {
                return _createCommand ?? (_createCommand = new DelegateCommand<Window>(
              x => Create(x as Window, true),
              x => CanExecuteApply()));
            }
        }


        protected DelegateCommand<Window> _updateCommand;
        public virtual DelegateCommand<Window> UpdateCommand
        {
            get
            {
                return _updateCommand ?? (_updateCommand = new DelegateCommand<Window>(
              x => Update(x as Window, true),
              x => CanExecuteApply()));
            }
        }

        protected DelegateCommand<Window> _okCommandOpt;
        public virtual DelegateCommand<Window> OkCommandOpt
        {
            get
            {
                return _okCommandOpt ?? (_okCommandOpt = new DelegateCommand<Window>(
              x => CloseOpt(x as Window, true),
              x => CanExecuteApplyOpt()));
            }
        }

        protected DelegateCommand<Window> _cancelCommand;
        public virtual DelegateCommand<Window> CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new DelegateCommand<Window>(
              x => Close(x as Window, false)));
            }
        }

        public virtual void Close(Window window, bool? result)
        {
            try
            {
                if (true == result) // OkCommand에서 호출
                {
                    if (_runCommandExecuted)
                    {
                        window.DialogResult = true;
                    }
                    else
                    {
                        if (ExecuteApply())
                        {
                            window.DialogResult = true;
                        }
                        else // 실패하면 일단 윈도우 닫지 말자.
                        {
                            ShowWarningMessage(ErrorMessage);
                        }
                    }
                }
                else if (false == result) // CancelCommand에서 호출
                {
                    if (_runCommandExecuted)
                    {
                        var confirmResult = ShowConfirmMessage("변경된 결과를 취소할까요?");
                        if (TaskDialogResult.Yes == confirmResult)
                        {
                            // 윈도우 닫기
                            window.DialogResult = result;
                        }
                        else
                        {
                            // 아무 일 안 함
                        }
                    }
                    else
                    {
                        // 윈도우 닫기
                        window.DialogResult = result;
                    }
                }
            }
            catch (Exception ex)
            {
                window.Close();
                throw ex;
            }
        }

        public virtual void Create(Window window, bool? result)
        {
            try
            {
                if (true == result) // OkCommand에서 호출
                {
                    if (_runCommandExecuted)
                    {
                        window.DialogResult = true;
                    }
                    else
                    {
                        if (ExecuteCreate())
                        {
                            window.DialogResult = true;
                        }
                        else // 실패하면 일단 윈도우 닫지 말자.
                        {
                            ShowWarningMessage(ErrorMessage);
                        }
                    }
                }
                else if (false == result) // CancelCommand에서 호출
                {
                    if (_runCommandExecuted)
                    {
                        var confirmResult = ShowConfirmMessage("변경된 결과를 취소할까요?");
                        if (TaskDialogResult.Yes == confirmResult)
                        {
                            // 윈도우 닫기
                            window.DialogResult = result;
                        }
                        else
                        {
                            // 아무 일 안 함
                        }
                    }
                    else
                    {
                        // 윈도우 닫기
                        window.DialogResult = result;
                    }
                }
            }
            catch (Exception ex)
            {
                window.Close();
                throw ex;
            }
        }

        public virtual void Update(Window window, bool? result)
        {
            try
            {
                if (true == result) // OkCommand에서 호출
                {
                    if (_runCommandExecuted)
                    {
                        window.DialogResult = true;
                    }
                    else
                    {
                        if (ExecuteUpdate())
                        {
                            window.DialogResult = true;
                        }
                        else // 실패하면 일단 윈도우 닫지 말자.
                        {
                            ShowWarningMessage(ErrorMessage);
                        }
                    }
                }
                else if (false == result) // CancelCommand에서 호출
                {
                    if (_runCommandExecuted)
                    {
                        var confirmResult = ShowConfirmMessage("변경된 결과를 취소할까요?");
                        if (TaskDialogResult.Yes == confirmResult)
                        {
                            // 윈도우 닫기
                            window.DialogResult = result;
                        }
                        else
                        {
                            // 아무 일 안 함
                        }
                    }
                    else
                    {
                        // 윈도우 닫기
                        window.DialogResult = result;
                    }
                }
            }
            catch (Exception ex)
            {
                window.Close();
                throw ex;
            }
        }


        public virtual void CloseOpt(Window window, bool? result)
        {
            try
            {
                if (true == result) // OkCommand에서 호출
                {
                    if (_runCommandExecutedOpt)
                    {
                        window.DialogResult = true;
                    }
                    else
                    {
                        if (ExecuteApplyOpt())
                        {
                            window.DialogResult = true;
                        }
                        else // 실패하면 일단 윈도우 닫지 말자.
                        {
                            ShowWarningMessage(ErrorMessage);
                        }
                    }

                }
                else if (false == result) // CancelCommand에서 호출
                {
                    if (_runCommandExecutedOpt)
                    {
                        var confirmResult = ShowConfirmMessage("변경된 결과를 취소할까요?");
                        if (TaskDialogResult.Yes == confirmResult)
                        {
                            // 윈도우 닫기
                            window.DialogResult = result;
                        }
                        else
                        {
                            // 아무 일 안 함
                        }
                    }
                    else
                    {
                        // 윈도우 닫기
                        window.DialogResult = result;
                    }
                }
            }
            catch (Exception ex)
            {
                window.Close();
                throw ex;
            }
        }
        #endregion

        #region 뷰모델 메인 명령 실행하기
        protected virtual bool CanExecuteApply()
        {
            return true;
        }
        protected virtual bool CanExecuteApplyOpt()
        {
            return true;
        }

        private DelegateCommand _applyCommand;
        public DelegateCommand ApplyCommand
        {
            get
            {
                return _applyCommand ?? (_applyCommand = new DelegateCommand(
                    () => ExecuteApply(),
                    () => CanExecuteApply()));
            }
        }
        private DelegateCommand _applyCommandOpt;
        public DelegateCommand ApplyCommandOpt
        {
            get
            {
                return _applyCommandOpt ?? (_applyCommandOpt = new DelegateCommand(
                    () => ExecuteApplyOpt(),
                    () => CanExecuteApplyOpt()));
            }
        }

        protected virtual bool ExecuteApply()
        {
            if (ValidateData())
            {
                RunCommand();
                _runCommandExecuted = true;

                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool ExecuteCreate()
        {
            if (ValidateFinishData())
            {

                DraftCommand();
                _runCommandExecuted = true;

                return true;
            }
            else
            {
                return false;
            }
        }



        protected virtual bool ExecuteUpdate()
        {
            if (ValidateFinishData())
            {

                UpdateFinishCommand();
                _runCommandExecuted = true;

                return true;
            }
            else
            {
                return false;
            }
        }


        protected virtual bool ExecuteApplyOpt()
        {
            if (ValidateDataOpt())
            {
                RunCommandOpt();
                _runCommandExecutedOpt = true;

                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual void RunCommand()
        {
            return;
        }

        public virtual void DraftCommand()
        {
            return;
        }

        public virtual void UpdateFinishCommand()
        {
            return;
        }

        public virtual void RunCommandOpt()
        {
            return;
        }
        #endregion

        #region ApplyCommand 실행하기 전 뷰모델 데이터 정합성 검토
        public virtual bool ValidateData()
        {
            return true;
        }
        public virtual bool ValidateDataOpt()
        {
            return true;
        }
        public virtual bool ValidateFinishData()
        {
            return true;
        }
        #endregion

        #region ISupportServices
        IServiceContainer _serviceContainer = null;
        protected IServiceContainer ServiceContainer
        {
            get
            {
                if (_serviceContainer == null)
                    _serviceContainer = new ServiceContainer(this);
                return _serviceContainer;
            }
        }
        IServiceContainer ISupportServices.ServiceContainer { get { return ServiceContainer; } }

        protected virtual string ResultMessage { get; set; }
        protected virtual void ShowResultMessage(string resultMessage)
        {
            ResultMessage = resultMessage;

            if (null != ResultMessage)
            {
                //MessageBox.Show(ResultMessage, "Result", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                TaskDialog taskDialog = new TaskDialog("Result");
                taskDialog.TitleAutoPrefix = true;
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconInformation;
                //taskDialog.MainInstruction = "Completed";
                taskDialog.MainContent = ResultMessage;
                taskDialog.CommonButtons = TaskDialogCommonButtons.Ok;
                taskDialog.Show();
            }
        }

        protected virtual string WarningMessage { get; set; }
        protected virtual void ShowWarningMessage(string warningMessage)
        {
            WarningMessage = warningMessage;

            if (null != WarningMessage)
            {
                //MessageBox.Show(WarningMessage, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                TaskDialog taskDialog = new TaskDialog("Warning");
                taskDialog.TitleAutoPrefix = true;
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
                //taskDialog.MainInstruction = "Completed";
                taskDialog.MainContent = WarningMessage;
                taskDialog.CommonButtons = TaskDialogCommonButtons.Ok;
                taskDialog.Show();
            }
        }

        protected virtual string ErrorMessage { get; set; }
        protected virtual void ShowErrorMessage(string errorMessage = "Error detected!!!")
        {
            ErrorMessage = errorMessage;

            if (null != ErrorMessage)
            {
                //MessageBox.Show(ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                TaskDialog taskDialog = new TaskDialog("Error");
                taskDialog.TitleAutoPrefix = true;
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconError;
                //taskDialog.MainInstruction = "Completed";
                taskDialog.MainContent = ErrorMessage;
                taskDialog.CommonButtons = TaskDialogCommonButtons.Ok;
                taskDialog.Show();
            }
        }

        protected virtual string ConfirmMessage { get; set; }
        protected virtual TaskDialogResult ShowConfirmMessage(string confirmMessage)
        {
            ConfirmMessage = confirmMessage;

            TaskDialogResult result = TaskDialogResult.No;

            if (null != ConfirmMessage)
            {
                //return MessageBox.Show(ConfirmMessage, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.Yes);
                TaskDialog taskDialog = new TaskDialog("Confirm");
                taskDialog.TitleAutoPrefix = true;
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconInformation;
                //taskDialog.MainInstruction = "Completed";
                taskDialog.MainContent = ConfirmMessage;
                taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                return taskDialog.Show();
            }

            return result;
        }
        #endregion

        #region 에러 윈도우
        protected void ShowErrorWindow(ObservableCollection<ErrorItem> errorItems)
        {
            var errorViewModel = new ErrorViewModel(errorItems);
            var errorWindow = new ErrorWindow() { DataContext = errorViewModel };
            errorWindow.ShowDialog();
        }

        #endregion

        #region 도움말 링크 커맨드
        protected string HelpLink { get; set; }

        protected ICommand _helpCommand;
        public virtual ICommand HelpCommand
        {
            get
            {
                return _helpCommand ?? (_helpCommand = new DelegateCommand
                    (() =>
                    {
                        if (null != HelpLink)
                            System.Diagnostics.Process.Start(HelpLink);
                    }));
            }
        }
        #endregion

        #region 프로그레스바
        public void UpdateProgress(double progress)
        {
            ProgressBar.Dispatcher.Invoke(new ProgressBarDelegate(() =>
            {
                ProgressBar.Value = progress;
            }), DispatcherPriority.Background);
        }
        #endregion
    }
}
