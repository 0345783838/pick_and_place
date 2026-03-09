using Newtonsoft.Json;
using PickAndPlace.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PickAndPlace.Models
{
    public class ModelInfo
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string CreatedTime { get; set; }
        public ModelInfo(string name, double width, double height, string createdTime) => (Name, Width, Height, CreatedTime) = (name, width, height, createdTime);
        public ModelInfo() { }
        public ModelInfo(string name)
        {
            Name = name;
            CreatedTime = DateTime.Now.ToString();
            Width = 0;
            Height = 0;
        }
        public static List<ModelInfo> LoadModelsList()
        {
            List<ModelInfo> modelNamesList = new List<ModelInfo>();
            IO.CreateFolderIfNotExists(Properties.Settings.Default.MODELS_PATH);
            string[] pathList = Directory.GetDirectories(Properties.Settings.Default.MODELS_PATH);
            for (int i = 0; i < pathList.Length; i++)
            {
                string name = Path.GetFileName(pathList[i]);
                var model = LoadModelByName(name);
                if (model != null)
                    modelNamesList.Add(model);
            }
            return modelNamesList;
        }
        public static ModelInfo LoadModelByName(string modelName)
        {
            ModelInfo model = null;
            string path = Properties.Settings.Default.MODELS_PATH + "/" + modelName + "/" + modelName + ".json";
            try
            {
                string str = File.ReadAllText(path);
                model = JsonConvert.DeserializeObject<ModelInfo>(str);
            }
            catch (Exception ex)
            {
                return null;
            }
            return model;
        }
        public void SaveModel()
        {
            try
            {
                string modelPath = Properties.Settings.Default.MODELS_PATH + "/" + this.Name;
                IO.CreateFolderIfNotExists(modelPath);
                string json = JsonConvert.SerializeObject(this);
                File.WriteAllText(modelPath + "/" + this.Name + ".json", json);
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
