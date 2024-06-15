using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;
using DataConcentrator;
using System.Xml.Serialization;
using System.Data.Entity;
using System.Data.SqlClient;


namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using(DBModel.IOContext context = new DBModel.IOContext())
            {
                context.Configuration.ProxyCreationEnabled = false;
            }

        }

        private void Window_Activated(object sender, EventArgs e)
        {
           
        }

        private void TagsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TagsWindow tags = new TagsWindow();
            this.Hide();
            tags.ShowDialog();
        }

        private void AlarmsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AllAlarmsWindow alarms = new AllAlarmsWindow();
            this.Hide();
            alarms.ShowDialog();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                using(DBAlarm.IOContext context1 = new DBAlarm.IOContext())
                    XmlHandler.SerializeData(context1, context, @"../../Configuration.xml");

            }
        }

        private void Start_Click(object sender, EventArgs e) {
            
            PLCDataHandler.PLCStart();

            List<DBModel.DI> tagsDI;
            using (var context = new DBModel.IOContext()) {
                tagsDI = context.Tags.OfType<DBModel.DI>().ToList();
            }

            foreach (var entry in tagsDI) {
                if (Convert.ToBoolean(entry.ScanState) && Convert.ToBoolean(entry.Connected)) {
                    PLCDataHandler.StartScanner(entry, entry.GetType().BaseType);
                }
            }
            List<DBModel.AI> tagsAI;
            using (var context = new DBModel.IOContext()) {
                tagsAI = context.Tags.OfType<DBModel.AI>().ToList();
            }
           
            foreach (var entry in tagsAI) {
                if (Convert.ToBoolean(entry.ScanState) && Convert.ToBoolean(entry.Connected)) {
                    PLCDataHandler.StartScanner(entry, entry.GetType().BaseType);
                }
            }

            PLCDataHandler.PLCStarted = true;
            MessageBox.Show("PLC started successfully");
        }
    }


    // TO FIX
    public class MainViewModel {
        public MainViewModel() {
            this.MyModel = new PlotModel { Title = "" };
            this.MyModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
        }

        public PlotModel MyModel { get; private set; }
    }

   
}
