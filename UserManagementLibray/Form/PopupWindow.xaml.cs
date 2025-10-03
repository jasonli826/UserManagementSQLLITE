using System;
using System.Windows;
using System.Windows.Controls;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Repository;
using UserManagementLibray;

namespace UserManagementlibrary.Form
{
    public partial class PopupWindow : Window
    {
        public PopupWindow(UserControl contentControl, string title = "Popup")
        {
            InitializeComponent();

            this.Title = title;

            if (contentControl != null)
                ShowContent(contentControl);
        }
        public void ShowContent(UserControl control)
        {

            if (control is ChangePassword)
            {
                this.WindowState = WindowState.Normal; // cancel maximize
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.Width = 400;
                this.Height = 300;
            }
            else if (control is RoleSequenceControl)
            {
                this.WindowState = WindowState.Normal; // cancel maximize
                this.ResizeMode = ResizeMode.NoResize;
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                this.Width = 500;
                this.Height = 400;

            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }

            ContentGrid.Children.Clear();
            ContentGrid.Children.Add(control);
        }
    }
}
