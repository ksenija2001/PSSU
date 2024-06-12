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
        private bool IO {  get; set; }
        private int LastSelectedD {  get; set; }
        private int LastSelectedA { get; set; }

        private AlarmsWindow alarms {  get; set; }
        public TagsWindow()
        {
            IO = true;
            LastSelectedD = -2;
            LastSelectedA = -2;

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
            }
            else
            {
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    dataGridDITags.ItemsSource = context.Tags.OfType<DBModel.DO>().ToList();
                    dataGridAITags.ItemsSource = context.Tags.OfType<DBModel.AO>().ToList();

                }
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dataGridA_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
   
            var itemA = dataGridAITags.SelectedItem;
            if (itemA != null && LastSelectedA != dataGridAITags.SelectedIndex)
            {
                LastSelectedA = dataGridAITags.SelectedIndex;
                if (IO)
                {
                    List<DBModel.Alarm> a = new List<DBModel.Alarm>();
                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        string name = ((DBModel.Tag)itemA).Name;
                        var temp = context.Tags.Where(n => n.Name == name).FirstOrDefault();
                        a = ((DBModel.AI)temp).Alarms;
                    }

                    if (alarms != null)
                        alarms.Close();
                    alarms = new AlarmsWindow(a);
                    alarms.Show();
                }
                else if (alarms != null)
                    alarms.Close();
            }
            else if (LastSelectedA != dataGridAITags.SelectedIndex)
            {
                LastSelectedA = -2;
            }

        }

        private void dataGridD_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var itemD = dataGridDITags.SelectedItem;

            if (itemD != null && LastSelectedD != dataGridDITags.SelectedIndex)
            {
                LastSelectedD = dataGridDITags.SelectedIndex;
                if (IO)
                {
                    List<DBModel.Alarm> a = new List<DBModel.Alarm>();
                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        string name = ((DBModel.Tag)itemD).Name;
                        var temp = context.Tags.Where(n => n.Name == name).FirstOrDefault();
                        a = ((DBModel.DI)temp).Alarms;
                    }

                    if (alarms != null)
                        alarms.Close();
                    alarms = new AlarmsWindow(a);
                    alarms.Show();
                }
                else if (alarms != null)
                    alarms.Close();
            }
            else if (LastSelectedD != dataGridDITags.SelectedIndex)
            {
                LastSelectedD = -2;
            }

        }

        private void MenuItemCreate_Click(object sender, RoutedEventArgs e)
        {
            TagDetails tag = new TagDetails();
            tag.ShowDialog();
            RefreshSources();
        }

        private void dataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
           
            if (e.PropertyName == "Connected" || e.PropertyName == "ScanState")
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

                List<string> items = new List<string>
                {
                    "ADDR001",
                    "ADDR002",
                    "ADDR003",
                    "ADDR004"
                };

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
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;
                    if (bindingPath == "Connected")
                    {
                        //TODO provera da li je neki drugi signal konektovan na istu adresu ako je checked = true
                        var el = e.EditingElement as CheckBox;
                        DBModel.Tag item = null;
                        
                        if (IO)
                            item = (DBModel.DI)e.EditingElement.DataContext;
                        else
                            item = (DBModel.DO)e.EditingElement.DataContext;

                        item.Connected = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        using (DBModel.IOContext context = new DBModel.IOContext())
                        {
                            DBTagHandler.Update(context, item);
                        }
                    }
                    else if (bindingPath == "ScanState")
                    {
                        var el = e.EditingElement as CheckBox;
                        DBModel.DI item = (DBModel.DI)e.EditingElement.DataContext;
                        item.ScanState = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        using (DBModel.IOContext context = new DBModel.IOContext())
                        {
                            DBTagHandler.Update(context, item);
                        }
                    }
                    else if (bindingPath == "IOAddress" && IO)
                    {
                        var el = e.EditingElement as ComboBox;
                        DBModel.Tag item = null;

                        if (IO)
                            item = (DBModel.DI)e.EditingElement.DataContext;
                        else
                            item = (DBModel.DO)e.EditingElement.DataContext;

                        item.IOAddress = el.Text;

                        using (DBModel.IOContext context = new DBModel.IOContext())
                        {
                            DBTagHandler.Update(context, item);
                        }
                    }
                }
            }
        }

        private void dataGridAITags_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;
                    if (bindingPath == "Connected")
                    {
                        //TODO provera da li je neki drugi signal konektovan na istu adresu ako je checked = true
                        var el = e.EditingElement as CheckBox;
                        DBModel.Tag item = null;

                        if (IO)
                            item = (DBModel.AI)e.EditingElement.DataContext;
                        else
                            item = (DBModel.AO)e.EditingElement.DataContext;

                        item.Connected = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        using (DBModel.IOContext context = new DBModel.IOContext())
                        {
                            DBTagHandler.Update(context, item);
                        }
                    }
                    else if (bindingPath == "ScanState")
                    {
                        var el = e.EditingElement as CheckBox;
                        DBModel.AI item = (DBModel.AI)e.EditingElement.DataContext;
                        item.ScanState = (el.IsChecked == true) ? (byte)1 : (byte)0;

                        using (DBModel.IOContext context = new DBModel.IOContext())
                        {
                            DBTagHandler.Update(context, item);
                        }
                    }
                    else if (bindingPath == "IOAddress" && IO)
                    {
                        var el = e.EditingElement as ComboBox;
                        DBModel.Tag item = null;

                        if (IO)
                            item = (DBModel.AI)e.EditingElement.DataContext;
                        else
                            item = (DBModel.AO)e.EditingElement.DataContext;

                        item.IOAddress = el.Text;

                        using (DBModel.IOContext context = new DBModel.IOContext())
                        {
                            DBTagHandler.Update(context, item);
                        }
                    }
                }
            }
        }
    }
}
