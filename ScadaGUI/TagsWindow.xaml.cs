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
            InitializeComponent();
            IO = true;
            LastSelectedD = -2;
            LastSelectedA = -2;

            dataGridDITags.SelectionChanged -= dataGridD_SelectionChanged;
            dataGridAITags.SelectionChanged -= dataGridA_SelectionChanged;

            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                dataGridDITags.ItemsSource = context.Tags.OfType<DBModel.DI>().ToList();
                dataGridAITags.ItemsSource = context.Tags.OfType<DBModel.AI>().ToList();
            }

            dataGridDITags.SelectionChanged += dataGridD_SelectionChanged;
            dataGridAITags.SelectionChanged += dataGridA_SelectionChanged;
        }

        private void MenuItemViewInputs_Click(object sender, RoutedEventArgs e)
        {
            // TODO change source of dataGridTags to display inputs
            IO = true;

            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                dataGridDITags.ItemsSource = context.Tags.OfType<DBModel.DI>().ToList();
                dataGridAITags.ItemsSource = context.Tags.OfType<DBModel.AI>().ToList();
            }
        }

        private void MenuItemViewOutputs_Click(object sender, RoutedEventArgs e)
        {
            // TODO change source of dataGridTags to display outputs
            IO = false;

            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                dataGridDITags.ItemsSource = context.Tags.OfType<DBModel.DO>().ToList();
                dataGridAITags.ItemsSource = context.Tags.OfType<DBModel.AO>().ToList();

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (alarms != null && alarms.IsEnabled == true)
                alarms.Close();
            Application.Current.MainWindow.Show();
            
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            //dataGridDITags.SelectionChanged -= dataGridD_SelectionChanged;
            //dataGridAITags.SelectionChanged -= dataGridA_SelectionChanged;

            //if (IO)
            //{
            //    using (DBModel.IOContext context = new DBModel.IOContext())
            //    {
            //        dataGridDITags.ItemsSource = context.Tags.OfType<DBModel.DI>().ToList();
            //        dataGridAITags.ItemsSource = context.Tags.OfType<DBModel.AI>().ToList();
            //    }
            //}
            //else
            //{
            //    using (DBModel.IOContext context = new DBModel.IOContext())
            //    {
            //        dataGridDITags.ItemsSource = context.Tags.OfType<DBModel.DO>().ToList();
            //        dataGridAITags.ItemsSource = context.Tags.OfType<DBModel.AO>().ToList();

            //    }
            //}

            //dataGridDITags.SelectionChanged += dataGridD_SelectionChanged;
            //dataGridAITags.SelectionChanged += dataGridA_SelectionChanged;

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

                    if (alarms != null && alarms.IsActive == true)
                        alarms.Close();
                    alarms = new AlarmsWindow(a);
                    alarms.Show();
                    this.Focus();
                }
                else if (alarms != null && alarms.IsActive == true)
                    alarms.Close();
            }
            else if (LastSelectedA != -2 && LastSelectedA != dataGridAITags.SelectedIndex)
            {
                var item = dataGridAITags.Items[LastSelectedA];
                using (DBModel.IOContext context = new DBModel.IOContext())
                {
                    DBTagHandler.Update(context, item);
                }
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

                    if (alarms != null && alarms.IsActive == true)
                        alarms.Close();
                    alarms = new AlarmsWindow(a);
                    alarms.Show();
                    this.Focus();
                }
                else if (alarms != null && alarms.IsEnabled == true)
                    alarms.Close();
            }
            else if (LastSelectedD != -2 && LastSelectedD != dataGridDITags.SelectedIndex)
            {
                //var item = dataGridDITags.Items[LastSelectedD];
                //using (DBModel.IOContext context = new DBModel.IOContext())
                //{
                //    DBTagHandler.Update(context, item);
                //}
                //sLastSelectedD = -2;
            }

        }

        private void MenuItemCreate_Click(object sender, RoutedEventArgs e)
        {
            TagDetails tag = new TagDetails("Create");
            tag.ShowDialog();
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
                col.IsReadOnly = false;
                // TODO Add addresses from PLC
                col.ItemsSource = new List<string>
                {
                    "ADDR001",
                    "ADDR002",
                    "ADDR003",
                    "ADDR004"
                };
                e.Column = col;
            }
            else if (e.PropertyName == "Alarms")
            {
                e.Cancel = true;
            }
        }

     
    }
}
