using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutoCode.Presentation
{
    /// <summary>
    /// Interaction logic for ColumnSelectionForm.xaml
    /// </summary>
    public partial class ColumnSelectionForm : Window
    {
        public static Dictionary<string, Tuple<string, bool, bool>> TableColumnList = new Dictionary<string, Tuple<string, bool, bool>>();
        CheckBox checkBox = new CheckBox();
        public ColumnSelectionForm()
        {
            InitializeComponent();
            TableColumnList = new Dictionary<string, Tuple<string, bool, bool>>();
            Dictionary<string, Tuple<string, bool, bool>> ColumnsForChecking = new Dictionary<string, Tuple<string, bool, bool>>();
            foreach (var item in SettingHelper.Temp_TableColumnList.Keys)
            {
                string key = item.ToString();
                if (SettingHelper.Temp_TableColumnList.TryGetValue(key, out Tuple<string, bool, bool> x))
                {
                    Tuple<string, bool, bool> value = x;
                    if (!TableColumnList.ContainsKey(key))
                    {
                        ColumnsForChecking.Add(key,value);
                        TableColumnList.Add(key, value);
                    }
                }
            }

            foreach (var item in ColumnsForChecking)
            {
                
                List<string> UncheckColumns = new List<string>() { "createddate", "createdby", "modifiedby", "modifieddate","modifyby","modifydate" };
                if (item.Key.ToString() == SettingHelper.Temp_primaryKeyOfTable.ToString() || item.Key.ToLower() == "id")
                {
                    checkBox = new CheckBox
                    {
                        Content = item.Key,
                        Margin = new Thickness(5),
                        FontSize = 15,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        IsChecked = true,
                        IsEnabled = false
                    };
                }
                else if (UncheckColumns.Contains(item.Key.ToLower()))
                {
                    checkBox = new CheckBox
                    {
                        Content = item.Key,
                        Margin = new Thickness(5),
                        FontSize = 15,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        IsChecked = false
                    };
                    TableColumnList.Remove(item.Key);
                }
                else
                {
                    checkBox = new CheckBox
                    {
                        Content = item.Key,
                        Margin = new Thickness(5),
                        FontSize = 15,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        IsChecked = true
                    };
                }
                checkBox.Checked += CheckBox_Checked;
                checkBox.Unchecked += CheckBox_Unchecked;
                CheckBoxContainer.Children.Add(checkBox);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            string key = cb.Content.ToString();
            if (SettingHelper.Temp_TableColumnList.TryGetValue(key, out Tuple<string, bool, bool> value))
            {
                TableColumnList.Add(key, value);
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            TableColumnList.Remove(cb.Content.ToString());
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnConnectServer_Click(object sender, RoutedEventArgs e)
        {
            SettingHelper.TableColumnList = TableColumnList;
            SettingHelper.tableName = SettingHelper.Temp_tableName.ToString();
            SettingHelper.primaryKeyOfTable = SettingHelper.Temp_primaryKeyOfTable.ToString();
            SettingHelper.primaryKeyDataType = SettingHelper.Temp_primaryKeyDataType.ToString();
            this.Close();
        }
    }
}
