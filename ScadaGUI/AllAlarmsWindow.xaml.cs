using DataConcentrator;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static DataConcentrator.DBModel;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for AllAlarmsWindow.xaml
    /// </summary>
    public partial class AllAlarmsWindow : Window
    { 
        public List<string> tagNames {  get; set; }
        public AllAlarmsWindow()
        {
            InitializeComponent();
            
            tagNames = new List<string>();
            using (DBModel.IOContext context = new DBModel.IOContext())
            {

                var allDI = context.Tags.OfType<DBModel.DI>().Where(n => n.ScanState == 1).ToList();

                foreach (var tag in allDI)
                {
                    int num = tag.Alarms.Count;
                    for (int i=0; i<num; i++)
                        tagNames.Add(tag.Name);
                }

                var allAI = context.Tags.OfType<DBModel.AI>().Where(n => n.ScanState == 1).ToList();
                foreach (var tag in allAI)
                {
                    int num = tag.Alarms.Count;
                    for (int i = 0; i < num; i++)
                        tagNames.Add(tag.Name);
                }
                List<List<DBModel.Alarm>> alarms = allDI.Select(n => n.Alarms).ToList();
                alarms.AddRange(allAI.Select(n => n.Alarms));

                List<DBModel.Alarm> a = alarms.Where(list => list.Count > 0).SelectMany(list => list).ToList();

                if (a.Count > 0)
                {
                    int i = 0;
                    dataGridAlarms.ItemsSource = a.Select(n => new
                    {
                        Id = n.Id,
                        Value = n.Value,
                        Message = n.Message,
                        Activate = n.Activate,
                        TagName = tagNames[i++]
                    });

                }

            };
        }

        private void MenuItemBack_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Show();
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.MainWindow.Show();
            this.Close();
        }
    }
}
