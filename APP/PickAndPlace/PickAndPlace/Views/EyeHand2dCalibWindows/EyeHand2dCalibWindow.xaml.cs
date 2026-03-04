using Emgu.CV;
using Emgu.CV.Structure;
using PickAndPlace.Controllers.Camera;
using PickAndPlace.Utils;
using PickAndPlace.Views.UtilitiesWindows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PickAndPlace.Views.EyeHand2dCalibWindows
{
    /// <summary>
    /// Interaction logic for EyeHand2dCalibWindow.xaml
    /// </summary>
    public partial class EyeHand2dCalibWindow : Window
    {
        CameraManager _cameraManager;
        LincolnCamera _cam;

        private System.Windows.Point _start;
        private System.Windows.Point _origin;
        private bool _isSelecting;
        private Image<Bgr, byte> _curImage;

        public EyeHand2dCalibWindow()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            _cameraManager = CameraManager.GetInstance();

            List<CamInfo> camInfoList = LincolnCamera.GetListCamInfo();
            for (int i = 0; i < camInfoList.Count; i++)
            {
                cbbCamSn.Items.Add(camInfoList[i].SN);
            }
        }

        private void btnConnectCamera_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (cbbCamSn.SelectedItem == null)
            {
                var error = new ErrorWindow("Please choose a camera!\rHãy chọn camera!");
                error.ShowDialog();
                return;
            }

            _cam = _cameraManager.GetCamera(cbbCamSn.SelectedValue.ToString()) as LincolnCamera;
            if (!_cam.IsOpen())
            {
                var error = new ErrorWindow($"Cannot open camera {cbbCamSn.SelectedValue.ToString()}!\rKhông mở được camera {cbbCamSn.SelectedValue.ToString()}!");
                error.ShowDialog();
                return;
            }
            _cam.Start();
            //var bitmap = _cam.GetBitmap();
            System.Drawing.Bitmap bitmap = new  System.Drawing.Bitmap(@"F:\working\pick_and_place\pick_and_place\Data\Image_20260304170350160.bmp");
            _curImage = new Image<Bgr, byte>(bitmap);

            UpdateImage(bitmap);
            _cam.Stop();
            _cam.Close();
        }
        private void UpdateImage(System.Drawing.Bitmap image)
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
        }
        private double GetFittedZoomScale(object imb, double imageWidth, double imageHeight)
        {
            var imageBox = imb as Heal.MyControl.ImageBox;
            var imageBoxWidth = imageBox.ActualWidth;
            var imageBoxHeight = imageBox.ActualHeight;
            var scale = Math.Min(imageBoxWidth / imageWidth, imageBoxHeight / imageHeight);
            return scale;
        }
        private void btnCaptureImage_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnLoadIntrinsicCalib_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnSelectPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting)
            {
                imbImage.Cursor = Cursors.Cross;
                Cursor = Cursors.Cross;
                _isSelecting = true;
            }
            else
            {
                imbImage.Cursor = Cursors.Arrow;
                Cursor = Cursors.Arrow;
                _isSelecting = false;
            }
            
        }

        private void btnGetRobotCoord_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnResetPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnAddPoint_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnRemoveRow_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnClearAll_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnCalibrate_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void imbImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isSelecting)
                return;

            var control = sender as Heal.MyControl.ImageBox;
            System.Windows.Point p = Mouse.GetPosition(control);
            double scale = control.ZoomScale;
            double x = control.TranslateX;
            double y = control.TranslateY;
            var clickPosition = new System.Windows.Point(p.X / scale - x / scale, p.Y / scale - y / scale);

            Console.WriteLine("X: " + clickPosition.X + " Y: " + clickPosition.Y);

            using (var image = _curImage.Clone())
            {
                CvInvoke.Circle(image, new System.Drawing.Point((int)clickPosition.X, (int)clickPosition.Y), 2, new MCvScalar(0, 0, 255), -1);
                UpdateImage(image.Bitmap);
            }
           
        }
        //private void GetDisplayedImageInfo(out double displayedW,
        //                           out double displayedH,
        //                           out double offsetX,
        //                           out double offsetY)
        //{
        //    double imgW = imbImage.Source.Width;
        //    double imgH = imbImage.Source.Height;

        //    double controlW = imbImage.ActualWidth;
        //    double controlH = imbImage.ActualHeight;

        //    double ratio = Math.Min(controlW / imgW, controlH / imgH);

        //    displayedW = imgW * ratio;
        //    displayedH = imgH * ratio;

        //    offsetX = (controlW - displayedW) / 2.0;
        //    offsetY = (controlH - displayedH) / 2.0;
        //}

        //private void imbImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (!_isSelecting)
        //    {
        //        imbImage.CaptureMouse();
        //        _start = e.GetPosition(this);
        //        _origin = new System.Windows.Point(translateTransform.X, translateTransform.Y);
        //        Cursor = Cursors.Hand;
        //    }

        //    else
        //    {
        //        if (imbImage.Source == null) return;

        //        Point mouse = e.GetPosition(overlayCanvas);

        //        GetDisplayedImageInfo(out double displayedW,
        //                              out double displayedH,
        //                              out double offsetX,
        //                              out double offsetY);

        //        double x = mouse.X - offsetX;
        //        double y = mouse.Y - offsetY;

        //        if (x < 0 || y < 0 || x > displayedW || y > displayedH)
        //            return;

        //        double pixelX = x * imbImage.Source.Width / displayedW;
        //        double pixelY = y * imbImage.Source.Height / displayedH;

        //        Console.WriteLine($"({pixelX}, {pixelY})");

        //        HighlightPoint(new Point(pixelX, pixelY));
        //    }
        //}

        //private void HighlightPoint(System.Windows.Point pixelPoint)
        //{
        //    overlayCanvas.Children.Clear();

        //    GetDisplayedImageInfo(out double displayedW,
        //                          out double displayedH,
        //                          out double offsetX,
        //                          out double offsetY);

        //    double uiX = pixelPoint.X * displayedW / imbImage.Source.Width + offsetX;
        //    double uiY = pixelPoint.Y * displayedH / imbImage.Source.Height + offsetY;

        //    Ellipse dot = new Ellipse()
        //    {
        //        Width = 10,
        //        Height = 10,
        //        Fill = Brushes.Red
        //    };

        //    Canvas.SetLeft(dot, uiX - 5);
        //    Canvas.SetTop(dot, uiY - 5);

        //    overlayCanvas.Children.Add(dot);
        //}

        //private void imbImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (!_isSelecting)
        //    {
        //        imbImage.ReleaseMouseCapture();
        //        Cursor = Cursors.Arrow;
        //    }

        //}

        //private void imbImage_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!imbImage.IsMouseCaptured) return;

        //    System.Windows.Point p = e.GetPosition(this);
        //    translateTransform.X = _origin.X + (p.X - _start.X);
        //    translateTransform.Y = _origin.Y + (p.Y - _start.Y);
        //}

        //private void imbImage_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ClickCount == 2)
        //        ResetView();
        //}
        //private void imbImage_MouseWheel(object sender, MouseWheelEventArgs e)
        //{
        //    double zoom = e.Delta > 0 ? 1.1 : 0.9;

        //    scaleTransform.ScaleX *= zoom;
        //    scaleTransform.ScaleY *= zoom;
        //}
        //private async void ResetView()
        //{
        //    var duration = TimeSpan.FromMilliseconds(150);

        //    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1, duration));
        //    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, duration));
        //    translateTransform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, duration));
        //    translateTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, duration));

        //    await Task.Delay(duration);

        //    // Xóa animation, trả quyền điều khiển về code
        //    scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        //    scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
        //    translateTransform.BeginAnimation(TranslateTransform.XProperty, null);
        //    translateTransform.BeginAnimation(TranslateTransform.YProperty, null);

        //    scaleTransform.ScaleX = 1;
        //    scaleTransform.ScaleY = 1;
        //    translateTransform.X = 0;
        //    translateTransform.Y = 0;
        //}
    }
}
