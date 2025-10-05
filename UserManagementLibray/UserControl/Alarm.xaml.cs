using LiveCharts.Wpf;
using LiveCharts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using UserManagementlibrary.Repository;
using UserManagementLibray.Entity;
using System.Runtime.InteropServices.ComTypes;

namespace UserManagementlibrary
{

    public partial class Alarm : UserControl
    {
        private List<AlarmMessage> filterAlarms;

        public Alarm()
        {
            InitializeComponent();
            dpStartDate.SelectedDate = DateTime.Today;
            dpEndDate.SelectedDate = DateTime.Today;
            dpPieStartDate.SelectedDate = DateTime.Today;
            dpPieEndDate.SelectedDate = DateTime.Today;
            LoadAlarms();

        }

        private void LoadAlarms()
        {
            try
            {
                filterAlarms = AlarmRepository.GetFilteredAlarms(dpStartDate.SelectedDate, dpEndDate.SelectedDate, string.Empty);
                dgAlarms.ItemsSource = filterAlarms;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Alarm Window Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            dpStartDate.SelectedDate = DateTime.Today;
            dpEndDate.SelectedDate = DateTime.Today;
            txtKeyword.Text = "";
            
            LoadAlarms();
        }
        private void txtKeyword_Keydown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchButton_Click(sender, e);
            }
        }

        private void SearchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DateTime? startDate = dpStartDate.SelectedDate;
                DateTime? endDate = dpEndDate.SelectedDate;
                string keyword = txtKeyword.Text?.Trim().ToLower();
                var filtered = AlarmRepository.GetFilteredAlarms(startDate, endDate,keyword);

                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        MessageBox.Show("Please select start and end dates.");
                        return;
                    }

                    if (startDate.Value.Date > endDate.Value.Date)
                    {
                        MessageBox.Show("Start date cannot be later than end date.");
                        return;
                    }


                dgAlarms.ItemsSource = filtered;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Search Alarm Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPieChart_Click(object sender, RoutedEventArgs e)
        {
            LoadPieChart();
        }

private void LoadPieChart()
{

    DateTime? start = dpPieStartDate.SelectedDate;
    DateTime? end = dpPieEndDate.SelectedDate;
    var filteredData = AlarmRepository.GetFilteredAlarms(start, end, string.Empty);

    var groupedData = filteredData.GroupBy(a => new { a.Alarm, a.Alarm_Description }).Select(g => new
                                                                        {
                                                                            AlarmCode = g.Key.Alarm.ToString(),
                                                                            Description = g.Key.Alarm_Description,
                                                                            Count = g.Count()
                                                                        }).ToList();

    pieChart.Series = new SeriesCollection();

    foreach (var item in groupedData)
    {
        pieChart.Series.Add(new PieSeries
        {
            Title = $"{item.AlarmCode} - {item.Description}", // Legend text
            Values = new ChartValues<int> { item.Count },
            DataLabels = true,
           // LabelPoint = chartPoint => $"{item.AlarmCode}: {chartPoint.Y}"
           LabelPoint = chartPoint =>$"{item.AlarmCode}: {chartPoint.Y} ({chartPoint.Participation:P})",

        });
    }
            pieChart.ToolTip = null;
            pieChart.Hoverable = false;
            pieChart.DataTooltip = new DefaultTooltip
            {
                SelectionMode = TooltipSelectionMode.OnlySender
            };
        }





private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    var tabControl = sender as TabControl;
    if (tabControl == null) return;

    var selectedTab = tabControl.SelectedItem as TabItem;
    if (selectedTab == null) return;

    if (selectedTab.Header.ToString() == "Alarm Pie Chart")
    {
        dpPieStartDate.SelectedDate = dpStartDate.SelectedDate;
        dpPieEndDate.SelectedDate = dpEndDate.SelectedDate;
        LoadPieChart();
    }
}

    }

    
}
