using DataConcentrator;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
        private List<ActiveWhen> active {  get; set; }
        private List<DBModel.Alarm> alarms {  get; set; }
        public AlarmsWindow(List<DBModel.Alarm> alarms, string tagName)
        {
            active = new List<ActiveWhen>() { ActiveWhen.BELOW, ActiveWhen.ABOVE, ActiveWhen.EQUALS };
            this.alarms = alarms;

            InitializeComponent();

            lblTitle.Content = "Alarms for tag " + tagName;

            RefreshSource();
            
        }

        private void RefreshSource()
        {
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
                        DBTagHandler.Update(context, item);
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

            var response = MessageBox.Show($"Do you really want to permenantly delete this alarm?", "Question?", MessageBoxButton.YesNo);

            if (response == MessageBoxResult.Yes)
            {
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    DBAlarmHandler.Delete(context, ((DBModel.Alarm)item).Id);
                    RefreshSource();
                }
            }
        }

        private void MenuItemCreate_Click(object sender, RoutedEventArgs e)
        {
            AlarmDetails alarm = new AlarmDetails();
            alarm.ShowDialog();
        }

        private void MenuItemBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

   
    }
}
