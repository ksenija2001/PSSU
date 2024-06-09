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
      
            //var tags = context.Tags.ToList();
            //var alarms = context.Alarms.ToList();
            //List<Object> list = new List<object>() { tags, alarms };
            //using (XmlWriter writer = XmlWriter.Create(path))
            //{
            //    XmlSerializer serializer = new XmlSerializer(list.GetType());
            //    serializer.Serialize(writer, list);
            //}
        }

        public void DeserializeData(DbContext context, string path)
        {

        }
    }
}
