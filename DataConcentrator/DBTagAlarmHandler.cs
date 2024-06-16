using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DataConcentrator
{
    public class DBTagAlarmHandler
    {


        public static void Create<T>(DBModel.IOContext context, T entity) where T : class
        {
            context.Set<T>().Add(entity);
            context.SaveChanges();
            MessageBox.Show("[INFO] Create successfull");

        }
        public static void Update<T>(DBModel.IOContext context, int id, string propertyName, object propertyValue, T obj) where T : class
        {
            var entity = context.Set<T>().Find(id);
            if (entity != null)
            {
                var property = typeof(T).GetProperty(propertyName);
                var value = Convert.ChangeType(propertyValue, property.PropertyType);
                property.SetValue(entity, value);
                context.SaveChanges();
                MessageBox.Show("[INFO] Update successfull");
            }
            else
            {
                MessageBox.Show("[ERROR] Alarm doesn't exist");
            }

        }

        public static T Find<T>(DBModel.IOContext context, int id) where T : class
        {
            var entity = context.Set<T>().Find(id);
            return entity;
        }

        public static void Delete<T>(DBModel.IOContext context, int id, T obj) where T : class
        {
            var entity = context.Set<T>().Find(id);
            if (entity != null)
            {
                context.Set<T>().Remove(entity);
                context.SaveChanges();
                MessageBox.Show("[INFO] Delete successfull");
            }
            else
            {
                MessageBox.Show("[ERROR] Alarm doesn't exist");
            }
        }
    }
}
