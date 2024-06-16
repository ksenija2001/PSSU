using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using static DataConcentrator.DBModel;

namespace DataConcentrator
{
    public class XmlHandler
    {
        public string Path { get; set; }

        public XmlHandler(string path)
        {
            Path = path;
        }

        public static void SerializeData(DBModel.IOContext contextAlarm, DBModel.IOContext context, string path)
        {
            var tags = context.Tags.ToList();
            List<DBModel.Tag> items = new List<DBModel.Tag>();

        
            foreach( var tag in tags)
            {
                if (tag is DBModel.DI)
                {
                    DBModel.DI di = new DBModel.DI();
                    di.Name = tag.Name;
                    di.Description = tag.Description;
                    di.Connected = tag.Connected;
                    di.ScanTime = ((DBModel.DI)tag).ScanTime;
                    di.ScanState = ((DBModel.DI)tag).ScanState;
                    di.IOAddress = tag.IOAddress;
                    di.Alarms = ((DBModel.DI)tag).Alarms;
                    items.Add(di);
                }
                else if (tag is DBModel.AI)
                {
                    DBModel.AI ai = new DBModel.AI();
                    ai.Name = tag.Name;
                    ai.Description = tag.Description;
                    ai.Connected = tag.Connected;
                    ai.ScanTime = ((DBModel.AI)tag).ScanTime;
                    ai.ScanState = ((DBModel.AI)tag).ScanState;
                    ai.IOAddress = tag.IOAddress;
                    ai.LowLimit = ((DBModel.AI)tag).LowLimit;
                    ai.HighLimit = ((DBModel.AI)tag).HighLimit;
                    ai.Units = ((DBModel.AI)tag).Units;
                    ai.Alarms = ((DBModel.AI)tag).Alarms;
                    items.Add(ai);
                }
                else if (tag is DBModel.DO)
                {
                    DBModel.DO dO = new DBModel.DO();
                    dO.Name = tag.Name;
                    dO.Description = tag.Description;
                    dO.Connected = tag.Connected;
                    dO.InitialValue = ((DBModel.DO)tag).InitialValue;
                    dO.IOAddress = tag.IOAddress;
                    items.Add(dO);
                }
                else if (tag is DBModel.AO)
                {
                    DBModel.AO ao = new DBModel.AO();
                    ao.Name = tag.Name;
                    ao.Description = tag.Description;
                    ao.Connected = tag.Connected;
                    ao.InitialValue = ((DBModel.AO)tag).InitialValue;
                    ao.IOAddress = tag.IOAddress;
                    ao.LowLimit = ((DBModel.AO)tag).LowLimit;
                    ao.HighLimit = ((DBModel.AO)tag).HighLimit;
                    ao.Units = ((DBModel.AO)tag).Units;
                    items.Add(ao);
                }
            }
           
            var alarms = contextAlarm.LogAlarms.ToList();
            List<DBModel.LogAlarm> newAlarm = new List<DBModel.LogAlarm>();

            foreach (var al in alarms)
            {
                DBModel.LogAlarm a = new DBModel.LogAlarm();
                a.AlarmTime = al.AlarmTime;
                a.Id = al.Id;
                a.TagId = al.TagId;
                a.Message = al.Message;
                newAlarm.Add(a);
            }

            using (XmlWriter writer = XmlWriter.Create(path))
            {
                var type = items.GetType();
                Type[] types = new Type[]{
                    typeof(DBModel.DI),
                    typeof(DBModel.AI),
                    typeof(DBModel.AO),
                    typeof(DBModel.DO),
                    typeof(DBModel.Alarm),
                    typeof(DBModel.LogAlarm),
                };

                writer.WriteDocType("xml", null, null, null);
                writer.WriteStartElement("Configuration");
               
                var nsSerializer = new XmlSerializerNamespaces();
                nsSerializer.Add("", "");

                var xmlSerializer = new XmlSerializer(items.GetType(), types);
                xmlSerializer.Serialize(writer, items, nsSerializer);

                xmlSerializer = new XmlSerializer(newAlarm.GetType(), "");
                xmlSerializer.Serialize(writer, newAlarm, nsSerializer);

                writer.WriteEndElement();

            }
        }

        public static void DeserializeData(DBModel.IOContext context, string path)
        {
            var result = DeserializeItem<DBModel.DI>(context, path);
            Console.WriteLine();
        }

        public static T DeserializeItem<T>(DBModel.IOContext context, string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));
            T item;
            using (XmlReader reader = XmlReader.Create(path))
            {
                item = (T)ser.Deserialize(reader);
            }

            return item;
        }
    }
}
