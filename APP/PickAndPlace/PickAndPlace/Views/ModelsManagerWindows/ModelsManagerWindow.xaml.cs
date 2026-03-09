using PickAndPlace.Models;
using PickAndPlace.Views.UtilitiesWindows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PickAndPlace.Views.ModelsManagerWindows
{
    /// <summary>
    /// Interaction logic for ModelsManagerWindow.xaml
    /// </summary>
    public partial class ModelsManagerWindow : Window, INotifyPropertyChanged
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        Properties.Settings _param = Properties.Settings.Default;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<ModelInfo> ModelsList { get; set; } = new ObservableCollection<ModelInfo>();

        private ModelInfo _selectedModel;
        public ModelInfo SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    OnPropertyChanged();
                }
            }
        }

        MainWindow _mainWindow;
        public ModelsManagerWindow(MainWindow window, string selectedModel)
        {
            InitializeComponent();
            _mainWindow = window;
            Init(selectedModel);
            DataContext = this;
        }

        private void Init(string selectedModel)
        {
            var modelList = ModelInfo.LoadModelsList();
            foreach (var item in modelList)
            {
                ModelsList.Add(item);
            }
            if (selectedModel == string.Empty)
            {
                SelectedModel = null;
            }
            else
            {
                SelectedModel = ModelsList.FirstOrDefault(x => x.Name == selectedModel);
            }
        }

        private void btReload_Click(object sender, RoutedEventArgs e)
        {
            var curModel = SelectedModel;
            ModelsList.Clear();
            var modelList = ModelInfo.LoadModelsList();
            foreach (var item in modelList)
            {
                ModelsList.Add(item);
            }
            SelectedModel = curModel;
        }

        private void btAddModel_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddModelNameWindow(this);
            window.ShowDialog();
        }

        private void btRemoveModel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cbbModelNames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnSave_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Convert.ToInt32(tbImageWidth.Text) == 0 || Convert.ToInt32(tbPcbHeight.Text) == 0)
            {
                var box = new ErrorWindow("Width and Height must be greater than 0!\rChiều rộng và chiều cao PCB phải lớn hơn 0!");
                box.ShowDialog();
                return;
            }
            SelectedModel.SaveModel();
            var info = new InformationWindow("Save successfully!\rLưu thành công!");
            info.ShowDialog();
        }

        internal void UpdateNewModelName(ModelInfo model)
        {
            ModelsList.Add(model);
            SelectedModel = model;
        }
    }
}
