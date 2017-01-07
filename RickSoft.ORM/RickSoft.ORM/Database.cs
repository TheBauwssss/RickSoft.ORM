using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NLog;
using RickSoft.ORM.Engine.Attributes;
using RickSoft.ORM.Engine.Controller;
using RickSoft.ORM.Engine.Model;

namespace RickSoft.ORM.Engine
{
    public class Database
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Singleton

        private static Database _instance;
        internal static DbConfig Config { get; set; }
        
        public static Database Instance
        {
            get
            {
                if (_instance == null)
                    throw new Exception("Please initialize the database first!");

                return _instance;
            }
        }

        public static void CreateInstance(DbConfig config)
        {
            if (_instance != null)
                return;

            Config = config;

            _instance = new Database();

        }

        #endregion

        #region Functions

        public static T Get<T>(int id) where T : DatabaseObject, new()
        {
            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(id, false, 1), connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                try
                {
                    while (reader.Read())
                        return DatabaseObject.Read<T>(reader);
                }
                finally
                {
                    reader.Close();
                }
            }

            return null;
        }

        public static T Get<T>(Expression<Predicate<T>> selector) where T : DatabaseObject, new()
        {
            var expression = LinqHelper.ParseLinqExpression(selector);

            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(expression), connection);
                expression.BindValue(ref cmd);

                MySqlDataReader reader = cmd.ExecuteReader();

                try
                {
                    if (reader.Read())
                        return DatabaseObject.Read<T>(reader);
                }
                finally
                {
                    reader.Close();
                }
            }

            return null; //No results
        }

        public static List<T> GetAll<T>(Expression<Predicate<T>> selector) where T : DatabaseObject, new()
        {
            var expression = LinqHelper.ParseLinqExpression(selector);

            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(expression), connection);
                expression.BindValue(ref cmd);

                MySqlDataReader reader = cmd.ExecuteReader();

                try
                {
                    return DatabaseObject.ReadAll<T>(reader);
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        internal static List<T> GetAll<T>(string whereField, int whereId, bool order = false, int limit = -1) where T : DatabaseObject, new()
        {
            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd =
                    new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(whereField, whereId, order, limit), connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                return DatabaseObject.ReadAll<T>(reader);
            }
        }

        public static List<T> GetAll<T>(DatabaseObject parent, bool order = false, int limit = -1) where T : DatabaseObject, new()
        {
            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(parent, order, limit), connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                return DatabaseObject.ReadAll<T>(reader);
            }
        }

        public static List<T> GetAll<T>() where T : DatabaseObject, new()
        {
            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(), connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                return DatabaseObject.ReadAll<T>(reader);
            }
        }

        public static List<T> GetAll<T>(bool order = false, int limit = -1) where T : DatabaseObject, new()
        {
            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(-1, order, limit), connection);
                MySqlDataReader reader = cmd.ExecuteReader();

                return DatabaseObject.ReadAll<T>(reader);
            }
        }

        public static void InsertAll<T>(ref List<T> objs) where T : DatabaseObject, new()
        {
            int batchSize = 100;

            T temp = new T();
            if (temp.HasAttribute<DoNotDuplicateAttribute>())
                throw new NotSupportedException("Bulk insert operations are not supported for complex objects.");

            for (int i = 0; i < objs.Count; i += batchSize)
            {
                List<T> batch = new List<T>();

                int size = batchSize;

                if (i + batchSize > objs.Count)
                    size = objs.Count - i;

                batch.AddRange(objs.GetRange(i, size));

                using (var connection = new MySqlConnection(Config.ConnectionString))
                {
                    connection.Open();

                    MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateBulkInsertQuery<T>(batch.Count), connection);

                    for (int currentParam = 0; currentParam < batch.Count; currentParam++)
                    {
                        CommandHelper.Bind(batch[currentParam], ref cmd, currentParam);
                    }

                    cmd.ExecuteNonQuery();
                }

                logger.Trace($"{batch.Count} objects successfully inserted into database.");

            }
        }

        public static void Insert<T>(ref T obj) where T : DatabaseObject, new()
        {
            if (obj.HasAttribute<DoNotDuplicateAttribute>())
            {
                logger.Trace("Checking for duplicate data fields");

                int fieldId = 0;
                string fieldName = "";

                //Read all fields
                PropertyInfo[] p = typeof(T).GetProperties();
                foreach (PropertyInfo prop in p)
                {
                    object[] attrs = prop.GetCustomAttributes(true);

                    DoNotDuplicateAttribute dupeAttr = null;
                    DataFieldAttribute dataAttr = null;

                    foreach (object attr in attrs)
                    {
                        if (dupeAttr == null)
                            dupeAttr = attr as DoNotDuplicateAttribute;

                        if (dataAttr == null)
                            dataAttr = attr as DataFieldAttribute;
                        
                    }

                    if (dataAttr != null && dupeAttr != null && dupeAttr.FieldType == DoNotDuplicateField.Key)
                    {
                        logger.Trace($"Found where field for duplicate data check for column {dataAttr.Column}");
                        fieldId = (int) prop.GetValue(obj, null);
                        fieldName = dataAttr.Column;
                        break;
                    }

                }

                //Check if we can go ahead with insertion

                bool updateOnly = true;

                //Get the two most recent data points
                List<T> data = GetAll<T>(fieldName, fieldId, true, 2);

                if (data.Count == 0)
                    updateOnly = false;
                else
                {
                    foreach (T o in data)
                    {
                        updateOnly = updateOnly && o.CompareTo(obj);
                    }
                }



                if (updateOnly)
                {
                    logger.Trace("Not inserting new object because of failed DoNotDuplicate constraint. Date field of most recent value will be updated instead.");

                    T temp = data[0];
                    bool fieldUpdated = false;

                    foreach (var field in temp.GetAllWithAttribute<SelectOptionAttribute>())
                    {
                        logger.Trace($"Updating SelectOption field for DoNotDuplicate object");
                        field.Key.SetValue(temp, DateTime.Now);
                        fieldUpdated = true;
                        break;
                    }
                    
                    if (!fieldUpdated)
                        throw new NotSupportedException("At least one field in a data object marked with a DoNotDuplicateAttribute must be marked with a SelectOptionAttribute");

                    Update(temp);

                    return;
                }
            }

            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateInsertQuery<T>(), connection);
                CommandHelper.Bind(obj, ref cmd);
                cmd.ExecuteNonQuery();
            

                foreach (var field in obj.GetAllWithAttribute<DataFieldAttribute>())
                {
                    if (field.Value.IsPrimaryKey)
                    {
                        logger.Trace("Binding insert id for " + field.Value.Column + "to inserted object");
                        field.Key.SetValue(obj, (int)cmd.LastInsertedId);
                        break;
                    }
                }

                logger.Trace("Object successfully inserted into database with id " + cmd.LastInsertedId + ".");
            }
        }

        public static void Drop<T>() where T : DatabaseObject, new()
        {
            T temp = new T();

            if (Config.SafeMode)
            {
                logger.Warn($"Unable to drop table {temp.TableName}, DROP TABLE not supported in safe mode!");
                throw new NotSupportedException("Operation is not supported while running in safe mode!");
            }

            logger.Warn("DROP TABLE requested for " + temp.TableName);

            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateDropQuery<T>(), connection);
                cmd.ExecuteNonQuery();
            }

            logger.Info("Table dropped successfully");
        }

        public static void Update<T>(T obj) where T : DatabaseObject, new()
        {
            using (var connection = new MySqlConnection(Config.ConnectionString))
            {
                connection.Open();

                MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateUpdateQuery<T>(), connection);
                CommandHelper.Bind(obj, ref cmd);
                CommandHelper.BindUpdateId(obj, ref cmd);
                cmd.ExecuteNonQuery();
            }
            logger.Trace("Object successfully updated");
        }

        #endregion

    }
}
