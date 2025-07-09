using AutoCode.Presentation.Helpers;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCode.Presentation.DatabaseUtils
{
    public class PostgreSQLHandler
    {
        public PostgreSQLHandler() { ConnectionString = SettingHelper.PostgreSqlConnectionStringBuilder.ConnectionString; }



        public string ConnectionString { get; set; }

        /// <summary>
        /// Execute as DataReader.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <returns>Object of SqlDataReader.</returns>
        public NpgsqlDataReader ExecuteAsDataReader(string storedProcedureName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                conn.Open();
                NpgsqlDataReader SQLReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return SQLReader;
            }
        }
        /// <summary>
        /// Execute as DataReader.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name. </param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <returns>Object of SqlDataReader.</returns>
        public NpgsqlDataReader ExecuteAsDataReader(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                conn.Open();
                NpgsqlDataReader SQLReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return SQLReader;
            }
        }





        /// <summary>
        /// Execute as DataReader.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <returns>Object of SqlDataReader.</returns>
        public NpgsqlDataReader ExecuteAsDataReader(string storedProcedureName, List<KeyValuePair<string, string>> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    command.Parameters.Add(new NpgsqlParameter(parameterCollection[i].Key, parameterCollection[i].Value));
                }


                conn.Open();
                NpgsqlDataReader SQLReader = command.ExecuteReader(CommandBehavior.CloseConnection);
                return SQLReader;
            }
        }

        public DataSet ExecuteRefCursor(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();
                NpgsqlTransaction transaction = conn.BeginTransaction();
                DataSet dataSet = new DataSet();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;


                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                NpgsqlDataReader dr = command.ExecuteReader();
                List<string> listResult = new List<string>();
                while (dr.Read())
                {
                    listResult.Add(string.Format("FETCH ALL IN \"{0}\"", dr.GetString(0)));
                }
                dr.Close();

                for (var i = 0; i < listResult.Count; i++)
                {
                    using (NpgsqlCommand cmd = new NpgsqlCommand(listResult[i], conn))
                    {
                        cmd.Transaction = command.Transaction;
                        cmd.CommandType = CommandType.Text;
                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            // Fill the DataSet using default values for DataTable names, etc
                            if (i == 0)
                                da.Fill(dataSet);
                            else
                                da.Fill(dataSet.Tables[i]);
                        }
                        if (i != listResult.Count - 1)
                            dataSet.Tables.Add(new DataTable());
                    }
                }
                transaction.Commit();

                conn.Close();

                return dataSet;
            }
        }

        /// <summary>
        /// Execute as DataSet.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <returns>Object of DataSet.</returns>
        public DataSet ExecuteAsDataSet(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;


                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                adapter.SelectCommand = command;

                DataSet dataSet = new DataSet();

                conn.Open();
                adapter.Fill(dataSet);

                return dataSet;
            }
        }

        public DataSet ExecuteAsDataSet(string query)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = query;
                command.CommandType = CommandType.Text;


                
                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                adapter.SelectCommand = command;

                DataSet dataSet = new DataSet();

                conn.Open();
                adapter.Fill(dataSet);

                return dataSet;
            }
        }




        /// <summary>
        /// Execute as DataSet.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <returns>Object of DataSet.</returns>
        public DataSet ExecuteAsDataSet(string storedProcedureName, List<KeyValuePair<string, string>> parameterCollection)
        {

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    command.Parameters.Add(new NpgsqlParameter(parameterCollection[i].Key, parameterCollection[i].Value));
                }

                NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();
                adapter.SelectCommand = command;

                DataSet dataSet = new DataSet();

                conn.Open();
                adapter.Fill(dataSet);

                return dataSet;
            }
        }




    

        /// <summary>
        /// Execute As List.
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="sqlQuery">Sql Query.</param>
        /// <returns>Type of list of object implementing.</returns>
        public List<T> ExecuteAsListUsingQuery<T>(string sqlQuery)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = sqlQuery;
                command.CommandType = CommandType.Text;

                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                List<T> mList = new List<T>();
                mList = DataSourceHelper.FillCollection<T>(reader);
                if (reader != null)
                {
                    reader.Close();
                }
                reader.Close();
                return mList;
            }
        }



        /// <summary>
        /// Execute As List.
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="storedProcedureName">Storedprocedure name.</param>
        /// <returns>Type of list of object implementing.</returns>
        public List<T> ExecuteAsList<T>(string storedProcedureName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                List<T> mList = new List<T>();
                mList = DataSourceHelper.FillCollection<T>(reader);
                if (reader != null)
                {
                    reader.Close();
                }
                reader.Close();
                return mList;
            }
        }






        /// <summary>
        /// Execute as list.
        /// </summary>
        /// <typeparam name="T">Given type of object</typeparam>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection">Accept Key Value collection for parameters.</param>
        /// <returns>Type of list of object implementing.</returns>
        public List<T> ExecuteAsList<T>(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                List<T> mList = new List<T>();
                mList = DataSourceHelper.FillCollection<T>(reader);
                if (reader != null)
                {
                    reader.Close();
                }
                reader.Close();
                return mList;
            }
        }
        public List<T> ExecuteAsList<T>(string storedProcedureName, List<NpgsqlParameter> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                foreach (NpgsqlParameter param in parameterCollection)
                {
                    param.IsNullable = true;
                    command.Parameters.Add(param);
                }

                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                List<T> mList = new List<T>();
                mList = DataSourceHelper.FillCollection<T>(reader);
                if (reader != null)
                {
                    reader.Close();
                }
                reader.Close();
                return mList;
            }
        }
        /// <summary>
        /// Execute As Object
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="sqlQuery">SQL Query</param>
        /// <returns>Type of list of object implementing.</returns>
        public T ExecuteAsObjectUsingQuery<T>(string sqlQuery)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = sqlQuery;
                command.CommandType = CommandType.Text;

                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                ArrayList arrColl = DataSourceHelper.FillCollection(reader, typeof(T));
                conn.Close();

                if (reader != null)
                    reader.Close();

                if (arrColl != null && arrColl.Count > 0)
                    return (T)arrColl[0];
                else
                    return default(T);
            }
        }



        /// <summary>
        /// Execute As Object
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="storedProcedureName">Accept Key Value Collection For Parameters</param>
        /// <returns> Type of the object implementing</returns>
        public T ExecuteAsObject<T>(string storedProcedureName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                ArrayList arrColl = DataSourceHelper.FillCollection(reader, typeof(T));
                conn.Close();

                if (reader != null)
                    reader.Close();

                if (arrColl != null && arrColl.Count > 0)
                    return (T)arrColl[0];
                else
                    return default(T);
            }
        }

        //public string Select(string spName, List<NpgsqlParameter> parameters)
        //{
        //    try
        //    {
        //        DataSet ds = new DataSet();
        //        using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
        //        {
        //            conn.Open();
        //            if (conn == null)
        //                throw new ArgumentNullException("connection");

        //            string functionCommand = "Select * from " + spName + "(" + string.Join(", ", parameters.Select(p => "@" + p.ParameterName)) + ")";
        //            using (NpgsqlCommand command = new NpgsqlCommand(functionCommand, conn))
        //            {
        //                command.CommandType = CommandType.Text;
        //                foreach (var param in parameters)
        //                    command.Parameters.AddWithValue(param.ParameterName, param.Value);

        //                NpgsqlDataAdapter da = new NpgsqlDataAdapter(command);
        //                da.Fill(ds);
        //            }
        //            string JSONString = JsonConvert.SerializeObject(ds, Formatting.Indented);
        //            return JSONString;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        UpdateSPExecutionCount(spName);
        //    }
        //}

        /// <summary>
        /// Execute as object.
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="paraMeterCollection">Accept key value collection for parameters.</param>
        /// <returns></returns>
        public T ExecuteAsObject<T>(string storedProcedureName, List<KeyValuePair<string, object>> paraMeterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < paraMeterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = paraMeterCollection[i].Key;
                    sqlParaMeter.Value = paraMeterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                ArrayList arrColl = DataSourceHelper.FillCollection(reader, typeof(T));
                conn.Close();

                if (reader != null)
                    reader.Close();

                if (arrColl != null && arrColl.Count > 0)
                    return (T)arrColl[0];
                else
                    return default(T);
            }


        }
        public T ExecuteAsObject<T>(string storedProcedureName, List<NpgsqlParameter> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                foreach (NpgsqlParameter param in parameterCollection)
                {
                    param.IsNullable = true;
                    command.Parameters.Add(param);
                }

                conn.Open();
                NpgsqlDataReader reader = command.ExecuteReader();
                ArrayList arrColl = DataSourceHelper.FillCollection(reader, typeof(T));
                conn.Close();

                if (reader != null)
                    reader.Close();

                if (arrColl != null && arrColl.Count > 0)
                    return (T)arrColl[0];
                else
                    return default(T);
            }
        }

        /// <summary>
        /// Execute As scalar.
        /// </summary>
        /// <typeparam name="T"> Given type of object.</typeparam>
        /// <param name="stroredProcedureName">Store procedure name</param>
        /// <returns>Type of the object implementing</returns>
        public T ExecuteAsScalar<T>(string stroredProcedureName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = stroredProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                conn.Open();
                return (T)command.ExecuteScalar();
            }
        }



        /// <summary>
        /// Execute As scalar .
        /// </summary>
        /// <typeparam name="T"> Given type of object.</typeparam>
        /// <param name="stroredProcedureName">Store procedure name</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <returns>Type of the object implementing</returns>
        public T ExecuteAsScalar<T>(string stroredProcedureName, List<KeyValuePair<string, object>> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = stroredProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }
                conn.Open();
                return (T)command.ExecuteScalar();
            }
        }
        public T ExecuteAsScalar<T>(string stroredProcedureName, List<NpgsqlParameter> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = stroredProcedureName;
                command.CommandType = CommandType.StoredProcedure;
                foreach (NpgsqlParameter param in parameterCollection)
                {
                    param.IsNullable = true;
                    command.Parameters.Add(param);
                }
                conn.Open();
                return (T)command.ExecuteScalar();
            }
        }


        /// <summary>
        /// Executes non query.
        /// </summary>
        /// <param name="sqlScript">SQL Script</param>
        /// <param name="timeout">Timeout </param>
        public void ExecuteNonQueryUsingSQL(string sqlScript, int timeout = 0)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = sqlScript;
                command.CommandType = CommandType.Text;

                conn.Open();
                command.CommandTimeout = timeout;
                command.ExecuteNonQuery();
            }
        }



        /// <summary>
        /// Executes non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        public void ExecuteNonQuery(string storedProcedureName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                conn.Open();
                command.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Execute non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="outputParameterName">Accept Output for parameters name.</param>
        /// <returns>Integer value.</returns>
        public int ExecuteNonQuery(string storedProcedureName, string outputParameterName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
                conn.Open();
                command.ExecuteNonQuery();
                int returnValue = (int)command.Parameters[outputParameterName].Value;
                return returnValue;
            }
        }


        /// <summary>
        /// Execute non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="outputParameterName">Accept output for parameter name.</param>
        /// <param name="outputParameterValue">Accept output for parameter value.</param>
        /// <returns>Integer value.</returns>
        public int ExecuteNonQuery(string storedProcedureName, string outputParameterName, object outputParameterValue)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
                command.Parameters[outputParameterName].Value = outputParameterValue;
                conn.Open();
                command.ExecuteNonQuery();
                int returnValue = (int)command.Parameters[outputParameterName].Value;
                return returnValue;
            }
        }


        /// <summary>
        /// Executes non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters. <KeyValuePair<string, string>> </param>
        public void ExecuteNonQuery(string storedProcedureName, List<KeyValuePair<string, string>> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    command.Parameters.Add(new NpgsqlParameter(parameterCollection[i].Key, parameterCollection[i].Value));
                }

                conn.Open();
                command.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Executes non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters. <KeyValuePair<string, string>> </param>
        public void ExecuteNonQuery(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                conn.Open();
                command.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Returning bool after execute non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection">Parameter collection.</param>
        /// <param name="outputParameterName">OutPut parameter name.</param>
        /// <param name="outputParameterValue">OutPut parameter value.</param>
        /// <returns>Bool</returns>
        public bool ExecuteNonQueryAsBool(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection, string outputParameterName, object outputParameterValue)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {

                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Bit));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
                command.Parameters[outputParameterName].Value = outputParameterValue;

                conn.Open();
                command.ExecuteNonQuery();
                bool returnValue = (bool)command.Parameters[outputParameterName].Value;
                return returnValue;
            }
        }


        /// <summary>
        /// Execute non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name in string.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <param name="outputParameterName">Accept output key value collection for parameters.</param>
        /// <returns>Integer value.</returns>
        public int ExecuteNonQuery(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection, string outputParameterName)
        {

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;

                conn.Open();
                command.ExecuteNonQuery();
                int returnValue = (int)command.Parameters[outputParameterName].Value;
                return returnValue;
            }
        }


        /// <summary>
        /// Execute non query.
        /// </summary>
        /// <param name="storedProcedureName"> Store procedure name.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <param name="outputParameterName">Accept output key value collection for parameters.</param>
        /// <returns>Integer value.</returns>
        public int ExecuteNonQuery(string storedProcedureName, List<KeyValuePair<string, string>> parameterCollection, string outputParameterName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    command.Parameters.Add(new NpgsqlParameter(parameterCollection[i].Key, parameterCollection[i].Value));
                }

                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;

                conn.Open();
                command.ExecuteNonQuery();
                int returnValue = (int)command.Parameters[outputParameterName].Value;
                return returnValue;
            }
        }


        /// <summary>
        /// Execute non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <param name="outputParameterName">Accept output  for parameters name.</param>
        /// <param name="outputParameterValue">Accept output for parameters value.</param>
        /// <returns>Integer value.</returns>
        public int ExecuteNonQuery(string storedProcedureName, List<KeyValuePair<string, string>> parameterCollection, string outputParameterName, object outputParameterValue)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    command.Parameters.Add(new NpgsqlParameter(parameterCollection[i].Key, parameterCollection[i].Value));
                }

                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
                command.Parameters[outputParameterName].Value = outputParameterValue;

                conn.Open();
                command.ExecuteNonQuery();
                int returnValue = (int)command.Parameters[outputParameterName].Value;
                return returnValue;
            }
        }



        /// <summary>
        /// Returning bool after execute non query.
        /// </summary>
        /// <param name="storedProcedureName">Store procedure name.</param>
        /// <param name="parameterCollection"> Parameter collection.</param>
        /// <param name="outputParameterName">Out parameter collection.</param>
        /// <returns>Bool</returns>
        public bool ExecuteNonQueryAsBool(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection, string outputParameterName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Bit));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;

                conn.Open();
                command.ExecuteNonQuery();
                bool returnValue = (bool)command.Parameters[outputParameterName].Value;
                return returnValue;
            }
        }


        /// <summary>
        /// Accept only int, Int16, long, DateTime, string (NVarcha of size  50),
        /// bool, decimal ( of size 16,2), float
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="storedProcedureName">Accet SQL procedure name in string.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <param name="outputParameterName">Accept output parameter for the stored procedures.</param>
        /// <returns>Type of the object implementing.</returns>
        public T ExecuteNonQueryAsGivenType<T>(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection, string outputParameterName)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                command = DataSourceHelper.AddOutPutParametrofGivenType<T>(command, outputParameterName);
                conn.Open();
                command.ExecuteNonQuery();
                return (T)command.Parameters[outputParameterName].Value;
            }
        }


        /// <summary>
        /// Accept only int, Int16, long, DateTime, string (NVarcha of size  50),
        /// bool, decimal ( of size 16,2), float
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="storedProcedureName">Accept SQL procedure name in string.</param>
        /// <param name="parameterCollection">Accept key value collection for parameters.</param>
        /// <param name="outputParameterName">Accept output parameter for the stored procedures.</param>
        /// <param name="outputParameterValue">OutPut parameter value.</param>
        /// <returns>Type of the object implementing.</returns>
        public T ExecuteNonQueryAsGivenType<T>(string storedProcedureName, List<KeyValuePair<string, object>> parameterCollection, string outputParameterName, object outputParameterValue)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                for (int i = 0; i < parameterCollection.Count; i++)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = parameterCollection[i].Key;
                    sqlParaMeter.Value = parameterCollection[i].Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                command = DataSourceHelper.AddOutPutParametrofGivenType<T>(command, outputParameterName, outputParameterValue);
                conn.Open();
                command.ExecuteNonQuery();
                return (T)command.Parameters[outputParameterName].Value; ;
            }
        }




        /// <summary>
        /// Executes non query with multipal output.
        /// </summary>
        /// <param name="storedProcedureName">Strored procedure name.</param>
        /// <param name="inputParamColl">Accept Key Value collection for parameters.</param>
        /// <param name="outputParamColl">Output Key Value collection for parameters.</param>
        /// <returns>List Key Value collection</returns>
        public List<KeyValuePair<string, object>> ExecuteNonQueryWithMultipleOutputStringObject(string storedProcedureName, List<KeyValuePair<string, object>> inputParamColl, List<KeyValuePair<string, object>> outputParamColl)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                foreach (KeyValuePair<string, object> kvp in inputParamColl)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = kvp.Key;
                    sqlParaMeter.Value = kvp.Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                foreach (KeyValuePair<string, object> kvp in outputParamColl)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = kvp.Key;
                    sqlParaMeter.Value = kvp.Value;
                    sqlParaMeter.Direction = ParameterDirection.InputOutput;
                    sqlParaMeter.Size = 256;
                    command.Parameters.Add(sqlParaMeter);
                }

                conn.Open();
                command.ExecuteNonQuery();
                List<KeyValuePair<string, object>> lstRetValues = new List<KeyValuePair<string, object>>();
                for (int i = 0; i < outputParamColl.Count; i++)
                {
                    lstRetValues.Add(new KeyValuePair<string, object>(i.ToString(), command.Parameters[inputParamColl.Count + i].Value.ToString()));
                }
                return lstRetValues;
            }
        }




        /// <summary>
        /// Executes non query with multipal output.
        /// </summary>
        /// <param name="transaction"> Transact-SQL transaction </param>
        /// <param name="commandType">Command type</param>
        /// <param name="storedProcedureName">Strored procedure name.</param>
        /// <param name="inputParamColl">Accept Key Value collection for parameters.</param>
        /// <param name="outputParamColl">Output Key Value collection for parameters.</param>
        /// <returns>List Key Value collection</returns>
        public List<KeyValuePair<int, string>> ExecuteNonQueryWithMultipleOutput(NpgsqlTransaction transaction, CommandType commandType, string storedProcedureName, List<KeyValuePair<string, object>> inputParamColl, List<KeyValuePair<string, object>> outputParamColl)
        {

            //create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, storedProcedureName);

            foreach (KeyValuePair<string, object> kvp in inputParamColl)
            {
                NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                sqlParaMeter.IsNullable = true;
                sqlParaMeter.ParameterName = kvp.Key;
                sqlParaMeter.Value = kvp.Value;
                cmd.Parameters.Add(sqlParaMeter);
            }

            foreach (KeyValuePair<string, object> kvp in outputParamColl)
            {
                NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                sqlParaMeter.IsNullable = true;
                sqlParaMeter.ParameterName = kvp.Key;
                sqlParaMeter.Value = kvp.Value;
                sqlParaMeter.Direction = ParameterDirection.InputOutput;
                sqlParaMeter.Size = 256;
                cmd.Parameters.Add(sqlParaMeter);
            }

            cmd.ExecuteNonQuery();
            List<KeyValuePair<int, string>> lstRetValues = new List<KeyValuePair<int, string>>();
            for (int i = 0; i < outputParamColl.Count; i++)
            {
                lstRetValues.Add(new KeyValuePair<int, string>(i, cmd.Parameters[inputParamColl.Count + i].Value.ToString()));
            }
            return lstRetValues;
        }




        /// <summary>
        /// Executes non query with multipal output.
        /// </summary>
        /// <param name="storedProcedureName">Strored procedure name.</param>
        /// <param name="inputParamColl">Accept Key Value collection for parameters.</param>
        /// <param name="outputParamColl">Output Key Value collection for parameters.</param>
        /// <returns>List Key Value collection</returns>
        public List<KeyValuePair<int, string>> ExecuteNonQueryWithMultipleOutput(string storedProcedureName, List<KeyValuePair<string, object>> inputParamColl, List<KeyValuePair<string, object>> outputParamColl)
        {

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = conn;
                command.CommandText = storedProcedureName;
                command.CommandType = CommandType.StoredProcedure;

                foreach (KeyValuePair<string, object> kvp in inputParamColl)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = kvp.Key;
                    sqlParaMeter.Value = kvp.Value;
                    command.Parameters.Add(sqlParaMeter);
                }

                foreach (KeyValuePair<string, object> kvp in outputParamColl)
                {
                    NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                    sqlParaMeter.IsNullable = true;
                    sqlParaMeter.ParameterName = kvp.Key;
                    sqlParaMeter.Value = kvp.Value;
                    sqlParaMeter.Direction = ParameterDirection.InputOutput;
                    sqlParaMeter.Size = 256;
                    command.Parameters.Add(sqlParaMeter);
                }


                conn.Open();
                command.ExecuteNonQuery();
                List<KeyValuePair<int, string>> lstRetValues = new List<KeyValuePair<int, string>>();
                for (int i = 0; i < outputParamColl.Count; i++)
                {
                    lstRetValues.Add(new KeyValuePair<int, string>(i, command.Parameters[inputParamColl.Count + i].Value.ToString()));
                }
                return lstRetValues;
            }
        }







        /// <summary>
        /// Executes non query
        /// </summary>
        /// <param name="transaction">Transact-SQL transaction</param>
        /// <param name="commandType">Command type</param>
        /// <param name="commandText">Command text.</param>
        /// <param name="parameterCollection">Output Key Value collection for parameters.</param>
        public void ExecuteNonQuery(NpgsqlTransaction transaction, CommandType commandType, string commandText, List<KeyValuePair<string, object>> parameterCollection)
        {

            //create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText);

            for (int i = 0; i < parameterCollection.Count; i++)
            {
                NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                sqlParaMeter.IsNullable = true;
                sqlParaMeter.ParameterName = parameterCollection[i].Key;
                sqlParaMeter.Value = parameterCollection[i].Value;
                cmd.Parameters.Add(sqlParaMeter);
            }

            cmd.ExecuteNonQuery();

            cmd.Parameters.Clear();
        }



        /// <summary>
        ///  Executes non query
        /// </summary>
        /// <param name="transaction">Transact-SQL transaction</param>
        /// <param name="commandType">Command type</param>
        /// <param name="commandText">Command text.</param>
        /// <param name="parameterCollection">Accept Key Value collection for parameters.</param>
        /// <param name="outParamName">Output parameter.</param>
        /// <returns>ID</returns>
        public int ExecuteNonQuery(NpgsqlTransaction transaction, CommandType commandType, string commandText, List<KeyValuePair<string, object>> parameterCollection, string outParamName)
        {
            //create a command and prepare it for execution
            NpgsqlCommand cmd = new NpgsqlCommand();
            PrepareCommand(cmd, transaction.Connection, transaction, commandType, commandText);

            for (int i = 0; i < parameterCollection.Count; i++)
            {
                NpgsqlParameter sqlParaMeter = new NpgsqlParameter();
                sqlParaMeter.IsNullable = true;
                sqlParaMeter.ParameterName = parameterCollection[i].Key;
                sqlParaMeter.Value = parameterCollection[i].Value;
                cmd.Parameters.Add(sqlParaMeter);
            }
            cmd.Parameters.Add(new NpgsqlParameter(outParamName, SqlDbType.Int));
            cmd.Parameters[outParamName].Direction = ParameterDirection.Output;

            cmd.ExecuteNonQuery();
            int id = (int)cmd.Parameters[outParamName].Value;

            // detach the Parameters from the command object, so they can be used again.
            cmd.Parameters.Clear();
            return id;
        }

        public DataTable ExecuteSQL(string sqlQuery)
        {
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                conn.Open();

                DataSet dataSet = new DataSet();

                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sqlQuery, conn))
                {
                    dataSet.Reset();
                    da.Fill(dataSet);
                    conn.Close();
                }

                DataTable table = null;
                if (dataSet != null && dataSet.Tables != null && dataSet.Tables[0] != null)
                {
                    table = dataSet.Tables[0];
                }
                return table;
            }
        }

        public bool CheckDatabaseConnectoin()
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
                {
                    conn.Open();
                    conn.Close();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Return  base class for a transaction.
        /// </summary>
        /// <returns>Object of SqlTransaction</returns>
        public NpgsqlTransaction GetTransaction()
        {
            NpgsqlConnection Conn = new NpgsqlConnection(ConnectionString);
            Conn.Open();
            NpgsqlTransaction transaction = Conn.BeginTransaction();
            return transaction;
        }


        /// <summary>
        ///Roll back SQL transaction.
        /// </summary>
        /// <param name="transaction">transaction</param>
        public void RollbackTransaction(NpgsqlTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            finally
            {
                if (transaction != null && transaction.Connection != null)
                {
                    transaction.Connection.Close();
                }
            }
        }


        /// <summary>
        /// Commit transaction on database.
        /// </summary>
        /// <param name="transaction"></param>
        public void CommitTransaction(NpgsqlTransaction transaction)
        {
            try
            {
                transaction.Commit();
            }
            finally
            {
                if (transaction != null && transaction.Connection != null)
                {
                    transaction.Connection.Close();
                }
            }
        }


        /// <summary>
        /// Prepare command for execute.
        /// </summary>
        /// <param name="command">Sql Command.</param>
        /// <param name="connection">connection</param>
        /// <param name="transaction">Transact-SQL transaction</param>
        /// <param name="commandType">Command type</param>
        /// <param name="commandText">Command text.</param>
        public void PrepareCommand(NpgsqlCommand command, NpgsqlConnection connection, NpgsqlTransaction transaction, CommandType commandType, string commandText)
        {
            //if the provided connection is not open, we will open it
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            //associate the connection with the command
            command.Connection = connection;
            command.Transaction = transaction;
            command.CommandType = commandType;
            command.CommandText = commandText;
            return;
        }


        /// <summary>
        /// Execute Transaction
        /// </summary>
        /// <param name="trans">Transact-SQL transaction.</param>
        /// <param name="sqlScript">SQL Script</param>
        private void ExecuteTransaction(NpgsqlTransaction trans, string sqlScript)
        {
            NpgsqlConnection connection = trans.Connection;
            NpgsqlCommand command = new NpgsqlCommand(sqlScript, trans.Connection);
            command.Transaction = trans;
            command.CommandTimeout = 0;
            command.ExecuteNonQuery();
        }



    }
}
