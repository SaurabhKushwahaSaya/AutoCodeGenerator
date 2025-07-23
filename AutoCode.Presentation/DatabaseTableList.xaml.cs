using ArkaPrn57.Core.Helper;
using AutoCode.Presentation.DatabaseUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
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
    /// Interaction logic for DatabaseTableList.xaml
    /// </summary>
    public partial class DatabaseTableList : Window
    {
        public DatabaseTableList()
        {
            InitializeComponent();
            this.Loaded += DatabaseTableList_Loaded;
        }

        private void DatabaseTableList_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                BusyIndicator.IsBusy = true;
                cmbTable.ItemsSource = null;
                BackgroundWorker getTableNameBackgroundWorker = new BackgroundWorker();
                getTableNameBackgroundWorker.DoWork += GetTableNameBackgroundWorker_DoWork;
                getTableNameBackgroundWorker.RunWorkerCompleted += GetTableNameBackgroundWorker_RunWorkerCompleted;
                getTableNameBackgroundWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region Bind Table Names in Combobox
        private void GetTableNameBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                List<string> tables = (List<string>)e.Result;
                cmbTable.ItemsSource = tables;
                BusyIndicator.IsBusy = false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void GetTableNameBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    e.Result = GetTableNamesFromSQLServer();
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    e.Result = GetTableNamesFromPostgreSQLServer();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        #region Get Table Name From SQL and Postgre
        private List<string> GetTableNamesFromPostgreSQLServer()
        {
            try
            {
                List<string> tables = new List<string>();
                string query = "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE'; ";
                PostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();
                DataSet ds = postgreSQLHandler.ExecuteAsDataSet(query);
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        foreach (DataRow dataRow in dt.Rows)
                        {
                            tables.Add(dataRow[0].ToString());
                        }
                    }
                }
                return tables;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private List<string> GetTableNamesFromSQLServer()
        {
            try
            {
                List<string> tables = new List<string>();
                string query = "SELECT name FROM sys.Tables";
                DataTable dt = SqlHelper.ExecuteDataTable(SettingHelper.SqlConnectionStringBuilder.ConnectionString, CommandType.Text, query);
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        tables.Add(dr[0].ToString());
                    }
                }
                return tables;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        private void cmbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetTempTableColumns();
            ColumnSelectionForm columnSelectionForm = new ColumnSelectionForm();
            columnSelectionForm.ShowDialog();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                txtBlockCode.Clear();
                txtBlockSp.Clear();
                if (cmbTable.SelectedValue == null || string.IsNullOrEmpty(cmbTable.SelectedValue.ToString().Trim()))
                {
                    MessageBox.Show("Please Select Table.");
                    return;
                }
                if (SettingHelper.TableColumnList.Count <= 0 || SettingHelper.TableColumnList == null)
                {
                    MessageBox.Show("Please Select Columns to Generate Code or SP.");
                    return;
                }
                if (chkObjModel.IsChecked.Value)
                {

                    txtBlockCode.Text = GenerateModelCode();
                }
                if (chkObjSP.IsChecked.Value)
                {
                    if (chkCRUDInsert.IsChecked.Value)
                    {
                        txtBlockSp.Text = txtBlockSp.Text + GenerateInsertStoreProcedure();
                    }
                    if (chkCRUDUpdate.IsChecked.Value)
                    {
                        txtBlockSp.Text = txtBlockSp.Text + GenerateUpdateStoreProcedureCode();
                    }
                    if (chkCRUDSoftDeleteById.IsChecked.Value)
                    {
                        txtBlockSp.Text = txtBlockSp.Text + GenerateSoftDeleteStoreProcedureCode();
                    }
                    if (chkCRUDHardDeleteById.IsChecked.Value)
                    {
                        txtBlockSp.Text = txtBlockSp.Text + GenerateHardDeleteStoreProcedureCode();
                    }
                    if (chkCRUDSelectById.IsChecked.Value)
                    {
                        txtBlockSp.Text = txtBlockSp.Text + GenerateSelectByIdStoreProcedureCode();
                    }
                    if (chkCRUDSelectAll.IsChecked.Value)
                    {
                        txtBlockSp.Text = txtBlockSp.Text + GenerateSelectAllWithPaginationStoreProcedureCode();
                    }
                    if (getList.IsChecked.Value)
                    {
                        txtBlockSp.Text = txtBlockSp.Text + GenerateListStoreProcedure();
                    }
                }
                if (chkObjCode.IsChecked.Value)
                {
                    if (chkCRUDInsert.IsChecked.Value)
                    {
                        txtBlockCode.Text = txtBlockCode.Text + GenerateInsertRecordCode();
                    }
                    if (chkCRUDUpdate.IsChecked.Value)
                    {
                        txtBlockCode.Text = txtBlockCode.Text + GenerateUpdateRecordCode();
                    }
                    if (chkCRUDSoftDeleteById.IsChecked.Value)
                    {
                        txtBlockCode.Text = txtBlockCode.Text + GenerateSoftDeleteRecordCode();
                    }
                    if (chkCRUDHardDeleteById.IsChecked.Value)
                    {
                        txtBlockCode.Text = txtBlockCode.Text + GenerateHardDeleteRecordCode();
                    }
                    if (chkCRUDSelectById.IsChecked.Value)
                    {
                        txtBlockCode.Text = txtBlockCode.Text + GenerateSelectRecordByIdCode();
                    }
                    if (chkCRUDSelectAll.IsChecked.Value)
                    {
                        txtBlockCode.Text = txtBlockCode.Text + GetPaginationModelCode();
                        txtBlockCode.Text = txtBlockCode.Text + GetPaginationDataListModelCode(SettingHelper.tableName);
                        txtBlockCode.Text = txtBlockCode.Text + GetDataTablePagintaionCode();
                        txtBlockCode.Text = txtBlockCode.Text + GenerateSelectAllRecordWithPagination();
                    }
                    if (getList.IsChecked.Value)
                    {
                        txtBlockCode.Text = txtBlockCode.Text + GenerateListCode();
                    }
                }
                if(!chkObjModel.IsChecked.Value && !chkObjSP.IsChecked.Value && !chkObjCode.IsChecked.Value)
                {
                    MessageBox.Show("Please Check Given Checkbox according to your requirment!");
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void GetTempTableColumns()
        {
            try
            {
                SettingHelper.Temp_TableColumnList = new Dictionary<string, Tuple<string, bool, bool>>();
                SettingHelper.Temp_primaryKeyOfTable = string.Empty;
                SettingHelper.Temp_tableName = string.Empty;
                DataTable dt = new DataTable();
                DataTable primaryKeyDt = new DataTable();
                DataSet primaryKeyDs = new DataSet();
                DataSet allColumnNamesDS = new DataSet();
                SettingHelper.Temp_tableName = cmbTable.SelectedValue.ToString();
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    string getAllColumnNames = string.Format(" SELECT COLUMN_NAME,DATA_TYPE ,IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = N'{0}'", SettingHelper.Temp_tableName);
                    dt = SqlHelper.ExecuteDataTable(SettingHelper.SqlConnectionStringBuilder.ConnectionString, CommandType.Text, getAllColumnNames);

                    string getPrimaryKeyColumnName = string.Format("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_NAME = '{0}' AND  CONSTRAINT_NAME like'PK_%'", SettingHelper.Temp_tableName);
                    primaryKeyDt = SqlHelper.ExecuteDataTable(SettingHelper.SqlConnectionStringBuilder.ConnectionString, CommandType.Text, getPrimaryKeyColumnName);
                    if (primaryKeyDt.Rows.Count == 1)
                        SettingHelper.Temp_primaryKeyOfTable = primaryKeyDt.Rows[0].ToString();
                    else
                        SettingHelper.Temp_primaryKeyOfTable = string.Empty;
                    foreach (DataRow dr in dt.Rows)
                    {
                        string columnName = dr[0].ToString();
                        string columnType = dr[1].ToString();
                        bool isNullable = dr[2].ToString().Trim().ToLower() == "YES".Trim().ToLower();
                        bool isPrimaryKey = dr[0].ToString().ToLower().Trim() == SettingHelper.Temp_primaryKeyOfTable.ToLower().Trim();
                        SettingHelper.Temp_TableColumnList.Add(dr[0].ToString(), new Tuple<string, bool, bool>(columnType, isNullable, isPrimaryKey));
                    }
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    PostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();
                    //string getPrimaryColumnName = string.Format("select kcu.column_name as key_column , datatype as DataType from information_schema.table_constraints tco join information_schema.key_column_usage kcu on kcu.constraint_name = tco.constraint_name and kcu.constraint_schema = tco.constraint_schema and kcu.constraint_name = tco.constraint_name where tco.constraint_type = 'PRIMARY KEY' and  kcu.table_name = '{0}' ", SettingHelper.Temp_tableName);
                    string getPrimaryColumnName = string.Format("SELECT kcu.column_name AS key_column, c.data_type, c.character_maximum_length, c.numeric_precision, c.numeric_scale FROM information_schema.table_constraints tco JOIN information_schema.key_column_usage kcu ON kcu.constraint_name = tco.constraint_name AND kcu.constraint_schema = tco.constraint_schema AND kcu.table_name = tco.table_name JOIN information_schema.columns c ON c.table_schema = kcu.table_schema AND c.table_name = kcu.table_name AND c.column_name = kcu.column_name WHERE tco.constraint_type = 'PRIMARY KEY' AND kcu.table_name = '{0}'",SettingHelper.Temp_tableName);
                    primaryKeyDs = postgreSQLHandler.ExecuteAsDataSet(getPrimaryColumnName);
                    if (primaryKeyDs != null && primaryKeyDs.Tables.Count > 0)
                    {
                        if (primaryKeyDs.Tables[0] != null && primaryKeyDs.Tables[0].Rows.Count == 1)
                        {
                            SettingHelper.Temp_primaryKeyOfTable = primaryKeyDs.Tables[0].Rows[0][0].ToString();
                            SettingHelper.Temp_primaryKeyDataType = primaryKeyDs.Tables[0].Rows[0][1].ToString();
                        }
                        else
                        {
                            SettingHelper.Temp_primaryKeyOfTable = string.Empty;
                            SettingHelper.Temp_primaryKeyDataType = string.Empty;
                        }

                    }
                    string getAllColumnNames = string.Format("select column_name as ColumnName, data_type as DataType  , is_nullable as IsNullable  FROM information_schema.columns WHERE table_name = '{0}'", SettingHelper.Temp_tableName);
                    allColumnNamesDS = postgreSQLHandler.ExecuteAsDataSet(getAllColumnNames);
                    if (allColumnNamesDS != null && allColumnNamesDS.Tables.Count > 0)
                    {
                        if (allColumnNamesDS.Tables[0] != null && allColumnNamesDS.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow dr in allColumnNamesDS.Tables[0].Rows)
                            {
                                string columnName = dr[0].ToString();
                                string columnType = dr[1].ToString();
                                bool isNullable = dr[2].ToString().Trim().ToLower() == "YES".Trim().ToLower();
                                bool isPrimaryKey = dr[0].ToString().ToLower().Trim() == SettingHelper.Temp_primaryKeyOfTable.ToLower().Trim();
                                SettingHelper.Temp_TableColumnList.Add(dr[0].ToString(), new Tuple<string, bool, bool>(columnType, isNullable, isPrimaryKey));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        #region Generate Model Code C#
        private string GenerateModelCode()
        {
            string modelCode = string.Empty;
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                classBuilder.AppendLine("---------------------------------- Start Model Code ----------------------------------");
                classBuilder.AppendLine("using System;");
                classBuilder.AppendLine();
                classBuilder.AppendLine($"public class {SettingHelper.tableName}");
                classBuilder.AppendLine("{");
                foreach (var item in SettingHelper.TableColumnList)
                {
                    string columnName = item.Key;
                    string dataType = string.Empty;
                    if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                        dataType = GetColumnTypeForMSSql(item.Value.Item1);
                    else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                        dataType = GetColumnTypeForPostgreSql(item.Value.Item1);
                    if (item.Value.Item2)
                    {
                        classBuilder.AppendLine($"\tpublic {dataType}? {columnName} {{ get; set; }}");
                    }
                    else
                    {
                        classBuilder.AppendLine($"\tpublic {dataType} {columnName} {{ get; set; }}");
                    }
                }
                classBuilder.AppendLine("}");
                classBuilder.AppendLine("---------------------------------- End Model Code ----------------------------------");
                classBuilder.AppendLine("");
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
        private string GenerateInsertStoreProcedure()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                StringBuilder insertBuilder = new StringBuilder();
                StringBuilder valueBuilder = new StringBuilder();
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine("---------------------------------- Start Insert SP Code ----------------------------------");
                    classBuilder.AppendLine($"USE [{SettingHelper.SqlConnectionStringBuilder.InitialCatalog}]");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("SET ANSI_NULLS ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("SET QUOTED_IDENTIFIER ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine($"CREATE PROCEDURE [{SettingHelper.tableName}_Insert]");
                    insertBuilder.AppendLine($"AS");
                    insertBuilder.AppendLine($"BEGIN");
                    insertBuilder.AppendLine($"SET NOCOUNT ON;");
                    insertBuilder.AppendLine($"INSERT INTO [{SettingHelper.tableName}]");
                    valueBuilder.AppendLine($"VALUES");
                    bool isFirstRow = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (!item.Value.Item3 || item.Key == "Id")
                        {
                            classBuilder.AppendLine(GetParameterForStoreProcedureForSQL(item.Key, item.Value.Item1));
                            if (isFirstRow == false)
                            {
                                classBuilder.Append(",");
                                insertBuilder.AppendLine($"([{item.Key}]");
                                valueBuilder.AppendLine($"(@{item.Key}");
                                isFirstRow = true;
                            }
                            else
                            {
                                if (i == SettingHelper.TableColumnList.Count() - 1)
                                {
                                    insertBuilder.AppendLine($",[{item.Key}])");
                                    valueBuilder.AppendLine($",@{item.Key})");
                                }
                                else
                                {
                                    classBuilder.Append(",");
                                    insertBuilder.AppendLine($",[{item.Key}]");
                                    valueBuilder.AppendLine($",@{item.Key}");
                                }
                            }
                        }
                    }
                    valueBuilder.AppendLine($"END");
                    valueBuilder.AppendLine($"---------------------------------- End Insert SP Code ----------------------------------");
                    valueBuilder.AppendLine();
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.AppendLine("---------------------------------- Start Insert SP Code ----------------------------------");
                    classBuilder.Append($"CREATE OR REPLACE FUNCTION public.{SettingHelper.tableName.ToLower()}_add(");
                    insertBuilder.Append($"\tINSERT INTO \"{SettingHelper.tableName}\"(");
                    valueBuilder.Append($"\tValues (");
                    bool isFirstRow = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (!item.Value.Item3)
                        {
                            if (isFirstRow == false)
                            {
                                classBuilder.Append($" {item.Key.ToLower()} {item.Value.Item1}");
                                insertBuilder.Append($"\"{item.Key}\"");
                                valueBuilder.Append($"{item.Key.ToLower()}");
                                isFirstRow = true;
                            }
                            else
                            {
                                classBuilder.Append(",");
                                classBuilder.Append($" {item.Key.ToLower()} {item.Value.Item1}");
                                insertBuilder.Append(",");
                                insertBuilder.Append($"\"{item.Key}\"");
                                valueBuilder.Append(",");
                                valueBuilder.Append($"{item.Key.ToLower()}");
                            }
                        }
                    }
                    classBuilder.Append(")");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("RETURNS boolean \r\nLANGUAGE plpgsql \r\nAS $function$ \r\nDECLARE  affected INTEGER;\r\nBEGIN\r\n");
                    insertBuilder.Append(")\r\n");
                    valueBuilder.Append(");\r\n");

                    valueBuilder.AppendLine($"\tGET DIAGNOSTICS affected = ROW_COUNT;\r\n\tIF affected = 1 THEN\r\n \t\tRETURN true;\r\n\tELSE\r\n\t\tRETURN false;\r\n\tEND IF;\r\n  END \r\n $function$\r\n;");
                    valueBuilder.AppendLine($"---------------------------------- End Insert SP Code ----------------------------------");
                    valueBuilder.AppendLine();
                }
                return classBuilder.ToString() + insertBuilder.ToString() + valueBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateUpdateStoreProcedureCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                StringBuilder UpdateBuilder = new StringBuilder();
                StringBuilder WhereBuilder = new StringBuilder();
                classBuilder.AppendLine("---------------------------------- Start Update SP Code ----------------------------------");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine($"USE [{SettingHelper.SqlConnectionStringBuilder.InitialCatalog}]");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("SET ANSI_NULLS ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("SET QUOTED_IDENTIFIER ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine($"CREATE PROCEDURE [{SettingHelper.tableName}_Update]");
                    bool appendComma = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (appendComma)
                        {
                            classBuilder.Append(",");
                        }
                        appendComma = true;
                        classBuilder.AppendLine(GetParameterForStoreProcedureForSQL(item.Key, item.Value.Item1));
                        if (item.Value.Item3 || item.Key == "Id")
                        {
                            WhereBuilder.AppendLine($"WHERE [{item.Key}] = @{item.Key}");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(UpdateBuilder.ToString()))
                            {
                                UpdateBuilder.AppendLine($"AS");
                                UpdateBuilder.AppendLine($"BEGIN");
                                UpdateBuilder.AppendLine($"SET NOCOUNT ON;");
                                UpdateBuilder.AppendLine($"UPDATE [{SettingHelper.tableName}]");
                                UpdateBuilder.AppendLine($"SET");
                                UpdateBuilder.AppendLine($"[{item.Key}] = @{item.Key}");
                            }
                            else
                            {
                                UpdateBuilder.AppendLine($",[{item.Key}] = @{item.Key}");
                            }
                        }
                    }
                    WhereBuilder.AppendLine($"END ");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.Append($"CREATE OR REPLACE FUNCTION public.{SettingHelper.tableName.ToLower()}_update(");
                    WhereBuilder.Append($"");
                    bool appendComma = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (appendComma)
                        {
                            classBuilder.Append(",");
                        }
                        appendComma = true;
                        classBuilder.Append($"{item.Key.ToLower()} {item.Value.Item1}");

                        if (item.Value.Item3)
                        {
                            WhereBuilder.AppendLine($"\r\n\tWHERE \"{SettingHelper.tableName}\".\"{item.Key}\" = {item.Key.ToLower()};");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(UpdateBuilder.ToString()))
                            {
                                UpdateBuilder.AppendLine($")\r\nRETURNS boolean");
                                UpdateBuilder.AppendLine($"LANGUAGE plpgsql");
                                UpdateBuilder.AppendLine($"AS $function$");
                                UpdateBuilder.AppendLine($"DECLARE  affected INTEGER;");
                                UpdateBuilder.AppendLine($"begin");
                                UpdateBuilder.Append($"\tUPDATE \"{SettingHelper.tableName}\"");
                                UpdateBuilder.Append($"\r\n\tSET \"{item.Key}\" = {item.Key.ToLower()}");
                            }
                            else
                            {
                                UpdateBuilder.Append($", \"{item.Key}\" = {item.Key.ToLower()}");
                            }
                        }
                    }
                    WhereBuilder.AppendLine($"\tGET DIAGNOSTICS affected = ROW_COUNT;\r\n\tIF affected = 1 THEN\r\n \t\tRETURN true;\r\n\tELSE\r\n\t\tRETURN false;\r\n\tEND IF;\r\n  END \r\n $function$\r\n;");
                }
                WhereBuilder.AppendLine($"---------------------------------- End UPDATE SP Code ----------------------------------");
                WhereBuilder.AppendLine();
                return classBuilder.ToString() + UpdateBuilder.ToString() + WhereBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateSoftDeleteStoreProcedureCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                StringBuilder WhereBuilder = new StringBuilder();
                string deletedId = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? SettingHelper.primaryKeyOfTable.ToString() : "Id";
                classBuilder.AppendLine("---------------------------------- Start Soft Delete SP Code ----------------------------------");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    string columnTypeforSqlServer = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? GetColumnTypeForSP(SettingHelper.TableColumnList.First(x => x.Key == SettingHelper.primaryKeyOfTable).Value.Item1) : "int";
                    classBuilder.AppendLine($"USE [{SettingHelper.SqlConnectionStringBuilder.InitialCatalog}]");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("SET ANSI_NULLS ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("SET QUOTED_IDENTIFIER ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine($"CREATE PROCEDURE [{SettingHelper.tableName}_Soft_Delete]");
                    classBuilder.AppendLine(GetParameterForStoreProcedureForSQL(deletedId, columnTypeforSqlServer));
                    classBuilder.AppendLine($"AS");
                    classBuilder.AppendLine($"BEGIN");
                    classBuilder.AppendLine($"SET NOCOUNT ON;");
                    classBuilder.AppendLine($"UPDATE [{SettingHelper.tableName}]");
                    classBuilder.AppendLine($"SET IsDeleted = 1");
                    WhereBuilder.AppendLine($"WHERE [{deletedId}] = @{deletedId}");
                    WhereBuilder.AppendLine($"END ");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    string columnTypeforPostgreSql = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? GetColumnTypeForSP(SettingHelper.TableColumnList.First(x => x.Key == SettingHelper.primaryKeyOfTable).Value.Item1) : "int";
                    classBuilder.Append($"CREATE OR REPLACE FUNCTION public.{SettingHelper.tableName.ToLower()}_softdelete");
                    classBuilder.Append($"({deletedId.ToLower()} {columnTypeforPostgreSql})");
                    classBuilder.AppendLine($"\r\nRETURNS boolean");
                    classBuilder.AppendLine($"LANGUAGE plpgsql");
                    classBuilder.AppendLine($"AS $function$");
                    classBuilder.AppendLine($"DECLARE  affected INTEGER;");
                    classBuilder.AppendLine($"begin");
                    classBuilder.Append($"\r\n\tUPDATE \"{SettingHelper.tableName}\"");
                    classBuilder.Append($"\r\n\tSET \"IsDeleted\" = TRUE \r\n\tWhere  \"{SettingHelper.tableName}\".\"{deletedId}\" = {deletedId.ToLower()};");
                    classBuilder.AppendLine($"\r\n\tGET DIAGNOSTICS affected = ROW_COUNT;\r\n\tIF affected = 1 THEN\r\n \t\tRETURN true;\r\n\tELSE\r\n\t\tRETURN false;\r\n\tEND IF;\r\n  END \r\n $function$\r\n;");
                }
                WhereBuilder.AppendLine($"---------------------------------- End Soft Delete SP Code ----------------------------------");
                WhereBuilder.AppendLine();
                return classBuilder.ToString() + WhereBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateHardDeleteStoreProcedureCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                StringBuilder WhereBuilder = new StringBuilder();
                string deletedId = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? SettingHelper.primaryKeyOfTable.ToString() : "Id";
                classBuilder.AppendLine("---------------------------------- Start Hard Delete SP Code ----------------------------------");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    string columnTypeforSqlServer = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? GetColumnTypeForSP(SettingHelper.TableColumnList.First(x => x.Key == SettingHelper.primaryKeyOfTable).Value.Item1) : "int";
                    classBuilder.AppendLine($"USE [{SettingHelper.SqlConnectionStringBuilder.InitialCatalog}]");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("SET ANSI_NULLS ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("SET QUOTED_IDENTIFIER ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine($"CREATE PROCEDURE [{SettingHelper.tableName}_Hard_Delete]");
                    classBuilder.AppendLine(GetParameterForStoreProcedureForSQL(deletedId, columnTypeforSqlServer));
                    classBuilder.AppendLine($"AS");
                    classBuilder.AppendLine($"BEGIN");
                    classBuilder.AppendLine($"SET NOCOUNT ON;");
                    classBuilder.AppendLine($"Delete From [{SettingHelper.tableName}]");
                    WhereBuilder.AppendLine($"WHERE [{deletedId}] = @{deletedId}");
                    WhereBuilder.AppendLine($"END ");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    string columnTypeforPostgreSql = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? GetColumnTypeForSP(SettingHelper.TableColumnList.First(x => x.Key == SettingHelper.primaryKeyOfTable).Value.Item1) : "int";
                    classBuilder.Append($"CREATE OR REPLACE FUNCTION public.{SettingHelper.tableName.ToLower()}_harddelete");
                    classBuilder.Append($"({deletedId.ToLower()} {columnTypeforPostgreSql})");
                    classBuilder.AppendLine($"\r\nRETURNS boolean");
                    classBuilder.AppendLine($"LANGUAGE plpgsql");
                    classBuilder.AppendLine($"AS $function$");
                    classBuilder.AppendLine($"DECLARE  affected INTEGER;");
                    classBuilder.AppendLine($"begin");
                    classBuilder.Append($"\r\n\t DELETE FROM \"{SettingHelper.tableName}\"");
                    classBuilder.Append($"\r\n\tWhere  \"{SettingHelper.tableName}\".\"{deletedId}\" = {deletedId.ToLower()};");
                    classBuilder.AppendLine($"\r\n\tGET DIAGNOSTICS affected = ROW_COUNT;\r\n\tIF affected = 1 THEN\r\n \t\tRETURN true;\r\n\tELSE\r\n\t\tRETURN false;\r\n\tEND IF;\r\n  END \r\n $function$\r\n;");
                }
                WhereBuilder.AppendLine($"\r\n---------------------------------- End Hard Delete SP Code ----------------------------------");
                WhereBuilder.AppendLine();
                return classBuilder.ToString() + WhereBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateSelectByIdStoreProcedureCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                StringBuilder parameters = new StringBuilder();
                StringBuilder WhereBuilder = new StringBuilder();
                string SelectId = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? SettingHelper.primaryKeyOfTable.ToString() : "Id";
                classBuilder.AppendLine("---------------------------------- Start Select SP Code ----------------------------------");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    string columnTypeforSqlServer = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? GetColumnTypeForSP(SettingHelper.TableColumnList.First(x => x.Key == SettingHelper.primaryKeyOfTable).Value.Item1) : "int";
                    classBuilder.AppendLine($"USE [{SettingHelper.SqlConnectionStringBuilder.InitialCatalog}]");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("SET ANSI_NULLS ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("SET QUOTED_IDENTIFIER ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine($"CREATE PROCEDURE [{SettingHelper.tableName}_Select_By_Id]");
                    classBuilder.AppendLine(GetParameterForStoreProcedureForSQL(SelectId, columnTypeforSqlServer));
                    classBuilder.AppendLine($"AS");
                    classBuilder.AppendLine($"BEGIN");
                    classBuilder.AppendLine($"SET NOCOUNT ON;");
                    classBuilder.AppendLine($"Select ");
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (i != 0)
                            classBuilder.Append($",");
                        classBuilder.AppendLine($"[{item.Key.ToString()}]");
                    }
                    classBuilder.AppendLine($" From [{SettingHelper.tableName}]");
                    WhereBuilder.AppendLine($"WHERE [{SelectId}] = @{SelectId}");
                    WhereBuilder.AppendLine($"END ");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    string columnTypeforPostgreSql = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? GetColumnTypeForSP(SettingHelper.TableColumnList.First(x => x.Key == SettingHelper.primaryKeyOfTable).Value.Item1) : "int";
                    classBuilder.Append($"CREATE OR REPLACE FUNCTION public.{SettingHelper.tableName.ToLower()}_select_by_id");
                    classBuilder.Append($"({SelectId.ToLower()} {columnTypeforPostgreSql})");
                    classBuilder.AppendLine($"\n RETURNS TABLE (");
                    parameters.Append($"   RETURN QUERY \n select ");

                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (i != 0)
                        {
                            classBuilder.Append($",");
                            parameters.Append($",");
                        }
                        else
                            parameters.Append("\t");
                        classBuilder.Append($" \"{item.Key.ToString()}\" {item.Value.Item1}");
                        parameters.Append($"  \"{SettingHelper.tableName}\".\"{item.Key.ToString()}\"");
                    }
                    parameters.AppendLine($"\r\n\tfrom \"{SettingHelper.tableName}\"");
                    classBuilder.AppendLine($")");
                    classBuilder.AppendLine($"\nLANGUAGE plpgsql");
                    classBuilder.AppendLine($"AS $function$");
                    classBuilder.AppendLine($"begin");

                    parameters.AppendLine($"\twhere  \"{SettingHelper.tableName}\".\"{SelectId}\" = {SelectId.ToLower()};");
                    parameters.AppendLine($"END\r\n$function$\r\n;");
                }
                WhereBuilder.AppendLine($"---------------------------------- End Select SP Code ----------------------------------");
                WhereBuilder.AppendLine();
                return classBuilder.ToString() + parameters.ToString() + WhereBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateSelectAllRecordWithPagination()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                classBuilder.AppendLine("---------------------------------- Start Select All with Pagination, Search And Sort Code ----------------------------------");
                string tableNameAsVariable = ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName);
                string tableListAsVariable = $"{ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName)}List";

                classBuilder.AppendLine($"public static string {SettingHelper.tableName}SelectListWithPaginationAndSearch(DataTablePagintaion dataTablePagintaion)");
                classBuilder.AppendLine("{\r\n\ttry\r\n\t{");

                classBuilder.AppendLine($"\t\tstring Data = \"\";");
                classBuilder.AppendLine($"\t\tint skip = ((dataTablePagintaion.PageIndex - 1) * dataTablePagintaion.PageSize);");
                classBuilder.AppendLine($"\t\tType myType = typeof({SettingHelper.tableName});");
                classBuilder.AppendLine($"\t\tvar field = myType.GetProperty(dataTablePagintaion.SortField);");
                classBuilder.AppendLine("\t\tif (field == null)\r\n\t\t{");
                classBuilder.AppendLine("\t\t\tdataTablePagintaion.SortField = \"createddate\";\n\t\t}\r\n\t\t");
                classBuilder.AppendLine("\t\telse");
                classBuilder.AppendLine($"\t\t\tdataTablePagintaion.SortField = field.Name.ToLower();");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine($"\t\t\tList<SqlParameter> paramList = new List<SqlParameter>();");
                    classBuilder.AppendLine($"\t\t\tparamList.Add(new SqlParameter(\"@SearchText\", dataTablePagintaion.SearchText));");
                    classBuilder.AppendLine($"\t\t\tparamList.Add(new SqlParameter(\"@PageIndex\", dataTablePagintaion.PageIndex));");
                    classBuilder.AppendLine($"\t\t\tparamList.Add(new SqlParameter(\"@PageSize\", dataTablePagintaion.PageSize));");
                    classBuilder.AppendLine($"\t\t\tparamList.Add(new SqlParameter(\"@SortColumn\", dataTablePagintaion.SortField.ToString().ToLower()));");
                    classBuilder.AppendLine($"\t\t\tparamList.Add(new SqlParameter(\"@SortOrder\", dataTablePagintaion.SortType));");
                    classBuilder.AppendLine($"\t\t\tDataTable dt = SqlHelper.ExecuteDataTable(SettingHelper.SqlConnectionStringBuilder.ConnectionString, CommandType.StoredProcedure, \"{SettingHelper.tableName}_Select_List_With_Pagination_And_Search\", paramList.ToArray());");
                    classBuilder.AppendLine("\t\t\tif(dt != null && dt.Rows.Count > 0)");
                    classBuilder.AppendLine($"\t\t\t\t\tData = JsonConvert.SerializeObject(dt);");
                    //classBuilder.AppendLine($"\t\t\t\tList<{SettingHelper.tableName}> objList = JsonConvert.DeserializeObject<List<{SettingHelper.tableName}>>(JsonString);");
                    //classBuilder.AppendLine($"\t\t\t\tint totalRowsCount = Convert.ToInt32(((Newtonsoft.Json.Linq.JValue)((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)JsonConvert.DeserializeObject(JsonString)).First.First).Value).Value);");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<NpgsqlParameter> paramList = new List<NpgsqlParameter>();");
                    classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"offsetsize\", skip > 0 ? skip : 0));");
                    classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"pagesize\", dataTablePagintaion.PageSize));");
                    classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"searchtext\", dataTablePagintaion.SearchText));");
                    classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"sortcolumn\", dataTablePagintaion.SortField));");
                    classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"sorttype\", dataTablePagintaion.SortType));");
                    classBuilder.AppendLine($"\t\tPostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();");
                    classBuilder.AppendLine($"\t\tData = postgreSQLHandler.Select(\"{SettingHelper.tableName.ToLower()}_select_list_with_pagination_search_and_sort\", paramList);");
                }
                classBuilder.AppendLine($"\t\treturn Data;");
                classBuilder.AppendLine("\t}\r\n\tcatch (Exception ex)\r\n\t{\r\n\t\tthrow ex;\r\n\t}\r\n}");
                classBuilder.AppendLine("---------------------------------- End Select All with Pagination, Search And Sort Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Method for Pagination by SIR 
        //private string GenerateSelectAllRecordWithPagination()
        //{
        //    try
        //    {
        //        StringBuilder classBuilder = new StringBuilder();
        //        classBuilder.AppendLine("---------------------------------- Start Select All with Pagination, Search And Sort Code ----------------------------------");
        //        string tableNameAsVariable = ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName);
        //        string tableListAsVariable = $"{ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName)}List";

        //        classBuilder.AppendLine($"public static PaginationModel {SettingHelper.tableName}SelectListWithPaginationAndSearch(DataTablePagintaion dataTablePagintaion)");
        //        classBuilder.AppendLine("{\r\n\ttry\r\n\t{");
        //        classBuilder.AppendLine($"\t\tPaginationModel paginationModel = new PaginationModel();");
        //        classBuilder.AppendLine($"\t\tint skip = ((dataTablePagintaion.PageIndex - 1) * dataTablePagintaion.PageSize);");
        //        classBuilder.AppendLine($"\t\tType myType = typeof({SettingHelper.tableName});");
        //        classBuilder.AppendLine($"\t\tvar field = myType.GetProperty(dataTablePagintaion.SortField);");
        //        classBuilder.AppendLine("\t\tif (field == null)\r\n\t\t{");
        //        classBuilder.AppendLine("\t\t\tdataTablePagintaion.SortField = \"createddate\";");
        //        classBuilder.AppendLine("\t\t\tdataTablePagintaion.SortType = -1;\r\n\t\t}");
        //        classBuilder.AppendLine("\t\telse");
        //        classBuilder.AppendLine($"\t\t\tdataTablePagintaion.SortField = field.Name.ToLower();");
        //        if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
        //        {
        //            classBuilder.AppendLine($"List<SqlParameter> paramList = new List<SqlParameter>();");
        //            classBuilder.AppendLine($"paramList.Add(new SqlParameter(\"@SearchText\", dataTablePagintaion.SearchText));");
        //            classBuilder.AppendLine($"paramList.Add(new SqlParameter(\"@PageIndex\", dataTablePagintaion.PageIndex));");
        //            classBuilder.AppendLine($"paramList.Add(new SqlParameter(\"@PageSize\", dataTablePagintaion.PageSize));");
        //            classBuilder.AppendLine($"paramList.Add(new SqlParameter(\"@SortColumn\", dataTablePagintaion.SortField.ToString().ToLower()));");
        //            classBuilder.AppendLine($"paramList.Add(new SqlParameter(\"@SortOrder\", dataTablePagintaion.SortType));");
        //            classBuilder.AppendLine($" DataTable dt = SqlHelper.ExecuteDataTable(SettingHelper.SqlConnectionStringBuilder.ConnectionString, CommandType.StoredProcedure, \"{SettingHelper.tableName}_Select_List_With_Pagination_And_Search\", paramList.ToArray());");
        //            classBuilder.AppendLine("if(dt != null && dt.Rows.Count > 0) { ");
        //            classBuilder.AppendLine($"string JsonString = JsonConvert.SerializeObject(dt);");
        //            classBuilder.AppendLine($"List<{SettingHelper.tableName}> objList = JsonConvert.DeserializeObject<List<{SettingHelper.tableName}>>(JsonString);");
        //            classBuilder.AppendLine($"int totalRowsCount = Convert.ToInt32(((Newtonsoft.Json.Linq.JValue)((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)JsonConvert.DeserializeObject(JsonString)).First.First).Value).Value);");
        //        }
        //        else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
        //        {
        //            classBuilder.AppendLine("\t\tList<NpgsqlParameter> paramList = new List<NpgsqlParameter>();");
        //            classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"offsetsize\", skip > 0 ? skip : 0));");
        //            classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"pagesize\", dataTablePagintaion.PageSize));");
        //            classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"searchtext\", dataTablePagintaion.SearchText));");
        //            classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"sortcolumn\", dataTablePagintaion.SortField));");
        //            classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"sorttype\", dataTablePagintaion.SortType));");
        //            classBuilder.AppendLine($"\t\tPostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();");
        //            classBuilder.AppendLine($"\t\tList<DataListModel> dataList = postgreSQLHandler.ExecuteAsObject<List<DataListModel>>\t\t(\"{SettingHelper.tableName.ToLower()}_select_list_with_pagination_search_and_sort\", paramList);");
        //            classBuilder.AppendLine($"\t\tlong totalRowsCount = 0;\r\n\t\tif (dataList != null && dataList.Any())");
        //            classBuilder.AppendLine($"\t\t\ttotalRowsCount = dataList.First().TotalCount;");
        //            classBuilder.AppendLine($"\t\tstring jsonString = JsonConvert.SerializeObject(dataList);");
        //            classBuilder.AppendLine($"\t\tList<{SettingHelper.tableName}> objList = JsonConvert.DeserializeObject<List<{SettingHelper.tableName}>>(jsonString);");
        //        }
        //        classBuilder.AppendLine($"\t\tpaginationModel.TotalPages = (int)Math.Ceiling((float)totalRowsCount / dataTablePagintaion.PageSize);");
        //        classBuilder.AppendLine($"\t\tpaginationModel.PageIndex = dataTablePagintaion.PageIndex;");
        //        classBuilder.AppendLine($"\t\tpaginationModel.PageSize = dataTablePagintaion.PageSize;");
        //        classBuilder.AppendLine($"\t\tpaginationModel.SearchText = dataTablePagintaion.SearchText;");
        //        classBuilder.AppendLine($"\t\tpaginationModel.SortField = dataTablePagintaion.SortField;");
        //        classBuilder.AppendLine($"\t\tpaginationModel.SortType = dataTablePagintaion.SortType;");
        //        classBuilder.AppendLine($"\t\tpaginationModel.TotalRows = totalRowsCount;");
        //        classBuilder.AppendLine($"\t\tpaginationModel.List = objList;");
        //        classBuilder.AppendLine($"\t\treturn paginationModel;");
        //        classBuilder.AppendLine("\t}\r\n\tcatch (Exception ex)\r\n\t{\r\n\t\tthrow ex;\r\n\t}\r\n}");
        //        classBuilder.AppendLine("---------------------------------- End Select All with Pagination, Search And Sort Code ----------------------------------");
        //        classBuilder.AppendLine();
        //        return classBuilder.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        private string GenerateSelectAllWithPaginationStoreProcedureCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                string selectId = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? SettingHelper.primaryKeyOfTable.ToString() : "Id";
                classBuilder.AppendLine("---------------------------------- Start Select all with pagination SP Code ----------------------------------");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine($"USE [{SettingHelper.SqlConnectionStringBuilder.InitialCatalog}]");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("SET ANSI_NULLS ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("SET QUOTED_IDENTIFIER ON");
                    classBuilder.AppendLine("GO");
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine($"CREATE PROCEDURE [{SettingHelper.tableName}_Select_List_With_Pagination_And_Search]");
                    classBuilder.AppendLine($"");
                    classBuilder.AppendLine($"@SearchText nvarchar(300)=null,\r\n@PageIndex INT = 1,\r\n@PageSize INT = 100,\r\n@SortColumn NVARCHAR(50) = '',\r\n@SortOrder int=1");
                    classBuilder.AppendLine($"");
                    classBuilder.AppendLine($"AS \r\n BEGIN");
                    classBuilder.AppendLine($"Declare ");
                    classBuilder.AppendLine($"@SearchTextTemp nvarchar(300),\r\n @PageIndexTemp INT,\r\n@PageSizeTemp INT,\r\n@SortColTemp NVARCHAR(50),\r\n@FirstRecTemp INT,");
                    classBuilder.AppendLine($"@LastRecTemp INT,\r\n@TotalRowsTemp INT");
                    classBuilder.AppendLine($"");
                    classBuilder.AppendLine($"SET @SearchTextTemp = LTRIM(RTRIM(@SearchText));");
                    classBuilder.AppendLine($"SET @PageIndexTemp = @PageIndex;");
                    classBuilder.AppendLine($"SET @PageSizeTemp = @PageSize;");
                    classBuilder.AppendLine($"SET @SortColTemp = LTRIM(RTRIM(@SortColumn));");
                    classBuilder.AppendLine($"SET @FirstRecTemp = ( @PageIndexTemp - 1 ) * @PageSizeTemp;");
                    classBuilder.AppendLine($"SET @LastRecTemp = ( @PageIndexTemp * @PageSizeTemp + 1 );");
                    classBuilder.AppendLine($"SET @TotalRowsTemp = @FirstRecTemp - @LastRecTemp + 1;");
                    classBuilder.AppendLine($"");
                    classBuilder.AppendLine($"WITH {SettingHelper.tableName}Data as(SELECT ROW_NUMBER() OVER (ORDER BY");
                    classBuilder.AppendLine($"");
                    bool addComma = false;

                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (IsColumnString(item.Value.Item1) || IsColumnDate(item.Value.Item1))
                        {
                            if (addComma)
                                classBuilder.Append($",");
                            classBuilder.AppendLine($" CASE WHEN @SortColTemp = '{item.Key.ToLower().ToString()}' AND @SortOrder = 1 THEN [{item.Key.ToString()}] END ASC");
                            classBuilder.AppendLine($" ,CASE WHEN @SortColTemp = '{item.Key.ToLower().ToString()}' AND @SortOrder = -1 THEN [{item.Key.ToString()}] END DESC");
                            addComma = true;
                        }
                    }
                    classBuilder.AppendLine($") AS ROWNUM, Count(Id) over () AS TotalCount, * From {SettingHelper.tableName}");
                    bool addOr = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (IsColumnString(item.Value.Item1))
                        {
                            if (addOr)
                                classBuilder.AppendLine($"OR");
                            else
                                classBuilder.AppendLine($" WHERE (");

                            classBuilder.AppendLine($" (@SearchTextTemp IS NULL OR [{item.Key.ToString()}]  LIKE '%' + @SearchTextTemp + '%')");
                            if (addOr == false)
                                addOr = true;
                        }
                    }
                    if (addOr)
                        classBuilder.AppendLine($")");

                    classBuilder.AppendLine($")");
                    classBuilder.AppendLine($"SELECT");
                    classBuilder.AppendLine($"ROWNUM");
                    classBuilder.AppendLine($",TotalCount");
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        classBuilder.AppendLine($",{item.Key.ToString()}");
                    }
                    classBuilder.AppendLine($"FROM {SettingHelper.tableName}Data");
                    classBuilder.AppendLine($"WHERE ROWNUM > @FirstRecTemp AND ROWNUM < @LastRecTemp");
                    classBuilder.AppendLine($"ORDER BY ROWNUM ASC");
                    classBuilder.AppendLine($"");
                    classBuilder.AppendLine($"END");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.Append($"CREATE OR REPLACE FUNCTION public.{SettingHelper.tableName.ToLower()}_select_list_with_pagination_search_and_sort");
                    classBuilder.Append($"(offsetsize bigint, pagesize bigint, searchtext varchar, sortcolumn varchar, sorttype integer DEFAULT '-1'::integer)");
                    classBuilder.Append($"\r\nRETURNS TABLE (\"ROWNUM\" bigint, \"TotalCount\" bigint,");
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        classBuilder.Append($"\"{item.Key.ToString()}\" {item.Value.Item1}");
                        if (i < SettingHelper.TableColumnList.Count() - 1)
                            classBuilder.Append($", ");
                    }
                    classBuilder.AppendLine($")");
                    classBuilder.AppendLine($"LANGUAGE plpgsql");
                    classBuilder.AppendLine($"AS $function$");
                    classBuilder.AppendLine($"declare");
                    classBuilder.AppendLine($"\tSortColTemp varchar = TRIM(BOTH FROM sortcolumn);");
                    classBuilder.AppendLine($"begin");
                    classBuilder.AppendLine($"\treturn query\r\n\tSELECT ROW_NUMBER() OVER (ORDER BY");
                    bool addComma = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (IsColumnString(item.Value.Item1) || IsColumnDate(item.Value.Item1))
                        {
                            if (addComma)
                                classBuilder.Append($"\t\t,");
                            else
                                classBuilder.Append($"\t\t");
                            classBuilder.AppendLine($"CASE WHEN sortcolumn = '{item.Key.ToLower().ToString()}' AND sorttype = 1 THEN \"{SettingHelper.tableName}\".\"{item.Key.ToString()}\" END ASC");
                            classBuilder.AppendLine($"\t\t,CASE WHEN sortcolumn = '{item.Key.ToLower().ToString()}' AND sorttype = -1 THEN \"{SettingHelper.tableName}\".\"{item.Key.ToString()}\" END DESC");
                            addComma = true;
                        }
                    }
                    classBuilder.AppendLine($"\t) AS \"ROWNUM\", count(\"{SettingHelper.tableName}\".\"{selectId.ToString()}\") over () AS \"TotalCount\",");
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        classBuilder.Append($"\t\"{SettingHelper.tableName}\".\"{item.Key.ToString()}\"");
                        if (i < SettingHelper.TableColumnList.Count() - 1)
                            classBuilder.Append($", \n");
                    }
                    classBuilder.Append($"\n\tFrom \"{SettingHelper.tableName}\"\n");

                    bool addOr = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (IsColumnString(item.Value.Item1))
                        {
                            if (addOr)
                                classBuilder.AppendLine($"\tOR");
                            else
                                classBuilder.AppendLine($"\tWHERE ");
                            classBuilder.AppendLine($"\t (searchtext IS NULL OR \"{SettingHelper.tableName}\".\"{item.Key.ToString()}\"  ILIKE CONCAT('%',searchtext,'%'))");
                            addOr = true;
                        }
                    }
                    classBuilder.AppendLine($"\toffset offsetsize");
                    classBuilder.AppendLine($"\tlimit pagesize;");
                    classBuilder.AppendLine($"END\r\n$function$\r\n;");
                }
                classBuilder.AppendLine($"---------------------------------- End Select all with pagination SP Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        //SP code For Pagination by SIR
        //private string GenerateSelectAllWithPaginationStoreProcedureCode()
        //{
        //    try
        //    {
        //        StringBuilder classBuilder = new StringBuilder();
        //        string selectId = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? SettingHelper.primaryKeyOfTable.ToString() : "Id";
        //        classBuilder.AppendLine("---------------------------------- Start Select all with pagination SP Code ----------------------------------");
        //        if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
        //        {
        //            classBuilder.AppendLine($"USE [{SettingHelper.SqlConnectionStringBuilder.InitialCatalog}]");
        //            classBuilder.AppendLine("");
        //            classBuilder.AppendLine("SET ANSI_NULLS ON");
        //            classBuilder.AppendLine("GO");
        //            classBuilder.AppendLine("SET QUOTED_IDENTIFIER ON");
        //            classBuilder.AppendLine("GO");
        //            classBuilder.AppendLine("");
        //            classBuilder.AppendLine($"CREATE PROCEDURE [{SettingHelper.tableName}_Select_List_With_Pagination_And_Search]");
        //            classBuilder.AppendLine($"");
        //            classBuilder.AppendLine($"@SearchText nvarchar(300)=null,\r\n@PageIndex INT = 1,\r\n@PageSize INT = 100,\r\n@SortColumn NVARCHAR(50) = '',\r\n@SortOrder int=1");
        //            classBuilder.AppendLine($"");
        //            classBuilder.AppendLine($"AS \r\n BEGIN");
        //            classBuilder.AppendLine($"Declare ");
        //            classBuilder.AppendLine($"@SearchTextTemp nvarchar(300),\r\n @PageIndexTemp INT,\r\n@PageSizeTemp INT,\r\n@SortColTemp NVARCHAR(50),\r\n@FirstRecTemp INT,");
        //            classBuilder.AppendLine($"@LastRecTemp INT,\r\n@TotalRowsTemp INT");
        //            classBuilder.AppendLine($"");
        //            classBuilder.AppendLine($"SET @SearchTextTemp = LTRIM(RTRIM(@SearchText));");
        //            classBuilder.AppendLine($"SET @PageIndexTemp = @PageIndex;");
        //            classBuilder.AppendLine($"SET @PageSizeTemp = @PageSize;");
        //            classBuilder.AppendLine($"SET @SortColTemp = LTRIM(RTRIM(@SortColumn));");
        //            classBuilder.AppendLine($"SET @FirstRecTemp = ( @PageIndexTemp - 1 ) * @PageSizeTemp;");
        //            classBuilder.AppendLine($"SET @LastRecTemp = ( @PageIndexTemp * @PageSizeTemp + 1 );");
        //            classBuilder.AppendLine($"SET @TotalRowsTemp = @FirstRecTemp - @LastRecTemp + 1;");
        //            classBuilder.AppendLine($"");
        //            classBuilder.AppendLine($"WITH {SettingHelper.tableName}Data as(SELECT ROW_NUMBER() OVER (ORDER BY");
        //            classBuilder.AppendLine($"");
        //            bool addComma = false;

        //            for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
        //            {
        //                var item = SettingHelper.TableColumnList.ElementAt(i);
        //                if (IsColumnString(item.Value.Item1) || IsColumnDate(item.Value.Item1))
        //                {
        //                    if (addComma)
        //                        classBuilder.Append($",");
        //                    classBuilder.AppendLine($" CASE WHEN @SortColTemp = '{item.Key.ToLower().ToString()}' AND @SortOrder = 1 THEN [{item.Key.ToString()}] END ASC");
        //                    classBuilder.AppendLine($" ,CASE WHEN @SortColTemp = '{item.Key.ToLower().ToString()}' AND @SortOrder = -1 THEN [{item.Key.ToString()}] END DESC");
        //                    addComma = true;
        //                }
        //            }
        //            classBuilder.AppendLine($") AS ROWNUM, Count(Id) over () AS TotalCount, * From {SettingHelper.tableName}");
        //            bool addOr = false;
        //            for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
        //            {
        //                var item = SettingHelper.TableColumnList.ElementAt(i);
        //                if (IsColumnString(item.Value.Item1))
        //                {
        //                    if (addOr)
        //                        classBuilder.AppendLine($"OR");
        //                    else
        //                        classBuilder.AppendLine($" WHERE (");

        //                    classBuilder.AppendLine($" (@SearchTextTemp IS NULL OR [{item.Key.ToString()}]  LIKE '%' + @SearchTextTemp + '%')");
        //                    if (addOr == false)
        //                        addOr = true;
        //                }
        //            }
        //            if (addOr)
        //                classBuilder.AppendLine($")");

        //            classBuilder.AppendLine($")");
        //            classBuilder.AppendLine($"SELECT");
        //            classBuilder.AppendLine($"TotalCount");
        //            classBuilder.AppendLine($",ROWNUM");
        //            for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
        //            {
        //                var item = SettingHelper.TableColumnList.ElementAt(i);
        //                classBuilder.AppendLine($",{item.Key.ToString()}");
        //            }
        //            classBuilder.AppendLine($"FROM {SettingHelper.tableName}Data");
        //            classBuilder.AppendLine($"WHERE ROWNUM > @FirstRecTemp AND ROWNUM < @LastRecTemp");
        //            classBuilder.AppendLine($"ORDER BY ROWNUM ASC");
        //            classBuilder.AppendLine($"");
        //            classBuilder.AppendLine($"END");
        //        }
        //        else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
        //        {
        //            classBuilder.Append($"CREATE OR REPLACE FUNCTION public.{SettingHelper.tableName.ToLower()}_select_list_with_pagination_search_and_sort");
        //            classBuilder.Append($"(offsetsize bigint, pagesize bigint, searchtext varchar, sortcolumn varchar, sorttype integer DEFAULT '-1'::integer)");
        //            classBuilder.Append($"\r\nRETURNS TABLE (\"ROWNUM\" bigint, \"TotalCount\" bigint,");
        //            for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
        //            {
        //                var item = SettingHelper.TableColumnList.ElementAt(i);
        //                classBuilder.Append($"\"{item.Key.ToString()}\" {item.Value.Item1}");
        //                if (i < SettingHelper.TableColumnList.Count() - 1)
        //                    classBuilder.Append($", ");
        //            }
        //            classBuilder.AppendLine($")");
        //            classBuilder.AppendLine($"LANGUAGE plpgsql");
        //            classBuilder.AppendLine($"AS $function$");
        //            classBuilder.AppendLine($"declare");
        //            classBuilder.AppendLine($"\tSortColTemp varchar = TRIM(BOTH FROM sortcolumn);");
        //            classBuilder.AppendLine($"begin");
        //            classBuilder.AppendLine($"\treturn query\r\n\tSELECT ROW_NUMBER() OVER (ORDER BY");
        //            bool addComma = false;
        //            for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
        //            {
        //                var item = SettingHelper.TableColumnList.ElementAt(i);
        //                if (IsColumnString(item.Value.Item1) || IsColumnDate(item.Value.Item1))
        //                {
        //                    if (addComma)
        //                        classBuilder.Append($"\t\t,");
        //                    else
        //                        classBuilder.Append($"\t\t");
        //                    classBuilder.AppendLine($"CASE WHEN sortcolumn = '{item.Key.ToLower().ToString()}' AND sorttype = 1 THEN \"{SettingHelper.tableName}\".\"{item.Key.ToString()}\" END ASC");
        //                    classBuilder.AppendLine($"\t\t,CASE WHEN sortcolumn = '{item.Key.ToLower().ToString()}' AND sorttype = -1 THEN \"{SettingHelper.tableName}\".\"{item.Key.ToString()}\" END DESC");
        //                    addComma = true;
        //                }
        //            }
        //            classBuilder.AppendLine($"\t) AS \"ROWNUM\", count(\"{SettingHelper.tableName}\".\"{selectId.ToString()}\") over () AS \"TotalCount\", \"{SettingHelper.tableName}\".* From \"{SettingHelper.tableName}\"");
        //            bool addOr = false;
        //            for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
        //            {
        //                var item = SettingHelper.TableColumnList.ElementAt(i);
        //                if (IsColumnString(item.Value.Item1))
        //                {
        //                    if (addOr)
        //                        classBuilder.AppendLine($"\tOR");
        //                    else
        //                        classBuilder.AppendLine($"\t WHERE ");
        //                    classBuilder.AppendLine($"\t (searchtext IS NULL OR \"{SettingHelper.tableName}\".\"{item.Key.ToString()}\"  ILIKE CONCAT('%',searchtext,'%'))");
        //                    addOr = true;
        //                }
        //            }
        //            classBuilder.AppendLine($"\toffset offsetsize");
        //            classBuilder.AppendLine($"\tlimit pagesize;");
        //            classBuilder.AppendLine($"END\r\n$function$\r\n;");
        //        }
        //        classBuilder.AppendLine($"---------------------------------- End Select all with pagination SP Code ----------------------------------");
        //        return classBuilder.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        private string GenerateListStoreProcedure()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                StringBuilder returnParams = new StringBuilder();
                StringBuilder tableParams = new StringBuilder();
                classBuilder.AppendLine("---------------------------------- Start List's SP Code ----------------------------------");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine($"USE [{SettingHelper.SqlConnectionStringBuilder.InitialCatalog}]");
                    classBuilder.AppendLine($"CREATE PROCEDURE dbo.getAllDataOf_{SettingHelper.tableName.ToLower()}");
                    classBuilder.Append($"AS\r\n" + $"BEGIN\r\n    " + $"SET NOCOUNT ON;");
                    returnParams.AppendLine($"\nSELECT ");
                    bool appendComma = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (appendComma)
                        {
                            returnParams.Append(",");
                        }
                        appendComma = true;
                        returnParams.Append($"{item.Key}");
                    }
                    returnParams.AppendLine($" From dbo.{SettingHelper.tableName}");
                    tableParams.AppendLine($"End;");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.AppendLine($"CREATE OR REPLACE FUNCTION public.get_allDataOf_{SettingHelper.tableName.ToLower()}()");
                    returnParams.Append($"RETURNS TABLE(");
                    tableParams.AppendLine($"SELECT ");
                    bool isFirstRow = false;
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (isFirstRow == false)
                        {
                            returnParams.Append($" \n\"{item.Key.ToLower()}\" {item.Value.Item1}");
                            tableParams.Append($"\"{SettingHelper.tableName}\".\"{item.Key}\"");
                            isFirstRow = true;
                        }
                        else
                        {
                            returnParams.Append(",");
                            returnParams.Append($" \n\"{item.Key.ToLower()}\" {item.Value.Item1}");
                            tableParams.Append(",");
                            tableParams.Append($"\"{SettingHelper.tableName}\".\"{item.Key}\"");
                        }
                    }
                    returnParams.Append(")");
                    returnParams.AppendLine($"\nLANGUAGE plpgsql");
                    returnParams.AppendLine($"AS $function$");
                    returnParams.AppendLine($"BEGIN");
                    returnParams.AppendLine($"   RETURN QUERY");
                    tableParams.AppendLine($" FROM public.\"{SettingHelper.tableName}\"");
                    tableParams.AppendLine($" Where \"IsDeleted\" = false;");
                    tableParams.AppendLine($"END;\r\n$function$\r\n;");
                }
                tableParams.AppendLine("\n---------------------------------- End List's SP Code ----------------------------------");
                tableParams.AppendLine();
                return classBuilder.ToString() + returnParams.ToString() + tableParams.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateListCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                classBuilder.AppendLine("---------------------------------- Start List's Code ----------------------------------");
                classBuilder.AppendLine($"public static List<{SettingHelper.tableName}> GetAllData()");
                classBuilder.AppendLine($"{{");
                classBuilder.AppendLine($"            try\r\n            {{");
                classBuilder.AppendLine($"                List<{SettingHelper.tableName}> {SettingHelper.tableName.ToLower()}List = new List<{SettingHelper.tableName}>();");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine($"                DataTable dt = SqlHelper.ExecuteDataTable(connectionString, \"dbo.getAllDataOf_{SettingHelper.tableName.ToLower()}\");");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.AppendLine("\t PostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();");
                    classBuilder.AppendLine($"                DataTable dt = postgreSQLHandler.ExecuteSQL(\"SELECT * FROM get_allDataOf_{SettingHelper.tableName.ToLower()}()\");");
                }
                classBuilder.AppendLine($"                if (dt.Rows.Count > 0)\r\n                {{");
                classBuilder.AppendLine($"                    foreach (DataRow row in dt.Rows)\r\n                    {{");
                classBuilder.AppendLine($"                        {SettingHelper.tableName} {SettingHelper.tableName.ToLower()}Data = new {SettingHelper.tableName}\r\n                        {{");
                for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                {
                    var item = SettingHelper.TableColumnList.ElementAt(i);
                    classBuilder.AppendLine($"                            {item.Key} = row[\"{item.Key}\"],");
                }
                classBuilder.AppendLine($"                        }};");
                classBuilder.AppendLine($"                        {SettingHelper.tableName.ToLower()}List.Add({SettingHelper.tableName.ToLower()}Data);");
                classBuilder.AppendLine($"                    }}");
                classBuilder.AppendLine($"                }}");
                classBuilder.AppendLine($"                return {SettingHelper.tableName.ToLower()}List;");
                classBuilder.AppendLine($"            }}");
                classBuilder.AppendLine($"            catch (Exception ex)\r\n            " +
                    $"{{\r\n                " +
                    $"throw ex;  " +
                    $"\n             }}");
                classBuilder.AppendLine($" }}");
                classBuilder.AppendLine("---------------------------------- End List's Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateInsertRecordCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                string tableNameAsVariable = ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName);
                classBuilder.AppendLine("---------------------------------- Start Add Record Code ----------------------------------");
                classBuilder.AppendLine($"public static void {SettingHelper.tableName}Insert ({SettingHelper.tableName} {tableNameAsVariable})");
                classBuilder.AppendLine("{\r\n\ttry\r\n\t{");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<SqlParameter> paramList = new List<SqlParameter>();");
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        if (item.Key.ToLower().Trim() != SettingHelper.primaryKeyOfTable.ToLower().Trim())
                        {
                            classBuilder.AppendLine($"\t\tparamList.Add(new SqlParameter(@\"{item.Key}\", {tableNameAsVariable}.{item.Key}));");
                        }
                    }
                    classBuilder.AppendLine($"\t\tSqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, \"{SettingHelper.tableName}_Insert\", paramList.ToArray());");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<NpgsqlParameter> paramList = new List<NpgsqlParameter>();");
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        string datatype = item.Value.Item1;
                        var NpgsqlDbType = dataTypeNpgsqlDbType(datatype);                       
                        if (item.Key.ToLower().Trim() != SettingHelper.primaryKeyOfTable.ToLower().Trim())
                        {
                            classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"{item.Key.ToLower().ToString()}\", NpgsqlTypes.NpgsqlDbType.{NpgsqlDbType}) {{Value = {tableNameAsVariable}.{item.Key}}});");
                        }
                    }
                    classBuilder.AppendLine("\t\tPostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();");
                    classBuilder.AppendLine($"\t\tbool spResult = postgreSQLHandler.ExecuteAsScalar<bool>(\"{SettingHelper.tableName.ToLower()}_add\", paramList);");
                }
                classBuilder.AppendLine("\t}\r\n\tcatch (Exception ex)\r\n\t{\r\n\t\tthrow ex;\r\n\t}\r\n}");
                classBuilder.AppendLine("---------------------------------- End Add Record Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

      
        private string GenerateUpdateRecordCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                string tableNameAsVariable = ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName);
                classBuilder.AppendLine("---------------------------------- Start Update Record Code ----------------------------------");
                classBuilder.AppendLine($"public static void {SettingHelper.tableName}Update ({SettingHelper.tableName} {tableNameAsVariable})");
                classBuilder.AppendLine("{\r\n\ttry\r\n\t{");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<SqlParameter> paramList = new List<SqlParameter>();");
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        classBuilder.AppendLine($"\t\tparamList.Add(new SqlParameter(@\"{item.Key}\", {tableNameAsVariable}.{item.Key}));");
                    }
                    classBuilder.AppendLine($"\t\tSqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, \"{SettingHelper.tableName}_Update\", paramList.ToArray());");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<NpgsqlParameter> paramList = new List<NpgsqlParameter>();");
                    for (int i = 0; i < SettingHelper.TableColumnList.Count(); i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        string datatype = item.Value.Item1;
                        var NpgsqlDbType = dataTypeNpgsqlDbType(datatype);
                        //if (item.Key.ToLower().Trim() != SettingHelper.primaryKeyOfTable.ToLower().Trim())
                        //{
                        classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"{item.Key.ToLower().ToString()}\", NpgsqlTypes.NpgsqlDbType.{NpgsqlDbType}) {{Value = {tableNameAsVariable}.{item.Key}}});");
                        //}
                    }
                    classBuilder.AppendLine("\t\tPostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();");
                    classBuilder.AppendLine($"\t\tbool spResult = postgreSQLHandler.ExecuteAsScalar<bool>(\"{SettingHelper.tableName.ToLower()}_update\", paramList);");
                }
                classBuilder.AppendLine("\t}\r\n\tcatch (Exception ex)\r\n\t{\r\n\t\tthrow ex;\r\n\t}\r\n}");
                classBuilder.AppendLine("---------------------------------- End Update Record Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
        private string GenerateSoftDeleteRecordCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                string deletedId = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? SettingHelper.primaryKeyOfTable.ToString() : "Id";
                string NpgsqlDbType = !string.IsNullOrEmpty(SettingHelper.primaryKeyDataType) && SettingHelper.primaryKeyDataType != "" ? dataTypeNpgsqlDbType(SettingHelper.primaryKeyDataType.ToString()) : "Interger";
                string tableNameAsVariable = ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName);
                classBuilder.AppendLine("---------------------------------- Start Soft Delete Record Code ----------------------------------");
                classBuilder.AppendLine($"public static void {SettingHelper.tableName}SoftDelete ({SettingHelper.tableName} {tableNameAsVariable})");
                classBuilder.AppendLine("{\r\n\ttry\r\n\t{");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<SqlParameter> paramList = new List<SqlParameter>();");
                    classBuilder.AppendLine($"\t\tparamList.Add(new SqlParameter(@\"{deletedId}\", {tableNameAsVariable}.{deletedId}));");
                    classBuilder.AppendLine($"\t\tSqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, \"{SettingHelper.tableName}_Soft_Delete\", paramList.ToArray());");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<NpgsqlParameter> paramList = new List<NpgsqlParameter>();");
                    classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"{deletedId.ToLower()}\",  NpgsqlTypes.NpgsqlDbType.{NpgsqlDbType}) {{Value = {tableNameAsVariable}.{deletedId}}});");
                    classBuilder.AppendLine("\t\tPostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();");
                    classBuilder.AppendLine($"\t\tbool spResult = postgreSQLHandler.ExecuteAsScalar<bool>(\"{SettingHelper.tableName.ToLower()}_softdelete\", paramList);");
                }
                classBuilder.AppendLine("\t}\r\n\tcatch (Exception ex)\r\n\t{\r\n\t\tthrow ex;\r\n\t}\r\n}");
                classBuilder.AppendLine("---------------------------------- End Soft Delete Record Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateHardDeleteRecordCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                string deletedId = SettingHelper.primaryKeyOfTable != "" ? SettingHelper.primaryKeyOfTable.ToString() : "Id";
                string NpgsqlDbType = !string.IsNullOrEmpty(SettingHelper.primaryKeyDataType) && SettingHelper.primaryKeyDataType != "" ? dataTypeNpgsqlDbType(SettingHelper.primaryKeyDataType.ToString()) : "Interger";
                string tableNameAsVariable = ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName);
                classBuilder.AppendLine("---------------------------------- Start Hard Delete Record Code ----------------------------------");
                classBuilder.AppendLine($"public static void {SettingHelper.tableName}HardDelete ({SettingHelper.tableName} {tableNameAsVariable})");
                classBuilder.AppendLine("{\r\n\ttry\r\n\t{");
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<SqlParameter> paramList = new List<SqlParameter>();");
                    classBuilder.AppendLine($"\t\tparamList.Add(new SqlParameter(@\"{deletedId}\", {tableNameAsVariable}.{deletedId}));");
                    classBuilder.AppendLine($"\t\tSqlHelper.ExecuteNonQuery(ConnString, CommandType.StoredProcedure, \"{SettingHelper.tableName}_Hard_Delete\", paramList.ToArray());");
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    classBuilder.AppendLine("\t\tList<NpgsqlParameter> paramList = new List<NpgsqlParameter>();");
                    classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"{deletedId.ToLower()}\",  NpgsqlTypes.NpgsqlDbType.{NpgsqlDbType}) {{Value = {tableNameAsVariable}.{deletedId}}});");
                    classBuilder.AppendLine("\t\tPostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();");
                    classBuilder.AppendLine($"\t\tbool spResult = postgreSQLHandler.ExecuteAsScalar<bool>(\"{SettingHelper.tableName.ToLower()}_harddelete\", paramList);");
                }
                classBuilder.AppendLine("\t}\r\n\tcatch (Exception ex)\r\n\t{\r\n\t\tthrow ex;\r\n\t}\r\n}");
                classBuilder.AppendLine("---------------------------------- End Hard Delete Record Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetPaginationModelCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                classBuilder.AppendLine("---------------------------------- Start Pagination Model Code ----------------------------------");
                classBuilder.AppendLine("public class PaginationModel\n{\r\n\tpublic object List{ get; set; }\r\n\tpublic int PageSize{ get; set; }\r\n\tpublic int PageIndex { get; set; }\r\n\tpublic int TotalRows { get; set; }\r\n\tpublic int TotalPages { get; set; }\r\n\tpublic string SearchText { get; set; }\r\n\tpublic string SortField { get; set; }\r\n\tpublic int SortType { get; set; }\r\n}");
                classBuilder.AppendLine("---------------------------------- End Pagination Model Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetPaginationDataListModelCode(string tableName)
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                classBuilder.AppendLine("---------------------------------- Start Data List Model Code ----------------------------------");
                classBuilder.AppendLine($"public class DataListModel : {tableName}" + "\r\n{\r\n\tpublic long ROWNUM { get; set; }\r\n\tpublic long TotalCount { get; set; }\r\n}");
                classBuilder.AppendLine("---------------------------------- End Data List Model Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetDataTablePagintaionCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                classBuilder.AppendLine("---------------------------------- Start Pagination Model Code ----------------------------------");
                classBuilder.AppendLine("public class DataTablePagintaion \r\n{ \r\n\tpublic DataTablePagintaion() \r\n\t{ \r\n\t\tPageSize = 20; \r\n\t\tSortType = 1; \r\n\t} \r\n\tpublic int PageIndex { get; set; }\r\n\tpublic int PageSize { get; set; }\r\n\tpublic string SearchText { get; set; }\r\n\tpublic string SortField { get; set; }\r\n\tpublic int SortType { get; set; } \r\n }");
                classBuilder.AppendLine("---------------------------------- End Pagination Model Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GenerateSelectRecordByIdCode()
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                string SelectId = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? SettingHelper.primaryKeyOfTable.ToString() : "Id";
                string data = SettingHelper.primaryKeyDataType.ToString();
                string NpgsqlDbType = !string.IsNullOrEmpty(SettingHelper.primaryKeyDataType) && SettingHelper.primaryKeyDataType != "" ? dataTypeNpgsqlDbType(SettingHelper.primaryKeyDataType.ToString()) : "Interger";
                classBuilder.AppendLine("---------------------------------- Start Select Record By Id Code ----------------------------------");
                string tableNameAsVariable = ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName);
                if (SettingHelper.ConnectionType == Enum.ConnectionType.MicrosoftSQLServer)
                {
                    string columnTypeforSqlServer = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? GetColumnTypeForMSSql(SettingHelper.TableColumnList.First(x => x.Key == SettingHelper.primaryKeyOfTable).Value.Item1) : "int";
                    classBuilder.AppendLine($"public static {SettingHelper.tableName} {SettingHelper.tableName}SelectById ({columnTypeforSqlServer} {SelectId})");
                    classBuilder.AppendLine("{\r\n\ttry\r\n\t{");
                    classBuilder.AppendLine($"\t\t{SettingHelper.tableName} {ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName)} = null;");
                    classBuilder.AppendLine($"");
                    classBuilder.AppendLine("List<SqlParameter> paramList = new List<SqlParameter>();");
                    classBuilder.AppendLine($"paramList.Add(new SqlParameter(@\"{SelectId}\", {SelectId}));");
                    classBuilder.AppendLine($"DataTable dt = SqlHelper.ExecuteDataTable(ConnString, CommandType.StoredProcedure, \"{SettingHelper.tableName}_Select_By_Id\", paramList.ToArray());");

                    classBuilder.AppendLine("if (dt != null && dt.Rows.Count>0)");
                    classBuilder.AppendLine("{");
                    classBuilder.AppendLine("DataRow dr = dt.Rows[0];");
                    classBuilder.AppendLine($"{ConvertProperCaseStringToCamelCaseString(SettingHelper.tableName)} = new {SettingHelper.tableName}();");

                    for (int i = 0; i < SettingHelper.TableColumnList.Count; i++)
                    {
                        var item = SettingHelper.TableColumnList.ElementAt(i);
                        string value = AssignModelPropertyComingFromDatabase(tableNameAsVariable, item.Key, item.Value.Item1, item.Value.Item2);
                        classBuilder.AppendLine(value);

                    }
                }
                else if (SettingHelper.ConnectionType == Enum.ConnectionType.PostgreSQLServer)
                {
                    string columnTypeforPostgreSql = !string.IsNullOrEmpty(SettingHelper.primaryKeyOfTable) && SettingHelper.primaryKeyOfTable != "" ? GetColumnTypeForPostgreSql(SettingHelper.TableColumnList.First(x => x.Key == SettingHelper.primaryKeyOfTable).Value.Item1) : "int";
                    classBuilder.AppendLine($"public static string {SettingHelper.tableName}SelectById ({columnTypeforPostgreSql} {SelectId})");
                    classBuilder.AppendLine("{\r\n\ttry\r\n\t{");
                    classBuilder.AppendLine("\t\tList<NpgsqlParameter> paramList = new List<NpgsqlParameter>();");
                    classBuilder.AppendLine($"\t\tparamList.Add(new NpgsqlParameter(@\"{SelectId.ToLower()}\",  NpgsqlTypes.NpgsqlDbType.{NpgsqlDbType}) {{Value = {SelectId}}});");
                    classBuilder.AppendLine($"\t\tPostgreSQLHandler postgreSQLHandler = new PostgreSQLHandler();");
                    classBuilder.AppendLine($"\t\tstring Data = postgreSQLHandler.Select(\"{SettingHelper.tableName.ToLower()}_select_by_id\", paramList);");
                }
                classBuilder.AppendLine($"\t\treturn Data;");
                classBuilder.AppendLine("\t}\r\n\tcatch (Exception ex)\r\n\t{\r\n\t\tthrow ex;\r\n\t}\r\n}");
                classBuilder.AppendLine("---------------------------------- End Select Record By Id Code ----------------------------------");
                classBuilder.AppendLine();
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string ConvertProperCaseStringToCamelCaseString(string value)
        {
            try
            {
                return char.ToLower(value[0]) + value.Substring(1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetParameterForStoreProcedureForSQL(string columnName, string type)
        {
            try
            {
                switch (type)
                {
                    case "int":
                    case "bigint":
                        return $"@{columnName} {type} = 0";
                    case "varchar":
                    case "nvarchar":
                    case "text":
                    case "ntext":
                        return $"@{columnName} {type} (200)= ''";
                    case "datetime":
                    case "smalldatetime":
                    case "date":
                    case "time":
                        return $"@{columnName} {type} = null";
                    default:
                        throw new Exception("New Type Found For SQL Parameter");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string AssignModelPropertyComingFromDatabase(string tableVariableName, string columnName, string type, bool isNullable)
        {
            try
            {
                StringBuilder classBuilder = new StringBuilder();
                switch (type)
                {
                    case "int":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = Convert.ToInt32(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = Convert.ToInt32(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "bigint":

                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = Convert.ToInt64(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = Convert.ToInt64(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "smallint":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = Convert.ToInt16(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = Convert.ToInt16(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "tinyint":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = Convert.ToByte(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = Convert.ToByte(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "bit":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = Convert.ToBoolean(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = Convert.ToBoolean(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "boolean":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = Convert.ToBoolean(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = Convert.ToBoolean(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "bool":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = Convert.ToBoolean(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = Convert.ToBoolean(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "float":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = Convert.ToDouble(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = Convert.ToDouble(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "real":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = (float) Convert.ToDouble(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = (float) Convert.ToDouble(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "money":
                    case "smallmoney":
                    case "decimal":
                    case "numeric":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = (float) Convert.ToDecimal(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = (float) Convert.ToDecimal(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "varchar":
                    case "character varying":
                    case "nvarchar":
                    case "text":
                    case "ntext":
                    case "char":
                    case "nchar":
                        classBuilder.AppendLine($"{tableVariableName}.{columnName} = dr[\"{columnName}\"].ToString().Trim();");
                        break;
                    case "datetime":
                    case "smalldatetime":
                    case "date":
                    case "time":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = (float) Convert.ToDateTime(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = (float) Convert.ToDateTime(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "timestamp without time zone":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = (float) Convert.ToDateTime(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = (float) Convert.ToDateTime(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "timestamp with time zone":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = (float) Convert.ToDateTime(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = (float) Convert.ToDateTime(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "timestamptz":
                        if (isNullable)
                        {
                            classBuilder.AppendLine($"if (string.IsNullOrEmpty(dr[\"{columnName}\"].ToString().Trim()))\r\n{tableVariableName}.{columnName} = null;");
                            classBuilder.AppendLine($"else \r\n {tableVariableName}.{columnName} = (float) Convert.ToDateTime(dr[\"{columnName}\"].ToString());");
                        }
                        else
                        {
                            classBuilder.AppendLine($"{tableVariableName}.{columnName} = (float) Convert.ToDateTime(dr[\"{columnName}\"].ToString());");
                        }
                        break;
                    case "binary":
                    case "varbinary":
                    case "image":
                        throw new Exception($"Not Implemented data type: {type}");
                    default:
                        throw new Exception($"Unsupported data type: {type}");
                }
                return classBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool IsColumnString(string type)
        {
            try
            {
                switch (type)
                {
                    case "varchar":
                    case "character varying":
                    case "nvarchar":
                    case "text":
                    case "ntext":
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private bool IsColumnDate(string type)
        {
            try
            {
                switch (type)
                {
                    case "datetime":
                    case "smalldatetime":
                    case "date":
                    case "timestamp with time zone":
                    case "timestamp without time zone":
                    case "time":
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetColumnTypeForMSSql(string type)
        {
            try
            {
                switch (type)
                {
                    case "int":
                        return "int";
                    case "bigint":
                        return "long";
                    case "smallint":
                        return "short";
                    case "tinyint":
                        return "byte";
                    case "bit":
                        return "bool";
                    case "float":
                        return "double";
                    case "real":
                        return "float";
                    case "money":
                    case "smallmoney":
                    case "decimal":
                    case "numeric":
                        return "decimal";
                    case "varchar":
                    case "nvarchar":
                    case "text":
                    case "ntext":
                    case "char":
                    case "nchar":
                        return "string";
                    case "datetime":
                    case "smalldatetime":
                    case "date":
                    case "time":
                        return "DateTime";
                    case "timestamp":
                        return "DateTime";
                    case "binary":
                    case "varbinary":
                    case "image":
                        return "byte[]";
                    case "uniqueidentifier":
                        return "Guid";

                    default:
                        throw new Exception($"Unsupported data type: {type}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetColumnTypeForPostgreSql(string type)
        {
            try
            {
                switch (type)
                {
                    case "integer":
                    case "int":
                    case "int4":
                    case "serial":
                    case "serial4":
                        return "int";
                    case "smallint":
                    case "int2":
                    case "smallserial":
                    case "serial2":
                        return "short";
                    case "bigint":
                    case "bigserial":
                        return "long";
                    case "numeric":
                    case "money":
                    case "decimal":
                        return "decimal";
                    case "real":
                    case "float4":
                        return "float";
                    case "bit":
                    case "boolean":
                    case "bool":
                        return "bool";

                    case "bytea":
                        return "byte[]";
                    case "character":
                    case "char":
                        return "char";
                    case "character varying":
                    case "varchar":
                    case "text":
                    case "json":
                    case "jsonb":
                        return "string";
                    case "time":
                    case "interval":
                    case "time without time zone":
                    case "timetz":
                        return "TimeSpan";
                    case "date":
                    case "timestamp":
                    case "timestamp without time zone":
                        return "DateTime";
                    case "timestamp with time zone":
                    case "timestamptz":
                        return "DateTimeOffset";
                    case "uuid":
                        return "Guid";
                    case "double precision":
                    case "float8":
                        return "double";

                    default:
                        throw new Exception($"Unsupported data type: {type}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string dataTypeNpgsqlDbType(string dataType)
        {
            try
            {
                var dataTypeNpgsqlDbType = char.ToUpper(dataType[0]) + dataType.Substring(1);
                if (dataTypeNpgsqlDbType == "Timestamp without time zone")
                {
                    dataTypeNpgsqlDbType = "Timestamp";
                }
                else if (dataTypeNpgsqlDbType == "Timestamp with time zone")
                {
                    dataTypeNpgsqlDbType = "TimestampTz";
                }
                else if (dataTypeNpgsqlDbType == "Character varying")
                {
                    dataTypeNpgsqlDbType = "Varchar";
                }
                else if (dataTypeNpgsqlDbType == "Timestamp with time zone")
                {
                    dataTypeNpgsqlDbType = "TimestampTz";
                }
                return dataTypeNpgsqlDbType;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private string GetColumnTypeForSP(string type)
        {
            try
            {
                switch (type)
                {
                    case "uuid":
                        return type;
                    default:
                        return type;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
