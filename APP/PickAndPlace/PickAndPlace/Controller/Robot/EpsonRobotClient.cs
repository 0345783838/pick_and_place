using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace PickAndPlace.Controller.Robot
{
    public class EpsonRobotClient : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;

        private readonly string _ip;
        private readonly int _port;

        private readonly object _lock = new object();

        public bool IsConnected => _client != null && _client.Connected;

        public EpsonRobotClient(string ip, int port = 5000)
        {
            _ip = ip;
            _port = port;
        }

        #region Connection

        public void EnsureConnected()
        {
            if (!IsConnected)
            {
                Connect();
            }
        }

        public void Connect()
        {
            Disconnect();

            _client = new TcpClient();

            _client.ReceiveTimeout = 5000;
            _client.SendTimeout = 5000;

            _client.Connect(_ip, _port);

            _stream = _client.GetStream();

            _reader = new StreamReader(_stream, Encoding.ASCII);

            _writer = new StreamWriter(_stream, Encoding.ASCII)
            {
                AutoFlush = true
            };
        }

        public void Disconnect()
        {
            try
            {
                _reader?.Close();
                _writer?.Close();
                _stream?.Close();
                _client?.Close();
            }
            catch
            {
            }

            _reader = null;
            _writer = null;
            _stream = null;
            _client = null;
        }

        #endregion

        #region Core Communication

        private string SendCommand(string command)
        {
            lock (_lock)
            {
                if (!IsConnected)
                    throw new Exception("Robot not connected");

                _writer.WriteLine(command);

                string response = _reader.ReadLine();

                if (response == null)
                    throw new Exception("Robot disconnected");

                if (response.StartsWith("ERROR"))
                    throw new Exception($"Robot error: {response}");

                return response;
            }
        }

        #endregion

        #region Robot Status

        public bool IsRobotReady()
        {
            string res = SendCommand("GET_STATUS");

            return res.Contains("IDLE");
        }

        #endregion

        #region Motion

        public void MoveXY(double x, double y)
        {
            string cmd = $"MOVE X{x:F3} Y{y:F3}";

            SendCommand(cmd);
        }

        public void Pick(double x, double y, double w)
        {
            string cmd = $"PICK X{x:F3} Y{y:F3} W{w:F3}";

            SendCommand(cmd);
        }

        #endregion

        #region Position

        public RobotPose GetCurrentPosition()
        {
            string res = SendCommand("GET_POSE");

            return ParsePose(res);
        }

        private RobotPose ParsePose(string data)
        {
            RobotPose pose = new RobotPose();

            var parts = data.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in parts)
            {
                if (p.StartsWith("X="))
                    pose.X = double.Parse(p.Substring(2));

                else if (p.StartsWith("Y="))
                    pose.Y = double.Parse(p.Substring(2));

                else if (p.StartsWith("Z="))
                    pose.Z = double.Parse(p.Substring(2));

                else if (p.StartsWith("W="))
                    pose.W = double.Parse(p.Substring(2));
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

        public double W { get; set; }
    }
}