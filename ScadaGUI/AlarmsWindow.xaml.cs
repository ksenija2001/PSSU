using DataConcentrator;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for AlarmsWindow.xaml
    /// </summary>
    public partial class AlarmsWindow : Window
    {
        public AlarmsWindow(List<DBModel.Alarm> alarms)
        {
            InitializeComponent();
            //var tagAlarms = ((Tag)obj).Alarms;
        }

        private void dataGridAlarms_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //TODO Check if all data is valid and Check if IO address for selected row exists, if it does update the value of the tag,
            // if it doesn't create a new tag 
        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
