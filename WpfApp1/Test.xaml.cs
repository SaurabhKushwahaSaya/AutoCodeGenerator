using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : Window
    {
        private bool isBusy;
        public event PropertyChangedEventHandler PropertyChanged;
        public Test()
        {
            InitializeComponent();
            IndicatorComboBox.SelectedIndex = 0;

            if (BusyIndicator.IsBusyAtStartup)
                Stop();
            this.Loaded += Test_Loaded;
        }
        protected void OnPropertyChanged(string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                OnPropertyChanged();
            }
        }

        private async void Stop()
        {
            await Task.Delay(System.TimeSpan.FromSeconds(3));
            IsBusy = false;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(MyTextBox.Text, out double duration))
            {
                duration = 5;
            }

            IsBusy = true;
            await Task.Delay(System.TimeSpan.FromSeconds(duration));
            IsBusy = false;
        }

        private void Test_Loaded(object sender, RoutedEventArgs e)
        {
            BusyIndicator.IsBusy = true;
        }
    }
}
