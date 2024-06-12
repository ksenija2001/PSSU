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

            context.SaveChanges();
            MessageBox.Show($"New {obj.GetType()} added to database");
        }

        public static void Update(DBModel.IOContext context, Object obj)
        {
            if (obj != null)
            {
                string name = ((DBModel.Tag)((Object)obj)).Name;
                if (obj is DBModel.DI)
                {
                    DBModel.DI item = (DBModel.DI)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    item.Connected = ((DBModel.DI)obj).Connected;
                    item.ScanState = ((DBModel.DI)obj).ScanState;
                    item.IOAddress = ((DBModel.DI)obj).IOAddress;
                }
                else if (obj is DBModel.AI)
                {
                    DBModel.AI item = (DBModel.AI)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    item.Connected = ((DBModel.AI)obj).Connected;
                    item.ScanState = ((DBModel.AI)obj).ScanState;
                    item.IOAddress = ((DBModel.AI)obj).IOAddress;
                }
                else if (obj is DBModel.DO)
                {
                    DBModel.DO item = (DBModel.DO)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    item.Connected = ((DBModel.DO)obj).Connected;
                    item.IOAddress = ((DBModel.DO)obj).IOAddress;
                }
                else if (obj is DBModel.AO)
                {
                    DBModel.AO item = (DBModel.AO)context.Tags.Where(n => n.Name == name).FirstOrDefault();

                    item.Connected = ((DBModel.AO)obj).Connected;
                    item.IOAddress = ((DBModel.AO)obj).IOAddress;
                }
            
                context.SaveChanges();
                MessageBox.Show($"{name} updated successfully");

            }
            else
            {
                MessageBox.Show($"Object doesn't exist");
            }
        }

        public static T ReadById<T>(DBModel.IOContext context, T obj)
        {
            string name = ((DBModel.Tag)((Object)obj)).Name;

            if (typeof(T) == typeof(DBModel.DI))
            {
                return (T) Convert.ChangeType(context.Tags.Where(n => n.Name == name).FirstOrDefault(), typeof(T));
            }
            else if (typeof(T) == typeof(DBModel.AI))
            {
                return (T)Convert.ChangeType(context.Tags.Where(n => n.Name == name).FirstOrDefault(), typeof(T));
            }
            else if (typeof(T) == typeof(DBModel.DO))
            {
                return (T)Convert.ChangeType(context.Tags.Where(n => n.Name == name).FirstOrDefault(), typeof(T));
            }
            else if (typeof(T) == typeof(DBModel.AO))
            {
                return (T) Convert.ChangeType(context.Tags.Where(n => n.Name == name).FirstOrDefault(), typeof(T));
            }
            else
            {
                return default(T);
            }
        }


        public static void Delete(DBModel.IOContext context, Object obj)
        {
            if (obj == null)
            {
                //var item = ReadById(context, obj);

                if (obj is DBModel.DI)
                {
                    context.Tags.Remove((DBModel.DI)obj);

                    foreach(DBModel.Alarm alarm in ((DBModel.DI)obj).Alarms)
                    {
                        //TODO delete alarms
                    }
                }
                else if (obj is DBModel.AI)
                {
                    context.Tags.Remove((DBModel.AI)obj);

                    foreach (DBModel.Alarm alarm in ((DBModel.AI)obj).Alarms)
                    {
                        //TODO delete alarms
                    }
                }
                else if (obj is DBModel.DO)
                {
                    context.Tags.Remove((DBModel.DO)obj);
                }
                else if (obj is DBModel.AO)
                {
                    context.Tags.Remove((DBModel.AO)obj);
                }

                context.SaveChanges();
                MessageBox.Show($"{obj.GetType()} deleted successfully");

            }
            else
            {
                MessageBox.Show($"{obj.GetType()} doesn't exist");
            }

        }
    }
}
