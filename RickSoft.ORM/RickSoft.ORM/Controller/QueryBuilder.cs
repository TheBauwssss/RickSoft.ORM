using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NLog;
using RickSoft.ORM.Engine.Attributes;
using RickSoft.ORM.Engine.Model;

namespace RickSoft.ORM.Engine.Controller
{
    internal class QueryBuilder
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        internal static string GenerateSelectQuery<T>(string whereField, object whereValue, bool honorSelectionOptions = false,
            int limit = -1) where T : DatabaseObject, new()
        {
            T temp = new T();

            bool useLimit = limit != -1;

            string sqlWhere = "";
            string sqlLimit = "";
            string sqlOrderBy = "";

            

            sqlWhere = $"WHERE {whereField} = '{whereValue}'";
            

            if (useLimit)
                sqlLimit = $"LIMIT {limit}";


            if (honorSelectionOptions)
            {
                //Get selection options if neccecary
                PropertyInfo[] props = typeof(T).GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    object[] attrs = prop.GetCustomAttributes(true);

                    DataFieldAttribute dataAttr = null;
                    SelectOptionAttribute selectAttr = null;

                    foreach (object attr in attrs)
                    {
                        if (selectAttr == null)
                            selectAttr = attr as SelectOptionAttribute;

                        if (dataAttr == null)
                            dataAttr = attr as DataFieldAttribute;

                    }

                    if (dataAttr != null && selectAttr != null)
                    {
                        //Found field with both database and selectoption attributes
                        logger.Trace($"Binding selection option for {temp.TableName}.{dataAttr.Column} to SQL command");

                        if (selectAttr.Option == OptionType.OrderBy)
                        {
                            string order = (selectAttr.Order == OrderingMode.Ascending) ? "ASC" : "DESC";

                            sqlOrderBy = $"ORDER BY {temp.TableName}.{dataAttr.Column} {order}";
                            break;
                        }
                    }


                }
            }

            string command = $"SELECT * FROM {temp.TableName} {sqlWhere} {sqlOrderBy} {sqlLimit}".Trim() + ";";

            logger.Trace("Generated query: " + command);

