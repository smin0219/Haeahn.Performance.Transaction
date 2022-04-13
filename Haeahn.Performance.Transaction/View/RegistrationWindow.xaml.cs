using Haeahn.Performance.Transaction.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Haeahn.Performance.Transaction.View
{
    /// <summary>
    /// Interaction logic for RegistrationWindow.xaml
    /// </summary>
    public partial class RegistrationWindow : Window
    {
        RegistrationViewModel viewModel = null;
        public LoginResult loginResult { get; set; }

        public RegistrationWindow()
        {
            InitializeComponent();

            viewModel = new RegistrationViewModel();
            DataContext = viewModel;

            System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(this);
            helper.Owner = Autodesk.Windows.ComponentManager.ApplicationWindow;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            viewModel.Password = (sender as PasswordBox).Password;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CmdRun.Execute(viewModel);
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CmdCancel.Execute(viewModel);
            loginResult = LoginResult.Cancelled;
            Close();
        }
    }
}
