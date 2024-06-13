using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void Create(DBModel.IOContext context, Object obj)
        {
            if (obj is DBModel.DI)
            {
                context.Tags.Add((DBModel.DI)obj);
            }
            else if (obj is DBModel.AI)
            {
                context.Tags.Add((DBModel.AI)obj);
            }
            else if (obj is DBModel.DO)
            {
                context.Tags.Add((DBModel.DO)obj);
            }
            else if (obj is DBModel.AO)
            {
                context.Tags.Add((DBModel.AO)obj);
            }
            else
            {
                MessageBox.Show("[ERROR] Tag couldn't be added");
            }

            context.SaveChanges();
            MessageBox.Show($"[INFO] New {obj.GetType().BaseType} tag added to database");
        }

        public static void Update(DBModel.IOContext context, Object obj)
        {
            if (obj != null)
            {
                string name = ((DBModel.Tag)((Object)obj)).Name;
                if (obj is DBModel.DI)
                {
                    DBModel.DI item = (DBModel.DI)context.Tags.Where(n => n.Name == name).FirstOrDefault();
                    var temp = item.Alarms;

                    item.Description = ((DBModel.DI)obj).Description;
                    item.ScanTime = ((DBModel.DI)obj).ScanTime;
                    item.Connected = ((DBModel.DI)obj).Connected;
                    item.ScanState = ((DBModel.DI)obj).ScanState;
                    item.IOAddress = ((DBModel.DI)obj).IOAddress;
                    //item.Alarms = ((DBModel.DI)obj).Alarms.ToList();

                }
                else if (obj is DBModel.AI)
                {
                    DBModel.AI item = (DBModel.AI)context.Tags.Where(n => n.Name == name).FirstOrDefault();
                    var temp = item.Alarms;

                    item.Description = ((DBModel.AI)obj).Description;
                    item.ScanTime = ((DBModel.AI)obj).ScanTime;
                    item.HighLimit = ((DBModel.AI)obj).HighLimit;
                    item.LowLimit = ((DBModel.AI)obj).LowLimit;
                    item.Units = ((DBModel.AI)obj).Units;
                    item.Connected = ((DBModel.AI)obj).Connected;
                    item.ScanState = ((DBModel.AI)obj).ScanState;
                    item.IOAddress = ((DBModel.AI)obj).IOAddress;

                    //item.Alarms = ((DBModel.AI)obj).Alarms.ToList();
                }
                else if (obj is DBModel.DO)
                {
                    DBModel.DO item = (DBModel.DO)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    item.Description = ((DBModel.DO)obj).Description;
                    item.InitialValue = ((DBModel.DO)obj).InitialValue;
                    item.Connected = ((DBModel.DO)obj).Connected;
                    item.IOAddress = ((DBModel.DO)obj).IOAddress;
                }
                else if (obj is DBModel.AO)
                {
                    DBModel.AO item = (DBModel.AO)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    item.Description = ((DBModel.AO)obj).Description;
                    item.InitialValue = ((DBModel.AO)obj).InitialValue;
                    item.HighLimit = ((DBModel.AO)obj).HighLimit;
                    item.LowLimit = ((DBModel.AO)obj).LowLimit;
                    item.Units = ((DBModel.AO)obj).Units;
                    item.Connected = ((DBModel.AO)obj).Connected;
                    item.IOAddress = ((DBModel.AO)obj).IOAddress;
                }
            
                context.SaveChanges();
                MessageBox.Show($"[INFO] Update of tag {name} successful");

            }
            else
            {
                MessageBox.Show($"[ERROR] Tag doesn't exist");
            }
        }

        //public static T ReadByName<T>(DBModel.IOContext context, string name)
        //{
        //    if (typeof(T) == typeof(DBModel.DI))
        //    {
        //        return (T)context.Tags.Where(n => n.Name == name).FirstOrDefault();
        //    }
        //    else if (typeof(T) == typeof(DBModel.AI))
        //    {
        //        return (T)Convert.ChangeType(context.Tags.Where(n => n.Name == name).FirstOrDefault(), typeof(T));
        //    }
        //    else if (typeof(T) == typeof(DBModel.DO))
        //    {
        //        return (T)Convert.ChangeType(context.Tags.Where(n => n.Name == name).FirstOrDefault(), typeof(T));
        //    }
        //    else if (typeof(T) == typeof(DBModel.AO))
        //    {
        //        return (T)Convert.ChangeType(context.Tags.Where(n => n.Name == name).FirstOrDefault(), typeof(T));
        //    }
        //    else
        //    {
        //        return default(T);
        //    }
        //}


        public static void Delete(DBModel.IOContext context, Object obj)
        {
            try 
            { 
                string name = ((DBModel.Tag)obj).Name;
                if (obj is DBModel.DI)
                {
                    DBModel.DI item = (DBModel.DI)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    context.Tags.Remove(item);

                }
                else if (obj is DBModel.AI)
                {
                    DBModel.AI item = (DBModel.AI)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    context.Tags.Remove(item);

                }
                else if (obj is DBModel.DO)
                {
                    DBModel.DO item = (DBModel.DO)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    context.Tags.Remove(item);
                }
                else if (obj is DBModel.AO)
                {
                    DBModel.AO item = (DBModel.AO)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    context.Tags.Remove(item);
                }

                context.SaveChanges();
                MessageBox.Show($"[INFO] Tag {name} deleted successfully");

            }
            catch
            {
                MessageBox.Show($"[ERROR] Tag doesn't exist");
            }

        }
    }
}
