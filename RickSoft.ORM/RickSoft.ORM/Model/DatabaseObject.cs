using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NLog;
using RickSoft.ORM.Engine.Attributes;

namespace RickSoft.ORM.Engine.Model
{
    public abstract class DatabaseObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private string tableName = null;
        private string primaryKeyColumn = null;

        private void UpdateTableInfo()
        {
            var tableAttr = this.GetType().GetCustomAttributes(typeof(DataTableAttribute), true).FirstOrDefault() as DataTableAttribute;

            if (tableAttr != null)
            {
                logger.Trace($"Read tablename and primary key column ({tableAttr.TableName}.{tableAttr.PrimaryKeyColumn}) from class attributes");
                tableName = tableAttr.TableName;
                primaryKeyColumn = tableAttr.PrimaryKeyColumn;
                return;
            }
            
            throw new InvalidDataException("This table object does not specify a DataTableAttribute. Please add it before continuing");
        }

        public bool HasAttribute<T>() where T : Attribute
        {
            //Read fields
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                    if (attr is T)
                        return true;
            }

            return false;
        }

        public Dictionary<PropertyInfo, Tuple<K, T>> GetAllWithAttributes<K, T>() where K : Attribute where T : Attribute
        {
            var result = new Dictionary<PropertyInfo, Tuple<K, T>>();

            //Read fields
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                DoNotDuplicateAttribute dupeAttr = null;
                DataFieldAttribute dataAttr = null;

                foreach (object attr in prop.GetCustomAttributes(true))
                {
                    if (dupeAttr == null)
                        dupeAttr = attr as DoNotDuplicateAttribute;

                    if (dataAttr == null)
                        dataAttr = attr as DataFieldAttribute;

                }

                if (dataAttr != null && dupeAttr != null)
                {
                    result.Add(prop, new Tuple<K, T>(dupeAttr as K ?? dataAttr as K, dupeAttr as T ?? dataAttr as T));
                    break;
                }
            }

            return result;
        }

        public Dictionary<PropertyInfo, T> GetAllWithAttribute<T>() where T : Attribute
        {
            var result = new Dictionary<PropertyInfo, T>();

            //Read fields
            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                foreach (object obj in prop.GetCustomAttributes(true))
                    if (obj is T)
                    {
                        result.Add(prop, obj as T);
                        break;
                    }
            }

            return result;
        }

        public string TableName
        {
            get
            {
                if (tableName == null)
                    UpdateTableInfo();

                return tableName;
            }
        }

        public string PrimaryKeyColumn
        {
            get
            {
                if (primaryKeyColumn == null)
                    UpdateTableInfo();

                return primaryKeyColumn;
            }
        }

        public bool CompareTo(DatabaseObject other)
        {
            if (this.GetType() != other.GetType())
                throw new NotSupportedException("A data instance can only be compared against another instance of the same type!");

            //Get all properties with DoNotDuplicate attributes

            PropertyInfo[] props = this.GetType().GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    DoNotDuplicateAttribute dupeAttr = attr as DoNotDuplicateAttribute;

                    if (dupeAttr != null && dupeAttr.FieldType == DoNotDuplicateField.Field)
                    {
                        var val1 = prop.GetValue(this, null);
                        var val2 = prop.GetValue(other, null);

                        //This field has a DoNotDuplicate attribute
                        if (!object.Equals(val1, val2))
                        {
                            logger.Trace($"Object comparison failed on {prop.Name}");

                            return false;
                        }
                    }
                }
            }

            logger.Trace($"Object comparison success. All fields marked with DoNotDuplicate(Field) have the same value");

            return true;

        }

    }
}
