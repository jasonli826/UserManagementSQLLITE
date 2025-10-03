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
                List<Role> filteredRoles = FilterRolesByUserRole(currentUserRole, allRolesFromRepo);

                allRoles = new ObservableCollection<Role>(filteredRoles);
                RoleDataList.ItemsSource = allRoles;

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
        // 假设 Role 定义至少包含:
        // public int RoleID;
        // public string Role_Name;
        // public int? ParentRoleID;
        // public int PriorityIndex;
        // public string Status;

        private List<Role> FilterRolesByUserRole(string userRole, List<Role> allRoles)
        {
            if (string.IsNullOrWhiteSpace(userRole) || allRoles == null || allRoles.Count == 0)
                return new List<Role>();

            // 1) 把用户可能拥有的角色名规范化为小写集合
            var userRoleNames = userRole
                .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim().ToLower())
                .ToHashSet();

            if (!userRoleNames.Any())
                return new List<Role>();

            // 2) 构建按 ID 的字典，便于上溯查 root
            var byId = allRoles.ToDictionary(r => r.RoleID);

            // 3) 根据 RoleID 缓存 root 计算（避免重复遍历）
            var rootCache = new Dictionary<int, Role>();

            Role GetRoot(Role r)
            {
                if (r == null) return null;
                if (rootCache.TryGetValue(r.RoleID, out var cached)) return cached;

                var cur = r;
                // 上溯直到 ParentRoleID 为 null 或找不到父
                while (cur.ParentRoleID.HasValue && byId.TryGetValue(cur.ParentRoleID.Value, out var parent))
                {
                    cur = parent;
                }

                rootCache[r.RoleID] = cur;
                return cur;
            }

            // 4) 找到用户在 DB 中对应的角色对象
            var userRoles = allRoles
                .Where(r => userRoleNames.Contains(r.Role_Name.Trim().ToLower()))
                .ToList();

            if (!userRoles.Any())
                return new List<Role>(); // 登录角色在 DB 中找不到，返回空（可加日志检查）

            // 5) 如果用户包含 System Administrator（根是 System Administrator），直接返回所有 Active 角色
            foreach (var ur in userRoles)
            {
                var root = GetRoot(ur);
                if (root != null && root.Role_Name.Equals("System Administrator", StringComparison.OrdinalIgnoreCase))
                {
                    return allRoles
                        .Where(r => string.Equals(r.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(r => r.ParentRoleID ?? r.RoleID) // COALESCE(ParentRoleID, RoleID)
                       .ThenBy(r => r.ParentRoleID)              // 按 ParentRoleID 排序
                       .ThenBy(r => r.PriorityIndex)             // 按 PriorityIndex 排序
                       .ToList();

                }
            }

            // 6) 收集用户每个角色对应的 rootPriority（可能有多个角色，取每个的 rootPriority 并对每个做允许集合，然后合并）
            var userRootPriorities = new HashSet<int>();
            foreach (var ur in userRoles)
            {
                var root = GetRoot(ur);
                if (root != null) userRootPriorities.Add(root.PriorityIndex);
            }

            // 7) 找出所有 root（ParentRoleID == null）
            var roots = allRoles.Where(r => !r.ParentRoleID.HasValue).ToList();

            // 8) 对于每个用户根优先级，收集 rootPriority > 用户 rootPriority 的那些 root 下所有角色
            var allowedRoleIds = new HashSet<int>();
            foreach (var userRootPriority in userRootPriorities)
            {
                var allowedRoots = roots.Where(rt => rt.PriorityIndex > userRootPriority).ToList();
                foreach (var ar in allowedRoots)
                {
                    // 把属于该 allowed root 的所有角色加入（包括 root 本身与其所有后代）
                    foreach (var role in allRoles)
                    {
                        var roleRoot = GetRoot(role);
                        if (roleRoot != null && roleRoot.RoleID == ar.RoleID)
                        {
                            // 只加入 Active 的
                            if (string.Equals(role.Status, "Active", StringComparison.OrdinalIgnoreCase))
                                allowedRoleIds.Add(role.RoleID);
                        }
                    }
                }
            }

            // 9) 最后去掉用户自己所对应的具体角色（如果你不想用户看到自己）
            foreach (var name in userRoleNames)
            {
                var own = allRoles.FirstOrDefault(r => r.Role_Name.Trim().ToLower() == name);
                if (own != null) allowedRoleIds.Remove(own.RoleID);
            }

            // 10) 将 id 集合映射回 Role 对象并排序返回
            var result = allRoles
               .Where(r => allowedRoleIds.Contains(r.RoleID))
               .OrderBy(r => r.ParentRoleID ?? r.RoleID) // COALESCE(ParentRoleID, RoleID)
               .ThenBy(r => r.ParentRoleID)              // 按 ParentRoleID 排序
               .ThenBy(r => r.PriorityIndex)             // 按 PriorityIndex 排序
               .ToList();

            return result;
        }









        private void btnClear_Click(object sender, RoutedEventArgs e)
        {

            ClearField();
        }
        private void ClearField()
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
                    ClearField();
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

                ClearField();
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
