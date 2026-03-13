using DiskInspection.Controllers;
using Emgu.CV;
using Emgu.CV.Structure;
using PickAndPlace.Controller.Robot;
using PickAndPlace.Controllers;
using PickAndPlace.Controllers.APIs;
using PickAndPlace.Controllers.Camera;
using PickAndPlace.Models;
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
        private ModelInfo _model;
        private bool _isRunning;
        private CancellationTokenSource _inspectCts;

        private CameraManager _cameraManager;
        private LincolnCamera _camera;
        private EpsonRobotClient _robot;

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

        internal bool Start(ModelInfo model)
        {
            _logger.Info("Starting inspection...");
            _ForceStopProcess = false;
            if (CheckAndStartCamera() && CheckAndStartRobot() && CheckAndStartEngine())
            {
                _logger.Debug("Camera, Robot and Engine are ready, Ready for detection...");
                AppLogger.Instance.Info("Camera, Robot and Engine are ready, Ready for detection...", "SYSTEM");

                // Load Templates to API
                _model = model;
                bool loadRes = LoadTemplatesToEngine(_model.Templates);
                if (!loadRes)
                {
                    _logger.Error("Load Templates to Engine failed, Stop inspection...\rLoad Templates đến Engine thất bại, dừng inspection...");
                    AppLogger.Instance.Error("Load Templates to Engine failed, Stop inspection...", "SYSTEM");
                    return false;
                }


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

        private bool LoadTemplatesToEngine(List<Template> templates)
        {
            var imageList = templates.Select(x => x.Image).ToList();
            return APICommunication.LoadTemplates(_param.ApiUrlAi, imageList);
        }

        internal void Stop()
        {
            if (_camera != null)
            {
                _camera.Stop();
            }
        }
        internal void Close()
        {
            if (_camera != null)
            {
                _camera.Stop();
                _camera.Close();
            }
            if (_robot != null)
            {
                _robot.Dispose();
            }
            AIServiceController.CloseProcessExisting();
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
            try
            {
                if (_robot != null && _robot.IsRobotReady())
                {
                    return true;
                }
                _robot = new EpsonRobotClient(_param.RobotIp);

                _robot.Connect();

                bool ready = _robot.IsRobotReady();

                if (!ready)
                {
                    AppLogger.Instance.Error("Robot is not connected, login failed!", "ROBOT_CONNECT_FAILED");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Instance.Error(ex.Message, "ROBOT_CONNECT_FAILED");
                return false;
            }
        }

        private bool CheckAndStartCamera()
        {
            //return true;
            _cameraManager = CameraManager.GetInstance();
            _camera = _cameraManager.GetCamera();
            if (!_camera.IsOpen())
            {
                _mainWindow.ShowError(string.Format("Không mở được camera với SN {0}\nCan't open  camera with SN:{0}", _param.CamSn));
                AppLogger.Instance.Error(string.Format("Không mở được camera với SN {0}\nCan't open  camera with SN:{0}", _param.CamSn), "CAMERA_OPEN_FAILED");
                return false;
            }

            _camera.SetExposureTime(_param.CamExposure);
            _camera.Start();
            return true;
        }

        internal void ProcessImage(ModelInfo model)
        {
            var bitmap = _camera.GetBitmap();
            AppLogger.Instance.Info("DONE Capturing Image", "SYSTEM");
            _mainWindow.UpdateImage(bitmap);

            var emguCvImage = new Image<Bgr, byte>(bitmap);

            var res = APICommunication.GetRealCoord(_param.ApiUrlAi, emguCvImage, model.Width, model.Height);
            if (res != null)
            {
                if (res.Result)
                {
                    AppLogger.Instance.Info("DONE: Calculating Real Coodinates", "SYSTEM");
                    _mainWindow.UpdateImage(Converter.Base64ToBitmap(res.ResImg));
                    _mainWindow.UpdateCalculateResult((double)res.Score, (double)res.ImageX, (double)res.ImageY, (double)res.ImageAngle, (double)res.RobotX, (double)res.RobotY, (double)res.RobotAngle);
                    _robot.Pick((double)res.RobotX, (double)res.RobotY, (double)res.RobotAngle, _param.WriteTimeout, _param.PickTimeout);
                    AppLogger.Instance.Info($"Sent Pick Command X: {res.RobotX} Y: {res.RobotY} Angle: {res.RobotAngle}", "SYSTEM");
                }
                else
                {
                    AppLogger.Instance.Error("ERROR: Cannot Find The Matching PCB Corner", "SYSTEM");
                }

                _mainWindow.UpdateStatistics(res.Result);
                _mainWindow.UpdateInspectionStatus(res.Result);
            }
            else
            {
                AppLogger.Instance.Error("INTERNAL ERROR: Cannot Calculate Real Coodinates", "SYSTEM");
            }
        }

        internal bool CheckLicense()
        {
            Thread.Sleep(2000);
            return true;
        }
    }
}
