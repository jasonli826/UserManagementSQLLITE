using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
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
using UserManagementLibray.Entity;
using UserManagementLibray.Repository;

namespace UserManagementLibray
{
    /// <summary>
    /// Interaction logic for AuditLog.xaml
    /// </summary>
    public partial class AuditLog : UserControl
    {
        public AuditLog()
        {
            InitializeComponent();
            LoadAuditLogs(DateTime.Today,string.Empty);

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
          try { 
            List<int> RoleList = SessionContext.RoleIds;
            if (!RoleControlRepository.HasMonitoringAuditLogAccess(RoleList, "Monitoring", "Audit Log"))
            {
                MessageBox.Show("Access Denied. You don't have permission to open this window.","Permission Denied",MessageBoxButton.OK,MessageBoxImage.Error);
                this.IsEnabled = false;

            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Audit Log Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        

        }
        private void DpLogDate_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (dpLogDate.SelectedDate.HasValue)
            {
                dpLogDate.Text = dpLogDate.SelectedDate.Value.ToString("dd MMM yyyy");
            }
               dpLogDate.SelectedDate = DateTime.Today;
        }
        private void txtKeyword_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearch_Click(sender, e); 
            }
        }
        private void DpLogDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpLogDate.SelectedDate.HasValue)
            {
                dpLogDate.Text = dpLogDate.SelectedDate.Value.ToString("dd MMM yyyy");
            }
        }
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void OnPropertyChanged(string name)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        //}

        private void LoadAuditLogs(DateTime? date = null, string keyword = null)
        {
            try { 
                List<Audit> logs = null;

                if (date.HasValue)
                {
                    DateTime targetDate = date.Value.Date; // Normalize to midnight
                    logs = AuditRepository.GetFilteredAudit(targetDate,keyword.ToLower());
                }

                AuditLogDataGrid.ItemsSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LoadAuditLogs Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
}
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            DateTime? selectedDate = dpLogDate.SelectedDate;
            string keyword = txtKeyword.Text.Trim();
            LoadAuditLogs(selectedDate,keyword);

        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            dpLogDate.SelectedDate = DateTime.Now;
            txtKeyword.Text = string.Empty;
            LoadAuditLogs();
        }
    }
}
