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
                List<DBModel.Alarm> alarms = context.Set<DBModel.Alarm>().Where(a => a.Tag.Connected == (byte)1).ToList();
                dataGridAlarms.ItemsSource = alarms;
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
