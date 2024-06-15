using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using PLCSimulator;
using static DataConcentrator.DBModel;

namespace DataConcentrator {
    public static class PLCDataHandler {

        private static PLCSimulatorManager PLCSim;
        public static Dictionary<string, double> PLCData = new Dictionary<string, double>();
        private static List<Thread> ActiveThreads = new List<Thread>();
        public static bool PLCStarted = false;

        public static void PLCStart() {
            PLCSim = new PLCSimulatorManager();
            PLCSim.StartPLCSimulator();
            PLCData = PLCSim.GetAllData().ToDictionary(x => x.Key, x => x.Value);
        }

        public static void StartScanner(Tag tag, Type type) {

            List<Alarm> alarms;

            if (type == typeof(DBModel.DI)) {

                DBModel.DI tagDI = (DBModel.DI) tag;
                using (var context = new DBModel.IOContext()) {
                    var item = context.Tags.Where(x => x.Name == tagDI.Name).FirstOrDefault();
                    alarms = ((DBModel.DI)item).Alarms;
                }
                Thread t = new Thread(() => Scanning(tagDI.Name, tagDI.ScanTime, tagDI.IOAddress, alarms, type));
                t.Name = tag.Name;
                ActiveThreads.Add(t);
                t.Start();
            }
            else if(type == typeof(DBModel.AI)) {

                DBModel.AI tagAI = (DBModel.AI)tag;
                using (var context = new DBModel.IOContext()) {
                    var item = context.Tags.Where(x => x.Name == tagAI.Name).FirstOrDefault();
                    alarms = ((DBModel.AI)item).Alarms;
                }
                Thread t = new Thread(() => Scanning(tagAI.Name, tagAI.ScanTime, tagAI.IOAddress, alarms, type));
                t.Name = tag.Name;
                ActiveThreads.Add(t);
                t.Start();
            }
        }

        // TODO: check whether alarms is a reference (are new alarms going to appear)?
        // TO CONSIDER: different functions for AI and DI?
        // TODO: test alarm logging
        private static void Scanning(string name, double time, string tagAddress, List<Alarm> alarms, Type type) {
            
            while (true) {
                PLCData[tagAddress] = PLCSim.GetDataForAddress(tagAddress);
                if (type == typeof(DBModel.AI)) {

                    var alarms_above = alarms.FindAll(x => x.Activate == ActiveWhen.ABOVE).Where(x => x.Value > PLCData[tagAddress]);
                    var alarms_below = alarms.FindAll(x => x.Activate == ActiveWhen.BELOW).Where(x => x.Value < PLCData[tagAddress]);
                    if (alarms_above.Any()) {
                        AddAlarm(name, alarms_above);
                    }
                    if (alarms_below.Any()) {
                        AddAlarm(name, alarms_below);
                    }

                }
                else {

                    var alarms_equal = alarms.FindAll(x => x.Activate == ActiveWhen.BELOW).Where(x => x.Value == PLCData[tagAddress]);
                    if (alarms_equal.Any()) {
                        AddAlarm(name, alarms_equal);
                    }
                }
                
                Thread.Sleep((int)(time*1000));
            }
        }

        public static void AddAlarm(string name, IEnumerable<DBModel.Alarm> filtered_alarms) {
            using (var context = new DBAlarm.IOContext()) {
                foreach (var a in filtered_alarms) {
                    DBAlarm.LogAlarm alarm = new DBAlarm.LogAlarm();
                    alarm.AlarmTime = DateTime.Now;
                    alarm.Message = a.Message;
                    alarm.TagId = name;
                    context.LogAlarms.Add(alarm);
                }
                context.SaveChanges();
            }
        }

        // TODO: stopping thread entry is deleted from the base?
        public static void TerminateThread(string name) {
            Thread t = ActiveThreads.Find(x => x.Name == name);
            t.Abort();
            ActiveThreads.Remove(t);
        }
    }

 
}
