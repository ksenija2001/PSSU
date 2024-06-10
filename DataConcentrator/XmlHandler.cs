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
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace DataConcentrator
{
    internal class XmlHandler
    {
        public string Path { get; set; }

        public XmlHandler(string path)
        {
            Path = path;
        }

        public void SerializeData(DbContext context, string path)
        {

            var DI = context.DI.ToList();
            var DO = context.DO.ToList();
            var AI = context.AI.ToList();
            var AO = context.AO.ToList();
            var alarms = context.Alarms.ToList();
            List<Object> list = new List<object>() { DI, DO, AI, AO, alarms };
            using (XmlWriter writer = XmlWriter.Create(path))
            {
                XmlSerializer serializer = new XmlSerializer(list.GetType());
                serializer.Serialize(writer, list);
            }
        }

        public T DeserializeData<T>(DbContext context, string path)
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
