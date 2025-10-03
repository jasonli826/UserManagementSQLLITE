using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UserManagementlibrary;
using UserManagementlibrary.Log;
using UserManagementOnSQLLite.Database;
using UserManagementOnSQLLite.Entity;
using UserManagementOnSQLLite.Repository;

namespace UserManagementOnSQLLite
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private string dbFile = ConfigurationManager.AppSettings["SQLLiteDBFilePath"];
        public Login()
        {
            InitializeComponent();

        }
        protected void Login_Click(object sender, RoutedEventArgs e)
        {
            string userName = txtUserName.Text.Trim();
            string password = PasswordBox.Password;
            string message = string.Empty;
            int RetVal = 0;
            if (!DatabaseInitializer.Initialize())
            {
                MessageBox.Show("SQLLITE Database "+dbFile+" is not created and please create database first", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
            if (string.IsNullOrEmpty(userName))
            {
                MessageBox.Show("UserName cannot be empty", "Login Error",MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Password cannot be empty", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            RetVal = UserManagementlibrary.UserAuthentication.Authenticated(userName.ToLower(), password,ref message);

            if (RetVal != 0)
            {
                MessageBox.Show(message, "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;

            }
            else
            {
                UserManagementlibrary.Log.ApiLogger.Log("UserAuthentication", $"Login Successfully and return message "+message);

            }
            SessionContext.UserId = userName;
            SessionContext.UserName = userName;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close(); 
        }
        private void txtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Login_Click(sender, e); // trigger the login button click
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

    }
}
