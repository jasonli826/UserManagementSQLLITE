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
using System.Windows.Shapes;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Helpers;
using UserManagementlibrary.Repository;
using System.Text.RegularExpressions;

namespace UserManagementlibrary
{
    public partial class UserManagement : UserControl
    {
        private bool resetPasswordFlag = false;
        List<User> userList = null;
        List<Domain> domainList = null;
        List<UserRole> userRoleList = null;
        List<Role> roleList = null;
        ObservableCollection<Role> allRoles = new ObservableCollection<Role>();
        ObservableCollection<Role> selectedRoles = new ObservableCollection<Role>();
        private static int ID = 0;
        public UserManagement()
        {
            InitializeComponent();



            domainList = DomainRepository.GetActiveDomains();
            domainList.Insert(0, new Domain
            {
                DomainID = 0,
                DomainName = "- Please select -"
            });
            cmbDomain.ItemsSource = domainList;
            cmbDomain.SelectedIndex = 0;
            LoadFilterRoleListBox();

            userList = UserRepository.GetAllUser(SessionContext.UserRole, RoleRepository.GetActiveRoleBasedOnSequence(),SessionContext.UserId);
            UserDataGrid.ItemsSource = userList;

        }
        private void LoadFilterRoleListBox()
        {
            try
            {
                var allRolesFromRepo = RoleRepository.GetActiveRoleBasedOnSequence();

                // Filter roles based on current user's role
                string currentUserRole = SessionContext.UserRole; // Assume this is stored at login
                List<Role> filteredRoles = UserRepository.FilterRolesByUserRole(currentUserRole, allRolesFromRepo);

                allRoles = new ObservableCollection<Role>(filteredRoles);
                if(allRoles.Count>0)
                RoleDataList.ItemsSource = allRoles.Where(x=>x.Status.ToUpper()=="ACTIVE");

                selectedRoles = new ObservableCollection<Role>();
                SelectedRoleDataList.ItemsSource = selectedRoles;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadFilterRoleListBox Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public bool IsValidPassword(string password)
        {
            string pattern = @"^(?=.*[A-Za-z])(?=.*[^A-Za-z]).{8,20}$";
            return Regex.IsMatch(password, pattern);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            string userId = txtUserId.Text;
            string EmpId = txtEmpId.Text;
            string password = txtPassword.Password;
            string userName = txtUserName.Text.Trim();
            string domainName =( cmbDomain.SelectedIndex == 0 ? string.Empty : cmbDomain.Text);
            string status = (rbActive.IsChecked == true ? "Active" : "Inactive");
            string remark = txtRemarks.Text.Trim();
            try
            {
                if (!string.IsNullOrEmpty(password)|(!string.IsNullOrEmpty(unmaskedPasswordTextBox.Text)&& showPasswordCheckBox.IsChecked==true))
                {
                    MessageBox.Show("Please Clear The Password before searching.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                }

                var selectedRoleIds = selectedRoles.Select(r => r.RoleID).ToList();
                userList = UserRepository.SearchUsers(SessionContext.UserRole,SessionContext.UserId,userId, userName, EmpId, domainName, status, remark, selectedRoleIds);
                UserDataGrid.ItemsSource = userList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search User Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }
        //private List<Role> FilterRolesByUserRole(string userRole, List<Role> allRoles)
        //{
        //    if (string.IsNullOrEmpty(userRole))
        //        return new List<Role>();
        //    var roles = userRole.Split('|', (char)StringSplitOptions.RemoveEmptyEntries)
        //                        .Select(r => r.Trim().ToLower())
        //                        .ToList();

        //    if (!roles.Any())
        //        return new List<Role>();

        //    var roleHierarchy = new List<string>();
        //    //{
        //    //    "system administrator",
        //    //    "service",
        //    //    "engineer",
        //    //    "technician",
        //    //    "operator"
        //    //};
        //    foreach (var role in allRoles)
        //    {
        //        roleHierarchy.Add(role.Role_Name.ToLower());
        //    }
        //    string highestRole = roleHierarchy
        //        .FirstOrDefault(r => roles.Contains(r));

        //    if (highestRole == null)
        //        return new List<Role>();
        //    switch (highestRole)
        //    {
        //        case "system administrator":
        //            return allRoles; // Admin sees all

        //        case "service":
        //            return allRoles.Where(r =>
        //                    r.Role_Name.Equals("Engineer", StringComparison.OrdinalIgnoreCase) ||
        //                    r.Role_Name.Equals("Technician", StringComparison.OrdinalIgnoreCase) ||
        //                    r.Role_Name.Equals("Operator", StringComparison.OrdinalIgnoreCase))
        //                .ToList();

        //        case "engineer":
        //            return allRoles.Where(r =>
        //                    r.Role_Name.Equals("Technician", StringComparison.OrdinalIgnoreCase) ||
        //                    r.Role_Name.Equals("Operator", StringComparison.OrdinalIgnoreCase))
        //                .ToList();

        //        case "technician":
        //            return allRoles.Where(r =>
        //                    r.Role_Name.Equals("Operator", StringComparison.OrdinalIgnoreCase))
        //                .ToList();

        //        default:
        //            return new List<Role>();
        //    }
        //}
        // public int RoleID;
        // public string Role_Name;
        // public int? ParentRoleID;
        // public int PriorityIndex;
        // public string Status;




        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

            ClearData();
        }
        private void ClearData()
        {

            txtRemarks.Text = string.Empty;
            txtUserId.Text = string.Empty;
            txtUserName.Text = string.Empty;
            SelectedRoleDataList.ItemsSource = null;
            txtPassword.Clear();
            cmbDomain.SelectedValue = 0;
            txtUserId.IsReadOnly = false;
            txtEmpId.Text = string.Empty;
            LoadFilterRoleListBox();
            selectedRoles.Clear();
            SelectedRoleDataList.ItemsSource = selectedRoles;
            btnAutoPassword.Visibility = Visibility.Visible;
            BtnRestPassword.Visibility = Visibility.Collapsed;
            userList = UserRepository.GetAllUser(SessionContext.UserRole, RoleRepository.GetActiveRoleBasedOnSequence(),SessionContext.UserId);
            UserDataGrid.ItemsSource = userList;
            btnSave.Content = "Save";
            resetPasswordFlag = false;
            rbActive.IsChecked = true;
            rbInactive.IsChecked = false;
            txtPassword.Password = string.Empty;
            txtPassword.IsEnabled = true;
            unmaskedPasswordTextBox.IsEnabled = true;
            showPasswordCheckBox.IsEnabled = true;
        }
        private void BtnMoveRight_Click(object sender, RoutedEventArgs e)
        {
            var selected = RoleDataList.SelectedItem as Role;
            if (selected != null && !selectedRoles.Any(r => r.RoleID == selected.RoleID))
            {
                selectedRoles.Add(selected);     // Add to right
            }
        }


        private void BtnMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            var selected = SelectedRoleDataList.SelectedItem as Role;
            if (selected != null)
            {
                selectedRoles.Remove(selected);  // Remove from right
                //if (!allRoles.Any(r => r.RoleID == selected.RoleID))
                //    allRoles.Add(selected);      // Add back to left
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string userId = txtUserId.Text.Trim();
            string empId = txtEmpId.Text.Trim();
            string userName = txtUserName.Text.Trim();
            string password = txtPassword.Password;
            int domainid = Convert.ToInt32(cmbDomain.SelectedValue);
            bool isCheckShowPassword = showPasswordCheckBox.IsChecked.Value;
            try
            {
                if (isCheckShowPassword)
                {
                    MessageBox.Show("Please Untick the show password checkbox.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;

                }
                if (btnSave.Content.ToString().ToUpper() == "SAVE")
                {

                    if (string.IsNullOrEmpty(userId))
                    {
                        MessageBox.Show("User Id Cannot be empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }
                    if (string.IsNullOrEmpty(empId))
                    {
                        MessageBox.Show("Employer Id Cannot be empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }

                    if (string.IsNullOrEmpty(userName))
                    {
                        MessageBox.Show("User Name Cannot be empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }

                    if (string.IsNullOrEmpty(password))
                    {
                        MessageBox.Show("Password Cannot Be Empty", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }
                    else if (!IsValidPassword(password))
                    {
                        MessageBox.Show("Password must be 8–20 characters long and it contains at least one alphabet letter and one non-alphabet character", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    if (selectedRoles.Count == 0)
                    {
                        MessageBox.Show("Please Assign User Role to this user", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (domainid == 0)
                    {
                        MessageBox.Show("Please Select a Domain", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }
                    if (UserRepository.UserIdExists(userId.ToLower()))
                    {
                        MessageBox.Show("User Id Already Exsits!", "User Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (UserRepository.EmployeeIdExists(empId))
                    {
                        MessageBox.Show("User Id Already Exsits!", "User Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    User newUser = new User
                    {
                        UserId = userId.ToLower(),
                        Empid = empId,
                        UserName = userName,
                        Password = CryptoHelper.Encrypt(password),
                        Domain = domainid,
                        Created_by = SessionContext.UserId == null ? "System" : SessionContext.UserId,
                        Updated_by = SessionContext.UserId == null ? "System" : SessionContext.UserId,
                        Status = rbActive.IsChecked == true ? "Active" : "Inactive",
                        Remarks = txtRemarks.Text,
                        Created_Date = DateTime.Now,
                        Updated_Date = DateTime.Now
                    };
                    List<UserRole> userRoleList = new List<UserRole>();
                    foreach (var role in selectedRoles)
                    {
                        userRoleList.Add(new UserRole
                        {
                            RoleId = role.RoleID,
                            UserId = userId.ToLower()

                        });

                    }
                    UserRepository.InsertUser(newUser);
                    UserRoleRepository.InsertUserRoles(userRoleList);
                    MessageBox.Show("Create User Successfully");
                    ClearData();
                }
                else
                {
                    UpdateUser(userId, empId, password, userName, domainid);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search User Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }
        private void UpdateUser(string userId,string empId,string password, string userName,int domainid)
        {
            try { 
                    if (string.IsNullOrEmpty(userId))
                    {
                        MessageBox.Show("User Id Cannot be empty", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }
                    if (string.IsNullOrEmpty(empId))
                    {
                        MessageBox.Show("Employee Id Cannot be empty");
                        return;

                    }

                if (string.IsNullOrEmpty(userName))
                    {
                        MessageBox.Show("User Name Cannot be empty", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }
                    if (resetPasswordFlag && password == string.Empty)
                    {

                        MessageBox.Show("Password Cannot be empty", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    else if(resetPasswordFlag)
                    {
                        if (!IsValidPassword(password))
                        {
            
                            MessageBox.Show("Password must be 8–20 characters long and it contains at least one alphabet letter and one non-alphabet character", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;

                        }
                        else if (rbActive.IsChecked == false && rbInactive.IsChecked == true)
                        {
                                MessageBox.Show("Inactive user cannot reset the password", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;

                        }
                
                    }

                    if (selectedRoles.Count == 0)
                    {
                        MessageBox.Show("Please Assign User Role to this user", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }
                    if (domainid == 0)
                    {
                        MessageBox.Show("Please Select one domain.", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;

                    }
                    if (UserRepository.UserIdExists(userId.ToLower(),ID))
                    {
                        MessageBox.Show("User Id Already Exsits.", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    if (UserRepository.EmployeeIdExists(empId, ID))
                    {
                        MessageBox.Show("Employee Id Already Exsits.", "User Update Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                User existingUser = new User
                    {
                        UserId = userId,
                        Empid = empId,
                        UserName = userName,
                        Password = password,
                        Domain = domainid,
                        Updated_by = SessionContext.UserId ?? "System",
                        Status = rbActive.IsChecked == true ? "Active" : "Inactive",
                        Remarks = txtRemarks.Text,
                    };

                    List<UserRole> userRoleList = new List<UserRole>();
                    foreach (var role in selectedRoles)
                    {
                        userRoleList.Add(new UserRole
                        {
                            RoleId = role.RoleID,
                            UserId = userId

                        });

                    }
                UserRepository.UpdateUser(existingUser);
                UserRoleRepository.DeleteUserRolesByUserId(userId);
                UserRoleRepository.InsertUserRoles(userRoleList);
                MessageBox.Show("Update User Successfully");

                ClearData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search User Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var user = button.DataContext as User; 
                if (user != null)
                {
                    ID = user.ID;
                    txtUserId.Text = user.UserId;
                    txtUserId.IsReadOnly = true;
                    txtEmpId.Text = user.Empid;
                    txtUserName.Text = user.UserName;
                    cmbDomain.SelectedValue = user.Domain;
                    txtRemarks.Text = user.Remarks;
                    selectedRoles = new ObservableCollection<Role>(GetRolesByUserId(user.UserId));
                    SelectedRoleDataList.ItemsSource = selectedRoles;
                    if (user.Status.ToUpper() == "Active".ToUpper())
                    {
                        rbActive.IsChecked = true;
                        rbInactive.IsChecked = false;
                    }
                    else
                    {
                        rbInactive.IsChecked = true;
                        rbActive.IsChecked = false;
                    }
                    btnAutoPassword.Visibility = Visibility.Collapsed;
                    BtnRestPassword.Visibility = Visibility.Visible;
                   
                    btnSave.Content = "Update";
                }
                BtnRestPassword.Content = "Reset";
                showPasswordCheckBox.IsChecked = false;
                unmaskedPasswordTextBox.Text = string.Empty;
                txtPassword.Password = string.Empty;
                txtPassword.IsEnabled = false;
                unmaskedPasswordTextBox.IsEnabled = false;
                showPasswordCheckBox.IsEnabled = false;
            }
        }
        private void BtnAutoPassword_Click(object sender, RoutedEventArgs e)
        {
                string id = string.Empty;
                string pwd= string.Empty;
                System.Guid guid = System.Guid.NewGuid();
                id = guid.ToString();
                id.Replace("-", string.Empty);
            if (showPasswordCheckBox.IsChecked == false)
            {
                txtPassword.Password = "getech" + id.Substring(0, 8);
            }
            else
                unmaskedPasswordTextBox.Text = "getech" + id.Substring(0, 8);



        }
        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            unmaskedPasswordTextBox.Text = txtPassword.Password; // Get the password from the PasswordBox
            txtPassword.Visibility = Visibility.Hidden;
            unmaskedPasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Visibility = Visibility.Visible;
            txtPassword.Password = unmaskedPasswordTextBox.Text;  
            unmaskedPasswordTextBox.Visibility = Visibility.Hidden;
            unmaskedPasswordTextBox.Clear(); // Clear the unmasked text for security
        }
        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            if (rbInactive.IsChecked == true && rbActive.IsChecked == false)
            {
               MessageBox.Show( "Inactive user cannot reset the password","Reset Password", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

   
            if (BtnRestPassword.Content.ToString().ToUpper() == "Reset".ToUpper())
            {
                resetPasswordFlag = true;
                unmaskedPasswordTextBox.IsEnabled = true;
                showPasswordCheckBox.IsEnabled = true;
                BtnRestPassword.Content = "Cancel";
                btnAutoPassword.Visibility = Visibility.Visible;
                txtPassword.IsEnabled = true;
            }
            else
            {
                txtPassword.Password = string.Empty;
                unmaskedPasswordTextBox.Text = string.Empty;
                resetPasswordFlag = false;
                unmaskedPasswordTextBox.IsEnabled = false;
                showPasswordCheckBox.IsEnabled = false;
                txtPassword.IsEnabled = false;
                btnAutoPassword.Visibility = Visibility.Collapsed;
                BtnRestPassword.Content = "Reset";
            }


        }
        private List<Role> GetRolesByUserId(string UserId)
        {
            List<Role> roleList = new List<Role>();
            try { 
        
                userRoleList = UserRoleRepository.GetUserRolesById(UserId);
                foreach (var usrrole in userRoleList)
                {

                    var role = RoleRepository.GetRoleById(usrrole.RoleId);
                    roleList.Add(role);
                }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "GetRolesByUserId Error ", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            return roleList;


        }
    }
}
