using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickAndPlace.Controllers.Camera
{
    internal class CameraManager
    {
        private static CameraManager _cameraManager;
        private LincolnCamera _camera1;
        public static CameraManager GetInstance()
        {
            if (_cameraManager == null)
            {
                _cameraManager = new CameraManager();
            }

            return _cameraManager;
        }
        public static void Reload()
        {
            _cameraManager = new CameraManager();
        }
        public LincolnCamera GetCamera()
        {
            if (((_camera1 != null) && (_camera1.SN != Properties.Settings.Default.CamSn)) || (_camera1 == null))
            {
                if (_camera1 != null)
                    _camera1.Close();
                _camera1 = new LincolnCamera(Properties.Settings.Default.CamSn);
            }
            return _camera1;
        }

        public bool CheckCameraConnection(string SN)
        {
            var cam = new LincolnCamera(SN);
            if (cam.IsOpen())
            {
                cam.Close();
                return cam.IsOpen();
            }
            return false;
        }
    }
}
