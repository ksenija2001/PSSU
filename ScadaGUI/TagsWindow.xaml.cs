using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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
            DataGrid dg = (DataGrid)contextMenu.PlacementTarget;
            var item = dg.SelectedCells[0].Item;

            var response = MessageBox.Show($"Do you really want to permenantly delete {((DBModel.Tag)item).Name}?", "Question?", MessageBoxButton.YesNo);

            if (response == MessageBoxResult.Yes)
            {
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    if (IO && dg == dataGridDITags)
                        DBTagHandler.DeleteTag(context, ((DBModel.Tag)item).Name, (DBModel.DI)item);
                    else if (IO && dg == dataGridAITags)
                        DBTagHandler.DeleteTag(context, ((DBModel.Tag)item).Name, (DBModel.AI)item);
                    else if (!IO && dg == dataGridDITags)
                        DBTagHandler.DeleteTag(context, ((DBModel.Tag)item).Name, (DBModel.DO)item);
                    else if (!IO && dg == dataGridAITags)
                        DBTagHandler.DeleteTag(context, ((DBModel.Tag)item).Name, (DBModel.AO)item);
                }

                RefreshSources();
                PLCDataHandler.TerminateThread(((DBModel.Tag)item).Name);
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

        private void dataGridTags_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            try
            {
                if (e.EditAction == DataGridEditAction.Commit) 
                {
                    DataGrid dg = (DataGrid)sender;
                    string bindingPath = e.Column.Header.ToString();
                    string name = ((DBModel.Tag)e.EditingElement.DataContext).Name;
                    object value = null;
                    DBModel.Tag item = null;

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        if (IO && dg == dataGridDITags)
                            item = DBTagHandler.FindTag<DBModel.DI>(context, name);
                        else if (IO && dg == dataGridAITags)
                            item = DBTagHandler.FindTag<DBModel.AI>(context, name);
                        else if(!IO && dg == dataGridDITags)
                            item = DBTagHandler.FindTag<DBModel.DO>(context, name);
                        else if(!IO && dg == dataGridAITags)
                            item = DBTagHandler.FindTag<DBModel.AO>(context, name);
                    }

                    if (bindingPath == "Connected")
                    {
                        var el = e.EditingElement as CheckBox;
                        value = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        using (DBModel.IOContext context = new DBModel.IOContext())
                        {
                            var tag = context.Tags.Where(n => n.Connected == 1 && n.IOAddress == item.IOAddress && n.Name != name).FirstOrDefault();
                            if (tag != default(DBModel.Tag))
                            {
                                MessageBox.Show("[WARNING] IOAdress already in use by other tag");
                                dg.UnselectAllCells();
                                dg.SelectedItem = null;
                                el.IsChecked = false;
                                return;
                            }
                        }

                        if ((byte)value == 0)
                        {
                            DataGridRow row = dg.ItemContainerGenerator.ContainerFromIndex(e.Row.GetIndex()) as DataGridRow;
                            DataGridColumn col = dg.Columns.Where(n => n.Header.ToString() == "ScanState").FirstOrDefault();
                            var scanState = col.GetCellContent(row) as CheckBox;
                            scanState.IsChecked = false;

                            using (DBModel.IOContext context = new DBModel.IOContext())
                            {
                                if (IO && dg == dataGridDITags)
                                    DBTagHandler.UpdateTag(context, name, "ScanState", value, (DBModel.DI)item);
                                else if (IO && dg == dataGridAITags)
                                    DBTagHandler.UpdateTag(context, name, "ScanState", value, (DBModel.AI)item);
                                else if (!IO && dg == dataGridDITags)
                                    DBTagHandler.UpdateTag(context, name, "ScanState", value, (DBModel.DO)item);
                                else if (!IO && dg == dataGridAITags)
                                    DBTagHandler.UpdateTag(context, name, "ScanState", value, (DBModel.AO)item);
                            }
                        }
                    }
                    else if (bindingPath == "ScanState")
                    {
                        var el = e.EditingElement as CheckBox;
                        value = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        if (item.Connected == 0)
                        {
                            MessageBox.Show("[WARNING] Tag isn't connected to any address");
                            dg.UnselectAllCells();
                            dg.SelectedItem = null;
                            el.IsChecked = false;
                            return;
                        }

                        if (PLCDataHandler.PLCStarted && IO)
                        {
                            if ((byte)value == (byte)0)
                                PLCDataHandler.TerminateThread(item.Name);
                            else
                                PLCDataHandler.StartScanner(item, item.GetType().BaseType);
                        }
                    }
                    else if (bindingPath == "IOAddress")
                    {
                        var el = e.EditingElement as ComboBox;
                        value = el.Text;

                        if (item.Connected == (byte)1)
                        {
                            using (DBModel.IOContext context = new DBModel.IOContext())
                            {
                                var tag = context.Tags.Where(n => n.Connected == 1 && n.IOAddress == el.Text && n.Name != name).FirstOrDefault();
                                if (tag != default(DBModel.Tag))
                                {
                                    DataGridRow row = dg.ItemContainerGenerator.ContainerFromIndex(e.Row.GetIndex()) as DataGridRow;
                                    DataGridColumn col = dg.Columns.Where(n => n.Header.ToString() == "Connected").FirstOrDefault();
                                    var cell = col.GetCellContent(row) as CheckBox;
                                    cell.IsChecked = false;
                                    DBTagHandler.UpdateTag(context, name, "Connected", (byte)0, item);

                                    MessageBox.Show("[WARNING] IOAdress already in use by other tag");
                                }
                            }
                        }
                    }
                    else if (bindingPath == "ScanTime" || bindingPath == "InitialValue" ||
                            bindingPath == "LowLimit" || bindingPath == "HighLimit")
                    {
                        var el = e.EditingElement as TextBox;
                        value = double.Parse(el.Text.Trim());
                    }
                    else if (bindingPath == "Description" || bindingPath == "Units")
                    {
                        var el = e.EditingElement as TextBox;
                        value = el.Text.Trim();
                    }

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        if (IO && dg == dataGridDITags)
                            DBTagHandler.UpdateTag(context, name, bindingPath, value, (DBModel.DI)item);
                        else if (IO && dg == dataGridAITags)
                            DBTagHandler.UpdateTag(context, name, bindingPath, value, (DBModel.AI)item);
                        else if (!IO && dg == dataGridDITags)
                            DBTagHandler.UpdateTag(context, name, bindingPath, value, (DBModel.DO)item);
                        else if (!IO && dg == dataGridAITags)
                            DBTagHandler.UpdateTag(context, name, bindingPath, value, (DBModel.AO)item);
                    }

                    dg.UnselectAllCells();
                    dg.SelectedItem = null;
                    dg.SelectedIndex = -1;
                }
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var errors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in errors.ValidationErrors)
                        MessageBox.Show($"Wrong Format for {validationError.PropertyName}: {validationError.ErrorMessage}");
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

        private void dataGridTags_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;
            dg.CancelEdit();
            dg.UnselectAllCells();
            dg.SelectedItem = null;
            return;
        }
    }
}
