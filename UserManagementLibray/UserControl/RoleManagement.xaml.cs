using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Repository;
using UserManagementLibray;
using UserManagementOnSQLLite;
using static System.Data.Entity.Infrastructure.Design.Executor;

namespace UserManagementlibrary
{
    /// <summary>
    /// Interaction logic for RoleManagement.xaml
    /// </summary>
    public partial class RoleManagement : UserControl
    {
        Role role = null;
        private static int roleId = 0;
        private ObservableCollection<Role> RoleList = new ObservableCollection<Role>();
        public event EventHandler ReturnRequested;
        public RoleManagement()
        {
            InitializeComponent();
            try
            {
                LoadRoles();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Example: Assume you store current logged-in user role in a static Session/UserContext
            string currentUserRole = SessionContext.UserRole; // e.g., "SystemAdmin", "Engineer", etc.
            if (string.IsNullOrEmpty(currentUserRole) ||!currentUserRole.ToUpper().Contains("System Administrator".ToUpper()))
            {
                MessageBox.Show("Access Denied. Only System Admin can access this page.",
                                "Permission Denied",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                this.IsEnabled = false;

            }
        }
        private void LoadRoles()
        {
            try
            {
                RoleList.Clear();
                foreach (var r in RoleRepository.GetAllRole())
                    RoleList.Add(r);

                RoleDataGrid.ItemsSource = RoleList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadRoles Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk?.DataContext is Role role)
            {

                role.IsSelected = chk.IsChecked ?? false;
                if (!role.IsSelected)
                {
                    chkSelectAll.IsChecked = false;
                }
                else
                {
                    if (RoleList.All(d => d.IsSelected))
                    {
                        chkSelectAll.IsChecked = true;
                    }
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected row data
            var button = sender as Button;
            if (button != null)
            {
                role = button.DataContext as Role; // Replace 'User' with your data class
                if (role != null)
                {
                    txtRoleName.Text = role.Role_Name;
                    txtRoleDescription.Text = role.Description;
                    BtnSave.Content = "Update";
                    roleId = role.RoleID;
                }
            }
        }
        protected void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string roleName = txtRoleName.Text.Trim();
            string roleDesc = txtRoleDescription.Text.Trim();
            try
            {

                if (string.IsNullOrEmpty(roleName))
                {
                    MessageBox.Show("Role Name Cannot be Empty.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (BtnSave.Content.ToString().ToUpper() == "UPDATE")
                {
                    if (role != null)
                    {

                        role.Role_Name = roleName;
                        role.Description = roleDesc;
                        if (RoleRepository.RoleNameExists(roleName, roleId))
                        {
                            //  MessageBox.Show(role.Role_Name + " already exsits");
                            MessageBox.Show("Role Name Already Exsits.", "Save Role", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            role.Updated_by = SessionContext.UserId;
                            role.Updated_Date = DateTime.Now;
                            RoleRepository.UpdateRole(role);

                        }

                        MessageBox.Show("Update The Role Successfully");
                    }


                }
                else
                {
                    if (RoleRepository.RoleNameExists(roleName.ToLower()))
                    {
                        MessageBox.Show("Role Name Already Exsits.", "Save Role", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var newRole = new Role { Role_Name = roleName, Description = roleDesc, Created_by = SessionContext.UserId, Created_Date = DateTime.Now, Updated_by = SessionContext.UserId, Updated_Date = DateTime.Now, Status = "Active" };
                    RoleRepository.InsertRole(newRole);

                    MessageBox.Show("Create The Role Successfully");
                }
                ClearData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Role Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        protected void BtnClear_Click(object sender, RoutedEventArgs e )
        {
            ClearData();
        }
        private void ClearData()
        {
            BtnSave.Content = "Save";
            txtRoleName.Text = string.Empty;
            txtRoleDescription.Text = string.Empty;
            LoadRoles();

        }
        private void BtnActive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RoleDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                RoleDataGrid.CommitEdit();

                var selectedRoles = RoleList.Where(r => r.IsSelected).ToList();
                if (!selectedRoles.Any())
                {
                    MessageBox.Show("Please select at least one role to deactivate.");
                    return;
                }

                foreach (var r in selectedRoles)
                {
                    r.Status = "Active";
                    RoleRepository.UpdateRoleStatus(r.RoleID, "Active");
                }

                RoleDataGrid.Items.Refresh();
                MessageBox.Show("Selected role(s) have been activated.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BtnActive_Click Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnInactive_Click(object sender, RoutedEventArgs e)
        {
            try { 
                    // Commit any pending edits in the DataGrid
                    RoleDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                    RoleDataGrid.CommitEdit(); // ensures cell-level edits are committed

                    var selectedRoles = RoleList.Where(r => r.IsSelected).ToList();
                    if (!selectedRoles.Any())
                    {
                        MessageBox.Show("Please select at least one role to deactivate.");
                        return;
                    }

                    foreach (var r in selectedRoles)
                    {
                        r.Status = "Inactive";
                        RoleRepository.UpdateRoleStatus(r.RoleID, "Inactive");
                    }

                    RoleDataGrid.Items.Refresh();
                    MessageBox.Show("Selected role(s) have been deactivated.");
            }catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BtnActive_Click Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        

        private void ChkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            bool checkAll = chkSelectAll.IsChecked ?? false;
            foreach (var r in RoleList)
                r.IsSelected = checkAll;
        }

        protected void BtnRolePrioritySequence_Click(object sender, RoutedEventArgs e)
        {
            var RoleSequenceControl = new RoleSequenceControl();
            UserManagementlibrary.Form.PopupWindow popupWindow = new UserManagementlibrary.Form.PopupWindow(RoleSequenceControl, "RoleSequenceControl");

            popupWindow.ShowDialog();

        }


        protected void BtnReturn_Click(object sender, RoutedEventArgs e )
        {
            ReturnRequested?.Invoke(this, EventArgs.Empty);
            var roleAdmin = new RoleAdministration();

            var popup = Window.GetWindow(this) as UserManagementlibrary.Form.PopupWindow;
            if (popup != null)
            {
                popup.Title = "Role Administration";
                popup.ShowContent(roleAdmin);
            }
        }
    }
}
