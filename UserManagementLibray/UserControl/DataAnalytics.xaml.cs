using LiveCharts;
using LiveCharts.Definitions.Charts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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

namespace UserManagementLibray
{

    public partial class DataAnalytics : UserControl
    {
        public DataAnalytics()
        {
            InitializeComponent();
            dpPanelStartDate.Value = DateTime.Today;
            dpPanelEndDate.Value = DateTime.Now;
            dpUPHStartDate.Value = DateTime.Today;
            dpUPHEndDate.Value = DateTime.Now;
            dpAlarmStartDate.Value = DateTime.Today;
            dpAlarmEndDate.Value = DateTime.Now;

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<int> RoleList = SessionContext.RoleIds;
                if (!RoleControlRepository.HasMonitoringAuditLogAccess(RoleList, "Monitoring", "Data Analytics"))
                {
                    MessageBox.Show("Access Denied. You don't have permission to open this window.", "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.IsEnabled = false;
                    return;
                }
                LoadCurrentData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCurrentData()
        {
            try
            {
                DateTime? panelStartDate = dpPanelStartDate.Value;
                DateTime? panelEndDate = dpPanelEndDate.Value;
                ShowDailyBarChartForPanelCount(panelStartDate, panelEndDate);

                DateTime? UPHStartDate = dpUPHStartDate.Value;
                DateTime? UPHEndDate = dpUPHEndDate.Value;
                ShowHourlyUPHChart(UPHStartDate.Value.AddHours(-1), UPHEndDate);

                DateTime? AlarmStartDate = dpAlarmStartDate.Value;
                DateTime? AlarmEndDate = dpAlarmEndDate.Value;
                ShowDailyAlarmChart(AlarmStartDate, AlarmEndDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Load Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private PanelChartData GetDummyPanelCountData()
        {
            var data = new PanelChartData
            {
                Labels = new List<string>(),
                TotalPass = new List<double>(),
                TotalReject = new List<double>(),
                TotalRework = new List<double>()
            };

            data.Labels.Add(DateTime.Now.ToString("dd MMM yyyy"));


            return data;
        }
        private UPHChartData GetDummyUPHData()
        {
            var data = new UPHChartData
            {
                Labels = new List<string>(),
                UPH = new List<double>()
            };

            data.Labels.Add(DateTime.Now.ToString("dd MMM yyyy"));


            return data;
        }
        private AlarmChartData GetDummyAlarmData()
        {
            var data = new AlarmChartData
            {
                Labels = new List<string>(),
                AlarmTotalCount = new List<double>()
            };

            data.Labels.Add(DateTime.Now.ToString("dd MMM yyyy"));


            return data;
        }

        private void BtnPanelCountGetData_Click(object sender, RoutedEventArgs e)
        {
            try { 
                DateTime? start = dpPanelStartDate.Value;
                DateTime? end = dpPanelEndDate.Value;

                if (!start.HasValue || !end.HasValue)
                {
                    MessageBox.Show("Please Select Start Or End dates.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (start.Value.Date > end.Value.Date)
                {
                    MessageBox.Show("Start date cannot be later than end date.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (rbPanelCountDaily.IsChecked == true && rbPanelCountHourly.IsChecked == false)
                {
                    ShowDailyBarChartForPanelCount(start, end);
                }
                else
                {
                    ShowHourlyBarChartForPanelCount(start, end);

                }
          }catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Panel Count Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void BtnUPHGetData_Click(object sender, RoutedEventArgs e)
        {
                try
                {
                    DateTime? start = dpUPHStartDate.Value;
                    DateTime? end = dpUPHEndDate.Value;

                    if (!start.HasValue || !end.HasValue)
                    {
                        MessageBox.Show("Please select start and end dates.");
                        return;
                    }

                    if (start.Value.Date > end.Value.Date)
                    {
                        MessageBox.Show("Start date cannot be later than end date.");
                        return;
                    }

                    if (rbUPHDaily.IsChecked == true && rbUPHHourly.IsChecked == false)
                    {
                        ShowDailyUPHChart(start, end);
                    }
                    else
                    {
                        ShowHourlyUPHChart(start, end);

                    }
                }catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "UPH Data Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }
        private void ShowDailyUPHChart(DateTime? start, DateTime? end)
        {
            try
            {
                if (!start.HasValue || !end.HasValue)
                {
                    MessageBox.Show("Please select start and end dates.");
                    return;
                }

                DateTime startDate = start.Value.Date;
                DateTime endDate = end.Value.Date.AddDays(1).AddSeconds(-1);

                if (startDate > endDate)
                {
                    MessageBox.Show("Start date cannot be later than end date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if ((endDate - startDate).Days > 14)
                {
                    MessageBox.Show("Daily Chart Can only display with 14 days.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;

                }

                var filteredRecords = DataAnalyticRepository.GetPanelCountSummaryForUPH(startDate, endDate, false).ToDictionary(r => r.Date.Date, r => r);

                List<DateTime> dayBlocks = new List<DateTime>();
                for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
                {
                    dayBlocks.Add(dt);
                }

                var labels = dayBlocks.Select(dt => dt.ToString("dd MMM yyyy")).ToList();
                var uphValues = new List<double>();

                foreach (var day in dayBlocks)
                {
                    uphValues.Add(filteredRecords.ContainsKey(day) ? filteredRecords[day].UPH : 0);
                }

                ChartUPH.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "UPH",
                        Values = new ChartValues<double>(uphValues),
                        Fill = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:F2}"
                    }
                };

                ChartUPH.AxisX.Clear();
                ChartUPH.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Daily",
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1,
                        IsEnabled = true
                    },
                    LabelsRotation = 0
                });
                ChartUPH.AxisY.Clear();
                ChartUPH.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "UPH",
                    MinValue = 0
                });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void ShowHourlyUPHChart(DateTime? start, DateTime? end)
        {
            try
            {
                if (!start.HasValue || !end.HasValue)
                {
                    MessageBox.Show("Please select start and end date/time.");
                    return;
                }

                DateTime startDate = start.Value;
                DateTime endDate = end.Value;

                if ((endDate - startDate).TotalHours > 24)
                {
                    MessageBox.Show("For hourly view, the date range cannot exceed 24 hours.");
                    return;
                }

                if (startDate >= endDate)
                {
                    MessageBox.Show("Start date/time must be earlier than end date/time.");
                    return;
                }

                var filteredRecords = DataAnalyticRepository.GetPanelCountSummaryForUPH(startDate, endDate, true)
                    .ToDictionary(r => r.Date, r => r);

                List<DateTime> hourBlocks = new List<DateTime>();
                for (var dt = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0);
                     dt <= endDate;
                     dt = dt.AddHours(1))
                {
                    hourBlocks.Add(dt);
                }

                var labels = hourBlocks.Select(dt => $"{dt:HH}:00-{dt.AddHours(1):HH}:00").ToList();
                var uphValues = new List<double>();

                foreach (var hour in hourBlocks)
                {
                    if (filteredRecords.ContainsKey(hour))
                    {
                        uphValues.Add(filteredRecords[hour].UPH);
                    }
                    else
                    {
                        uphValues.Add(0); // Fill missing hours with zero
                    }
                }

                // ✅ Update chart series
                ChartUPH.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "UPH",
                        Values = new ChartValues<double>(uphValues),
                        Fill = new SolidColorBrush(Color.FromRgb(0, 122, 204)),
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y:F2}"
                    }
                };

                // ✅ X Axis
                ChartUPH.AxisX.Clear();
                ChartUPH.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Hour Range",
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1,
                        IsEnabled = true
                    },
                    LabelsRotation = 0
                });

                // ✅ Y Axis (always start at 0 to avoid floating middle zeros)
                ChartUPH.AxisY.Clear();
                ChartUPH.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "UPH",
                    MinValue = 0
                });

                ChartUPH.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void ShowDailyBarChartForPanelCount(DateTime? start, DateTime? end)
        {
            try
            {
                if (!start.HasValue || !end.HasValue)
                {
                    MessageBox.Show("Please select start and end dates.");
                    return;
                }

                DateTime startDate = start.Value.Date;
                DateTime endDate = end.Value.Date.AddDays(1).AddSeconds(-1);
                if (startDate > endDate)
                {
                    //MessageBox.Show("Start date cannot be later than end date.");
                    MessageBox.Show("Start date cannot be later than end date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if ((endDate - startDate).Days > 14)
                {
                    MessageBox.Show("Daily Chart Can only display with 14 days.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;

                }

                var filteredRecords = DataAnalyticRepository
                    .GetPanelCountSummaryDaily(startDate, endDate)
                    .ToDictionary(r => r.Date.Date, r => r);

                List<DateTime> dayBlocks = new List<DateTime>();
                for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
                {
                    dayBlocks.Add(dt);
                }

                var labels = dayBlocks.Select(dt => dt.ToString("dd MMM yyyy")).ToList();
                var passValues = new List<double>();
                var rejectValues = new List<double>();
                var reworkValues = new List<double>();

                foreach (var day in dayBlocks)
                {
                    if (filteredRecords.ContainsKey(day))
                    {
                        passValues.Add(filteredRecords[day].TotalPass);
                        rejectValues.Add(filteredRecords[day].TotalReject);
                        reworkValues.Add(filteredRecords[day].TotalRework);
                    }
                    else
                    {
                        passValues.Add(0);
                        rejectValues.Add(0);
                        reworkValues.Add(0);
                    }
                }

                ChartPanelCount.Series = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Total Pass",
                        Values = new ChartValues<double>(passValues),
                        Fill = new SolidColorBrush(Color.FromRgb(34, 177, 76)),
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y}"
                    },
                    new ColumnSeries
                    {
                        Title = "Total Reject",
                        Values = new ChartValues<double>(rejectValues),
                        Fill = new SolidColorBrush(Color.FromRgb(237, 28, 36)),
                        DataLabels = true,
                         LabelPoint = point => $"{point.Y}"
                    },
                    new ColumnSeries
                    {
                        Title = "Total Rework",
                        Values = new ChartValues<double>(reworkValues),
                        Fill = new SolidColorBrush(Color.FromRgb(255, 127, 39)),
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y}"
                    }
                };

                ChartPanelCount.LegendLocation = LegendLocation.Top;
                //ChartPanelCount.AxisX.Clear();
                //ChartPanelCount.AxisX.Add(new LiveCharts.Wpf.Axis
                //{
                //    Title = "Daily",
                //    Labels = labels,
                //    Separator = new LiveCharts.Wpf.Separator
                //    {
                //        Step = 1,
                //        IsEnabled = true
                //    },
                //    LabelsRotation = 0
                //});

                //double maxY = Math.Max(
                //    Math.Max(passValues.DefaultIfEmpty(0).Max(),
                //             rejectValues.DefaultIfEmpty(0).Max()),
                //    reworkValues.DefaultIfEmpty(0).Max());

                //ChartPanelCount.AxisY.Clear();
                //ChartPanelCount.AxisY.Add(new LiveCharts.Wpf.Axis
                //{
                //    Title = "Count",
                //    MinValue = 0,
                //    MaxValue = maxY == 0 ? 10 : double.NaN // fallback range if all values = 0
               // });
                ChartPanelCount.AxisX.Clear();
                ChartPanelCount.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Daily",
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1,
                        IsEnabled = true
                    },
                    LabelsRotation = 0
                });

