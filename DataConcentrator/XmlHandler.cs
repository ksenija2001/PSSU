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
        public static void SerializeData(DBModel.IOContext context, string path)
        {
            // Reads all tags from database
            var tags = context.Tags.ToList();

            // Tags that are read are wrapped with a EF proxy wrapper and need to be unwrapped in order to be eligable for serialization
            List<DBModel.Tag> items = new List<DBModel.Tag>();
        
            // Extracting usefull data for all tags
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
                    di.Alarms = new List<DBModel.Alarm>();
                    foreach (DBModel.Alarm a in ((DBModel.DI)tag).Alarms)
                    {
                        DBModel.Alarm alarm = new DBModel.Alarm();
                        alarm.Id = a.Id;
                        alarm.Value = a.Value;
                        alarm.TagId = a.TagId;
                        alarm.Message = a.Message;
                        alarm.Activate = a.Activate;
                        di.Alarms.Add(alarm);

                    }
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
                    ai.Alarms = new List<DBModel.Alarm>();
                    foreach (DBModel.Alarm a in ((DBModel.AI)tag).Alarms)
                    {
                        DBModel.Alarm alarm = new DBModel.Alarm();
                        alarm.Id = a.Id;
                        alarm.Value = a.Value;
                        alarm.TagId = a.TagId;
                        alarm.Message = a.Message;
                        alarm.Activate = a.Activate;
                        ai.Alarms.Add(alarm);

                    }
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
       
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8
            };
            using (XmlWriter writer = XmlWriter.Create(path, xmlWriterSettings))
            {
                // Types that need to be serialized
                var type = items.GetType();
                Type[] types = new Type[]{
                    typeof(DBModel.DI),
                    typeof(DBModel.AI),
                    typeof(DBModel.AO),
                    typeof(DBModel.DO),
                    typeof(DBModel.Alarm),
                };

                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");

                // Root element
                writer.WriteStartElement("Configuration");

                var xmlSerializer = new XmlSerializer(items.GetType(), types);
                xmlSerializer.Serialize(writer, items, ns);

                writer.WriteEndElement();
            }
        }

        public static void DeserializeData(string path)
        {
            try
            {
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    if (context.Database.Exists())
                    {
                        var response = MessageBox.Show("Existing database detected, if it differs too much from the configuration you are trying to load problems will occur. Would you like to overwritte the local database?", "Question", MessageBoxButton.YesNo);
                        if (response == MessageBoxResult.Yes)
                        {
                            context.Database.Connection.Close();
                            context.Database.Delete();
                            MessageBox.Show("Local database has been deleted. Loading configuration...");
                        }
                        else
                        {
                            MessageBox.Show("Trying to update local database with configuration...");
                        }
                    }
                    
                }

                XDocument xml = XDocument.Load(path);

                // List of all tags that need to be in the database after launching the application
                var tags = xml.Root.Elements("ArrayOfTag").Elements("Tag").ToList();

                // Definitions of needed serializers
                var DIser = new XmlSerializer(typeof(DBModel.DI));
                var AIser = new XmlSerializer(typeof(DBModel.AI));
                var DOser = new XmlSerializer(typeof(DBModel.DO));
                var AOser = new XmlSerializer(typeof(DBModel.AO));

                List<string> tagNames = new List<string>();

                foreach (var tag in tags)
                {
                    // Removing namespaces made during serialization
                    foreach (XElement XE in tag.DescendantsAndSelf())
                    {
                        XE.Name = XE.Name.LocalName;
                        XE.ReplaceAttributes((from xattrib in XE.Attributes().Where(xa => !xa.IsNamespaceDeclaration) select new XAttribute(xattrib.Name.LocalName, xattrib.Value)));
                    }

                    var type = tag.Attribute("type").Value;
                    tag.Name = type;

                    // Deserialization of all tags based on their type
                    // Depending of what is already in the database, tags can be updated or created
                    switch (type)
                    {
                        case "DI":
                            DBModel.DI DIitem = (DBModel.DI)DIser.Deserialize(tag.CreateReader());
                            tagNames.Add(DIitem.Name);
                            using (DBModel.IOContext context = new DBModel.IOContext())
                            {
                                var baseTag = DBTagHandler.FindTag<DBModel.DI>(context, DIitem.Name);

                                if (baseTag != null)
                                {
                                    List<DBModel.Alarm> baseAlarms = new List<Alarm>();
                                    if (baseTag.Alarms.Count > 0)
                                        baseAlarms = baseTag.Alarms.ToList();

                                    foreach (var prop in DIitem.GetType().GetProperties())
                                    {
                                        if (prop.Name == "Alarms")
                                        {
                                            List<int> ids = new List<int>();
                                            foreach (var a in DIitem.Alarms)
                                            {
                                                ids.Add(a.Id);

                                                var basea = baseAlarms.Where(n => n.Id == a.Id).FirstOrDefault();
                                                if (basea != default(DBModel.Alarm))
                                                {
                                                    foreach (var aprop in a.GetType().GetProperties())
                                                    {
                                                        if (aprop.Name != "Tag" && !object.Equals(aprop.GetValue(a), aprop.GetValue(basea)))
                                                        {
                                                            DBTagAlarmHandler.Update(context, a.Id, aprop.Name, aprop.GetValue(a), a);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    DBTagAlarmHandler.Create(context, a);
                                                    List<DBModel.Alarm> tagAl = DIitem.Alarms.ToList();
                                                    tagAl.Add(a);
                                                    DBTagHandler.UpdateTag(context, DIitem.Name, "Alarms", tagAl, DIitem);
                                                }
                                            }

                                            var ala = baseAlarms.Where(n => !ids.Contains(n.Id)).ToList();
                                            foreach (var item in ala)
                                            {
                                                DBTagAlarmHandler.Delete(context, item.Id, item);

                                            }
                                        }
                                        else if (!object.Equals(prop.GetValue(DIitem), prop.GetValue(baseTag)))
                                        {
                                            DBTagHandler.UpdateTag(context, baseTag.Name, prop.Name, prop.GetValue(DIitem), DIitem);
                                        }
                                    }
                                }
                                else
                                {
                                    DBTagHandler.CreateTag(context, DIitem);
                                }
                            }
                            break;
                        case "AI":
                            DBModel.AI AIitem = (DBModel.AI)AIser.Deserialize(tag.CreateReader());
                            tagNames.Add(AIitem.Name);
                            using (DBModel.IOContext context = new DBModel.IOContext())
                            {
                                var baseTag = DBTagHandler.FindTag<DBModel.AI>(context, AIitem.Name);

                                if (baseTag != null)
                                {
                                    List<DBModel.Alarm> baseAlarms = new List<Alarm>();
                                    if (baseTag.Alarms.Count > 0)
                                        baseAlarms = baseTag.Alarms.ToList();

                                    foreach (var prop in baseTag.GetType().GetProperties())
                                    {
                                        if (prop.Name == "Alarms")
                                        {
                                            List<int> ids = new List<int>();
                                            foreach (var a in AIitem.Alarms)
                                            {
                                                ids.Add(a.Id);
                                                var basea = baseAlarms.Where(n => n.Id == a.Id).FirstOrDefault();
                                                if (basea != default(DBModel.Alarm))
                                                {
                                                    foreach (var aprop in a.GetType().GetProperties())
                                                    {
                                                        if (aprop.Name != "Tag" && !object.Equals(aprop.GetValue(a), aprop.GetValue(basea)))
                                                        {
                                                            DBTagAlarmHandler.Update(context, a.Id, aprop.Name, aprop.GetValue(a), a);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    DBTagAlarmHandler.Create(context, a);
                                                    List<DBModel.Alarm> tagAl = AIitem.Alarms.ToList();
                                                    tagAl.Add(a);
                                                    DBTagHandler.UpdateTag(context, AIitem.Name, "Alarms", tagAl, AIitem);
                                                }
                                            }

                                            if (ids.Count > 0)
                                            {
                                                var ala = baseAlarms.Where(n => !ids.Contains(n.Id)).ToList();
                                                foreach (var item in ala)
                                                {
                                                    DBTagAlarmHandler.Delete(context, item.Id, item);

                                                }
                                            }

                                        }
                                        else if (!object.Equals(prop.GetValue(AIitem), prop.GetValue(baseTag)))
                                        {
                                            DBTagHandler.UpdateTag(context, baseTag.Name, prop.Name, prop.GetValue(AIitem), baseTag);
                                        }
                                    }
                                }
                                else
                                {
                                    DBTagHandler.CreateTag(context, AIitem);
                                }
                            }
                            break;
                        case "DO":
                            DBModel.DO DOitem = (DBModel.DO)DOser.Deserialize(tag.CreateReader());
                            tagNames.Add(DOitem.Name);
                            using (DBModel.IOContext context = new DBModel.IOContext())
                            {
                                var baseTag = DBTagHandler.FindTag<DBModel.DO>(context, DOitem.Name);
                                if (baseTag != null)
                                {
                                    foreach (var prop in baseTag.GetType().GetProperties())
                                    {
                                        if (!object.Equals(prop.GetValue(DOitem), prop.GetValue(baseTag)))
                                        {
                                            DBTagHandler.UpdateTag(context, baseTag.Name, prop.Name, prop.GetValue(DOitem), baseTag);
                                        }
                                    }
                                }
                                else
                                {
                                    DBTagHandler.CreateTag(context, DOitem);
                                }
                            }
                            break;
                        case "AO":
                            DBModel.AO AOitem = (DBModel.AO)AOser.Deserialize(tag.CreateReader());
                            tagNames.Add(AOitem.Name);
                            using (DBModel.IOContext context = new DBModel.IOContext())
                            {
                                var baseTag = DBTagHandler.FindTag<DBModel.AO>(context, AOitem.Name);
                                if (baseTag != null)
                                {
                                    foreach (var prop in baseTag.GetType().GetProperties())
                                    {
                                        if (!object.Equals(prop.GetValue(AOitem), prop.GetValue(baseTag)))
                                        {
                                            DBTagHandler.UpdateTag(context, baseTag.Name, prop.Name, prop.GetValue(AOitem), baseTag);
                                        }
                                    }
                                }
                                else
                                {
                                    DBTagHandler.CreateTag(context, AOitem);
                                }
                            }
                            break;
                    }


                }

                if (tagNames.Count > 0)
                {
                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        var items = context.Tags.Where(n => !tagNames.Contains(n.Name)).ToList();
                        foreach (var item in items)
                        {
                            DBTagHandler.DeleteTag(context, item.Name, item);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[WARNING] XML file could not be deserialized. Different local databse found. Application will be started with current databse state");
                //MessageBox.Show(ex.Message.ToString());
            }



        }

    }
}
