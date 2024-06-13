﻿using DataConcentrator;
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
    public partial class TagDetails : Window {
        public TagDetails() {
            InitializeComponent();

            cmbAddress.ItemsSource = PLCDataHandler.PLCData.Keys.Skip(8).Take(4);

            txtLow.IsEnabled = false;
            txtHigh.IsEnabled = false;
            txtUnits.IsEnabled = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e) {
            if (ValidateInput()) {
                try {
                    if (rbDI.IsChecked == true) {
                        DBModel.DI tag = new DBModel.DI();
                        tag.Name = txtName.Text.Trim();
                        tag.Description = txtDescription.Text.Trim();
                        tag.IOAddress = cmbAddress.Text.Trim();
                        tag.Connected = (ckbConnected.IsChecked == true) ? (byte)1 : (byte)0;
                        tag.ScanTime = double.Parse(txtScanTime.Text.Trim());
                        tag.ScanState = (ckbScanState.IsChecked == true) ? (byte)1 : (byte)0;
                        tag.Alarms = new List<DBModel.Alarm>();

                        using (DBModel.IOContext context = new DBModel.IOContext()) {
                            DBTagHandler.Create(context, tag);
                        }
                    }
                    else if (rbAI.IsChecked == true) {
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

                        using (DBModel.IOContext context = new DBModel.IOContext()) {
                                DBTagHandler.Create(context, tag);
                        }
                    }
                    else {
                        MessageBox.Show("Type of input not selected");
                        return;
                    }

                    this.Close();
                }
                catch (DbEntityValidationException ex) {
                    foreach (var errors in ex.EntityValidationErrors) {
                        foreach (var validationError in errors.ValidationErrors) {
                            MessageBox.Show($"Wrong Format for {validationError.PropertyName}: {validationError.ErrorMessage}");

                        }
                    }
                }
                catch (FormatException ex) {
                    MessageBox.Show($"{ex.Message}");
                }
            }
        }

        private void rbDI_Checked(object sender, RoutedEventArgs e) {
            if (txtLow != null) {
                txtLow.IsEnabled = false;
                txtHigh.IsEnabled = false;
                txtUnits.IsEnabled = false;
            }

            if (cmbAddress != null) {
                cmbAddress.ItemsSource = PLCDataHandler.PLCData.Keys.Skip(8).Take(4);
            }

        }

        private void rbAI_Checked(object sender, RoutedEventArgs e) {
            cmbAddress.ItemsSource = PLCDataHandler.PLCData.Keys.Take(4);
            txtLow.IsEnabled = true;
            txtHigh.IsEnabled = true;
            txtUnits.IsEnabled = true;
        }

        private bool ValidateInput() {

            bool valid = true;

            //checking common fields
            if (IsEmptyField(txtName) | IsEmptyField(txtDescription)
                | NumberCheck(txtScanTime)) {
                valid = false;
            }

            if (cmbAddress.SelectedIndex == -1) {
                cmbAddress.Background = cmbAddress.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB39DDB");
                cmbAddress.ToolTip = "Nothing selected!";
                valid = false;
            }
            else {
                cmbAddress.ClearValue(Border.BackgroundProperty);
                cmbAddress.ClearValue(TextBox.ToolTipProperty);
            }

            //checking additional fields
            if (rbAI.IsChecked == true) {
                if (IsEmptyField(txtLow) | IsEmptyField(txtHigh) | IsEmptyField(txtUnits)
                    | NumberCheck(txtHigh) | NumberCheck(txtLow)) {
                    valid = false;
                }
            }
                return valid;
        }

        private bool IsEmptyField(TextBox Box) {

            if (String.IsNullOrEmpty(Box.Text)) {
                Box.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB39DDB");
                Box.ToolTip = Box.Name.Replace("txt", "") + " field mustn't be empty!";
                return true;
            }

            Box.ClearValue(Border.BackgroundProperty);
            Box.ClearValue(TextBox.ToolTipProperty);
            return false;
        }

        private bool NumberCheck(TextBox Box) {
            double parsed_value;

            if (Double.TryParse(Box.Text, out parsed_value)) {
                if ((Box != txtLow && parsed_value == 0) || parsed_value < 0 ) {
                    Box.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB39DDB");
                    Box.ToolTip = Box.Name.Replace("txt", "") + " field must be positive!";
                    return true;
                }
            }
            else {
                Box.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB39DDB");
                Box.ToolTip = "Invalid input!";
                return true;
            }

            Box.ClearValue(Border.BackgroundProperty);
            Box.ClearValue(TextBox.ToolTipProperty);
            return false;

        }

        private void rbDI_Clicked(object sender, RoutedEventArgs e) {
            BackgroundClear();
        }

        private void rbAI_Clicked(object sender, RoutedEventArgs e) {
            BackgroundClear();
        }

        private void BackgroundClear() {
            foreach (UIElement Control in TextBoxPanel.Children) {
                if (Control.GetType() == typeof(DockPanel)) {
                    foreach (UIElement DockChild in ((DockPanel)(Control)).Children) {
                        DockChild.ClearValue(Border.BackgroundProperty);
                    }
                }
                Control.ClearValue(Border.BackgroundProperty);
                Control.ClearValue(TextBox.ToolTipProperty);
            }
        }

        private void On_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                Save_Click(sender, e);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