                double maxY = Math.Max(Math.Max(passValues.DefaultIfEmpty(0).Max(),rejectValues.DefaultIfEmpty(0).Max()),reworkValues.DefaultIfEmpty(0).Max());

                // ✅ Y Axis (always start at 0 to avoid floating middle zeros)
                ChartPanelCount.AxisY.Clear();
                ChartPanelCount.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Count",
                    MinValue = 0,
                    MaxValue = maxY == 0 ? 10 : double.NaN // fallback range if all values = 0
                                                           // MaxValue = maxY
                });
                ChartPanelCount.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ShowHourlyBarChartForPanelCount(DateTime? start, DateTime? end)
        {
            try
            {
                if (!start.HasValue || !end.HasValue)
                {
                    MessageBox.Show("Please select start and end date/time.");
                    return;
                }

                DateTime startDate = start.Value;
                DateTime endDate = end.Value;
                if ((endDate - startDate).TotalHours > 24)
                {
                    MessageBox.Show("For hourly view, the date range cannot exceed 24 hours.");
                    return;
                }

                if (startDate >= endDate)
                {
                    MessageBox.Show("Start date/time must be earlier than end date/time.");
                    return;
                }

                var filteredRecords = DataAnalyticRepository
                    .GetPanelCountSummaryHourly(startDate, endDate)
                    .ToDictionary(r => r.Date, r => r);

                List<DateTime> hourBlocks = new List<DateTime>();
                for (var dt = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0);
                     dt <= endDate;
                     dt = dt.AddHours(1))
                {
                    hourBlocks.Add(dt);
                }

                var labels = hourBlocks
                    .Select(dt => $"{dt:HH}:00-{dt.AddHours(1):HH}:00")
                    .ToList();

                var passValues = new List<double>();
                var rejectValues = new List<double>();
                var reworkValues = new List<double>();

                foreach (var hour in hourBlocks)
                {
                    if (filteredRecords.ContainsKey(hour))
                    {
                        passValues.Add(filteredRecords[hour].TotalPass);
                        rejectValues.Add(filteredRecords[hour].TotalReject);
                        reworkValues.Add(filteredRecords[hour].TotalRework);
                    }
                    else
                    {
                        passValues.Add(0);
                        rejectValues.Add(0);
                        reworkValues.Add(0);
                    }
                }

                // ✅ Chart series
                ChartPanelCount.Series = new SeriesCollection
        {
            new ColumnSeries
            {
                Title = "Total Pass",
                Values = new ChartValues<double>(passValues),
                Fill = new SolidColorBrush(Color.FromRgb(34, 177, 76)),
                DataLabels = true,
                LabelPoint = point => $"{point.Y}"
            },
            new ColumnSeries
            {
                Title = "Total Reject",
                Values = new ChartValues<double>(rejectValues),
                Fill = new SolidColorBrush(Color.FromRgb(237, 28, 36)),
                DataLabels = true,
                LabelPoint = point => $"{point.Y}"
            },
            new ColumnSeries
            {
                Title = "Total Rework",
                Values = new ChartValues<double>(reworkValues),
                Fill = new SolidColorBrush(Color.FromRgb(255, 127, 39)),
                DataLabels = true,
                LabelPoint = point => $"{point.Y}"
            }
        };

                ChartPanelCount.LegendLocation = LegendLocation.Top;
                ChartPanelCount.AxisX.Clear();
                ChartPanelCount.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Hour Range",
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1,
                        IsEnabled = true
                    },
                    LabelsRotation = 0
                });
                double maxY = Math.Max(
                    Math.Max(passValues.DefaultIfEmpty(0).Max(),
                             rejectValues.DefaultIfEmpty(0).Max()),
                    reworkValues.DefaultIfEmpty(0).Max());

                ChartPanelCount.AxisY.Clear();
                ChartPanelCount.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Count",
                    MinValue = 0,
                    MaxValue = maxY == 0 ? 10 : double.NaN
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading hourly chart: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ShowDailyAlarmChart(DateTime? start, DateTime? end)
        {
            string groupBy = "DATE(RaiseTime)";

            try
            {
                if (!start.HasValue || !end.HasValue)
                {
                    MessageBox.Show("Please select start and end dates.");
                    return;
                }

                DateTime startDate = start.Value.Date;
                DateTime endDate = end.Value.Date.AddDays(1).AddSeconds(-1);

                if (startDate > endDate)
                {
                    MessageBox.Show("Start date cannot be later than end date.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if ((endDate - startDate).Days > 14)
                {
                    MessageBox.Show("Daily Chart Can only display with 14 days.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;

                }

                var filteredRecords = AlarmRepository.GetAlarmsGrouped(startDate, endDate, groupBy)
                    .ToDictionary(r => r.GroupedTime?.Date ?? DateTime.MinValue, r => r);

                List<DateTime> dayBlocks = new List<DateTime>();
                for (var dt = startDate; dt <= endDate; dt = dt.AddDays(1))
                {
                    dayBlocks.Add(dt);
                }

                var labels = dayBlocks.Select(dt => dt.ToString("dd MMM yyyy")).ToList();
                var alarmCounts = new List<int>();

                foreach (var day in dayBlocks)
                {
                    if (filteredRecords.ContainsKey(day))
                    {
                        alarmCounts.Add(filteredRecords[day].Count);
                    }
                    else
                    {
                        alarmCounts.Add(0);
                    }
                }

                ChartAlarm.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Total Alarm",
                    Values = new ChartValues<int>(alarmCounts),
                    Fill = new SolidColorBrush(Color.FromRgb(237, 28, 36)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y}"
                }
            };
                // ✅ X Axis
                ChartAlarm.AxisX.Clear();
                ChartAlarm.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Daily",
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1,
                        IsEnabled = true
                    },
                    LabelsRotation = 0
                });

                // ✅ Y Axis (always start at 0 to avoid floating middle zeros)
                ChartAlarm.AxisY.Clear();
                ChartAlarm.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Total Alarm",
                    MinValue = 0
                });
                //ChartAlarm.AxisX.Clear();
                //ChartAlarm.AxisX.Add(new LiveCharts.Wpf.Axis
                //{
                //    Title = "Date",
                //    Labels = labels,
                //    Separator = new LiveCharts.Wpf.Separator
                //    {
                //        Step = 1,
                //        IsEnabled = true
                //    },
                //    LabelsRotation = 0
                //});

                //ChartAlarm.AxisY.Clear();
                //ChartAlarm.AxisY.Add(new LiveCharts.Wpf.Axis
                //{
                //    Title = "Total Alarm"
                //});

                //ChartAlarm.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ShowHourlyAlarmChart(DateTime? start, DateTime? end)
        {
            string groupBy = "strftime('%Y-%m-%d %H:00:00', RaiseTime)"; // group by hour
            try
            {
                if (!start.HasValue || !end.HasValue)
                {
                    MessageBox.Show("Please select start and end date/time.");
                    return;
                }

                DateTime startDate = start.Value;
                DateTime endDate = end.Value;

                if ((endDate - startDate).TotalHours > 24)
                {
                    MessageBox.Show("For hourly view, the date range cannot exceed 24 hours.");
                    return;
                }

                if (startDate >= endDate)
                {
                    MessageBox.Show("Start date/time must be earlier than end date/time.");
                    return;
                }

                var filteredRecords = AlarmRepository.GetAlarmsGrouped(startDate, endDate, groupBy)
                    .ToDictionary(r => r.GroupedTime?.Date.AddHours(r.GroupedTime?.Hour ?? 0) ?? DateTime.MinValue, r => r);

                List<DateTime> hourBlocks = new List<DateTime>();
                for (var dt = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0); dt <= endDate; dt = dt.AddHours(1))
                {
                    hourBlocks.Add(dt);
                }

                var labels = hourBlocks.Select(dt => $"{dt:HH}:00-{dt.AddHours(1):HH}:00").ToList();
                var alarmCounts = new List<int>();

                foreach (var hour in hourBlocks)
                {
                    if (filteredRecords.ContainsKey(hour))
                    {
                        alarmCounts.Add(filteredRecords[hour].Count);
                    }
                    else
                    {
                        alarmCounts.Add(0);
                    }
                }

                ChartAlarm.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Total Alarm",
                    Values = new ChartValues<int>(alarmCounts),
                    Fill = new SolidColorBrush(Color.FromRgb(237, 28, 36)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y}"
                }
            };

                ChartAlarm.AxisX.Clear();
                ChartAlarm.AxisX.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Hour Range",
                    Labels = labels,
                    Separator = new LiveCharts.Wpf.Separator
                    {
                        Step = 1,
                        IsEnabled = true
                    },
                    LabelsRotation = 0
                });

                ChartAlarm.AxisY.Clear();
                ChartAlarm.AxisY.Add(new LiveCharts.Wpf.Axis
                {
                    Title = "Total Alarm",
                    MinValue = 0
                });
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        private void BtnAlarmGetData_Click(object sender, RoutedEventArgs e)
        {
            try { 
                    
                    DateTime? start = dpAlarmStartDate.Value;
                    DateTime? end = dpAlarmEndDate.Value;

                    if (!start.HasValue || !end.HasValue)
                    {
                        MessageBox.Show("Please select start and end dates.");
                        return;
                    }

                    if (start.Value.Date > end.Value.Date)
                    {
                        MessageBox.Show("Start date cannot be later than end date.");
                        return;
                    }

                    if (rbAlarmDaily.IsChecked == true && rbAlarmHourly.IsChecked == false)
                    {
                        ShowDailyAlarmChart(start, end);
                    }
                    else
                    {
                        ShowHourlyAlarmChart(start, end);

                    }
                 }catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Alarm Count Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
}
        private void LoadPanelCountChart(PanelChartData data)
        {
            ChartPanelCount.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Total Pass",
                    Values = new ChartValues<double>(data.TotalPass)
                },
                new ColumnSeries
                {
                    Title = "Total Reject",
                    Values = new ChartValues<double>(data.TotalReject)
                },
                new ColumnSeries
                {
                    Title = "Total Rework",
                    Values = new ChartValues<double>(data.TotalRework)
                }
            };

            ChartPanelCount.AxisX.Clear();
            ChartPanelCount.AxisX.Add(new Axis
            {
                Title = "Date",
                Labels = data.Labels
            });

            ChartPanelCount.AxisY.Clear();
            ChartPanelCount.AxisY.Add(new Axis
            {
                Title = "Count",
                LabelFormatter = value => value.ToString("N0")
            });
        }
        private void LoadUPHChart(UPHChartData data)
        {
            ChartUPH.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "UPH",
                    Values = new ChartValues<double>(data.UPH)
                }
            };

            ChartUPH.AxisX.Clear();
            ChartUPH.AxisX.Add(new Axis
            {
                Title = "Date",
                Labels = data.Labels
            });

            ChartUPH.AxisY.Clear();
            ChartUPH.AxisY.Add(new Axis
            {
                Title = "UPH Value",
                LabelFormatter = value => value.ToString("N2") 
            });
        }
        private void LoadAlarmChart(AlarmChartData data)
        {
            ChartAlarm.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Total Alarm",
                    Fill = new SolidColorBrush(Color.FromRgb(237, 28, 36)),
                    Values = new ChartValues<double>(data.AlarmTotalCount)
                }
            };

            ChartAlarm.AxisX.Clear();
            ChartAlarm.AxisX.Add(new Axis
            {
                Title = "Date",
                Labels = data.Labels
            });

            ChartAlarm.AxisY.Clear();
            ChartAlarm.AxisY.Add(new Axis
            {
                Title = "Total Alarm",
                LabelFormatter = value => value.ToString("N0")
            });
        }
        private void LoadUPHChart(AlarmChartData data)
        {
            ChartUPH.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Total Alarm",
                    Values = new ChartValues<double>(data.AlarmTotalCount)
                }
            };

            ChartUPH.AxisX.Clear();
            ChartUPH.AxisX.Add(new Axis
            {
                Title = "Date",
                Labels = data.Labels
            });

            ChartUPH.AxisY.Clear();
            ChartUPH.AxisY.Add(new Axis
            {
                Title = "Total Alarm",
                LabelFormatter = value => value.ToString("N0")
            });
        }
    }
}
