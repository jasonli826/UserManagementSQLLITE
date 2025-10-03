using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Repository;

namespace UserManagementlibrary
{
    public partial class ChangePassword : UserControl
    {
        public string CurrentUserId { get; set; } // set this when loading control

        public ChangePassword()
        {
            InitializeComponent();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            txtOldPassword.Clear();
            txtNewPassword.Clear();
            txtConfirmPassword.Clear();
        }
        private bool IsValidPassword(string password)
        {
            string pattern = @"^(?=.*[A-Za-z])(?=.*[^A-Za-z]).{8,20}$";
            return Regex.IsMatch(password, pattern);
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string oldPwd = txtOldPassword.Password.Trim();
            string newPwd = txtNewPassword.Password.Trim();
            string confirmPwd = txtConfirmPassword.Password.Trim();

            // Validation
            if (string.IsNullOrEmpty(oldPwd))
            {
                MessageBox.Show("Old password cannot be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(newPwd))
            {
                MessageBox.Show("New password cannot be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(confirmPwd))
            {
                MessageBox.Show("Confirm password cannot be empty.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (newPwd != confirmPwd)
            {
                MessageBox.Show("New password and Confirm password do not match.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!IsValidPassword(newPwd))
            {
                MessageBox.Show("Password must be 8–20 characters long and it contains at least one alphabet letter and one non-alphabet character ");
                return;

            }

            try
            {

                CurrentUserId = SessionContext.UserId;
                // Call repository function (you must implement this)
                bool result = UserRepository.ChangePassword(CurrentUserId.ToLower(), oldPwd, newPwd);

                if (result)
                {
                    MessageBox.Show("Password changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    BtnClear_Click(null, null); // clear fields
                }
                else
                {
                    MessageBox.Show("Password change failed. Please check your old password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Change Password Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
