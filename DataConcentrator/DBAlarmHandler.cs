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

        public static void Create(DBModel.IOContext context, DBModel.LogAlarm obj)
        {
            if (obj != null)
            {
                context.LogAlarms.Add(obj);

                context.SaveChanges(); 
                MessageBox.Show($"[INFO] New alarm added to database");
            }
            else
            {
                MessageBox.Show($"[ERROR] Couldn't add alarm");

            }
        }

        //public static void Update(DBAlarm.IOContext context, DBAlarm.Alarm obj)
        //{
        //    DBAlarm.Alarm alarm = context.Alarms.Where(n => n.Id == obj.Id).FirstOrDefault();
        //    if (alarm != null)
        //    {
        //        alarm.Id = obj.Id;
        //        alarm.AlarmTime = obj.AlarmTime;
        //        alarm.Message = obj.Message;
        //        alarm.TagId = obj.TagId;
        //        context.SaveChanges();
        //        MessageBox.Show($"[INFO] Alarm updated successfully");
        //    }
        //    else
        //    {
        //        MessageBox.Show($"[ERROR] Alarm doesn't exist");
        //    }
        //}

        public static DBModel.LogAlarm ReadById(DBModel.IOContext context, int id)
        {
            return context.LogAlarms.Where(n => n.Id == id).FirstOrDefault();
        }


        //public static void Delete(DBAlarm.IOContext context, int id)
        //{
        //    DBAlarm.Alarm alarm = ReadById(context, id);
        //    if (alarm != null)
        //    {
        //        context.Alarms.Remove(alarm);

        //        context.SaveChanges();
        //        MessageBox.Show($"[INFO] Alarm deleted successfully");

        //    }
        //    else
        //    {
        //        MessageBox.Show($"[ERROR] Alarm doesn't exist");
        //    }
        //}
    }
}
