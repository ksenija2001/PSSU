using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Xml.Linq;
using static DataConcentrator.DBModel;

namespace DataConcentrator
{
    public class DBTagHandler
    {

        public static void CreateTag<T>(DBModel.IOContext context, T entity) where T : class
        {
            context.Set<T>().Add(entity);
            context.SaveChanges();
            //MessageBox.Show("[INFO] Create successfull");

        }
        public static void UpdateTag<T>(DBModel.IOContext context, string id, string propertyName, object propertyValue, T obj) where T : class
        {
            var entity = context.Set<T>().Find(id);
            if (entity != null)
            {
                var property = typeof(T).GetProperty(propertyName);
                var value = Convert.ChangeType(propertyValue, property.PropertyType);
                property.SetValue(entity, value);
                context.SaveChanges();
                //MessageBox.Show("[INFO] Update successfull");
            }
            else
            {
                MessageBox.Show("[ERROR] Tag doesn't exist");
            }

        }

        public static T FindTag<T>(DBModel.IOContext context, string id) where T : class
        {
            var entity = context.Set<T>().Find(id);
            return entity;
        }

        public static void DeleteTag<T>(DBModel.IOContext context, string id, T obj) where T : class
        {
            var entity = context.Set<T>().Find(id);
            if (entity != null)
            {
                context.Set<T>().Remove(entity);
                context.SaveChanges();
                //MessageBox.Show("[INFO] Delete successfull");
            }
            else
            {
                MessageBox.Show("[ERROR] Tag doesn't exist");
            }
        }
    }
}
