using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Repository;
using UserManagementOnSQLLite.Repository;

namespace UserManagementlibrary
{
    public partial class RoleAdministration : UserControl, INotifyPropertyChanged
    {
        private string _currentSubMenuHeader;
        public string CurrentSubMenuHeader
        {
            get => _currentSubMenuHeader;
            set
            {
                if (_currentSubMenuHeader == value) return;
                _currentSubMenuHeader = value;
                OnPropertyChanged(nameof(CurrentSubMenuHeader));
                if (colChildMenu != null)
                    colChildMenu.Header = _currentSubMenuHeader;
            }
        }

        private bool isChange = false;
        private bool suppressChangePrompt = false; // NEW: suppress prompts during programmatic changes

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private List<string> ParentMenus = new List<string>();
        private List<Role> roleList = null;
        private Dictionary<string, ObservableCollection<SubMenuItem>> SubMenus = new Dictionary<string, ObservableCollection<SubMenuItem>>();
        public event EventHandler CreateEditRoleRequested;
        private Role _previousSelectedRole;
        public RoleAdministration()
        {
            InitializeComponent();
            try
            {
                DataContext = this;

                LoadMenuItemsFromRepository();

                InitializeParentMenuButtons();
                roleList = RoleRepository.GetActiveRoleBasedOnSequence() ?? new List<Role>();
                if(roleList.Count>0)
                cmbRole.ItemsSource = roleList.Where(x=>x.Status.ToUpper()=="ACTIVE");
                if (roleList.Count > 0) cmbRole.SelectedIndex = 0;

                cmbRole.SelectionChanged += CmbRole_SelectionChanged;

                LoadDefaultSubMenu();

                // safe to call — isDirty is false at startup so no prompt
                CmbRole_SelectionChanged(cmbRole, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "RoleAdministration Constructor Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (isChange)
            {
                var result = MessageBox.Show("Changes are made, do you want to save?",
                                             "Confirm", MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveSubMenu_Click(null, null);
                }
            }
        }

        private void LoadMenuItemsFromRepository()
        {
            try
            {
                var menuList = MenuItemRepository.GetAllMenuItems() ?? new List<Entity.MenuItem>();

                foreach (var menu in menuList.OrderBy(m => m.Parent_Menu).ThenBy(m => m.Sno))
                {
                    string parent = menu.Parent_Menu ?? string.Empty;
                    string child = menu.Child_Menu ?? string.Empty;
                    int menuId = menu.MenuID;

                    if (!ParentMenus.Contains(parent))
                        ParentMenus.Add(parent);

                    if (!SubMenus.ContainsKey(parent))
                        SubMenus[parent] = new ObservableCollection<SubMenuItem>();

                    SubMenus[parent].Add(new SubMenuItem { MenuID = menuId, Child_Menu = child, IsChecked = false });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadMenuItemsFromRepository Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void InitializeParentMenuButtons()
        {
            if (ParentMenus.Count > 0)
            {
                var orderedParents = ParentMenus.OrderBy(p => p == "Product" ? 0 : 1).ThenBy(p => p).ToList();

                ParentMenuButtonsPanel.Children.Clear();

                foreach (var parent in orderedParents)
                {
                    var btn = new Button
                    {
                        Content = parent,
                        Tag = parent,
                        Style = (Style)FindResource("ParentMenuButtonStyle")
                    };
                    btn.Click += ParentMenuButton_Click;
                    ParentMenuButtonsPanel.Children.Add(btn);
                }
            }
        }

        // Keep the click handler but delegate to SwitchToParentMenu
        private void ParentMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || !(btn.Tag is string parentMenu)) return;
            SwitchToParentMenu(parentMenu, suppressPrompt: false);
        }

        private void SwitchToParentMenu(string parentMenu, bool suppressPrompt)
        {
            // If we're not suppressing and there are unsaved changes -> prompt
            if (!suppressPrompt && isChange)
            {
                var result = MessageBox.Show("Changes are made, do you want to save?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveSubMenu_Click(null, null);
                }

            }

            // Suppress dirty prompts while we make programmatic changes (safe guard)
            var previousSuppress = suppressChangePrompt;
            suppressChangePrompt = true;
            try
            {
                // reset header checkbox visually and clear dirty (either saved or user chose No)
                chkSelectAll.IsChecked = false;
                isChange = false;

                if (!SubMenus.ContainsKey(parentMenu))
                {
                    MessageBox.Show($"No submenu found for '{parentMenu}'");
                    SubMenuDataGrid.ItemsSource = null;
                    CurrentSubMenuHeader = $"{parentMenu} Menus";
                    return;
                }

                CurrentSubMenuHeader = parentMenu + " Menus";

                // Set ItemsSource to the ObservableCollection for this parent
                SubMenuDataGrid.ItemsSource = SubMenus[parentMenu];

                // Sync checkbox states to selected role (this will set IsChecked programmatically)
                UpdateSubMenuCheckedStatusForRole();
            }  
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SwitchToParentMenu Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                suppressChangePrompt = previousSuppress;
            }
        }

        private void LoadDefaultSubMenu()
        {
            const string defaultParent = "Product";
            string toLoad = SubMenus.ContainsKey(defaultParent) ? defaultParent : (ParentMenus.FirstOrDefault() ?? null);

            if (toLoad != null && SubMenus.ContainsKey(toLoad))
            {
                CurrentSubMenuHeader = toLoad + " SubMenus";
                SubMenuDataGrid.ItemsSource = SubMenus[toLoad];
                UpdateSubMenuCheckedStatusForRole();
            }
        }

        private void UpdateSubMenuCheckedStatusForRole()
        {
            try { 
            if (!(cmbRole.SelectedItem is Role selectedRole)) return;

            // Load assigned IDs once
            var assignedMenuIds = RoleControlRepository
                                    .GetRoleControlsByRoleId(selectedRole.RoleID)
                                    .Select(r => r.MenuId)
                                    .ToHashSet();

            foreach (var kvp in SubMenus)
            {
                foreach (var item in kvp.Value)
                {
                    // Programmatic set — suppressDirtyPrompt protects us from marking dirty
                    item.IsChecked = assignedMenuIds.Contains(item.MenuID);
                }
            }
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UpdateSubMenuCheckedStatusForRole Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // called by checkbox click in the DataGrid cell (see note below about XAML)
        private void SubMenuCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (!suppressChangePrompt)
                isChange = true;
        }
        private void CmbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var newRole = cmbRole.SelectedItem as Role;

            if (_previousSelectedRole != null && isChange)
            {
                var result = MessageBox.Show("Changes are made, do you want to save?","Confirm", MessageBoxButton.YesNo,MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveSubMenuForRole(_previousSelectedRole);
                }
                //else if (result == MessageBoxResult.Cancel)
                //{
                //    e.Handled = true; // Cancel role change
                //    cmbRole.SelectionChanged -= CmbRole_SelectionChanged;
                //    cmbRole.SelectedItem = _previousSelectedRole;
                //    cmbRole.SelectionChanged += CmbRole_SelectionChanged;
                //    return;
                //}
            }

            _previousSelectedRole = newRole;
            isChange = false;

            if (newRole == null)
            {
                txtRoleDescription.Text = string.Empty;
            }
            else
            {
                txtRoleDescription.Text = newRole.Description ?? string.Empty;
            }

            UpdateSubMenuCheckedStatusForRole();

            var firstBtn = ParentMenuButtonsPanel.Children.OfType<Button>().FirstOrDefault();
            if (firstBtn != null)
            {
                ParentMenuButton_Click(firstBtn, null);
            }
        }


        private void CreateEditRole_Click(object sender, RoutedEventArgs e)
        {
            CreateEditRoleRequested?.Invoke(this, EventArgs.Empty);

            var roleManagement = new RoleManagement();

            // Find the hosting window
            var popup = Window.GetWindow(this) as UserManagementlibrary.Form.PopupWindow;
            if (popup != null)
            {
                popup.Title = "Role Management";
                popup.ShowContent(roleManagement);
            }
        }
        private void SaveSubMenuForRole(Role role)
        {
            if (role == null) return;
            try
            {

                IEnumerable<SubMenuItem> itemsEnumerable = SubMenuDataGrid.ItemsSource as IEnumerable<SubMenuItem>;
                var itemsList = (itemsEnumerable != null) ? itemsEnumerable.ToList() : SubMenuDataGrid.Items.Cast<SubMenuItem>().ToList();
                if (itemsList.Count == 0) return;
                var checkedMenus = itemsList.Where(m => m.IsChecked).Select(m => m.MenuID).ToList();
                var allMenus = itemsList.Select(m => m.MenuID).ToList();
                string userId = string.IsNullOrEmpty(SessionContext.UserId) ? "System" : SessionContext.UserId;

                RoleControlRepository.UpdateRoleControlsByMenuIds(role.RoleID, allMenus, checkedMenus, userId);

                isChange = false;
                MessageBox.Show($"Updated the access rights for role '{role.Role_Name}' successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "InsertAudit Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        private void SaveSubMenu_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedRole = cmbRole.SelectedItem as Role;
                if (selectedRole == null)
                {
                    MessageBox.Show("Please select a role first.");
                    return;
                }

                // Get displayed items (either ItemsSource or DataGrid rows)
                IEnumerable<SubMenuItem> itemsEnumerable = SubMenuDataGrid.ItemsSource as IEnumerable<SubMenuItem>;
                var itemsList = (itemsEnumerable != null)
                                    ? itemsEnumerable.ToList()
                                    : SubMenuDataGrid.Items.Cast<SubMenuItem>().ToList();

                if (itemsList.Count == 0)
                {
                    MessageBox.Show("No submenu items to save for the selected parent menu.");
                    return;
                }

                var checkedMenus = itemsList.Where(m => m.IsChecked).Select(m => m.MenuID).ToList();
                var allMenus = itemsList.Select(m => m.MenuID).ToList();
                string userId = string.IsNullOrEmpty(SessionContext.UserId) ? "System" : SessionContext.UserId;

                RoleControlRepository.UpdateRoleControlsByMenuIds(selectedRole.RoleID, allMenus, checkedMenus, userId);
                isChange = false; // reset
                MessageBox.Show("Updated the selected role's access rights successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "SaveSubMenu_Click Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        private void chkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            bool newValue = chkSelectAll.IsChecked == true;
            IEnumerable<SubMenuItem> itemsEnumerable = SubMenuDataGrid.ItemsSource as IEnumerable<SubMenuItem>;
            var itemsList = (itemsEnumerable != null)? itemsEnumerable.ToList(): SubMenuDataGrid.Items.Cast<SubMenuItem>().ToList();
            foreach (var item in itemsList)
            {
                item.IsChecked = newValue;
            }
            if (!suppressChangePrompt)
                isChange = true;
        }
    }

    public class SubMenuItem : INotifyPropertyChanged
    {
        private bool _isChecked;

        public int MenuID { get; set; }
        public string Child_Menu { get; set; }

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked == value) return;
                _isChecked = value;
                OnPropertyChanged(nameof(IsChecked));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
