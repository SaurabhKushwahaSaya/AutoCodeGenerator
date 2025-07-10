using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCode.Presentation.Helpers
{
    public class DatabaseHelper
    {
        public static string DatabaseConnectionString { get; set; }

        //public string _dbConnectionString;
        //private ConnectionString _connectionString = new ConnectionString();
        private NpgsqlConnection dbConnection = new NpgsqlConnection();
        //public SayaPostgreSQL(string environment)
        //{
        //    if (!string.IsNullOrEmpty(environment) && environment.ToLower().Contains("production"))
        //        _dbConnectionString = _connectionString.LiveConnectionString;
        //    else if (!string.IsNullOrEmpty(environment) && environment.ToLower() == ("testing"))
        //        _dbConnectionString = _connectionString.TestingConnectionString;
        //    else
        //        _dbConnectionString = _connectionString.DevConnectionString;
        //}
        public NpgsqlConnection GetDbConnection()
        {
            return new NpgsqlConnection(DatabaseConnectionString);
        }
        //~SayaPostgreSQL()
        //{
        //    //dbConnection.Close();
        //}
        public string Select(string spName, List<NpgsqlParameter> parameters)
        {
            try
            {
                DataSet ds = new DataSet();
                using (dbConnection = GetDbConnection())
                {
                    dbConnection.Open();
                    if (dbConnection == null)
                        throw new ArgumentNullException("connection");

                    string functionCommand = "Select * from " + spName + "(" + string.Join(", ", parameters.Select(p => "@" + p.ParameterName)) + ")";
                    using (NpgsqlCommand command = new NpgsqlCommand(functionCommand, dbConnection))
                    {
                        command.CommandType = CommandType.Text;
                        foreach (var param in parameters)
                            command.Parameters.AddWithValue(param.ParameterName, param.Value);

                        NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                        da.Fill(ds);
                    }
                    string JSONString = JsonConvert.SerializeObject(ds, Formatting.Indented);
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
        public string SelectAll(string spName)
        {
            try
            {
                DataSet ds = new DataSet();
                using (dbConnection = GetDbConnection())
                {
                    dbConnection.Open();
                    if (dbConnection == null)
                        throw new ArgumentNullException("connection");

                    string functionCommand = "Select * from " + spName + "()";
                    using (NpgsqlCommand command = new NpgsqlCommand(functionCommand, dbConnection))
                    {
                        command.CommandType = CommandType.Text;

                        NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
                        da.Fill(ds);
                    }
                    string JSONString = JsonConvert.SerializeObject(ds, Formatting.Indented);
                    return JSONString;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //finally
            //{
            //    UpdateSPExecutionCount(spName);
            //}
        }
        public string SelectFromCursor(string spName, List<NpgsqlParameter> parameters)
        {
            try
            {
                DataSet ds = new DataSet();
                using (dbConnection = GetDbConnection())
                {
                    dbConnection.Open();
                    if (dbConnection == null)
                        throw new ArgumentNullException("connection");
                    NpgsqlTransaction transaction = dbConnection.BeginTransaction();

                    string functionCommand = "Select * from " + spName + "(" + string.Join(", ", parameters.Select(p => "@" + p.ParameterName)) + ")";
                    using (NpgsqlCommand command = new NpgsqlCommand(functionCommand, dbConnection))
                    {
                        command.CommandType = CommandType.Text;
                        foreach (var param in parameters)
                            command.Parameters.AddWithValue(param.ParameterName, param.Value);
                        NpgsqlDataReader dr = command.ExecuteReader();
                        List<string> listResult = new List<string>();
                        while (dr.Read())
                        {
                            listResult.Add(string.Format("FETCH ALL IN \"{0}\"", dr.GetString(0)));
                        }
                        dr.Close();
                        for (var i = 0; i < listResult.Count; i++)
                        {
                            using (NpgsqlCommand cmd = new NpgsqlCommand(listResult[i], dbConnection))
                            {
                                cmd.Transaction = command.Transaction;
                                cmd.CommandType = CommandType.Text;
                                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                                {
                                    // Fill the DataSet using default values for DataTable names, etc
                                    if (i == 0)
                                        da.Fill(ds);
                                    else
                                        da.Fill(ds.Tables[i]);
                                }
                                if (i != listResult.Count - 1)
                                    ds.Tables.Add(new DataTable());
                            }
                        }
                    }
                    transaction.Commit();
                    string JSONString = JsonConvert.SerializeObject(ds, Formatting.Indented);
                    return JSONString;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
        public T ExecuteScalar<T>(string spName, List<NpgsqlParameter> parameters)
        {
            try
            {
                T result = default(T);
                using (dbConnection = GetDbConnection())
                {
                    dbConnection.Open();
                    if (dbConnection == null)
                        throw new ArgumentNullException("connection");

                    string functionCommand = "Select * from " + spName + "(" + string.Join(", ", parameters.Select(p => "@" + p.ParameterName)) + ")";
                    using (NpgsqlCommand command = new NpgsqlCommand(functionCommand, dbConnection))
                    {
                        command.CommandType = CommandType.Text;
                        foreach (var param in parameters)
                            command.Parameters.AddWithValue(param.ParameterName, param.Value);
                        var checkNull = command.ExecuteScalar();
                        result = (checkNull == null || checkNull == DBNull.Value) ? result : (T)checkNull;
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
        //private void UpdateSPExecutionCount(string spName)
        //{
        //    try
        //    {
        //        using (dbConnection = GetDbConnection())
        //        {
        //            dbConnection.Open();
        //            if (dbConnection == null)
        //                throw new ArgumentNullException("connection");

        //            using (NpgsqlCommand command = new NpgsqlCommand("Select * from spexecutioncount_update(@spname)", dbConnection))
        //            {
        //                command.CommandType = CommandType.Text;
        //                command.Parameters.AddWithValue("spname", spName);
        //                object checkNull = command.ExecuteScalar();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // do not add lambda logs as it create core package version issue in whole solution.
        //    }
        //}

    }
}
