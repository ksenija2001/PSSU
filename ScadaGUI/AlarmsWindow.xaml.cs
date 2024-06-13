using DataConcentrator;
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
                if (data.GetType() == typeof(DBModel.AI))
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
                if (data.GetType() == typeof(DBModel.AI))
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

            ((DataGridComboBoxColumn)dataGridAlarms.Columns[2]).ItemsSource = active;
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
                   

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {

                        if (tagType == typeof(DBModel.DI))
                        {
                            DBModel.DI tag = context.Tags.OfType<DBModel.DI>().Where(n => n.Name == tagName).FirstOrDefault();
                            DBModel.Alarm alarm = tag.Alarms.Where(n => n.Id == item.Id).FirstOrDefault();
                            alarm.Value = item.Value;
                            alarm.Activate = item.Activate;
                            alarm.Message = item.Message;
                            context.SaveChanges();
                            //DBTagHandler.Update(context, tag);
                        }
                        else
                        {
                            DBModel.AI tag = context.Tags.OfType<DBModel.AI>().Where(n => n.Name == tagName).FirstOrDefault();
                            DBModel.Alarm alarm = tag.Alarms.Where(n => n.Id == item.Id).FirstOrDefault();
                            alarm.Value = item.Value;
                            alarm.Activate = item.Activate;
                            alarm.Message = item.Message;
                            context.SaveChanges();
                            //DBTagHandler.Update(context, tag);
                        }

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
            var item = ((DataGrid)contextMenu.PlacementTarget).SelectedItem;

            var response = MessageBox.Show($"Do you really want to permanantly delete this alarm?", "Question?", MessageBoxButton.YesNo);

            if (response == MessageBoxResult.Yes)
            {
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    var data = context.Tags.Where(n => n.Name == tagName).FirstOrDefault();
                    if (data.GetType() == typeof(DBModel.AI))
                    {
                        DBModel.Alarm alarm = ((DBModel.AI)data).Alarms.Where(n => n.Id == ((DBModel.Alarm)item).Id).FirstOrDefault();
                        ((DBModel.AI)data).Alarms.Remove(alarm);
                        DBTagHandler.Update(context, (DBModel.AI)data);
                    }
                    else
                    {
                        DBModel.Alarm alarm = ((DBModel.DI)data).Alarms.Where(n => n.Id == ((DBModel.Alarm)item).Id).FirstOrDefault();
                        ((DBModel.DI)data).Alarms.Remove(alarm);
                        DBTagHandler.Update(context, (DBModel.DI)data);
                    }
                }

                RefreshSource();
            }
        }

        private void MenuItemCreate_Click(object sender, RoutedEventArgs e)
        {
            AlarmDetails alarm = new AlarmDetails(tagType, tagName);
            alarm.ShowDialog();
            RefreshSource();
        }

        private void MenuItemBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

   
    }
}
