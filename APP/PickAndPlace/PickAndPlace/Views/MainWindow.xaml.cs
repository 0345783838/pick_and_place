using PickAndPlace.Controller;
using PickAndPlace.Controllers;
using PickAndPlace.Models;
using PickAndPlace.Services;
using PickAndPlace.Views.EyeHand2dCalibWindows;
using PickAndPlace.Views.UtilitiesWindows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace PickAndPlace.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private Properties.Settings _param = Properties.Settings.Default;
        public event PropertyChangedEventHandler PropertyChanged;

        public AppLogger Logger => AppLogger.Instance;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private MainController _mainController;

        public int AiStatus { get; set; } = (int)(StatusState.Unknown);

        public MainWindow()
        {
            InitializeComponent();
            _mainController = new MainController(this);
            DataContext = this;
            Logger.Logs.CollectionChanged += Logs_CollectionChanged;
        }
        private void Logs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action !=
                System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                return;

            if (LogListBox.Items.Count == 0)
                return;

            // Scroll sau khi UI render xong
            LogListBox.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    LogListBox.ScrollIntoView(
                        LogListBox.Items[LogListBox.Items.Count - 1]);
                }),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            new Task(new Action(() =>
            {
                var res = _mainController.RunServiceAsync(20000, "Program is loading...");
            })).Start();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCalibIntrinsic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCalibEyeToHand2D_Click(object sender, RoutedEventArgs e)
        {
            var window = new EyeHand2dCalibWindow();
            window.ShowDialog();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            WaitingWindow wait = new WaitingWindow("Checking running conditions...\rKiểm tra điều kiện chạy");
            bool startOK = false;
            new Task(() =>
            {
                startOK = _mainController.Start();
                wait.KillMe = true;
            }).Start();
            wait.ShowDialog();

            if (startOK)
            {
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
                btnTest.IsEnabled = true;
            }
            else
            {
                btnStart.IsEnabled = true;
                btnStop.IsEnabled = false;
                btnTest.IsEnabled = false;
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {

        }

        internal void SetLoadingService(string content)
        {
            var timeout = 10000;
            new Thread(() =>
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    WaitingWindow wait = new WaitingWindow(content);
                    new Task(() =>
                    {
                        var timestep = timeout / 500;
                        for (int i = 0; i < timestep; i++)
                        {
                            Thread.Sleep(500);
                            if (_mainController._serviceIsRun)
                            {
                                break;
                            }
                        }
                        wait.KillMe = true;
                        UpdateAIStatus(true);
                        if (!_mainController._serviceIsRun)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                UpdateAIStatus(false);
                                var box = new ErrorWindow("Cannot start AI service! Please contact IT!\rKhông khởi động được AI, Hãy liên hệ bộ phận PI");
                                box.ShowDialog();
                            }));
                        }
                    }).Start();
                    wait.ShowDialog();
                }));
            }).Start();
        }
        private void UpdateAIStatus(bool resAI)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                AiStatus = resAI ? (int)(StatusState.Ok) : (int)(StatusState.Ng);
                OnPropertyChanged(nameof(AiStatus));
            }));
        }
     
        public bool ShowWarning(string content)
        {
            var res = false;
            this.Dispatcher.Invoke(new Action(() =>
            {
                var box = new WarningWindow(content);
                box.ShowDialog();
                res = (bool)box.DialogResult;
            }));
            return res;
        }
        public void ShowError(string content)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                ErrorService.ShowError(content);
            }));
        }
        public void ShowInfo(string content)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                var box = new InformationWindow(content);
                box.ShowDialog();
            }));
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            _mainController.ProcessImage());
        }
        public void UpdateImage(System.Drawing.Bitmap image)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                if (image == null)
                {
                    imbImage.Source = null;
                }
                else if (imbImage.Source == null)
                {
                    imbImage.SourceFromBitmap = image;
                    var scale = GetFittedZoomScale(imbImage, image.Width, image.Height);
                    imbImage.SetZoomScale(scale);
                }
                else
                {
                    imbImage.SourceFromBitmap = image;
                }
            }));
        }
        private double GetFittedZoomScale(object imb, double imageWidth, double imageHeight)
        {
            var imageBox = imb as Heal.MyControl.ImageBox;
            var imageBoxWidth = imageBox.ActualWidth;
            var imageBoxHeight = imageBox.ActualHeight;
            var scale = Math.Min(imageBoxWidth / imageWidth, imageBoxHeight / imageHeight);
            return scale;
        }
    }
}
