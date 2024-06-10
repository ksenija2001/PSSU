using Microsoft.Xaml.Behaviors.Layout;
using OxyPlot;
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
    /// Interaction logic for TagDetails.xaml
    /// </summary>
    public partial class TagDetails : Window
    {
        public TagDetails()
        {
            InitializeComponent();

            txtLow.IsEnabled = false;
            txtHigh.IsEnabled = false;
            txtUnits.IsEnabled = false;

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Museum museum = new Museum();

                //museum.Name = txtName.Text.Trim();
                //museum.Type = txtType.Text.Trim();
                //string city = txtCity.Text.Trim();


                //int n = 0;
                //museum.Id = int.TryParse(txtID.Text.Trim(), out n) ? n : -1;
                //float k = 0;

                //museum.Price = float.TryParse(txtPrice.Text.Trim(), out k) ? k : -1;
                //museum.Visitors = float.TryParse(txtVisitors.Text.Trim(), out k) ? k : -1;
                //museum.Rating = float.TryParse(txtRating.Text.Trim(), out k) ? k : -1;


                //using (MuseumContext context = new MuseumContext())
                //{
                //    var id = int.Parse(context.Cities.Where(c => c.Name == city).Select(c => c.Id).FirstOrDefault().ToString());
                //    if (id == 0)
                //    {
                //        MessageBox.Show("City doesn't exist");
                //        return;
                //    }

                //    museum.CityId = id;

                //    if (Mode == "Create")
                //        MuseumHandler.Create(context, museum);
                //    else if (Mode == "Update")
                //        MuseumHandler.Update(context, Museum.Id, museum);

                // }


                this.Close();
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

        private void rbDI_Checked(object sender, RoutedEventArgs e)
        {
            if (txtLow != null)
            {
                txtLow.IsEnabled = false;
                txtHigh.IsEnabled = false;
                txtUnits.IsEnabled = false;
            }
           
        }

        private void rbAI_Checked(object sender, RoutedEventArgs e)
        {
            txtLow.IsEnabled = true;
            txtHigh.IsEnabled = true;
            txtUnits.IsEnabled = true;
        }
    }
}
