using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            _instance = new Database();

            _instance.Connection = new MySqlConnection(config.ConnectionString);

            try
            {
                logger.Trace("Opening connection...");

                _instance.Connection.Open();

                logger.Info("Database connection successful");
            }
            catch (Exception ex)
            {

                logger.Fatal(ex, "Error in database connection");

                throw;
            }
        }

        #endregion

        public MySqlConnection Connection { get; private set; }

        #region Functions

        public static T Get<T>(int id) where T : DatabaseObject, new()
        {
            MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(id, false, 1), Database.Instance.Connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            try
            {
                while (reader.Read())
                {
                    return Read<T>(reader);
                }
            }
            finally
            {
                reader.Close();
            }

            return null;
        }

        internal static List<T> GetAll<T>(string whereField, int whereId, bool order = false, int limit = -1) where T : DatabaseObject, new()
        {
            MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(whereField, whereId, order, limit), Database.Instance.Connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            return ReadAll<T>(reader);
        }

        public static List<T> GetAll<T>(DatabaseObject parent, bool order = false, int limit = -1) where T : DatabaseObject, new()
        {
            MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(parent, order, limit), Database.Instance.Connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            return ReadAll<T>(reader);
        }

        public static List<T> GetAll<T>() where T : DatabaseObject, new()
        {
            MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(), Database.Instance.Connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            return ReadAll<T>(reader);
        }

        public static List<T> GetAll<T>(bool order = false, int limit = -1) where T : DatabaseObject, new()
        {
            MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateSelectQuery<T>(-1, order, limit), Database.Instance.Connection);
            MySqlDataReader reader = cmd.ExecuteReader();

            return ReadAll<T>(reader);
        }

        public static List<T> ReadAll<T>(MySqlDataReader reader) where T : DatabaseObject, new()
        {
            List<T> objects = new List<T>();

            try
            {
                while (reader.Read())
                {
                    objects.Add(Read<T>(reader));
                }
            }
            finally
            {
                reader.Close();
            }

            return objects;
        }

        public static T Read<T>(MySqlDataReader reader) where T : DatabaseObject, new()
        {
            T obj = new T();

            var fields = obj.GetAllWithAttribute<DataFieldAttribute>();

            foreach(var field in fields)
            {
                logger.Trace($"Reading field {obj.TableName}.{field.Value.Column} from database");
                field.Key.SetValue(obj, reader[field.Value.Column]);
            }

            logger.Trace("Object successfully read from database");
            
            return obj;
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
            else if (obj.HasAttribute<DataFieldAttribute>())
            {
                var fields = obj.GetAllWithAttribute<DataFieldAttribute>();
                
                foreach (var field in fields)
                {
                    if (field.Value.Options == ColumnOption.Unique)
                    {
                        //We have an unique constraint, check if this row already exists in the database
                        object value = field.Key.GetValue(obj);

                        string select = QueryBuilder.GenerateSelectQuery<T>(field.Value.Column, value, false, 1);

                        MySqlCommand c = new MySqlCommand(select, Database.Instance.Connection);

                        var reader = c.ExecuteReader();

                        var data = reader.Read();

                        if (data)
                        {
                            T existingObject = Read<T>(reader);

                            

                            if (existingObject != null)
                            {
                                obj = existingObject;
                                logger.Trace(
                                    "Value marked with Unique constraint already exists in database, skipping requested insert");
                                reader.Close();
                                return;
                            }
                        }

                        reader.Close();

                    }
                }
            }


            MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateInsertQuery<T>(), Database.Instance.Connection);
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

        public static void Update<T>(T obj) where T : DatabaseObject, new()
        {
            MySqlCommand cmd = new MySqlCommand(QueryBuilder.GenerateUpdateQuery<T>(), Database.Instance.Connection);
            CommandHelper.Bind(obj, ref cmd);
            CommandHelper.BindUpdateId(obj, ref cmd);
            cmd.ExecuteNonQuery();

            logger.Trace("Object successfully updated");
        }

        #endregion

    }
}
