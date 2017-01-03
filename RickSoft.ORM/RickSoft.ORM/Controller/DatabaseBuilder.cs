using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NLog;
using RickSoft.ORM.Engine.Attributes;
using RickSoft.ORM.Engine.Model;

namespace RickSoft.ORM.Engine.Controller
{
    public class DatabaseBuilder
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static void Create<T>(T model, bool drop = false) where T : DatabaseObject, new()
        {

            T instance = new T();

            if (drop)
            {
                logger.Info("Dropping pre-existing table if exists...");
                Database.Drop<T>();
            }
            
            logger.Info("Generating insert SQL for table " + instance.TableName);

            var tableName = model.TableName;
            var primaryKey = model.PrimaryKeyColumn;
            var fields = model.GetAllWithAttribute<DataFieldAttribute>();

            

            var columns = new List<string>();

            foreach (var field in fields)
            {
                if (field.Value.IsPrimaryKey)
                    continue;

                string options = "";

                if (field.Value.Options.HasFlag(ColumnOption.Unique | ColumnOption.NotNull))
                {
                    options = "NOT NULL UNIQUE";
                } else if (field.Value.Options.HasFlag(ColumnOption.Unique))
                {
                    options = "UNIQUE";
                } else if (field.Value.Options.HasFlag(ColumnOption.NotNull))
                {
                    options = "NOT NULL";
                }

                logger.Trace($"Found table field {field.Value.Column}, detected type: {GetSqlDataType(field.Key.PropertyType)}");
                
                columns.Add($"{field.Value.Column} {GetSqlDataType(field.Key.PropertyType)} {options}");
            }

            var temp = "";

            foreach (string s in columns)
            {
                temp += s + ",";
            }

            temp = temp.Substring(0, temp.Length - 1);

            var sql = $"CREATE TABLE {tableName}( {primaryKey} INT NOT NULL AUTO_INCREMENT, {temp}, PRIMARY KEY ({primaryKey}));";

            logger.Trace("Generated query: " + sql);

            MySqlCommand cmd = new MySqlCommand(sql, Database.Instance.Connection);
            cmd.ExecuteNonQuery();

            logger.Info($"Table {tableName} created successfully.");

        }

        public static string GetSqlDataType(Type t)
        {
            var typeName = t.FullName;

            if (typeName == typeof(int).FullName)
            {
                return "INT";
            }

            if (typeName == typeof(DateTime).FullName)
            {
                return "TIMESTAMP";
            }

            if (typeName == typeof(short).FullName)
            {
                return "SMALLINT(6)";
            }

            if (typeName == typeof(string).FullName)
            {
                return "VARCHAR(255)";
            }

            if (typeName == typeof(byte[]).FullName)
                return "MEDIUMBLOB";

            throw new InvalidDataException($"Type {typeName} not supported!");
            
        }

    }
}