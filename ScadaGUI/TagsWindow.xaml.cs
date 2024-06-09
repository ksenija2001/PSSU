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
    /// Interaction logic for TagsWindow.xaml
    /// </summary>
    public partial class TagsWindow : Window
    {
        private bool IO {  get; set; }
        public TagsWindow()
        {
            InitializeComponent();
        }

        private void MenuItemViewInputs_Click(object sender, RoutedEventArgs e)
        {
            // TODO change source of dataGridTags to display inputs
            IO = true;
        }

        private void MenuItemViewOutputs_Click(object sender, RoutedEventArgs e)
        {
            // TODO change source of dataGridTags to display outputs
            IO = false;
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

        private void dataGridTags_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {

        }

        private void dataGridTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Closes the most current open window
            Application.Current.Windows[Application.Current.Windows.Count - 1].Close();
            if (IO)
            {
                // TODO check if there is an item in selected row
                var item = dataGridTags.SelectedItem;
                AlarmsWindow alarms = new AlarmsWindow(item);
                alarms.Show();
            }
        }
    }
}
