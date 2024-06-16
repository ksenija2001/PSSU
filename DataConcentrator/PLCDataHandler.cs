using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using PLCSimulator;
using static DataConcentrator.DBModel;

namespace DataConcentrator {

    public delegate void ValueScanned(double val, double time);

    public static class PLCDataHandler {

        private static PLCSimulatorManager PLCSim;
        public static Dictionary<string, double> PLCData = new Dictionary<string, double>();
        private static List<Thread> ActiveThreads = new List<Thread>();
        public static bool PLCStarted = false;
        public static event ValueScanned ValueChanged;
        public static string currently_showing;
        static readonly Object locker = new Object();

        public static void ConnectPLC() {
            PLCSim = new PLCSimulatorManager();
            PLCData = PLCSim.GetAllData().ToDictionary(x => x.Key, x => x.Value);
            PLCSim.StartPLCSimulator();
        }

        public static void StartScanner(Tag tag, Type type) {
            if (type == typeof(DBModel.DI)) {

                DBModel.DI tagDI = (DBModel.DI)tag;
                Thread t = new Thread(() => Scanning(tagDI.Name, tagDI.ScanTime, tagDI.IOAddress, type));
                t.Name = tag.Name;
                ActiveThreads.Add(t);
                t.Start();
            }
            else if (type == typeof(DBModel.AI)) {

                DBModel.AI tagAI = (DBModel.AI)tag;
                Thread t = new Thread(() => Scanning(tagAI.Name, tagAI.ScanTime, tagAI.IOAddress, type));
                t.Name = tag.Name;
                ActiveThreads.Add(t);
                t.Start();
            }
        }

        // TODO: test alarm logging
        private static void Scanning(string name, double time, string tagAddress, Type type) {

            List<Alarm> alarms = new List<Alarm>();
            while (true) {
                PLCData[tagAddress] = PLCSim.GetDataForAddress(tagAddress);
                lock (locker) {
                    if (currently_showing == name) {
                        ValueChanged(PLCData[tagAddress], time);
                    }
                }
                if (type == typeof(DBModel.AI)) {

                    using (var context = new DBModel.IOContext()) {
                        var AIs = context.Tags.OfType<DBModel.AI>().Where(x => x.Name == name).FirstOrDefault();
                        if (AIs != null) {
                            alarms = AIs.Alarms;
                        }
                    }

                    if (alarms != null) {
                        var alarms_above = alarms.FindAll(x => x.Activate == ActiveWhen.ABOVE).Where(x => x.Value > PLCData[tagAddress]);
                        var alarms_below = alarms.FindAll(x => x.Activate == ActiveWhen.BELOW).Where(x => x.Value < PLCData[tagAddress]);
                        if (alarms_above.Any()) {
                            AddAlarm(name, alarms_above);
                        }
                        if (alarms_below.Any()) {
                            AddAlarm(name, alarms_below);
                        }
                    }
                }
                else {

                    using (var context = new DBModel.IOContext()) {
                        var DIs = context.Tags.OfType<DBModel.DI>().Where(x => x.Name == name).FirstOrDefault();
                        if (DIs != null) {
                            if (DIs != null) {
                                alarms = DIs.Alarms;
                            }
                        }
                    }

                    if (alarms != null) {
                        var alarms_equal = alarms.FindAll(x => x.Activate == ActiveWhen.BELOW).Where(x => x.Value == PLCData[tagAddress]);
                        if (alarms_equal.Any()) {
                            AddAlarm(name, alarms_equal);
                        }
                    }
                }

                Thread.Sleep((int)(time * 1000));
            }
        }

        public static void AddAlarm(string name, IEnumerable<DBModel.Alarm> filtered_alarms) {
            using (var context = new DBModel.IOContext()) {
                foreach (var a in filtered_alarms) {
                    LogAlarm alarm = new LogAlarm();
                    if (context.LogAlarms.ToList().Count > 0)
                        alarm.Id = context.LogAlarms.Max(x => x.Id) + 1;
                    else
                        alarm.Id = 1;

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
            if (t != null) {
                t.Abort();
                ActiveThreads.Remove(t);
            }
        }

        public static void TerminateAllThreads() {
            foreach (var thread in ActiveThreads) {
                thread.Abort();
            }
            if (PLCStarted) {
                PLCSim.Abort();
            }

        }

        // TODO: forcing outputs when editing fields
        public static void ForceOutput(string addr, double val) {
            if (PLCStarted) {
                PLCData[addr] = val;
                PLCSim.SetValue(addr, val);
            }
        }
    } 
}
