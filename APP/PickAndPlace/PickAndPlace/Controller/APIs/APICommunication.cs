using PickAndPlace.Models;
using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PickAndPlace.Controllers.APIs
{
    class APICommunication
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static Properties.Settings _param = Properties.Settings.Default;


        //public static DebugImageResponse DebugImages(string url,Mat image, EnvironmentConfig envConfig, int timeout=10000)
        //{
        //    dynamic obj = new DebugImageResponse();
        //    var options = new RestClientOptions(url)
        //    {
        //        Timeout = TimeSpan.FromMilliseconds(timeout)
        //    };
        //    var client = new RestClient(options);
        //    var request = new RestRequest(_param.EndPointDebug, Method.Post);
        //    request.AlwaysMultipartFormData = true;

        //    // Add File
        //    byte[] jpegData = image.ToImage<Bgr, byte>().ToJpegData();
        //    request.AddFile("image", jpegData, $"image.jpg");

        //    // Tạo payload JSON
        //    var payload = new
        //    {
        //        segment_threshold = envConfig.SegmentThreshold,
        //        segment_iou = envConfig.SegmentIou,
        //        detect_threshold = envConfig.DetectThreshold,
        //        detect_iou = envConfig.DetectIou,
        //        caliper_min_edge_distance = envConfig.CaliperMinEdgeDistance,
        //        caliper_max_edge_distance = envConfig.CaliperMaxEdgeDistance,
        //        caliper_length_rate = envConfig.CaliperLengthRate,
        //        caliper_thickness_list = envConfig.CaliperThicknessList,
        //        disk_num = envConfig.DiskNumber,
        //        disk_max_distance = envConfig.DiskMaxDistance,
        //        disk_min_distance = envConfig.DiskMinDistance,
        //        disk_min_area = envConfig.DiskMinArea
        //    };
        //    string paramsJson = JsonConvert.SerializeObject(payload);
        //    request.AddParameter(
        //                        "params_json",
        //                        paramsJson,
        //                        ParameterType.GetOrPost
        //    );

        //    var response = client.Execute(request);
        //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //    {
        //        try
        //        {
                    
        //            obj = JsonConvert.DeserializeObject<DebugImageResponse>(response.Content);
        //            return obj;
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.Debug(ex.Message);
        //            return null;
        //        }
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
    }
}
