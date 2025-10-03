using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserManagementlibrary;
using UserManagementOnSQLLite.Entity;

namespace UserManagementOnSQLLite

{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }


        private void RoleAdmin_CreateEditRoleRequested(object sender, EventArgs e)
        {
            var roleMgmt = new UserManagementlibrary.RoleManagement();
            roleMgmt.ReturnRequested += RoleMgmt_ReturnRequested;
            MainContent.Content = roleMgmt;
        }
        private void RoleMgmt_ReturnRequested(object sender, EventArgs e)
        {
            // Go back to RoleAdministration
            var roleAdmin = new UserManagementlibrary.RoleAdministration();
            roleAdmin.CreateEditRoleRequested += RoleAdmin_CreateEditRoleRequested;

           // MainContent.Content = roleAdmin;
        }
        private void HomeMenu_Click(object sender, RoutedEventArgs e)
        {
            // Show default welcome image
            Grid grid = new Grid { Background = System.Windows.Media.Brushes.White };
            Image image = new Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(
                    new System.Uri("Image/welcome.png", System.UriKind.Relative)),
                Stretch = System.Windows.Media.Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            grid.Children.Add(image);

            MainContent.Content = grid;
        }
        private void MenuUserManagement_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.OpenUserManagemeWindow();
         //  MainContent.Content = new UserManagementlibrary.UserManagement();
        }

        private void MenuUserRole_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.OpenRoleAdministrationWindow();
            //var roleAdmin = new UserManagementlibrary.RoleAdministration();
            //roleAdmin.CreateEditRoleRequested+= RoleAdmin_CreateEditRoleRequested;

         //   MainContent.Content = roleAdmin;
        }

        private void MenuDomain_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.OpenDomainWindow();
            //MainContent.Content = new UserManagementlibrary.DomainUserControl();
        }
        private void MenuChangePassword_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.OpenChangePasswordWindow();
            //MainContent.Content = new UserManagementlibrary.ChangePassword();

        }
        private void MenuAuditLog_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.OpenAuditLog();
            //MainContent.Content = new UserManagementlibrary.ChangePassword();

        }
        private void MenuDataAnalytics_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.OpenDataAnalyticsWindow();
            //MainContent.Content = new UserManagementlibrary.ChangePassword();

        }
        private void MenuAlarm_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.OpenAlarmWindow();
            //MainContent.Content = new UserManagementlibrary.ChangePassword();

        }
        private void MenuLog_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication.OpenLogWindow();
            //MainContent.Content = new UserManagementlibrary.ChangePassword();

        }
        private void MenuLogout_Click(object sender, RoutedEventArgs e)
        {
           UserManagementlibrary.Repository.UserRepository.UpdateLogoutTime(SessionContext.UserId);
            UserAuthentication.Logout();
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }

    }

}