            return command;
        }

        internal static string GenerateSelectQuery<T>(DatabaseObject parent, bool honorSelectionOptions = false,
            int limit = -1) where T : DatabaseObject, new()
        {
            T temp = new T();

            bool useWhere = parent != null;
            bool useLimit = limit != -1;

            string sqlWhere = "";
            string sqlLimit = "";
            string sqlOrderBy = "";

            if (useWhere)
            {
                int parentId = -1;

                //Bind fields
                PropertyInfo[] props = parent.GetType().GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    object[] attrs = prop.GetCustomAttributes(true);
                    foreach (object attr in attrs)
                    {
                        DataFieldAttribute dataAttr = attr as DataFieldAttribute;
                        if (dataAttr != null && dataAttr.IsPrimaryKey)
                        {
                            logger.Trace("Found parent ID field in " + temp.TableName + "." + dataAttr.Column);

                            var val = prop.GetValue(parent, null);

                            parentId = (int)val;
                        }
                    }
                }

                if (parentId == -1)
                    throw new InvalidDataContractException("Unable to limit to children from parent, parent does not have a primary key!");

                sqlWhere = $"WHERE {temp.TableName}.{parent.PrimaryKeyColumn} = {parentId}";
            }

            if (useLimit)
                sqlLimit = $"LIMIT {limit}";


            if (honorSelectionOptions)
            {
                //Get selection options if neccecary
                PropertyInfo[] props = typeof(T).GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    object[] attrs = prop.GetCustomAttributes(true);

                    DataFieldAttribute dataAttr = null;
                    SelectOptionAttribute selectAttr = null;

                    foreach (object attr in attrs)
                    {
                        if (selectAttr == null)
                            selectAttr = attr as SelectOptionAttribute;

                        if (dataAttr == null)
                            dataAttr = attr as DataFieldAttribute;

                    }

                    if (dataAttr != null && selectAttr != null)
                    {
                        //Found field with both database and selectoption attributes
                        logger.Trace($"Binding selection option for {temp.TableName}.{dataAttr.Column} to SQL command");

                        if (selectAttr.Option == OptionType.OrderBy)
                        {
                            string order = (selectAttr.Order == OrderingMode.Ascending) ? "ASC" : "DESC";

                            sqlOrderBy = $"ORDER BY {temp.TableName}.{dataAttr.Column} {order}";
                            break;
                        }
                    }


                }
            }

            string command = $"SELECT * FROM {temp.TableName} {sqlWhere} {sqlOrderBy} {sqlLimit}".Trim() + ";";

            logger.Trace("Generated query: " + command);

            return command;
        }

        internal static string GenerateSelectQuery<T>(int id = -1, bool honorSelectionOptions = false, int limit = -1) where T : DatabaseObject, new()
        {
            T temp = new T();

            bool useWhere = id != -1;
            bool useLimit = limit != -1;

            string sqlWhere = "";
            string sqlLimit = "";
            string sqlOrderBy = "";

            if (useWhere)
                sqlWhere = $"WHERE {temp.TableName}.{temp.PrimaryKeyColumn} = {id}";

            if (useLimit)
                sqlLimit = $"LIMIT {limit}";


            if (honorSelectionOptions)
            {
                //Get selection options if neccecary
                PropertyInfo[] props = typeof(T).GetProperties();
                foreach (PropertyInfo prop in props)
                {
                    object[] attrs = prop.GetCustomAttributes(true);

                    DataFieldAttribute dataAttr = null;
                    SelectOptionAttribute selectAttr = null;

                    foreach (object attr in attrs)
                    {
                        if (selectAttr == null)
                            selectAttr = attr as SelectOptionAttribute;

                        if (dataAttr == null)
                            dataAttr = attr as DataFieldAttribute;

                    }

                    if (dataAttr != null && selectAttr != null)
                    {
                        //Found field with both database and selectoption attributes
                        logger.Trace($"Binding selection option for {temp.TableName}.{dataAttr.Column} to SQL command");

                        if (selectAttr.Option == OptionType.OrderBy)
                        {
                            string order = (selectAttr.Order == OrderingMode.Ascending) ? "ASC" : "DESC";

                            sqlOrderBy = $"ORDER BY {temp.TableName}.{dataAttr.Column} {order}";
                            break;
                        }
                    }


                }
            }

            string command = $"SELECT * FROM {temp.TableName} {sqlWhere} {sqlOrderBy} {sqlLimit}".Trim() + ";";

            logger.Trace("Generated query: " + command);

            return command;
        }

        internal static string GenerateInsertQuery<T>() where T : DatabaseObject, new()
        {
            T temp = new T();

            string fields = "";
            string parameters = "";

            //Bind fields
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    DataFieldAttribute dataAttr = attr as DataFieldAttribute;
                    if (dataAttr != null && !dataAttr.IsPrimaryKey)
                    {
                        logger.Trace("Binding insert field for " + temp.TableName + "." + dataAttr.Column + " to SQL command");
                        fields += " " + dataAttr.Column + ",";
                        parameters += " @" + dataAttr.Column + ",";
                    }
                }
            }

            //Remove first and last characters
            fields = fields.Substring(1, fields.Length - 2);
            parameters = parameters.Substring(1, parameters.Length - 2);

            string command = $"INSERT INTO {temp.TableName} ({fields}) VALUES({parameters});";
            logger.Trace("Generated query: " + command);

            return command;
        }

        internal static string GenerateUpdateQuery<T>() where T : DatabaseObject, new()
        {
            T temp = new T();

            string fields = "";
            
            //Bind fields
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    DataFieldAttribute dataAttr = attr as DataFieldAttribute;
                    if (dataAttr != null && !dataAttr.IsPrimaryKey)
                    {
                        logger.Trace("Binding update field for " + temp.TableName + "." + dataAttr.Column + " to SQL command");
                        fields += $" {dataAttr.Column} = @{dataAttr.Column},";
                    }
                }
            }

            //Remove first and last characters
            fields = fields.Substring(1, fields.Length - 2);
            
            string command = $"UPDATE {temp.TableName} SET {fields} WHERE {temp.TableName}.{temp.PrimaryKeyColumn} = @id;";
            logger.Trace("Generated query: " + command);

            return command;
        }
    }
}
