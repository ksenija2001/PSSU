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
        public List<ScanState> StatesList { get; set; }
        public TagsWindow()
        {
            InitializeComponent();
            IO = true;

            StatesList = Enum.GetValues(typeof(ScanState)).Cast<ScanState>().ToList();
            States.ItemsSource = StatesList;

            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                dataGridDITags.ItemsSource = context.DIs.ToList();
                dataGridAITags.ItemsSource = context.AIs.ToList();
            }
        }

        private void MenuItemViewInputs_Click(object sender, RoutedEventArgs e)
        {
            // TODO change source of dataGridTags to display inputs
            IO = true;

            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                dataGridDITags.ItemsSource = context.DIs.ToList();
                dataGridAITags.ItemsSource = context.AIs.ToList();
            }
        }

        private void MenuItemViewOutputs_Click(object sender, RoutedEventArgs e)
        {
            // TODO change source of dataGridTags to display outputs
            IO = false;

            using (DBModel.IOContext context = new DBModel.IOContext())
            {
                dataGridDITags.ItemsSource = context.DOs.ToList();
                dataGridAITags.ItemsSource = context.AOs.ToList();

            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Application.Current.MainWindow.Show();
        }

        private void Window_Activated(object sender, EventArgs e)
        {

        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void dataGridTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Closes the most current open window
            for(int i=0; i<Application.Current.Windows.Count-1; i++)
            {
                if (Application.Current.Windows[i].GetType() == typeof(AlarmsWindow))
                {
                    Application.Current.Windows[i].Close();
                }
            }
            // Application.Current.Windows[Application.Current.Windows.Count - 1].Close();
            if (IO)
            {
                // TODO check if there is an item in selected row
                var itemD = dataGridDITags.SelectedItem;
                var itemA = dataGridAITags.SelectedItem;
                AlarmsWindow alarms;
                if (itemD != null)
                {
                    alarms = new AlarmsWindow(itemD);
                    alarms.Show();
                    this.Focus();
                }
                else if (itemA != null)
                {
                    alarms = new AlarmsWindow(itemA);
                    alarms.Show();
                    this.Focus();
                }
            }

           
           
           
        }

        private void dataGridDITags_LostFocus(object sender, RoutedEventArgs e)
        {
           

        }

        private void dataGridDITags_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            
        }

        private void MenuItemCreate_Click(object sender, RoutedEventArgs e)
        {
            TagDetails tag = new TagDetails();
            tag.ShowDialog();
        }
    }
}
