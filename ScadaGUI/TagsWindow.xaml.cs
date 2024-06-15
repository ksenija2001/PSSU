using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Xml.Linq;
using DataConcentrator;
using MaterialDesignThemes.Wpf;
using Microsoft.Xaml.Behaviors.Layout;
using OxyPlot;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for TagsWindow.xaml
    /// </summary>
    public partial class TagsWindow : Window
    {
        // IO = true -> inputs
        // IO = false -> outputs
        private bool IO {  get; set; }

        private AlarmsWindow alarms {  get; set; }
        public TagsWindow()
        {
            IO = true;

            InitializeComponent();
            RefreshSources();
          
        }

        private void MenuItemViewInputs_Click(object sender, RoutedEventArgs e)
        {
            IO = true;
            RefreshSources();        
        }

        private void MenuItemViewOutputs_Click(object sender, RoutedEventArgs e)
        {
            IO = false;
            RefreshSources();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (alarms != null)
                alarms.Close();
            Application.Current.MainWindow.Show();
            
        }

        private void RefreshSources()
        {
            dataGridAITags.ItemsSource = null;
            dataGridDITags.ItemsSource = null;

            if (IO)
            {
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    dataGridDITags.ItemsSource = context.Tags.OfType<DBModel.DI>().ToList();
                    dataGridAITags.ItemsSource = context.Tags.OfType<DBModel.AI>().ToList();
                }

                var item = DIMenu.Items[1] as MenuItem;
                item.IsEnabled = true;
                item.Visibility = Visibility.Visible;

                //var addr = dataGridDITags.Items[2] as ComboBox;
                //addr.ItemsSource = PLCDataHandler.PLCData.Keys.Skip(8).Take(4);

                item = AIMenu.Items[1] as MenuItem;
                item.IsEnabled = true;
                item.Visibility = Visibility.Visible;
            }
            else
            {
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    dataGridDITags.ItemsSource = context.Tags.OfType<DBModel.DO>().ToList();
                    dataGridAITags.ItemsSource = context.Tags.OfType<DBModel.AO>().ToList();

                }

                var item = DIMenu.Items[1] as MenuItem;
                item.IsEnabled = false;
                item.Visibility = Visibility.Collapsed;

                item = AIMenu.Items[1] as MenuItem;
                item.IsEnabled = false;
                item.Visibility = Visibility.Collapsed;
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var contextMenu = (ContextMenu)menuItem.Parent;
            var item = ((DataGrid)contextMenu.PlacementTarget).SelectedCells[0].Item;

            var response = MessageBox.Show($"Do you really want to permenantly delete {((DBModel.Tag)item).Name}?", "Question?", MessageBoxButton.YesNo);

            if (response == MessageBoxResult.Yes)
            {
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    DBTagHandler.Delete(context, item);
                    RefreshSources();
                }
            }
        }

        private void MenuItemAlarm_Click(object sender, RoutedEventArgs e)
        {
            if (IO)
            {
                var menuItem = (MenuItem)sender;
                var contextMenu = (ContextMenu)menuItem.Parent;
                var data = (DataGrid)contextMenu.PlacementTarget;
                if (data.SelectedCells.Count > 0)
                {
                    var item = data.SelectedCells[0].Item;
                    string tagName = ((DBModel.Tag)item).Name;

                    if (alarms != null)
                        alarms.Close();
                    alarms = new AlarmsWindow(tagName);
                    alarms.Show();
                }
            }
            
            
        }

      
        private void MenuItemCreate_Click(object sender, RoutedEventArgs e)
        {
            if (IO)
            {
                TagDetails tag = new TagDetails();
                tag.DataChanged += new EventHandler(tag_DataChanged);
                tag.Show();
            }
            else
            {
                TagOutputDetails tag = new TagOutputDetails();
                tag.DataChanged += new EventHandler(tag_DataChanged);
                tag.ShowDialog();
            }
        }

        void tag_DataChanged(object sender, EventArgs e)
        {
            RefreshSources();
        }

        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
        
            if (e.PropertyName == "Name")
            {
                e.Column.IsReadOnly = true;
            }
            else if (e.PropertyName == "Connected" || e.PropertyName == "ScanState" || (e.PropertyName == "InitialValue" && dg == dataGridDITags))
            {
                DataGridCheckBoxColumn col = new DataGridCheckBoxColumn();
                col.Header = e.Column.Header;
                Binding binding = new Binding(e.PropertyName);
                
                col.Binding = binding;
                col.IsReadOnly = false;
                e.Column = col;
            }
            else if (e.PropertyName == "IOAddress")
            {
                System.Windows.Controls.DataGridComboBoxColumn col = new System.Windows.Controls.DataGridComboBoxColumn();
                col.Header = e.Column.Header;

                Binding binding = new Binding(e.PropertyName);
                col.SelectedItemBinding = binding;

                List<string> items = new List<string>();
                
                if (IO) {
                    if (dg == dataGridDITags) {
                        items = PLCDataHandler.PLCData.Keys.Skip(8).Take(4).ToList();
                    }
                    else {
                        items = PLCDataHandler.PLCData.Keys.Take(4).ToList();
                    }
                }
                else {
                    if (dg == dataGridDITags) {
                        items = PLCDataHandler.PLCData.Keys.Skip(12).Take(4).ToList();
                    }
                    else {
                        items = PLCDataHandler.PLCData.Keys.Skip(4).Take(4).ToList();
                    }
                }

                col.ItemsSource = items;
                col.IsReadOnly = false;

                e.Column = col;
            }
            else if (e.PropertyName == "Alarms")
            {
                e.Cancel = true;
            }

            DataGrid data = (DataGrid)sender;
            var column = data.Columns.Where(n => n.Header.ToString() == "Name").FirstOrDefault();
            if (column != null)
                column.DisplayIndex = 0;
            column = data.Columns.Where(n => n.Header.ToString() == "Description").FirstOrDefault();
            if (column != null)
                column.DisplayIndex = 1;
            column = data.Columns.Where(n => n.Header.ToString() == "IOAddress").FirstOrDefault();
            if (column != null)
                column.DisplayIndex = 2;
            column = data.Columns.Where(n => n.Header.ToString() == "Connected").FirstOrDefault();
            if (column != null)
                column.DisplayIndex = 3;
        }

        private void MenuItemBack_Click(object sender, RoutedEventArgs e)
        {
            if (alarms != null)
                alarms.Close();
            Application.Current.MainWindow.Show();
            this.Close();
        }

        private void dataGridDITags_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.EditAction == DataGridEditAction.Commit) {
                    string bindingPath = e.Column.Header.ToString();
                    string name = ((DBModel.Tag)e.EditingElement.DataContext).Name;
                    DBModel.Tag item;

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        if (IO)
                            item = context.Tags.OfType<DBModel.DI>().Where(n => n.Name == name).FirstOrDefault();
                        else
                            item = context.Tags.OfType<DBModel.DO>().Where(n => n.Name == name).FirstOrDefault();
                    }


                    if (bindingPath == "Connected") {
                        //TODO provera da li je neki drugi signal konektovan na istu adresu ako je checked = true
                        var el = e.EditingElement as CheckBox;
                        item.Connected = (el.IsChecked == true) ? (byte)1 : (byte)0;
                    }
                    else if (bindingPath == "ScanState") {
                        var el = e.EditingElement as CheckBox;

                        if (item.Connected == 0)
                        {
                            MessageBox.Show("[WARNING] Tag isn't connected to any address");
                            dataGridDITags.UnselectAllCells();
                            dataGridDITags.SelectedItem = null;
                            return;
                        }
                        else
                            ((DBModel.DI)item).ScanState = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        ((DBModel.DI)item).ScanState = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        if (PLCDataHandler.PLCStarted) { 
                            if (!Convert.ToBoolean(((DBModel.DI)item).ScanState)) {

                                PLCDataHandler.TerminateThread(item.Name);

                            } else {

                                PLCDataHandler.StartScanner(item, typeof(DBModel.DI));
                            }
                        }

                    }
                    else if (bindingPath == "IOAddress")
                    {
                        var el = e.EditingElement as ComboBox;
                        item.IOAddress = el.Text;
                    }
                    else if (bindingPath == "ScanTime")
                    {
                        var el = e.EditingElement as TextBox;
                        // TODO napraviti proveru da li moze da se parsira
                        ((DBModel.DI)item).ScanTime = double.Parse(el.Text.Trim());
                    }
                    else if (bindingPath == "InitialValue")
                    {
                        var el = e.EditingElement as CheckBox;
                        ((DBModel.DO)item).InitialValue = (el.IsChecked == true) ? (byte)1 : (byte)0;
                    }
                    else if (bindingPath == "Description")
                    {
                        var el = e.EditingElement as TextBox;
                        item.Description = el.Text.Trim();
                    }

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        DBTagHandler.Update(context, item);
                    }

                    dataGridDITags.UnselectAllCells();

                    dataGridDITags.SelectedItem = null;

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
            
        

        private void dataGridAITags_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.EditAction == DataGridEditAction.Commit)
                {
                    string bindingPath = e.Column.Header.ToString();
                    string name = ((DBModel.Tag)e.EditingElement.DataContext).Name;
                    DBModel.Tag item = null;

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        if (IO)
                            item = context.Tags.OfType<DBModel.AI>().Where(n => n.Name == name).FirstOrDefault();
                        else
                            item = context.Tags.OfType<DBModel.AO>().Where(n => n.Name == name).FirstOrDefault();
                    }

                    if (bindingPath == "Connected")
                    {
                        //TODO provera da li je neki drugi signal konektovan na istu adresu ako je checked = true
                        var el = e.EditingElement as CheckBox;
                        item.Connected = (el.IsChecked == true) ? (byte)1 : (byte)0;
                    }
                    else if (bindingPath == "ScanState")
                    {
                        var el = e.EditingElement as CheckBox;

                        if (item.Connected == 0)
                        {
                            MessageBox.Show("[WARNING] Tag isn't connected to any address");
                            dataGridDITags.UnselectAllCells();
                            dataGridDITags.SelectedItem = null;
                            return;
                        }
                        else
                            ((DBModel.AI)item).ScanState = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        ((DBModel.AI)item).ScanState = (el.IsChecked == true) ? (byte)1 : (byte)0;
                        if (PLCDataHandler.PLCStarted) {
                            if (!Convert.ToBoolean(((DBModel.AI)item).ScanState)) {

                                PLCDataHandler.TerminateThread(item.Name);

                            }
                            else {

                                PLCDataHandler.StartScanner(item, typeof(DBModel.AI));
                            }
                        }

                    }
                    else if (bindingPath == "IOAddress")
                    {
                        var el = e.EditingElement as ComboBox;
                        item.IOAddress = el.Text;
                    }
                    else if (bindingPath == "ScanTime")
                    {
                        var el = e.EditingElement as TextBox;
                        // TODO napraviti proveru da li moze da se parsira
                        ((DBModel.AI)item).ScanTime = double.Parse(el.Text.Trim());
                    }
                    else if (bindingPath == "InitialValue")
                    {
                        var el = e.EditingElement as TextBox;
                        // TODO napraviti proveru da li moze da se parsira
                        ((DBModel.AO)item).InitialValue = double.Parse(el.Text.Trim());
                    }
                    else if (bindingPath == "Description")
                    {
                        var el = e.EditingElement as TextBox;
                        item.Description = el.Text.Trim();
                    }
                    else if (bindingPath == "Units")
                    {
                        var el = e.EditingElement as TextBox;
                        if (IO)
                            ((DBModel.AI)item).Units = el.Text.Trim();
                        else
                            ((DBModel.AO)item).Units = el.Text.Trim();

                    }
                    else if (bindingPath == "LowLimit")
                    {
                        var el = e.EditingElement as TextBox;
                        // TODO napraviti proveru da li moze da se parsira
                        if (IO)
                            ((DBModel.AI)item).LowLimit = double.Parse(el.Text.Trim());
                        else
                            ((DBModel.AO)item).LowLimit = double.Parse(el.Text.Trim());

                    }
                    else if (bindingPath == "HighLimit")
                    {
                        var el = e.EditingElement as TextBox;
                        // TODO napraviti proveru da li moze da se parsira
                        if (IO)
                            ((DBModel.AI)item).HighLimit = double.Parse(el.Text.Trim());
                        else
                            ((DBModel.AO)item).HighLimit = double.Parse(el.Text.Trim());

                    }

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        DBTagHandler.Update(context, item);
                    }

                    dataGridAITags.UnselectAllCells();

                    dataGridAITags.SelectedItem = null;

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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (alarms != null)
                alarms.Close();
            Application.Current.MainWindow.Show();
            this.Close();
        }

        private void dataGridDITags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            dataGridDITags.UnselectAllCells();

            dataGridDITags.SelectedItem = null;
            return;
        }

        private void dataGridAITags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            dataGridAITags.UnselectAllCells();

            dataGridAITags.SelectedItem = null;
            return;
        }
    }
}
