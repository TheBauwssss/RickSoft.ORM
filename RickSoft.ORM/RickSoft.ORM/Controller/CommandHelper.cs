using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using NLog;
using RickSoft.ORM.Engine.Attributes;
using RickSoft.ORM.Engine.Model;

namespace RickSoft.ORM.Engine.Controller
{
    internal class CommandHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Binds a set of data values to a <see cref="MySqlCommand"/> object.
        /// </summary>
        /// <typeparam name="T">type of object to bind</typeparam>
        /// <param name="dataObject">object to bind</param>
        /// <param name="command">command to bind to</param>
        internal static void Bind<T>(T dataObject, ref MySqlCommand command) where T : DatabaseObject, new()
        {
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    DataFieldAttribute dataAttr = attr as DataFieldAttribute;
                    if (dataAttr != null && !dataAttr.IsPrimaryKey)
                    {
                        logger.Trace("Binding value for " + dataAttr.Column + " to SQL command");
                        command.Parameters.AddWithValue("@" + dataAttr.Column, prop.GetValue(dataObject, null));
                    }
                }
            }
        }

        internal static void BindUpdateId<T>(T dataObject, ref MySqlCommand command) where T : DatabaseObject, new()
        {
            PropertyInfo[] props = typeof(T).GetProperties();
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    DataFieldAttribute dataAttr = attr as DataFieldAttribute;
                    if (dataAttr != null && dataAttr.IsPrimaryKey)
                    {
                        logger.Trace("Binding update id for " + dataAttr.Column + " to SQL command");
                        command.Parameters.AddWithValue("@id", prop.GetValue(dataObject, null));
                    }
                }
            }
        }
    }
}
