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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Series;


namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void Window_Activated(object sender, EventArgs e)
        {
           
        }

        private void TagsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TagsWindow tags = new TagsWindow();
            this.Hide();
            tags.ShowDialog();
        }

        private void AlarmsMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }


    // TO FIX
    public class MainViewModel {
        public MainViewModel() {
            this.MyModel = new PlotModel { Title = "" };
            this.MyModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1, "cos(x)"));
        }

        public PlotModel MyModel { get; private set; }
    }

   
}
