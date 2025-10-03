using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using UserManagementlibrary.Entity;
using UserManagementlibrary.Repository;

namespace UserManagementlibrary
{
    /// <summary>
    /// Interaction logic for DomainUserControl.xaml
    /// </summary>
    public partial class DomainUserControl : UserControl
    {
        Domain domain = null;
        private static int domainId = 0;
        private ObservableCollection<Domain> DomainList = new ObservableCollection<Domain>();
        public DomainUserControl()
        {
            InitializeComponent();
            LoadDomains();
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Example: Assume you store current logged-in user role in a static Session/UserContext
            string currentUserRole = SessionContext.UserRole; // e.g., "SystemAdmin", "Engineer", etc.
            if (string.IsNullOrEmpty(currentUserRole) || !currentUserRole.ToUpper().Contains("System Administrator".ToUpper()))
            {
                MessageBox.Show("Access Denied. Only System Admin can access this page.",
                                "Permission Denied",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                this.IsEnabled = false;

            }
        }
        private void LoadDomains()
        {
            try
            {
                DomainList.Clear();
                var Domains = DomainRepository.GetAllDomains();
                foreach (var d in Domains)
                    DomainList.Add(d);

                DomainDataGrid.ItemsSource = DomainList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Domain Window Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
        #region Button Clicks

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearData();
        }
        private void ChkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            bool checkAll = chkSelectAll.IsChecked ?? false;
            foreach (var r in DomainList)
                r.IsSelected = checkAll;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string domainName = txtDomainName.Text.Trim();
            string cn = txtCN.Text.Trim();
            string dc2 = txtDC2.Text.Trim();
            string dc3 = txtDC3.Text.Trim();
            string domainDesc = txtDescription.Text.Trim();
            string domainName2 = txtDomainNameRow2.Text.Trim();
            try
            {

                if (string.IsNullOrEmpty(domainName))
                {
                    MessageBox.Show("Domain Cannot be Empty.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                }
                if (string.IsNullOrEmpty(domainName2))
                {
                    MessageBox.Show("Domain Name Cannot be Empty.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;

                }
                if (btnSave.Content.ToString().ToUpper() == "UPDATE")
                {
                    if (domain != null)
                    {

                        domain.DomainName = domainName;
                        domain.CN = cn;
                        domain.DC2 = dc2;
                        domain.DC3 = dc3;
                        domain.DC1 = domainName + "." + cn + "." + dc2 + "." + dc3;
                        domain.Description = domainDesc;
                        domain.DomainNme = domainName2;
                        if (DomainRepository.IsDomainExistsById(domainName2, domainId))
                        {
                            MessageBox.Show(domain.DomainName + " already exsits");
                            return;
                        }
                        else
                        {
                            domain.Updated_by = SessionContext.UserId;
                            domain.Updated_Date = DateTime.Now;
                            DomainRepository.UpdateDomain(domain);

                        }

                        MessageBox.Show("Update The Domain Successfully");
                    }


                }
                else
                {
                    Domain newDomain = new Domain
                    {
                        DomainName = domainName,
                        CN = cn,
                        DC1 = domainName + "." + cn + "." + dc2 + "." + dc3,
                        DC2=dc2,
                        DC3=dc3,
                        Description = domainDesc,
                        Created_by = SessionContext.UserId,
                        Created_Date = DateTime.Now,
                        Updated_by = SessionContext.UserId,
                        Updated_Date = DateTime.Now,
                        Status = "Active",
                        DomainNme = domainName2
                    };
                    if (DomainRepository.IsDomainExists(newDomain.DomainName))
                    {
                        MessageBox.Show("Domain Name Already Exists");
                        return;
                    
                    }
                    DomainRepository.InsertDomain(newDomain);

                    MessageBox.Show("Create The Domain Successfully");
                }
                ClearData();
            }catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save Domain Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }
        private void ClearData()
        {
            btnSave.Content = "Save";
            txtDomainName.Text = string.Empty;
            txtDomainNameRow2.Text = string.Empty;
            txtCN.Text = string.Empty;
            txtDC2.Text = string.Empty;
            txtDC3.Text = string.Empty;
            LoadDomains();
            txtDescription.Text = string.Empty;
            txtDomainName.IsReadOnly = false;

        }
        private void RowCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var chk = sender as CheckBox;
            if (chk?.DataContext is Domain domain)
            {
                domain.IsSelected = chk.IsChecked ?? false;
                if (!domain.IsSelected)
                {
                    chkSelectAll.IsChecked = false;
                }
                else
                {
                    if (DomainList.All(d => d.IsSelected))
                    {
                        chkSelectAll.IsChecked = true;
                    }
                }
            }
        }


        private void BtnActive_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DomainDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                DomainDataGrid.CommitEdit();
                var selectedDomains = DomainList.Where(r => r.IsSelected).ToList();
                if (!selectedDomains.Any())
                {
                    MessageBox.Show("Please select at least one domain to activate.");
                    return;
                }

                foreach (var r in selectedDomains)
                {
                    r.Status = "Active";
                    DomainRepository.UpdateDomain(r);
                }

                LoadDomains();
                MessageBox.Show("Selected domain(s) have been activated.");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Active Domain Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected row data
            var button = sender as Button;
            if (button != null)
            {
               domain= button.DataContext as Domain; // Replace 'User' with your data class
                if (domain != null)
                {
                    txtDomainName.Text= domain.DomainName;
                    txtDomainNameRow2.Text = domain.DomainNme;
                    txtCN.Text = domain.CN;
                    txtDC2.Text = domain.DC2;
                    txtDC3.Text = domain.DC3;
                    txtDescription.Text = domain.Description;
                    btnSave.Content = "Update";
                    domainId = domain.DomainID;
                    txtDomainName.IsReadOnly = true;

                }
            }
        }
        private void BtnInactive_Click(object sender, RoutedEventArgs e)
        {
            try { 
                    DomainDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
                    DomainDataGrid.CommitEdit();

                    var selectedDomains = DomainList.Where(r => r.IsSelected).ToList();
                    if (!selectedDomains.Any())
                    {
                        MessageBox.Show("Please select at least one domain to inactivate.");
                        return;
                    }

                    foreach (var r in selectedDomains)
                    {
                        if (r.DomainName.Equals("LOCAL DOMAIN", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("Local Domain cannot be deactivated.");
                            return; 
                        }
                    }

                    foreach (var r in selectedDomains)
                    {
                        r.Status = "Inactive";
                        DomainRepository.UpdateDomain(r);

                    }
                    DomainDataGrid.Items.Refresh();
                    LoadDomains();
                    MessageBox.Show("Selected domain(s) have been Inactivated.");
                }catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message, "Deactive Domain Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

}

//private void DomainDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
//        {

//        }
 }

        #endregion

       
}
