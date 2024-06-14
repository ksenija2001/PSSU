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
        public AllAlarmsWindow()
        {
            InitializeComponent();

            using (DBModel.IOContext context = new DBModel.IOContext())
            {

                // TODO get to which tag the alarm belongs to
                var allDI = context.Tags.OfType<DBModel.DI>().ToList();
                var allAI = context.Tags.OfType<DBModel.AI>().ToList();

                List<List<DBModel.Alarm>> alarms = allDI.Select(n => n.Alarms).ToList();
                alarms.AddRange(allAI.Select(n => n.Alarms));

                List<DBModel.Alarm> a = alarms.Where(list => list.Count > 0).SelectMany(list => list).ToList();

                dataGridAlarms.ItemsSource = a;

                List<int> ids = a.Select(n => n.Id).ToList();

            };
        }

        private void MenuItemBack_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Show();
            this.Close();
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var item = ((DataGrid)contextMenu.PlacementTarget).SelectedCells[0].Item;

            var response = MessageBox.Show($"Do you really want to permanantly delete this alarm?", "Question?", MessageBoxButton.YesNo);

            if (response == MessageBoxResult.Yes)
            {
                //using (DBModel.IOContext context = new DBModel.IOContext())
                //{
                //    var data = context.Tags.Where(n => n.Name == tagName).FirstOrDefault();
                //    if (data.GetType() == typeof(DBModel.AI))
                //    {
                //        DBModel.Alarm alarm = ((DBModel.AI)data).Alarms.Where(n => n.Id == ((DBModel.Alarm)item).Id).FirstOrDefault();
                //        ((DBModel.AI)data).Alarms.Remove(alarm);
                //        DBTagHandler.Update(context, (DBModel.AI)data);
                //    }
                //    else
                //    {
                //        DBModel.Alarm alarm = ((DBModel.DI)data).Alarms.Where(n => n.Id == ((DBModel.Alarm)item).Id).FirstOrDefault();
                //        ((DBModel.DI)data).Alarms.Remove(alarm);
                //        DBTagHandler.Update(context, (DBModel.DI)data);
                //    }
                //}

                //RefreshSource();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.MainWindow.Show();
            this.Close();
        }
    }
}
