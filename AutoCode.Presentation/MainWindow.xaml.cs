using AutoCode.Presentation.DatabaseUtils;
using AutoCode.Presentation.Model;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoCode.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int developmentType = Convert.ToInt32(ConfigurationManager.AppSettings["Development"]);
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }


        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BusyIndicator.IsBusy = false;
            cmbServerType.SelectedIndex = 2;

            if (developmentType == 0)
            {
                txtDatabaseName.Text = "sayatesting";
                txtPassword.Password = "duca$$0234";
                txtServerName.Text = "saya-dev2.cq6nozddb1mr.us-west-2.rds.amazonaws.com";
                txtUserName.Text = "SayaDev";
            }
            else
            {
                txtDatabaseName.Text = "";
                txtPassword.Password = "";
                txtServerName.Text = "";
                txtUserName.Text = "";
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void btnConnectServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //----DotNet----
                if (DotNet.IsSelected)
                {
                    if (string.IsNullOrEmpty(txtServerName.Text))
                    {
                        MessageBox.Show("Please Insert Server Name.");
                        return;
                    }
                    if (string.IsNullOrEmpty(txtDatabaseName.Text))
                    {
                        MessageBox.Show("Please Insert Database Name.");
                        return;
                    }
                    if (string.IsNullOrEmpty(txtUserName.Text))
                    {
                        MessageBox.Show("Please Insert User Name.");
                        return;
                    }
                    if (string.IsNullOrEmpty(txtPassword.Password))
                    {
                        MessageBox.Show("Please Insert Password.");
                        return;
                    }
                    switch (cmbServerType.SelectedIndex)
                    {
                        case 1:
                            BackgroundWorker sqlServerBackroundWorker = new BackgroundWorker();
                            sqlServerBackroundWorker.DoWork += SqlServerBackroundWorker_DoWork;
                            sqlServerBackroundWorker.RunWorkerCompleted += SqlServerBackroundWorker_RunWorkerCompleted;
                            List<string> mssqlParamList = new List<string>();
                            mssqlParamList.Add(txtServerName.Text);
                            mssqlParamList.Add(txtDatabaseName.Text);
                            mssqlParamList.Add(txtUserName.Text);
                            mssqlParamList.Add(txtPassword.Password);
                            BusyIndicator.IsBusy = true;
                            BusyIndicator.BusyContent = "We are connecting to MSSQL database.";
                            sqlServerBackroundWorker.RunWorkerAsync(mssqlParamList);
                            break;
                        case 2:
                            BackgroundWorker postgreSQLServerBackroundWorker = new BackgroundWorker();
                            postgreSQLServerBackroundWorker.DoWork += PosgreSqlServerBackroundWorker_DoWork;
                            postgreSQLServerBackroundWorker.RunWorkerCompleted += PosgreSqlServerBackroundWorker_RunWorkerCompleted;
                            List<string> posgresSQLParamList = new List<string>();
                            posgresSQLParamList.Add(txtServerName.Text);
                            posgresSQLParamList.Add(txtDatabaseName.Text);
                            posgresSQLParamList.Add(txtUserName.Text);
                            posgresSQLParamList.Add(txtPassword.Password);
                            BusyIndicator.IsBusy = true;
                            BusyIndicator.BusyContent = "We are connecting to MSSQL database.";
                            postgreSQLServerBackroundWorker.RunWorkerAsync(posgresSQLParamList);

                            break;
                        default:

                            break;
                    }
                }

                //----Flutter---
                else if (Flutter.IsSelected)
                {

                    if (string.IsNullOrWhiteSpace(txtFormName.Text))
                    {
                        MessageBox.Show("Please Insert Form Name.");
                        return;
                    }
                    //if (cmbFormType.SelectedIndex == 0)
                    //{
                    //    MessageBox.Show("Please select form type.");
                    //    return;
                    //}
                    if (string.IsNullOrWhiteSpace(txtFormModel.Text))
                    {
                        MessageBox.Show("Please insert model to generate form.");
                        return;
                    }
                    List<FieldProperties> properties = ExtractProperties(txtFormModel.Text.Trim());
                    if (properties == null || !properties.Any())
                    {
                        MessageBox.Show("Please insert valid model to generate form.");
                        return;
                    }
                    BackgroundWorker flutterBackroundWorker = new BackgroundWorker();
                    flutterBackroundWorker.DoWork += FlutterBackgroundWorker_DoWork;
                    flutterBackroundWorker.RunWorkerCompleted += FlutterBackroundWorker_RunWorkerCompleted;
                    List<string> flutterParamList = new List<string>();
                    flutterParamList.Add(txtFormName.Text.Trim());
                    //flutterParamList.Add(cmbFormType.SelectedIndex.ToString());
                    flutterParamList.Add(JsonConvert.SerializeObject(properties));
                    BusyIndicator.IsBusy = true;
                    BusyIndicator.BusyContent = "Loading...";
                    flutterBackroundWorker.RunWorkerAsync(flutterParamList);
                }

                //----Angular---
                else if (Angular.IsSelected)
                {

                    if (string.IsNullOrWhiteSpace(txtFormName1.Text))
                    {
                        MessageBox.Show("Please Insert Form Name.");
                        return;
                    }
                    //if (cmbFormType1.SelectedIndex == 0)
                    //{
                    //    MessageBox.Show("Please select form type.");
                    //    return;
                    //}
                    if (string.IsNullOrWhiteSpace(txtFormModel1.Text))
                    {
                        MessageBox.Show("Please insert model to generate form.");
                        return;
                    }
                    List<FieldProperties> properties = ExtractProperties(txtFormModel1.Text.Trim());
                    if (properties == null || !properties.Any())
                    {
                        MessageBox.Show("Please insert valid model to generate form.");
                        return;
                    }
                    BackgroundWorker angularBackroundWorker = new BackgroundWorker();
                    angularBackroundWorker.DoWork += AngularBackgroundWorker_DoWork;
                    angularBackroundWorker.RunWorkerCompleted += AngularBackroundWorker_RunWorkerCompleted;
                    List<string> angularParamList = new List<string>();
                    angularParamList.Add(txtFormName1.Text.Trim());
                    //angularParamList.Add(cmbFormType1.SelectedIndex.ToString());
                    angularParamList.Add(JsonConvert.SerializeObject(properties));
                    BusyIndicator.IsBusy = true;
                    BusyIndicator.BusyContent = "Loading...";
                    angularBackroundWorker.RunWorkerAsync(angularParamList);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private List<FieldProperties> ExtractProperties(string input)
        {
            List<string> tempProps = input.Split('\n').ToList();
            List<string> propStrings = new List<string>();
            foreach (string prop in tempProps)
            {
                string temp = prop;
                if (temp.Contains("="))
                    temp = prop.Remove(prop.IndexOf("=")).Trim();
                if (temp.Contains("{"))
                    temp = prop.Remove(prop.IndexOf("{")).Trim();
                if (temp.Contains("}"))
                    temp = prop.Remove(prop.IndexOf("}")).Trim();
                if(temp.Contains(";"))
                    temp = prop.Remove(prop.IndexOf(";")).Trim();
                if(temp.Contains("?"))
                    temp = prop.Remove(prop.IndexOf("?"),1).Trim();
                temp = Regex.Replace(temp, "public", "", RegexOptions.IgnoreCase);
                temp = Regex.Replace(temp, "private", "", RegexOptions.IgnoreCase);
                temp = Regex.Replace(temp, "protected", "", RegexOptions.IgnoreCase);
                 if (temp.Trim().Length > 0)
                    propStrings.Add(temp.Trim());
            }
            List<FieldProperties> properties = new List<FieldProperties>();
            foreach (string prop in propStrings)
            {
                if (prop.Split(' ').ToList().Count > 1)
                {
                    string type = prop.Split(' ')[0];
                    string name = prop.Split(' ')[1];
                    properties.Add(new FieldProperties
                    {
                        Type = type,
                        Name = name,
                        Label = labelCreater(name)
                    });
                }
            }
            return properties;
        }
        //Function to add space and first latter capital of label
        private static string labelCreater(string label)
        {
            string result = Regex.Replace(label, "(?<=[a-z])(?=[A-Z])", " ");
            result = char.ToUpper(result[0]) + result.Substring(1);
            return result;
        }
        private void PosgreSqlServerBackroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                bool isSuccess = (bool)e.Result;
                BusyIndicator.IsBusy = false;
                if (isSuccess)
                {
                    MessageBox.Show("Database Connection Established.");
                    //MessageBox.Show(SettingHelper.SqlConnectionStringBuilder.ConnectionString);
                    DatabaseTableList databaseTableList = new DatabaseTableList();
                    SettingHelper.ConnectionType = Enum.ConnectionType.PostgreSQLServer;
                    this.Hide();
                    databaseTableList.ShowDialog();
                    this.Show();
                }
                else
                {
                    MessageBox.Show("Database Connection Fail.");

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void PosgreSqlServerBackroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<string> paramList = e.Argument as List<string>;
                string serverName = paramList[0];
                string databaseName = paramList[1];
                string userName = paramList[2];
                string password = paramList[3];

                NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder()
                {
                    Database = databaseName,
                    Host = serverName,
                    Port = 5432,
                    Username = userName,
                    Password = password,
                    Timeout = 1024,
                    CommandTimeout = 0
                };
                SettingHelper.PostgreSqlConnectionStringBuilder = npgsqlConnectionStringBuilder;
                PostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();
                bool result = postgreSQLHandler.CheckDatabaseConnectoin();
                e.Result = result;
            }
            catch (Exception)
            {
                e.Result = false;
            }
        }
        private void SqlServerBackroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                bool isSuccess = (bool)e.Result;
                BusyIndicator.IsBusy = false;
                if (isSuccess)
                {
                    MessageBox.Show("Database Connection Established.");
                    //MessageBox.Show(SettingHelper.SqlConnectionStringBuilder.ConnectionString);
                    DatabaseTableList databaseTableList = new DatabaseTableList();
                    SettingHelper.ConnectionType = Enum.ConnectionType.MicrosoftSQLServer;
                    this.Hide();
                    databaseTableList.ShowDialog();
                    this.Show();
                }
                else
                {
                    MessageBox.Show("Database Connection Fail.");

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private void FlutterBackroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                BusyIndicator.IsBusy = false;
                List<string> flutterParamList = e.Result as List<string>;
                FlutterForm flutterForm = new FlutterForm(flutterParamList);
                this.Hide();
                flutterForm.ShowDialog();
                this.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void AngularBackroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                BusyIndicator.IsBusy = false;
                List<string> angularParamList = e.Result as List<string>;
                AngularForm angularForm = new AngularForm(angularParamList);
                this.Hide();
                angularForm.ShowDialog();
                this.Show();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void SqlServerBackroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<string> paramList = e.Argument as List<string>;
                string sqlServerName = paramList[0];
                string sqlDatabaseName = paramList[1];
                string sqlUserName = paramList[2];
                string sqlPassword = paramList[3];
                SettingHelper.SqlConnectionStringBuilder = new SqlConnectionStringBuilder()
                {
                    DataSource = sqlServerName,
                    InitialCatalog = sqlDatabaseName,
                    UserID = sqlUserName,
                    Password = sqlPassword
                };

                SqlConnection conn = new SqlConnection(SettingHelper.SqlConnectionStringBuilder.ConnectionString);
                conn.Open();
                conn.Close();
                conn.Dispose();
                e.Result = true;

            }
            catch (Exception)
            {
                e.Result = false;
            }
        }
        private void FlutterBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> flutterParamList = e.Argument as List<string>;
            e.Result = flutterParamList;
        }
        private void AngularBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> angularParamList = e.Argument as List<string>;
            e.Result = angularParamList;
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
