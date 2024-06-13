using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PLCSimulator;
using static DataConcentrator.DBModel;

namespace DataConcentrator {
    public static class PLCDataHandler {

        private static PLCSimulatorManager PLCSim;
        public static Dictionary<string, double> PLCData = new Dictionary<string, double>();

        public static void PLCStart() {
            PLCSim = new PLCSimulatorManager();
            PLCSim.StartPLCSimulator();
            PLCData = PLCSim.GetAllData().ToDictionary(x => x.Key, x => x.Value);
        }

        public static void StartScanner(string tagName) {
            using (var context = new DBModel.IOContext()) {
                var tag = context.Tags.Where(x => x.Name == tagName).FirstOrDefault();

                if (tag.GetType() == typeof(DBModel.DI)) {

                    DBModel.DI tagDI = (DBModel.DI) tag;
                    if (Convert.ToBoolean(tagDI.ScanState)) {
                        Thread t = new Thread(() => Scanning(tagDI.ScanTime, tagDI.IOAddress, tagDI.Alarms, "DI"));
                        t.Name = tagName;
                        t.Start();
                    }
                }else if(tag.GetType() == typeof(DBModel.AI)) {

                    DBModel.AI tagAI = (DBModel.AI)tag;
                    if (Convert.ToBoolean(tagAI.ScanState)) {
                        Thread t = new Thread(() => Scanning(tagAI.ScanTime, tagAI.IOAddress, tagAI.Alarms, "AI"));
                        t.Name = tagName;
                        t.Start();
                    }
                }
                
            }
        }

        private static void Scanning(double time, string tagAddress, List<Alarm> alarms, string type) {
            // TODO: stopping thread when scan state is off
            while (true) {
                PLCData[tagAddress] = PLCSim.GetDataForAddress(tagAddress);
                if (type == "AI") {
                    if (alarms.FindAll(x => x.Activate == ActiveWhen.ABOVE).Exists(x => x.Value > PLCData[tagAddress])) {
                        // TODO: raise event
                    }

                    if (alarms.FindAll(x => x.Activate == ActiveWhen.BELOW).Exists(x => x.Value < PLCData[tagAddress])) { 
                    
                    }

                }
                else {
                    if (alarms.FindAll(x => x.Activate == ActiveWhen.EQUALS).Exists(x => x.Value == PLCData[tagAddress])) {

                    }
                }
                
                Thread.Sleep((int)(time*1000));
            }
        }

        // TODO: stopping thread when scan state is off
        public static void TerminateThread(Thread name) {
            name.Abort();
        }
    }

 
}
