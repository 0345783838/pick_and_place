using DiskInspection.Controllers;
using PickAndPlace.Controllers;
using PickAndPlace.Controllers.APIs;
using PickAndPlace.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PickAndPlace.Controller
{
    class MainController
    {
        private MainWindow _mainWindow;
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private Properties.Settings _param = Properties.Settings.Default;

        public bool _serviceIsRun = false;
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
    }
}
