using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace UserManagementLibray
{
    public partial class LogUserControl : UserControl
    {
        private readonly string mesLogPath;
        private readonly string runtimeLogPath;

        private FileSystemWatcher mesWatcher;
        private FileSystemWatcher runtimeWatcher;
        private DispatcherTimer refreshTimer;

        public LogUserControl()
        {
            try
            {
                InitializeComponent();

                string logBasePath = @"C:\Getech_Router_MES\SCADA\Log";
                string dateFolder = DateTime.Now.ToString("yyyyMMMdd"); // e.g., 2025Oct02

                mesLogPath = Path.Combine(logBasePath, dateFolder, "Log.txt");
                runtimeLogPath = Path.Combine(logBasePath, dateFolder, "Log.txt");

                StartWatchingLogs();
                LoadLogsInitially();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing LogUserControl: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadLogsInitially()
        {
            try
            {
                UpdateLog(mesLogPath, MesLogTextBox);
                UpdateLog(runtimeLogPath, RuntimeLogTextBox);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading logs: {ex.Message}", "LoadLogs Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartWatchingLogs()
        {
            try
            {
                mesWatcher = CreateWatcher(mesLogPath, MesLogTextBox);
                runtimeWatcher = CreateWatcher(runtimeLogPath, RuntimeLogTextBox);

                refreshTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(2)
                };
                refreshTimer.Tick += (s, e) =>
                {
                    try
                    {
                        UpdateLog(mesLogPath, MesLogTextBox);
                        UpdateLog(runtimeLogPath, RuntimeLogTextBox);
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"Error during refresh: {innerEx.Message}");
                    }
                };
                refreshTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting log watcher: {ex.Message}", "Watcher Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FileSystemWatcher CreateWatcher(string path, TextBox targetTextBox)
        {
            try
            {
                var directory = Path.GetDirectoryName(path);
                var filename = Path.GetFileName(path);

                if (!Directory.Exists(directory))
                {
                    MessageBox.Show($"Log directory does not exist: {directory}", "Watcher Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                var watcher = new FileSystemWatcher(directory, filename)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size
                };

                watcher.Changed += (s, e) =>
                {
                    try
                    {
                        UpdateLog(path, targetTextBox);
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"Watcher change error: {innerEx.Message}");
                    }
                };

                watcher.EnableRaisingEvents = true;
                return watcher;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating watcher for {path}: {ex.Message}", "Watcher Creation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        private void UpdateLog(string filePath, TextBox targetTextBox, int maxLines = 100)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    try
                    {
                        if (!File.Exists(filePath))
                        {
                            targetTextBox.Text = $"Log file not found: {filePath}";
                            return;
                        }

                        var lines = ReadLastLines(filePath, maxLines);
                        lines.Reverse();

                        targetTextBox.Text = string.Join(Environment.NewLine, lines);
                        targetTextBox.ScrollToHome();
                    }
                    catch (Exception innerEx)
                    {
                        targetTextBox.AppendText($"Error updating log: {innerEx.Message}\n");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "UpdateLog Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<string> ReadLastLines(string filePath, int lineCount)
        {
            var lines = new List<string>();
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs))
                {
                    while (!sr.EndOfStream)
                    {
                        lines.Add(sr.ReadLine());
                    }
                }
                return lines.Skip(Math.Max(0, lines.Count - lineCount)).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading log file {filePath}: {ex.Message}", "Read Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<string>();
            }
        }
    }
}
