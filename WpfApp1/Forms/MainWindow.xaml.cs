using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BusyIndicator.IsBusy=true;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void btnConnectServer_Click(object sender, RoutedEventArgs e)
        {
            //try
            //{
            //    if (string.IsNullOrEmpty(txtServerName.Text))
            //    {
            //        MessageBox.Show("Please Insert Server Name.");
            //        return;
            //    }
            //    if (string.IsNullOrEmpty(txtDatabaseName.Text))
            //    {
            //        MessageBox.Show("Please Insert Database Name.");
            //        return;
            //    }
            //    if (string.IsNullOrEmpty(txtUserName.Text))
            //    {
            //        MessageBox.Show("Please Insert User Name.");
            //        return;
            //    }
            //    if (string.IsNullOrEmpty(txtPassword.Password))
            //    {
            //        MessageBox.Show("Please Insert Password.");
            //        return;
            //    }
            //    SqlConnectionStringBuilder sConnB = new SqlConnectionStringBuilder()
            //    {
            //        DataSource = txtServerName.Text.Trim(),
            //        InitialCatalog = txtDatabaseName.Text.Trim(),
            //        UserID = txtUserName.Text.Trim(),
            //        Password = txtPassword.Password.Trim()
            //    };
            //    SqlConnection conn = new SqlConnection(sConnB.ConnectionString);
            //    conn.Open();
            //    MessageBox.Show("Congratulation Your Database connected successfully.");
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}

        }

        private static readonly Regex _regexForOnlyNumeric = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regexForOnlyNumeric.IsMatch(text);
        }

        private void txtServerPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

       
    }
}
