using DataConcentrator;
using Microsoft.Xaml.Behaviors.Layout;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
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
        private string Mode {  get; set; }
        public TagDetails(string mode)
        {
            InitializeComponent();

            cmbAddress.ItemsSource = new List<string>
            {
                "ADDR001",
                "ADDR002",
                "ADDR003",
                "ADDR004",
            };

            txtLow.IsEnabled = false;
            txtHigh.IsEnabled = false;
            txtUnits.IsEnabled = false;
            Mode = mode;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rbDI.IsChecked == true)
                {
                    DBModel.DI tag = new DBModel.DI();
                    tag.Name = txtName.Text.Trim();
                    tag.Description = txtDescription.Text.Trim();
                    tag.IOAddress = cmbAddress.Text.Trim();
                    tag.Connected = (ckbConnected.IsChecked == true) ? (byte)1 : (byte)0;
                    tag.ScanTime = double.Parse(txtScanTime.Text.Trim());
                    tag.ScanState = (ckbScanState.IsChecked == true) ? (byte)1 : (byte)0;
                    tag.Alarms = new List<DBModel.Alarm>();

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        if (Mode == "Create")
                            DBTagHandler.Create(context, tag);
                        else if (Mode == "Update")
                            DBTagHandler.Update(context, tag);
                    }
                }
                else if (rbAI.IsChecked == true)
                {
                    DBModel.AI tag = new DBModel.AI();
                    tag.Name = txtName.Text.Trim();
                    tag.Description = txtDescription.Text.Trim();
                    tag.IOAddress = cmbAddress.Text.Trim();
                    tag.Connected = (ckbConnected.IsChecked == true) ? (byte)1 : (byte)0;
                    tag.ScanTime = double.Parse(txtScanTime.Text.Trim());
                    tag.ScanState = (ckbScanState.IsChecked == true) ? (byte)1 : (byte)0;
                    tag.LowLimit = double.Parse(txtLow.Text.Trim());
                    tag.HighLimit = double.Parse(txtHigh.Text.Trim());
                    tag.Units = txtUnits.Text.Trim();
                    tag.Alarms = new List<DBModel.Alarm>();

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        if (Mode == "Create")
                            DBTagHandler.Create(context, tag);
                        else if (Mode == "Update")
                            DBTagHandler.Update(context, tag);
                    }
                }
                else
                {
                    MessageBox.Show("Type of input not selected");
                    return;
                }

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
