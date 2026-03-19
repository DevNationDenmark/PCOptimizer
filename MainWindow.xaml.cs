using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PCOptimizer
{
    public partial class MainWindow : Window
    {
        private SystemMonitor _systemMonitor;
        private VirusScanner _virusScanner;
        private DiskCleaner _diskCleaner;
        private RAMOptimizer _ramOptimizer;

        public MainWindow()
        {
            InitializeComponent();
            _systemMonitor = new SystemMonitor();
            _virusScanner = new VirusScanner();
            _diskCleaner = new DiskCleaner();
            _ramOptimizer = new RAMOptimizer();
            
            // Initialize monitoring
            StartMonitoring();
            ShowDashboard();
        }

        private void StartMonitoring()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var stats = _systemMonitor.GetSystemStats();
                        Dispatcher.Invoke(() =>
                        {
                            CPUStatus.Text = $"CPU: {stats.CPUUsage:F1}%";
                            RAMStatus.Text = $"RAM: {stats.RAMUsage:F1}%";
                            TempStatus.Text = $"Temp: {stats.Temperature:F0}°C";
                        });
                        await Task.Delay(2000);
                    }
                    catch { }
                }
            });
        }

        private void ShowDashboard()
        {
            var dashboard = new Dashboard(_systemMonitor);
            ContentArea.Content = dashboard;
            StatusText.Text = "Dashboard - System Overview";
        }

        private void DashboardBtn_Click(object sender, RoutedEventArgs e) => ShowDashboard();

        private void VirusScanBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Running virus scan...";
            _virusScanner.StartScan();
        }

        private void DiskCleanBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Scanning disk for junk files...";
            _diskCleaner.ScanJunk();
        }

        private void RAMOptBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Optimizing RAM...";
            _ramOptimizer.OptimizeMemory();
        }

        private void StartupBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Startup Manager";
        }

        private void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Registry Cleaner";
        }

        private void ProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Process Manager";
        }

        private void MonitorBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "System Monitor";
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Settings";
        }

        private async void BoostBtn_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Running ONE-CLICK BOOST...";
            await Task.Run(() =>
            {
                _diskCleaner.ScanJunk();
                _diskCleaner.CleanJunk();
                _ramOptimizer.OptimizeMemory();
                _virusScanner.QuickScan();
            });
            StatusText.Text = "✓ Optimization complete!";
        }
    }

    public class Dashboard : UserControl
    {
        public Dashboard(SystemMonitor monitor)
        {
            var grid = new Grid();
            grid.Background = System.Windows.Media.Brushes.Transparent;
            grid.Margin = new Thickness(20);

            var stack = new StackPanel { Orientation = Orientation.Vertical };

            // Title
            var title = new TextBlock
            {
                Text = "System Dashboard",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stack.Children.Add(title);

            // Stats cards
            var stats = monitor.GetSystemStats();
            
            var cpuCard = CreateStatCard("CPU Usage", $"{stats.CPUUsage:F1}%", "#00d4ff");
            var ramCard = CreateStatCard("RAM Usage", $"{stats.RAMUsage:F1}%", "#00ff88");
            var diskCard = CreateStatCard("Disk Usage", $"{stats.DiskUsage:F1}%", "#ff6b6b");
            var tempCard = CreateStatCard("Temperature", $"{stats.Temperature:F0}°C", "#ffa500");

            var cardsPanel = new WrapPanel { Margin = new Thickness(0, 20, 0, 0) };
            cardsPanel.Children.Add(cpuCard);
            cardsPanel.Children.Add(ramCard);
            cardsPanel.Children.Add(diskCard);
            cardsPanel.Children.Add(tempCard);

            stack.Children.Add(cardsPanel);
            grid.Children.Add(stack);

            this.Content = grid;
        }

        private Border CreateStatCard(string label, string value, string color)
        {
            var brush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
            
            var border = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#2d2d2d")),
                BorderBrush = brush,
                BorderThickness = new Thickness(2),
                Margin = new Thickness(10),
                Padding = new Thickness(20),
                Width = 200,
                Height = 100
            };

            var stack = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center };
            
            var labelText = new TextBlock
            {
                Text = label,
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                FontSize = 12,
                Opacity = 0.7
            };

            var valueText = new TextBlock
            {
                Text = value,
                Foreground = brush,
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stack.Children.Add(labelText);
            stack.Children.Add(valueText);
            border.Child = stack;

            return border;
        }
    }
}
