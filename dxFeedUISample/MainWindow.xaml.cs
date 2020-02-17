using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using com.dxfeed.api;
using com.dxfeed.api.connection;
using com.dxfeed.api.data;
using dxFeedUISample.Controllers;
using dxFeedUISample.Models;
using dxFeedUISample.Services;

namespace dxFeedUISample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly ConnectionService _connectionService;
        private readonly UiEventPrinter _uiEventPrinter;

        public MainWindow()
        {
            _connectionService = new ConnectionService();
            _uiEventPrinter = new UiEventPrinter();

            InitializeComponent();

            LogListView.ItemsSource = _uiEventPrinter.GetLog();
            var timer = new DispatcherTimer{Interval = TimeSpan.FromMilliseconds(125)};
            timer.Tick += (sender, args) =>
            {
                if (LogListView.Items.Count > 0 && AutoScrollCheckBox.IsChecked.HasValue &&
                    AutoScrollCheckBox.IsChecked.Value)
                {
                    LogListView.ScrollIntoView(LogListView.Items[LogListView.Items.Count - 1]);
                }
            };
            timer.Start();

            if (Dispatcher != null)
                Dispatcher.ShutdownStarted += OnDispatcherOnShutdownStarted;
        }

        private void OnDispatcherOnShutdownStarted(object sender, EventArgs args)
        {
            _uiEventPrinter.Dispose();
            _connectionService.Dispose();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var item = SubscriptionItem.Create(SymbolTextBox.Text, TypeComboBox.SelectionBoxItem, SourceTextBox.Text);

            if (item.Equals(SubscriptionItem.InvalidItem) || !_connectionService.Connected)
            {
                return;
            }

            if (_connectionService.Subscribe(item, _uiEventPrinter))
            {
                SubscriptionListView.Items.Add(item);
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            _connectionService.Connect(AddressTextBox.Text, DisconnectListener, ConnectionStatusListener);
            ConnectButton.IsEnabled = false;
        }

        private void ConnectionStatusListener(IDxConnection connection, ConnectionStatus oldStatus,
            ConnectionStatus newStatus)
        {
            Console.WriteLine($@"{connection.ConnectedAddress}>> {oldStatus} -> {newStatus}");
            _uiEventPrinter.Print($@"{connection.ConnectedAddress}>> {oldStatus} -> {newStatus}");

            Dispatcher?.Invoke(() =>
            {
                Ping.ToolTip = newStatus.ToString();
                Ping.Fill = newStatus switch
                {
                    ConnectionStatus.NotConnected => new SolidColorBrush(Color.FromRgb(127, 127, 127)),
                    ConnectionStatus.Connected => new SolidColorBrush(Color.FromRgb(255, 127, 0)),
                    ConnectionStatus.LoginRequired => new SolidColorBrush(Color.FromRgb(127, 0, 0)),
                    ConnectionStatus.Authorized => new SolidColorBrush(Color.FromRgb(0, 127, 0)),
                    _ => throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, null)
                };
            });
        }

        private void DisconnectListener(IDxConnection connection)
        {
        }

        private void SubscriptionListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!(SubscriptionListView.SelectedItem is SubscriptionItem item) ||
                !_connectionService.Connected) return;

            if (_connectionService.Unsubscribe(item))
            {
                SubscriptionListView.Items.Remove(SubscriptionListView.SelectedItem);
            }
        }

        private void TypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SourceTextBox.IsEnabled = e.AddedItems != null && e.AddedItems.Count == 1 &&
                                      e.AddedItems[0].GetType() == typeof(EventType) &&
                                      EventType.Order.Equals(e.AddedItems[0]);
        }
    }
}