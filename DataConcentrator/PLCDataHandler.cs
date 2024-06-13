using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PLCSimulator;

namespace DataConcentrator {
    public static class PLCDataHandler {

        private static PLCSimulatorManager PLCSim;
        public static Dictionary<string, double> PLCData = new Dictionary<string, double>();

        public static void PLCStart() {
            PLCSim = new PLCSimulatorManager();
            PLCSim.StartPLCSimulator();
            PLCData = PLCSim.GetData().ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
