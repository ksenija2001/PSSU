using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace DataConcentrator
{
    public class DBAlarmHandler
    {

        public static void Create(DBModel.IOContext context, DBModel.Alarm obj)
        {
            if (obj != null)
            {
                context.Alarms.Add(obj);

                context.SaveChanges(); 
                MessageBox.Show($"[INFO] New alarm added to database");
            }
            else
            {
                MessageBox.Show($"[ERROR] Couldn't add null alarm");

            }
        }

        public static void Update(DBModel.IOContext context, DBModel.Alarm obj)
        {
            DBModel.Alarm alarm = context.Alarms.Where(n => n.Id == obj.Id).FirstOrDefault();
            if (alarm != null)
            {
                alarm.Value = obj.Value;
                alarm.AlarmTime = obj.AlarmTime;
                alarm.Activate = obj.Activate;
                alarm.Message = obj.Message;
                context.SaveChanges();
                MessageBox.Show($"[INFO] Alarm updated successfully");
            }
            else
            {
                MessageBox.Show($"[ERROR] Alarm doesn't exist");
            }
        }

        public static DBModel.Alarm ReadById(DBModel.IOContext context, int id)
        {
            return context.Alarms.Where(n => n.Id == id).FirstOrDefault();
        }


        public static void Delete(DBModel.IOContext context, int id)
        {
            DBModel.Alarm alarm = ReadById(context, id);
            if (alarm != null)
            {
                context.Alarms.Remove(alarm);
                
                context.SaveChanges();
                MessageBox.Show($"[INFO] Alarm deleted successfully");

            }
            else
            {
                MessageBox.Show($"[ERROR] Alarm doesn't exist");
            }

        }
    }
}
