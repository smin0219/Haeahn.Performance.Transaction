using Autodesk.Revit.UI;
using PropertyChanged;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Haeahn.Performance.Transaction.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public BaseViewModel()
        {
        }


        protected virtual string WarningMessage { get; set; }
        protected virtual void ShowWarningMessage(string warningMessage)
        {
            WarningMessage = warningMessage;

            if (null != WarningMessage)
            {
                TaskDialog taskDialog = new TaskDialog("Warning");
                taskDialog.TitleAutoPrefix = true;
                taskDialog.MainIcon = TaskDialogIcon.TaskDialogIconWarning;
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
    }
}
