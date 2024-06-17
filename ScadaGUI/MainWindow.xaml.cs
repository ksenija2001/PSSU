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
using OxyPlot.Wpf;
using System.Runtime.Remoting.Contexts;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        GraphViewModel graph = new GraphViewModel();

        public MainWindow() {
            InitializeComponent();

            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                XmlHandler.DeserializeData(@"../../Configuration.xml");

                alarmListView.ItemsSource = context.LogAlarms.ToList();

                alarmListView.SelectedIndex = alarmListView.Items.Count - 1;
                alarmListView.ScrollIntoView(alarmListView.SelectedItem);

                var cmb_list = context.Tags.OfType<DBModel.DI>().Select(x => x.Name).ToList();
                cmb_list.AddRange(context.Tags.OfType<DBModel.AI>().Select(x => x.Name).ToList());
                tagComboBox.ItemsSource = cmb_list;
            }

            PLCDataHandler.AlarmRaised += OnAlarmRaised;
        }

        private void Window_Activated(object sender, EventArgs e) {

        }

        private void TagsMenuItem_Click(object sender, RoutedEventArgs e) {
            TagsWindow tags = new TagsWindow();
            tags.InputListChanged += new EventHandler(OnComboBoxDataChanged);
            this.Hide();
            tags.ShowDialog();
        }

        private void AlarmsMenuItem_Click(object sender, RoutedEventArgs e) {
            AllAlarmsWindow alarms = new AllAlarmsWindow();
            this.Hide();
            alarms.ShowDialog();
        }


        private void Window_Closed(object sender, EventArgs e)
        {
            using (DBModel.IOContext context = new DBModel.IOContext())
            {
               XmlHandler.SerializeData(context, @"../../Configuration.xml");
            }

            PLCDataHandler.TerminateAllThreads();
        }

        private void Start_Click(object sender, EventArgs e) {

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
                tagComboBox.ItemsSource = context.Tags.Select(x => x.Name).ToList();
            }

            foreach (var entry in tagsAI) {
                if (Convert.ToBoolean(entry.ScanState) && Convert.ToBoolean(entry.Connected)) {
                    PLCDataHandler.StartScanner(entry, entry.GetType().BaseType);
                }
            }

            PLCDataHandler.PLCStarted = true;
            MessageBox.Show("Scanning started successfully");

        }

        private void OnDataChanged(object sender, EventArgs e) {
            if (((ComboBox)sender).SelectedIndex != -1) {
                PLCDataHandler.currently_showing = ((ComboBox)sender).Text;
                graph.ReinitializeList();
                GraphCtl.Model = graph.GraphDisplay;
                GraphCtl.InvalidatePlot(true);
            }
        }

        private void OnComboBoxDataChanged(object sender, EventArgs e) {
            using (DBModel.IOContext context = new DBModel.IOContext()) {
                var cmb_list = context.Tags.OfType<DBModel.DI>().Select(x => x.Name).ToList();
                cmb_list.AddRange(context.Tags.OfType<DBModel.AI>().Select(x => x.Name).ToList());
                tagComboBox.ItemsSource = cmb_list;
            }

            if (PLCDataHandler.currently_showing == null) {
                graph.ReinitializeList();
                GraphCtl.Model = graph.GraphDisplay;
                GraphCtl.InvalidatePlot(true);
            }
        }

        private void OnAlarmRaised() {
            this.Dispatcher.Invoke(() => {
                using (var context = new DBModel.IOContext()) {
                    alarmListView.ItemsSource = context.LogAlarms.ToList();
                    alarmListView.SelectedIndex = alarmListView.Items.Count - 1;
                    alarmListView.ScrollIntoView(alarmListView.SelectedItem);

                }
            });
        }
        
    }

    public class GraphViewModel {

        public PlotModel GraphDisplay { get; private set; }
        private LineSeries line = new LineSeries();
        private int len = 0;
        
        public GraphViewModel() {
            this.GraphDisplay = new PlotModel { Title = "" };
            PLCDataHandler.ValueChanged += DisplayData;
            line.Color = OxyColor.Parse("#FFB39DDB");
            this.GraphDisplay.Series.Add(line);
        }

        private void DisplayData(double val, double scanTime) {
            line.Points.Add(new DataPoint(len*scanTime, val));
            GraphDisplay.InvalidatePlot(true);
            ++len;
        }

        public void ReinitializeList() {
            len = 0;
            line.Points.Clear();
            GraphDisplay.Series.Clear();
            GraphDisplay.Series.Add(line);
            GraphDisplay.InvalidatePlot(true);
        }
    }


}
