using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using CheckoutBot.Models.Shipping;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models;
using Newtonsoft.Json;
using EO;
using EO.WebBrowser;
using EO.WebEngine;

namespace CheckoutBot
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ThreadRunner runner = new ThreadRunner();
            var webView = runner.CreateWebView(new BrowserOptions());

            webView.Engine.Options.BypassUserGestureCheck = true;
            

            List<TaskItem> items = new List<TaskItem>();
            items.Add(new TaskItem() { Keywords = "nike air", Size = 12, Retries = "1", Status="Checking out", ListImage="/images/list_progress.png" });
            items.Add(new TaskItem() { Keywords = "adidas", Size = 7, Retries = "3", Status = "Error", ListImage = "/images/list_error.png" });
            items.Add(new TaskItem() { Keywords = "puma", Size = 8, Retries = "0", Status = "Done", ListImage = "/images/list_done.png" });
            tasksList.ItemsSource = items;


            List<TaskItem> successfulItems = new List<TaskItem>
            {
                new TaskItem()
                {
                    Keywords = "nike air",
                    Size = 12,
                    Retries = "1",
                    Status = "Done",
                    ListImage = "/images/list_done.png"
                },
                new TaskItem()
                {
                    Keywords = "adidas",
                    Size = 7,
                    Retries = "3",
                    Status = "Done",
                    ListImage = "/images/list_done.png"
                },
                new TaskItem()
                {
                    Keywords = "puma",
                    Size = 8,
                    Retries = "0",
                    Status = "Done",
                    ListImage = "/images/list_done.png"
                }
            };
            successfulTaks.ItemsSource = successfulItems;
            List<TokenItem> tokenItems = new List<TokenItem>
            {
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"},
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"},
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"},
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"},
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"},
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"},
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"},
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"},
                new TokenItem() {Site = "http://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub"}
            };
            tokens.ItemsSource = tokenItems;

            foreach (var item in Enum.GetValues(typeof(Countries)))
            {
                shippingAddress_country.Items.Add(item);
                billingAddress_country.Items.Add(item);

            }

            foreach (var item in Enum.GetValues(typeof(States)))
            {
                shippingAddress_state.Items.Add(item);
                billingAddress_state.Items.Add(item);

            }

            shippingAddress_country.SelectedValue = Countries.UnitedStated;
            billingAddress_country.SelectedValue = Countries.UnitedStated;
            shippingAddress_state.SelectedValue = States.Alabama;
            billingAddress_state.SelectedValue = States.Alabama;

        }


        private void HandleAccountType(object sender, RoutedEventArgs e)
        {
            accComboBox.Visibility = userRadioBtn.IsChecked == true ? Visibility.Visible : Visibility.Hidden;
        }

        private void HandleVisaSelect(object sender, RoutedEventArgs e)
        {
            mastercardCheckBox.IsChecked = false;
            americanexpressCheckBox.IsChecked = false;
        }

        private void HandleMastercardSelect(object sender, RoutedEventArgs e)
        {
            visaCheckBox.IsChecked = false;
            americanexpressCheckBox.IsChecked = false;
        }

        private void HandleAmericanExpressSelect(object sender, RoutedEventArgs e)
        {
            mastercardCheckBox.IsChecked = false;
            visaCheckBox.IsChecked = false;
        }

        private void HandleShippingCountrySelect(object sender, RoutedEventArgs e)
        {
            if (shippingAddress_country.SelectedValue == null)
            {
                return;
            }

            Enum.TryParse<Countries>(shippingAddress_country.SelectedValue.ToString(), out var shippingCountry);
            
            shippingState.Visibility = shippingCountry != Countries.UnitedStated ? Visibility.Hidden : Visibility.Visible;
        }

        private void HandleBillingCountrySelect(object sender, RoutedEventArgs e)
        {
            if (billingAddress_country.SelectedValue == null)
            {
                return;
            }

            Enum.TryParse<Countries>(billingAddress_country.SelectedValue.ToString(), out var shippingCountry);

            billingState.Visibility = shippingCountry != Countries.UnitedStated ? Visibility.Hidden : Visibility.Visible;
        }


        private void ExportProfiles(object sender, RoutedEventArgs e)
        {
            string output = JsonConvert.SerializeObject(profileList.Items);
            Console.WriteLine(output);

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "profiles", DefaultExt = ".json", Filter = "JSON (.json)|*.json"
            };
            // Default file name
            // Default file extension
            // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result != true) return;
            // Save document
            string filename = dlg.FileName;
            System.IO.File.WriteAllText(filename, output);

        }

        private void ImportProfiles(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "profiles", DefaultExt = ".json", Filter = "JSON (.json)|*.json"
            };
            // Default file name
            // Default file extension
            // Filter files by extension

            // Show save file dialog box
            bool? result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result != true) return;
            // Save document
            string filename = dlg.FileName;
            string json = System.IO.File.ReadAllText(filename);

            var profiles = JsonConvert.DeserializeObject<Profile[]>(json);
            profileList.ItemsSource = profiles;


        }




        private void AddProfile(object sender, RoutedEventArgs e)
        {
            Enum.TryParse<Countries>(shippingAddress_country.SelectedValue.ToString(), out var shippingCountry);
            Enum.TryParse<Countries>(billingAddress_country.SelectedValue.ToString(), out var billingCountry);


            
            var shippingAddress = new ShippinInfo()
            {
                City = shippingAddress_city.Text,
                ZipCode = shippingAddress_zip.Text,
                AddressLine1 = shippingAddress_address1.Text,
                AddressLine2 = shippingAddress_address2.Text,
                Country = shippingCountry,
                FirstName = shippingAddress_firstName.Text,
                LastName = shippingAddress_lastName.Text
            };



            var billingAddress = new ShippinInfo()
            {
                City = billingAddress_city.Text,
                ZipCode = billingAddress_zip.Text,
                AddressLine1 = billingAddress_address1.Text,
                AddressLine2 = billingAddress_address2.Text,
                Country = billingCountry,
                FirstName = billingAddress_firstName.Text,
                LastName = billingAddress_lastName.Text
            };

            if (shippingState.Visibility != Visibility.Hidden)
            {
                Enum.TryParse<States>(shippingAddress_state.SelectedValue.ToString(), out var state);
                shippingAddress.State = state;
            }

            if (billingState.Visibility != Visibility.Hidden)
            {
                Enum.TryParse<States>(billingAddress_state.SelectedValue.ToString(), out var state);
                billingAddress.State = state;
            }


            CardType ccType = CardType.Visa;

            if (visaCheckBox.IsChecked == true)
            {
                ccType = CardType.Visa;
            }else if (mastercardCheckBox.IsChecked == true)
            {
                ccType = CardType.MaterCard;
            }
            else if (americanexpressCheckBox.IsChecked == true)
            {
                ccType = CardType.AmericanExpress;
            }

            var creditCard = new Card()
            {
                Id = cardNumber.Text,
                CSC = cardCSC.Text,
                ValidUntil = new DateTime(int.Parse(cardExpYear.Text), int.Parse(cardExpMonth.Text), 1),
                CardHolderName = cardHolder.Text,
                TypeOfCard = ccType
            };

            Profile profile = new Profile()
            {
                Name = profileName.Text,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                CreditCard = creditCard,
                DateCreated =  DateTime.Now
     
        };

            profileList.Items.Add(profile);
        }

    }



    public class TaskItem
    {
        public string Keywords { get; set; }

        public int Size { get; set; }

        public string Retries { get; set; }
        public string Status { get; set; }
        public string ListImage { get; set; }

    }


    public class TokenItem
    {
        public string Site { get; set; }
        public string Token { get; set; }



    }


    public sealed class CreditCard4DigitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "";
            return value.ToString().Length < 4 ? value.ToString() : "**** " + value.ToString().Substring(value.ToString().Length - 4);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented yet!");
        }
    }


    public sealed class ShippingNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return "";
            ShippinInfo address = (ShippinInfo)value;
            return address.FirstName + " " + address.LastName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("Not implemented yet!");
        }
    }



}
