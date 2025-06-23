using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AutoCode.Presentation.Helpers
{
    public class DataSourceHelper
    {

        /// <summary>
        /// Return list of array.
        /// </summary>
        /// <param name="dr">The DataReader</param>
        /// <param name="objType">Type od datatype.</param>
        /// <returns>Object of ArrayList</returns>
        public static ArrayList FillCollection(IDataReader dr, Type objType)
        {

            ArrayList objFillCollection = new ArrayList();
            object objFillObject;

            // get properties for type
            ArrayList objProperties = GetPropertyInfo(objType);

            // get ordinal positions in datareader
            int[] arrOrdinals = GetOrdinals(objProperties, dr);

            // iterate datareader
            while (dr.Read())
            {
                // fill business object
                objFillObject = CreateObject(objType, dr, objProperties, arrOrdinals);
                // add to collection
                objFillCollection.Add(objFillObject);
            }

            // close datareader
            if ((dr != null))
            {
                dr.Close();
            }

            return objFillCollection;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr">The DataReader.</param>
        /// <param name="objType">Type of datatype.</param>
        /// <param name="objToFill">The List.</param>
        /// <returns>Object of list.</returns>
        public static IList FillCollection(IDataReader dr, Type objType, ref IList objToFill)
        {

            object objFillObject;

            // get properties for type
            ArrayList objProperties = GetPropertyInfo(objType);

            // get ordinal positions in datareader
            int[] arrOrdinals = GetOrdinals(objProperties, dr);

            // iterate datareader
            while (dr.Read())
            {
                // fill business object
                objFillObject = CreateObject(objType, dr, objProperties, arrOrdinals);
                // add to collection
                objToFill.Add(objFillObject);
            }

            // close datareader
            if ((dr != null))
            {
                dr.Close();
            }

            return objToFill;

        }




        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillCollection fills a List custom business object of a specified type 
        /// from the supplied DataReader
        /// </summary>
        /// <typeparam name="T">The type of the business object</typeparam>
        /// <param name="dr">The IDataReader to use to fill the object</param>
        /// <returns>A List of custom business objects</returns>
        /// <remarks></remarks>
        public static List<T> FillCollection<T>(IDataReader dr)
        {
            List<T> objFillCollection = new List<T>();
            T objFillObject;

            // iterate datareader
            while (dr.Read())
            {
                // fill business object
                objFillObject = CreateObject<T>(dr);
                // add to collection
                objFillCollection.Add(objFillObject);
            }

            // close datareader
            if ((dr != null))
            {
                dr.Close();
            }

            return objFillCollection;

        }



        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillCollection fills a provided IList with custom business 
        /// objects of a specified type from the supplied DataReader
        /// </summary>
        /// <typeparam name="T">The type of the business object</typeparam>
        /// <param name="dr">The IDataReader to use to fill the object</param>
        /// <param name="objToFill">The IList to fill</param>
        /// <returns>An IList of custom business objects</returns>
        /// <remarks></remarks>
        public static IList<T> FillCollection<T>(IDataReader dr, ref IList<T> objToFill)
        {

            T objFillObject;

            while (dr.Read())
            {
                objFillObject = CreateObject<T>(dr);
                objToFill.Add(objFillObject);
            }

            if ((dr != null))
            {
                dr.Close();
            }

            return objToFill;

        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillCollection fills a List custom business object of a specified type 
        /// from the supplied DataTable
        /// </summary>
        /// <typeparam name="T">The type of the business object</typeparam>
        /// <param name="dt">The DataTable to use to fill the object</param>
        /// <returns>A List of custom business objects</returns>
        /// <remarks></remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public static List<T> FillCollection<T>(DataTable dt)
        {
            List<T> objFillCollection = new List<T>();
            T objFillObject;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                objFillObject = CreateObject<T>(dt.Rows[i]);
                objFillCollection.Add(objFillObject);
            }
            return objFillCollection;
        }


        /// <summary>
        /// Return object.
        /// </summary>
        /// <param name="objObject">object</param>
        /// <param name="objType">Type of datatype.</param>
        /// <returns>object</returns>
        public static object InitializeObject(object objObject, Type objType)
        {
            PropertyInfo objPropertyInfo;
            object objValue;
            int intProperty;

            // get properties for type
            ArrayList objProperties = GetPropertyInfo(objType);

            // initialize properties
            for (intProperty = 0; intProperty <= objProperties.Count - 1; intProperty++)
            {
                objPropertyInfo = (PropertyInfo)objProperties[intProperty];
                if (objPropertyInfo.CanWrite)
                {
                    objValue = null;
                    objPropertyInfo.SetValue(objObject, objValue, null);
                }
            }

            return objObject;

        }




        /// <summary>
        /// Reuturn object based on given parameters.
        /// </summary>
        /// <param name="dr">The DataReader.</param>
        /// <param name="objType">Type od datatype.</param>
        /// <returns>Object</returns>
        public static object FillObject(IDataReader dr, Type objType)
        {
            return FillObject(dr, objType, true);
        }


        public static object FillObject(IDataReader dr, Type objType, bool ManageDataReader)
        {

            object objFillObject;

            // get properties for type
            ArrayList objProperties = GetPropertyInfo(objType);

            // get ordinal positions in datareader
            int[] arrOrdinals = GetOrdinals(objProperties, dr);

            bool Continue;
            if (ManageDataReader)
            {
                Continue = false;
                // read datareader
                if (dr.Read())
                {
                    Continue = true;
                }
            }
            else
            {
                Continue = true;
            }

            if (Continue)
            {
                // create custom business object
                objFillObject = CreateObject(objType, dr, objProperties, arrOrdinals);
            }
            else
            {
                objFillObject = null;
            }

            if (ManageDataReader)
            {
                // close datareader
                if ((dr != null))
                {
                    dr.Close();
                }
            }

            return objFillObject;

        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillObject fills a custom business object of a specified type 
        /// from the supplied DataReader
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="dr">The IDataReader to use to fill the object</param>
        /// <returns>The object</returns>
        /// <remarks>This overloads sets the ManageDataReader parameter to true and calls 
        /// the other overload</remarks>
        /// <history>
        /// 	[cnurse]	10/10/2005	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static T FillObject<T>(IDataReader dr)
        {

            return FillObject<T>(dr, true);

        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of FillObject fills a custom business object of a specified type 
        /// from the supplied DataReader
        /// </summary>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <param name="dr">The IDataReader to use to fill the object</param>
        /// <param name="ManageDataReader">A boolean that determines whether the DatReader
        /// is managed</param>
        /// <returns>The object</returns>
        /// <remarks>This overloads allows the caller to determine whether the ManageDataReader 
        /// parameter is set</remarks>
        /// <history>
        /// 	[cnurse]	10/10/2005	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static T FillObject<T>(IDataReader dr, bool ManageDataReader)
        {

            T objFillObject;

            bool Continue;
            if (ManageDataReader)
            {
                Continue = false;
                // read datareader
                if (dr.Read())
                {
                    Continue = true;
                }
            }
            else
            {
                Continue = true;
            }

            if (Continue)
            {
                // create custom business object
                objFillObject = CreateObject<T>(dr);
            }
            else
            {
                objFillObject = default(T);
            }

            if (ManageDataReader)
            {
                // close datareader
                if ((dr != null))
                {
                    dr.Close();
                }
            }

            return objFillObject;

        }


        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of CreateObject creates an object of a specified type from the 
        /// provided DataRow
        /// </summary>
        /// <typeparam name="T">The type of the business object</typeparam>
        /// <param name="dr">The DataRow</param>
        /// <returns>The custom business object</returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        private static T CreateObject<T>(DataRow dr)
        {

            PropertyInfo objPropertyInfo;
            object objValue;
            Type objPropertyType = null;
            int intProperty;
            T objObject = Activator.CreateInstance<T>();
            ArrayList objProperties = GetPropertyInfo(objObject.GetType());
            for (intProperty = 0; intProperty <= objProperties.Count - 1; intProperty++)
            {
                objPropertyInfo = (PropertyInfo)objProperties[intProperty]; if (objPropertyInfo.CanWrite)
                {
                    objValue = Null.SetNull(objPropertyInfo);
                    try
                    {
                        if (System.Convert.IsDBNull(dr[objPropertyInfo.Name]))
                        {
                            objPropertyInfo.SetValue(objObject, objValue, null);
                        }
                        else
                        {
                            try
                            {
                                objPropertyInfo.SetValue(objObject, dr[objPropertyInfo.Name], null);
                            }
                            catch
                            {
                                try
                                {
                                    objPropertyType = objPropertyInfo.PropertyType;
                                    if (objPropertyType.BaseType.Equals(typeof(System.Enum)))
                                    {
                                        int testint = 0;
                                        if (testint.GetType() == dr[objPropertyInfo.Name].GetType())
                                        {
                                            ((PropertyInfo)objProperties[intProperty]).SetValue(objObject, System.Enum.ToObject(objPropertyType, Convert.ToInt32(dr[objPropertyInfo.Name])), null);
                                        }
                                        else
                                        {
                                            ((PropertyInfo)objProperties[intProperty]).SetValue(objObject, System.Enum.ToObject(objPropertyType, dr[objPropertyInfo.Name]), null);
                                        }
                                    }
                                    else
                                    {
                                        objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr[objPropertyInfo.Name], objPropertyType), null);
                                    }
                                }
                                catch
                                {
                                    objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr[objPropertyInfo.Name], objPropertyType), null);
                                }
                            }
                        }
                    }
                    catch
                    {
                        objPropertyInfo.SetValue(objObject, objValue, null);
                    }
                }
            }

            return objObject;

        }



        /// <summary>
        /// Return object of XmlDocument.
        /// </summary>
        /// <param name="objObject">object</param>
        /// <returns>Object of XmlDocument.</returns>
        public static XmlDocument Serialize(object objObject)
        {

            XmlSerializer objXmlSerializer = new XmlSerializer(objObject.GetType());
            StringBuilder objStringBuilder = new StringBuilder();
            TextWriter objTextWriter = new StringWriter(objStringBuilder);
            objXmlSerializer.Serialize(objTextWriter, objObject);
            StringReader objStringReader = new StringReader(objTextWriter.ToString());
            DataSet objDataSet = new DataSet();
            objDataSet.ReadXml(objStringReader);
            XmlDocument xmlSerializedObject = new XmlDocument();
            xmlSerializedObject.LoadXml(objDataSet.GetXml());
            return xmlSerializedObject;

        }




        /// <summary>
        /// Return object based on parameters.
        /// </summary>
        /// <param name="objType">Type od datatype.</param>
        /// <param name="dr">The DataReader</param>
        /// <param name="objProperties">ArrayList</param>
        /// <param name="arrOrdinals">Array of integer.</param>
        /// <returns>Object</returns>
        private static object CreateObject(Type objType, IDataReader dr, ArrayList objProperties, int[] arrOrdinals)
        {

            PropertyInfo objPropertyInfo;
            object objValue;
            Type objPropertyType = null;
            int intProperty;

            //objPropertyInfo.ToString() == BuiltyNumber
            object objObject = Activator.CreateInstance(objType);

            // fill object with values from datareader
            for (intProperty = 0; intProperty <= objProperties.Count - 1; intProperty++)
            {
                objPropertyInfo = (PropertyInfo)objProperties[intProperty];
                if (objPropertyInfo.CanWrite)
                {
                    objValue = Null.SetNull(objPropertyInfo);
                    if (arrOrdinals[intProperty] != -1)
                    {
                        if (System.Convert.IsDBNull(dr.GetValue(arrOrdinals[intProperty])))
                        {
                            // translate Null value
                            objPropertyInfo.SetValue(objObject, objValue, null);
                        }
                        else
                        {
                            try
                            {
                                // try implicit conversion first
                                objPropertyInfo.SetValue(objObject, dr.GetValue(arrOrdinals[intProperty]), null);
                            }
                            catch
                            {
                                // business object info class member data type does not match datareader member data type
                                try
                                {
                                    objPropertyType = objPropertyInfo.PropertyType;
                                    //need to handle enumeration conversions differently than other base types
                                    if (objPropertyType.BaseType.Equals(typeof(System.Enum)))
                                    {
                                        // check if value is numeric and if not convert to integer ( supports databases like Oracle )
                                        int test = 0;
                                        if (test.GetType() == dr.GetValue(arrOrdinals[intProperty]).GetType())
                                        {
                                            ((PropertyInfo)objProperties[intProperty]).SetValue(objObject, System.Enum.ToObject(objPropertyType, Convert.ToInt32(dr.GetValue(arrOrdinals[intProperty]))), null);
                                        }
                                        else
                                        {
                                            ((PropertyInfo)objProperties[intProperty]).SetValue(objObject, System.Enum.ToObject(objPropertyType, dr.GetValue(arrOrdinals[intProperty])), null);
                                        }
                                    }
                                    else if (objPropertyType.FullName.Equals("System.Guid"))
                                    {
                                        // guid is not a datatype common across all databases ( ie. Oracle )
                                        objPropertyInfo.SetValue(objObject, Convert.ChangeType(new Guid(dr.GetValue(arrOrdinals[intProperty]).ToString()), objPropertyType), null);
                                    }
                                    else
                                    {
                                        // try explicit conversion
                                        objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr.GetValue(arrOrdinals[intProperty]), objPropertyType), null);
                                    }
                                }
                                catch
                                {
                                    objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr.GetValue(arrOrdinals[intProperty]), objPropertyType), null);
                                }
                            }
                        }
                    }
                    else
                    {
                        // property does not exist in datareader
                    }
                }
            }

            return objObject;

        }



        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Generic version of CreateObject creates an object of a specified type from the 
        /// provided DataReader
        /// </summary>
        /// <typeparam name="T">The type of the business object</typeparam>
        /// <param name="dr">The DataReader</param>
        /// <returns>The custom business object</returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[cnurse]	10/10/2005	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        private static T CreateObject<T>(IDataReader dr)
        {

            PropertyInfo objPropertyInfo;
            object objValue;
            Type objPropertyType = null;
            int intProperty;

            T objObject = Activator.CreateInstance<T>();

            // get properties for type
            ArrayList objProperties = GetPropertyInfo(objObject.GetType());

            // get ordinal positions in datareader
            int[] arrOrdinals = GetOrdinals(objProperties, dr);

            // fill object with values from datareader
            for (intProperty = 0; intProperty <= objProperties.Count - 1; intProperty++)
            {
                objPropertyInfo = (PropertyInfo)objProperties[intProperty];
                if (objPropertyInfo.CanWrite)
                {
                    objValue = Null.SetNull(objPropertyInfo);
                    if (arrOrdinals[intProperty] != -1)
                    {
                        if (System.Convert.IsDBNull(dr.GetValue(arrOrdinals[intProperty])))
                        {
                            // translate Null value
                            objPropertyInfo.SetValue(objObject, objValue, null);
                        }
                        else
                        {
                            try
                            {
                                // try implicit conversion first
                                objPropertyInfo.SetValue(objObject, dr.GetValue(arrOrdinals[intProperty]), null);
                            }
                            catch
                            {
                                // business object info class member data type does not match datareader member data type
                                try
                                {
                                    objPropertyType = objPropertyInfo.PropertyType;
                                    //need to handle enumeration conversions differently than other base types
                                    if (objPropertyType.BaseType.Equals(typeof(System.Enum)))
                                    {
                                        // check if value is numeric and if not convert to integer ( supports databases like Oracle )
                                        int testint = 0;
                                        if (testint.GetType() == dr.GetValue(arrOrdinals[intProperty]).GetType())
                                        {
                                            ((PropertyInfo)objProperties[intProperty]).SetValue(objObject, System.Enum.ToObject(objPropertyType, Convert.ToInt32(dr.GetValue(arrOrdinals[intProperty]))), null);
                                        }
                                        else
                                        {
                                            ((PropertyInfo)objProperties[intProperty]).SetValue(objObject, System.Enum.ToObject(objPropertyType, dr.GetValue(arrOrdinals[intProperty])), null);
                                        }
                                    }
                                    else
                                    {
                                        // try explicit conversion
                                        objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr.GetValue(arrOrdinals[intProperty]), objPropertyType), null);
                                    }
                                }
                                catch
                                {
                                    objPropertyInfo.SetValue(objObject, Convert.ChangeType(dr.GetValue(arrOrdinals[intProperty]), objPropertyType), null);
                                }
                            }
                        }
                    }
                    else
                    {
                        // property does not exist in datareader
                    }
                }
            }

            return objObject;

        }

        /// <summary>
        /// Return out put parametr of given type.
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="command">SQL command.</param>
        /// <param name="outputParameterName">Out put parameter name.</param>
        /// <returns>Object of SqlCommand.</returns>
        public static NpgsqlCommand AddOutPutParametrofGivenType<T>(NpgsqlCommand command, string outputParameterName)
        {
            if (typeof(T) == typeof(int))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(Int16))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(long))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.BigInt));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(DateTime))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.DateTime));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(string))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.NVarChar));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(bool))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Bit));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(decimal))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Decimal));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
                command.Parameters[outputParameterName].Precision = 16;
                command.Parameters[outputParameterName].Scale = 2;
            }
            if (typeof(T) == typeof(float))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Float));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            return command;
        }



        /// <summary>
        /// Return out put parametr of given type.
        /// </summary>
        /// <typeparam name="T">Given type of object.</typeparam>
        /// <param name="command">SQL command.</param>
        /// <param name="outputParameterName">Out put paramerter name.</param>
        ///   <param name="outputParameterValue">Out put paramerter value.</param>
        /// <returns>Object of SqlCommand.</returns>
        public static NpgsqlCommand AddOutPutParametrofGivenType<T>(NpgsqlCommand command, string outputParameterName, object outputParameterValue)
        {

            if (typeof(T) == typeof(int))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(Int16))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Int));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(long))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.BigInt));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(DateTime))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.DateTime));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(string))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.NVarChar));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(bool))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Bit));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            if (typeof(T) == typeof(decimal))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Decimal));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
                command.Parameters[outputParameterName].Precision = 16;
                command.Parameters[outputParameterName].Scale = 2;
            }
            if (typeof(T) == typeof(float))
            {
                command.Parameters.Add(new NpgsqlParameter(outputParameterName, SqlDbType.Float));
                command.Parameters[outputParameterName].Direction = ParameterDirection.Output;
            }
            command.Parameters[outputParameterName].Value = outputParameterValue;
            return command;
        }



        /// <summary>
        /// Bulid Collection of List<KeyValuePair<string, string>> for Given object
        /// </summary>
        /// <typeparam name="List">List of Type(string,string)</typeparam>
        /// <param name="paramCollection">List of Type(string,string)</param>
        /// <param name="obj">Object</param>        
        /// <returns> Collection of KeyValuePair<string, string> </returns>
        public List<KeyValuePair<string, string>> BuildParameterCollection(List<KeyValuePair<string, string>> paramCollection, object obj)
        {
            try
            {
                foreach (PropertyInfo objProperty in obj.GetType().GetProperties())
                {
                    if (objProperty.GetValue(obj, null).ToString() != null)
                    {
                        paramCollection.Add(new KeyValuePair<string, string>("@" + objProperty.Name.ToString(), objProperty.GetValue(obj, null).ToString()));
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return paramCollection;
        }



        /// <summary>
        /// Bulid Collection of List<KeyValuePair<string, object>> for Given object
        /// </summary>
        /// <typeparam name="List">List of Type(string,object)</typeparam>
        /// <param name="paramCollection">List of Type(string,object)</param>
        /// <param name="obj">Object</param>
        /// <param name="excludeNullValue">Set True To Exclude Properties Having Null Value In The Object From Adding To The Collection</param>
        /// <returns> Collection of KeyValuePair<string, object></returns>
        public List<KeyValuePair<string, object>> BuildParameterCollection(List<KeyValuePair<string, object>> paramCollection, object obj, bool excludeNullValue)
        {
            try
            {
                if (excludeNullValue)
                {
                    foreach (PropertyInfo objProperty in obj.GetType().GetProperties())
                    {
                        if (objProperty.GetValue(obj, null) != null)
                        {
                            paramCollection.Add(new KeyValuePair<string, object>("@" + objProperty.Name.ToString(), objProperty.GetValue(obj, null)));
                        }
                    }
                }
                else
                {
                    foreach (PropertyInfo objProperty in obj.GetType().GetProperties())
                    {
                        paramCollection.Add(new KeyValuePair<string, object>("@" + objProperty.Name.ToString(), objProperty.GetValue(obj, null)));
                    }
                    return paramCollection;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return paramCollection;
        }


        /// <summary>
        /// Return array list from object.
        /// </summary>
        /// <param name="objType">Type od datatype.</param>
        /// <returns>Array list</returns>
        public static ArrayList GetPropertyInfo(Type objType)
        {

            // Use the cache because the reflection used later is expensive
            ArrayList objProperties = null;

            if (objProperties == null)
            {
                objProperties = new ArrayList();
                foreach (PropertyInfo objProperty in objType.GetProperties())
                {
                    objProperties.Add(objProperty);
                }
            }

            return objProperties;

        }
        /// <summary>
        /// Return object of array of integer.
        /// </summary>
        /// <param name="objProperties">ArrayList</param>
        /// <param name="dr">The DataReader</param>
        /// <returns>Array of integer.</returns>
        private static int[] GetOrdinals(ArrayList objProperties, IDataReader dr)
        {

            int[] arrOrdinals = new int[objProperties.Count + 1];
            int intProperty;

            if ((dr != null))
            {
                for (intProperty = 0; intProperty <= objProperties.Count - 1; intProperty++)
                {
                    arrOrdinals[intProperty] = -1;
                    try
                    {
                        arrOrdinals[intProperty] = dr.GetOrdinal(((PropertyInfo)objProperties[intProperty]).Name);
                    }
                    catch
                    {
                        // property does not exist in datareader
                    }
                }
            }

            return arrOrdinals;

        }


    }
}
