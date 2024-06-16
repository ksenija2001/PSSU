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
using System.Xml.Linq;
using DataConcentrator;

namespace ScadaGUI
{
    /// <summary>
    /// Interaction logic for AlarmDetails.xaml
    /// </summary>
    /// 
    public partial class AlarmDetails : Window
    {
        private Type tagType {  get; set; }
        private string tagName { get; set; }

        public event EventHandler DataChanged;

        public AlarmDetails(string tagName, Type tagType)
        {
            this.tagType = tagType;
            this.tagName = tagName;

            InitializeComponent();

            if (tagType == typeof(DBModel.DI))
            {
                txtValue.IsEnabled = false;
                cmbActivate.ItemsSource = new List<ActiveWhen>() { ActiveWhen.EQUALS };

            }
            else
            {
                ckbValue.IsEnabled = false;
                cmbActivate.ItemsSource = new List<ActiveWhen>() { ActiveWhen.BELOW, ActiveWhen.ABOVE };

            }
        }

        protected virtual void OnDataChanged(EventArgs e)
        {
            EventHandler eh = DataChanged;
            if (eh != null)
            {
                eh(this, e);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                try
                {
                    DBModel.Alarm alarm = new DBModel.Alarm();
                    alarm.Activate = (ActiveWhen)cmbActivate.SelectedValue;
                    alarm.Message = txtMessage.Text.Trim();
                    alarm.TagId = tagName;

                    DBModel.Tag tag;

                    using (DBModel.IOContext context = new DBModel.IOContext())
                    {
                        List<DBModel.Alarm> alarms = context.Set<DBModel.Alarm>().ToList();
                        if (alarms.Count > 0)
                            alarm.Id = alarms.Max(n => n.Id) + 1;
                        else
                            alarm.Id = 1;

                        if (tagType == typeof(DBModel.DI))
                        {
                            tag = DBTagHandler.FindTag<DBModel.DI>(context, tagName); 
                            alarm.Value = (ckbValue.IsChecked == true) ? (byte)1 : (byte)0;
                            DBTagAlarmHandler.Create(context, alarm);
                            List<DBModel.Alarm> al = ((DBModel.DI)tag).Alarms;
                            al.Add(alarm);
                            DBTagHandler.UpdateTag(context, tagName, "Alarms", al, (DBModel.DI)tag);
                            OnDataChanged(null);
                        }
                        else
                        {
                            tag = DBTagHandler.FindTag<DBModel.AI>(context, tagName); 
                            alarm.Value = double.Parse(txtValue.Text.Trim());
                            DBTagAlarmHandler.Create(context, alarm);
                            List<DBModel.Alarm> al = ((DBModel.AI)tag).Alarms;
                            al.Add(alarm);
                            DBTagHandler.UpdateTag(context, tagName, "Alarms", al, (DBModel.AI)tag);
                            OnDataChanged(null);
                        }
                    }


                    ClearAllControls();
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
        }

        private void ClearAllControls()
        {
            foreach (var child in TextBoxPanel.Children)
            {
                switch (child)
                {
                    case CheckBox cb:
                        cb.IsChecked = false;
                        break;
                    case ComboBox cb:
                        cb.SelectedIndex = -1;
                        break;
                    case TextBox txt:
                        txt.Text = "";
                        break;
                    case DockPanel dp:
                        foreach (var c in dp.Children)
                        {
                            switch (c)
                            {
                                case CheckBox cb:
                                    cb.IsChecked = false;
                                    break;
                                case ComboBox cb:
                                    cb.SelectedIndex = -1;
                                    break;
                                case TextBox txt:
                                    txt.Text = "";
                                    break;
                            }
                        }
                        break;
                }
            }
        }

        private bool ValidateInput()
        {

            bool valid = true;

            //checking common fields
            if (IsEmptyField(txtMessage))
            {
                valid = false;
            }

            if (tagType == typeof(DBModel.AI))
            {
                if (IsEmptyField(txtValue) | NumberCheck(txtValue))
                    valid = false;
            }
            

            if (cmbActivate.SelectedIndex == -1)
            {
                cmbActivate.Background = cmbActivate.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB39DDB");
                cmbActivate.ToolTip = "Nothing selected!";
                valid = false;
            }
            else
            {
                cmbActivate.ClearValue(Border.BackgroundProperty);
            }

            return valid;
        }

        private bool IsEmptyField(TextBox Box)
        {

            if (String.IsNullOrEmpty(Box.Text))
            {
                Box.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB39DDB");
                Box.ToolTip = Box.Name.Replace("txt", "") + " field mustn't be empty!";
                return true;
            }

            Box.ClearValue(Border.BackgroundProperty);
            return false;
        }

        private bool NumberCheck(TextBox Box)
        {
            double parsed_value;

            if (Double.TryParse(Box.Text, out parsed_value))
            {
                if (parsed_value <= 0)
                {
                    Box.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB39DDB");
                    Box.ToolTip = Box.Name.Replace("txt", "") + " field must be greater than zero!";
                    return true;
                }
            }
            else
            {
                Box.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFB39DDB");
                Box.ToolTip = "Invalid input!";
                return true;
            }

            Box.ClearValue(Border.BackgroundProperty);
            return false;

        }

        private void BackgroundClear()
        {
            foreach (UIElement Control in TextBoxPanel.Children)
            {
                if (Control.GetType() == typeof(DockPanel))
                {
                    foreach (UIElement DockChild in ((DockPanel)(Control)).Children)
                    {
                        DockChild.ClearValue(Border.BackgroundProperty);
                    }
                }
                Control.ClearValue(Border.BackgroundProperty);
            }
        }

        private void On_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Save_Click(sender, e);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }
    }
}
