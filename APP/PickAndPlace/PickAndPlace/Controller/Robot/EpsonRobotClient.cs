using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PickAndPlace.Controller.Robot
{
    public class EpsonRobotClient : IDisposable
    {
        
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly string _ip;
        private readonly int _port;
        private bool _connected;

        public bool IsConnected => _connected;

        public EpsonRobotClient(string ip, int port = 5000)
        {
            _ip = ip;
            _port = port;
        }

        #region Connection
        public async Task EnsureConnectedAsync()
        {
            if (_client == null || !_client.Connected)
            {
                await ConnectAsync();
            }
        }

        public async Task ConnectAsync()
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_ip, _port);
            _stream = _client.GetStream();
            _connected = true;
        }

        public void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _connected = false;
        }

        #endregion

        #region Core Communication

        private async Task<string> SendCommandAsync(string command)
        {
            if (!_connected)
                throw new InvalidOperationException("Robot not connected.");

            byte[] data = Encoding.ASCII.GetBytes(command + "\r\n");
            await _stream.WriteAsync(data, 0, data.Length);

            byte[] buffer = new byte[1024];
            int bytes = await _stream.ReadAsync(buffer, 0, buffer.Length);

            return Encoding.ASCII.GetString(buffer, 0, bytes);
        }

        #endregion

        #region Robot Basic Control

        public async Task ServoOnAsync()
        {
            await SendCommandAsync("Servo On");
        }

        public async Task ServoOffAsync()
        {
            await SendCommandAsync("Servo Off");
        }

        public async Task SetSpeedAsync(int speedPercent)
        {
            await SendCommandAsync($"Speed {speedPercent}");
        }

        #endregion

        #region Motion

        public async Task MoveXYUAsync(double x, double y, double z, double u)
        {
            string cmd = $"Go X{x:F3} Y{y:F3} Z{z:F3} U{u:F3}";
            await SendCommandAsync(cmd);
        }

        public async Task MoveLinearXYUAsync(double x, double y, double z, double u)
        {
            string cmd = $"Move X{x:F3} Y{y:F3} Z{z:F3} U{u:F3}";
            await SendCommandAsync(cmd);
        }

        #endregion

        #region Position Read

        public async Task<RobotPose> GetCurrentPositionAsync()
        {
            string response = await SendCommandAsync("Where");

            // Expected response format:
            // X=123.456 Y=234.567 Z=0.000 U=15.000

            return ParsePosition(response);
        }

        private RobotPose ParsePosition(string data)
        {
            var pose = new RobotPose();

            //var parts = data.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var parts = data.Split(' ');

            foreach (var p in parts)
            {
                if (p.StartsWith("X="))
                    pose.X = double.Parse(p.Substring(2));
                else if (p.StartsWith("Y="))
                    pose.Y = double.Parse(p.Substring(2));
                else if (p.StartsWith("Z="))
                    pose.Z = double.Parse(p.Substring(2));
                else if (p.StartsWith("U="))
                    pose.U = double.Parse(p.Substring(2));
            }

            return pose;
        }

        #endregion

        public void Dispose()
        {
            Disconnect();
        }
    }
    public class RobotPose
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double U { get; set; }
    }

}
