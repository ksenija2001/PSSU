using DataConcentrator;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for AlarmsWindow.xaml
    /// </summary>
    public partial class AlarmsWindow : Window
    {
        private List<ActiveWhen> active {  get; set; }
        private List<DBModel.Alarm> alarms {  get; set; }
        private string tagName { get; set; }
        private Type tagType {  get; set; }
        public AlarmsWindow(string tagName)
        {
            active = new List<ActiveWhen>() { ActiveWhen.BELOW, ActiveWhen.ABOVE, ActiveWhen.EQUALS };
            this.tagName = tagName;
            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                var data = context.Tags.Where(n => n.Name == tagName).FirstOrDefault();
                if (data.GetType().BaseType == typeof(DBModel.AI))
                    tagType = typeof(DBModel.AI);
                else
                    tagType = typeof(DBModel.DI);
            }

            InitializeComponent();

            lblTitle.Content = "Alarms for tag " + tagName;

            RefreshSource();
        }

        private void RefreshSource()
        {

            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                var data = context.Tags.Where(n => n.Name == tagName).FirstOrDefault();
                if (data.GetType().BaseType == typeof(DBModel.AI))
                {
                    alarms = ((DBModel.AI)data).Alarms;
                    active = new List<ActiveWhen>() { ActiveWhen.BELOW, ActiveWhen.ABOVE};
                }
                else
                {
                    alarms = ((DBModel.DI)data).Alarms;
                    active = new List<ActiveWhen>() { ActiveWhen.EQUALS };
                }
            }

            dataGridAlarms.ItemsSource = null;
            dataGridAlarms.ItemsSource = alarms;

            ((System.Windows.Controls.DataGridComboBoxColumn)dataGridAlarms.Columns[2]).ItemsSource = active;
        }

        private void dataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.EditAction == DataGridEditAction.Commit)
                {
                    string bindingPath = e.Column.Header.ToString();
                    DBModel.Alarm item = (DBModel.Alarm)e.EditingElement.DataContext;
                
                    if (bindingPath == "Value")
                    {
                        var el = e.EditingElement as TextBox;
                        // TODO napraviti proveru da li moze da se parsira
                        item.Value = double.Parse(el.Text.Trim());
                    }
                    else if (bindingPath == "Activate")
                    {
                        var el = e.EditingElement as ComboBox;
                        item.Activate = (ActiveWhen)el.SelectedItem;
                    }
                    else if (bindingPath == "Message")
                    {
                        var el = e.EditingElement as TextBox;
                        item.Message = el.Text.Trim();
                    }

                    DBModel.Alarm alarm;
                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        if (tagType == typeof(DBModel.DI))
                        {
                            DBModel.DI tag = DBTagHandler.FindTag<DBModel.DI>(context, tagName);
                            alarm = tag.Alarms.Where(n => n.Id == item.Id).FirstOrDefault();
                        }
                        else
                        {
                            DBModel.AI tag = DBTagHandler.FindTag<DBModel.AI>(context, tagName);
                            alarm = tag.Alarms.Where(n => n.Id == item.Id).FirstOrDefault();
                        }

                        alarm.Value = item.Value;
                        alarm.Activate = item.Activate;
                        alarm.Message = item.Message;

                        context.SaveChanges();
                    }

                    dataGridAlarms.UnselectAllCells();
                    dataGridAlarms.SelectedItem = null;

                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var errors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in errors.ValidationErrors)
                    {
                        MessageBox.Show($"Wrong Format for {validationError.PropertyName}: {validationError.ErrorMessage}");

                    }
                }
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"{ex.Message}");
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var item = ((DataGrid)contextMenu.PlacementTarget).SelectedCells[0].Item;

            var response = MessageBox.Show($"Do you really want to permanantly delete this alarm?", "Question?", MessageBoxButton.YesNo);

            if (response == MessageBoxResult.Yes)
            {
                int id = ((DBModel.Alarm)item).Id;
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    if (tagType == typeof(DBModel.AI))
                    {
                        DBModel.AI data = DBTagHandler.FindTag<DBModel.AI>(context, tagName);
                        DBModel.Alarm alarm = data.Alarms.Where(n => n.Id == id).FirstOrDefault();
                        DBTagAlarmHandler.Delete(context, id, alarm);
                        List<DBModel.Alarm> al = data.Alarms.ToList();
                        al.Remove(alarm);
                        DBTagHandler.UpdateTag(context, tagName, "Alarms", al, data);
                    
                    }
                    else
                    {
                        DBModel.DI data = DBTagHandler.FindTag<DBModel.DI>(context, tagName);
                        DBModel.Alarm alarm = data.Alarms.Where(n => n.Id == id).FirstOrDefault();
                        DBTagAlarmHandler.Delete(context, id, alarm);
                        List<DBModel.Alarm> al = data.Alarms.ToList();
                        al.Remove(alarm);
                        DBTagHandler.UpdateTag(context, tagName, "Alarms", al, data);
                    }
                }

                RefreshSource();
            }
        }

        private void MenuItemCreate_Click(object sender, RoutedEventArgs e)
        {
            AlarmDetails alarm = new AlarmDetails(tagName, tagType);
            alarm.DataChanged += new EventHandler(alarm_DataChanged);
            alarm.Show();
        }

        void alarm_DataChanged(object sender, EventArgs e)
        {
            AlarmDetails child = sender as AlarmDetails;
            if (child != null)
            {
                RefreshSource();
            }
        }

        private void MenuItemBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

   
    }
}
