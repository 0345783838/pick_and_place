using DiskInspection.Controllers;
using Emgu.CV;
using Emgu.CV.Structure;
using PickAndPlace.Controllers;
using PickAndPlace.Controllers.APIs;
using PickAndPlace.Controllers.Camera;
using PickAndPlace.Utils;
using PickAndPlace.Views;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace PickAndPlace.Controller
{
    class MainController
    {
        private MainWindow _mainWindow;
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private Properties.Settings _param = Properties.Settings.Default;

        public bool _serviceIsRun = false;
        private bool _ForceStopProcess;
        private bool _isRunning;
        private CancellationTokenSource _inspectCts;

        private CameraManager _cameraManager;
        private LincolnCamera _camera;

        public MainController(MainWindow window)
        {
            _mainWindow = window;
        }
        public bool RunServiceAsync(int timeout, string content)
        {
            _mainWindow.SetLoadingService(content);
            _logger.Info("Start Service");
            AppLogger.Instance.Info("Loading Program...", "SYSTEM");
            AIServiceController.CloseProcessExisting();
            AIServiceController.Start();

            var timeStep = timeout / 1000;
            for (int i = 0; i < timeStep; i++)
            {
                Thread.Sleep(1000);
                if (APICommunication.CheckAPIStatus(_param.ApiUrlAi, 200))
                {
                    _logger.Info("Start AI Engine Successfuly!");
                    AppLogger.Instance.Info("Loaded Program Successfuly!", "SYSTEM");
                    _serviceIsRun = true;
                    return true;
                }
            }
            return false;
        }

        internal bool Start()
        {
            _logger.Info("Starting inspection...");
            _ForceStopProcess = false;
            if (CheckAndStartCamera() && CheckAndStartRobot() && CheckAndStartEngine())
            {
                _logger.Debug("Camera, Robot and Engine are ready, Ready for detection...");
                AppLogger.Instance.Info("Camera, Robot and Engine are ready, Ready for detection...", "SYSTEM");
                _isRunning = true;
                //_inspectCts = new CancellationTokenSource();
                //StartStatusTimer();
                //StartPlcTimer();
                return true;
            }
            else
            {
                _logger.Error("Camera, Robot and Engine  are not ready, Stop inspection...");
                AppLogger.Instance.Error("Camera, Robot and Engine  are not ready, Stop inspection...", "SYSTEM");
                return false;
            }
        }

        private bool CheckAndStartEngine()
        {
            if (!APICommunication.CheckAPIStatus(_param.ApiUrlAi))
            {
                var res = _mainWindow.ShowWarning($"Engine is not running, proceed to restart?\nAI engine đang không chạy, bạn muốn khởi động lại AI engine?!");
                var resRestart = RunServiceAsync(20000, "Restarting AI engine...");
                if (!resRestart)
                {
                    _mainWindow.ShowError("Restart AI engine fail, please contact the vendor!\r AI engine khởi động thất bại, hãy liên hệ với vendor!");
                    return false;
                }
            }

            if (!APICommunication.CheckMatrixReady(_param.ApiUrlAi))
            {
                _mainWindow.ShowError($"Calibration is not ready, please process calibration!\nChưa thực hiện Calibration, hãy tiến hành Calibration!");
                return false;
            }
            return true;
        }

        private bool CheckAndStartRobot()
        {
            return true;
        }

        private bool CheckAndStartCamera()
        {
            return true;
            _cameraManager = CameraManager.GetInstance();
            _camera = _cameraManager.GetCamera();
            if (!_camera.IsOpen())
            {
                _mainWindow.ShowError(string.Format("Không mở được camera 1 với SN {0}\nCan't open 1 camera with SN:{0}", _param.CamSn));
                return false;
            }

            _camera.SetExposureTime(_param.CamExposure);
            _camera.Start();
            return true;
        }

        internal void ProcessImage()
        {
            //var bitmap = _camera.GetBitmap();
            var bitmap = new System.Drawing.Bitmap(@"C:\Users\CH Computer\Downloads\pcb\Image_20260305144528806.bmp");
            _mainWindow.UpdateImage(bitmap);

            var emguCvImage = new Image<Bgr, byte>(bitmap);

            var res = APICommunication.GetRealCoord(_param.ApiUrlAi, emguCvImage, 507, 598);
            if (res != null)
            {
                if (res.Result)
                {
                    _mainWindow.UpdateImage(Converter.Base64ToBitmap(res.ResImg));
                }
            }

        }
    }
}
