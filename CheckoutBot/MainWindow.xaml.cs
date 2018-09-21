using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CheckoutBot.CheckoutBots.FootSites;
using CheckoutBot.CheckoutBots.FootSites.EastBay;
using CheckoutBot.Core;
using CheckoutBot.Interfaces;
using CheckoutBot.Models.Shipping;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models;
using CheckoutBot.Models.Checkout;
using Newtonsoft.Json;
using EO;
using EO.WebBrowser;
using StoreScraper.Core;
using StoreScraper.Helpers;

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


            loadingBox.Visibility = Visibility.Visible;
            activeArea.Visibility = Visibility.Hidden;

            foreach (var bot in AppData.AvailableBots)
            {
                cbx_Websites.Items.Add(bot);
            }


            ReleasedProductsMonitor.Default = new ReleasedProductsMonitor();
            tasksList.ItemsSource = new List<CheckoutTask>();
            Logger.Instance.OnLogged += (message, color) => tbx_Log.AppendText(message, color);

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

            tasksList.ItemsSource = AppData.Session.CurrentTasks;

        }

        async Task PutTaskDelay()
        {
            await Task.Delay(5000);
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            await PutTaskDelay();
            loadingBox.Visibility = Visibility.Hidden;
            activeArea.Visibility = Visibility.Visible;

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


        private void AddProxy(object sender, RoutedEventArgs e)
        {
 
            proxies.Items.Add( new ProxyItem() 
            {
                Proxy = proxy.Text
            });

            var proxyList = proxy.Text.Split('\n').Select(txt => new WebProxy(txt.Trim())).ToList();
            foreach (var site in AppData.AvailableBots)
            {
                if (site is IProxyChecker checker)
                {
                    AppData.Session.ParsedProxies[site] = checker.ChooseBestProxies(proxyList, proxyList.Count / 2);
                }
                else
                {
                    AppData.Session.ParsedProxies[site] = proxyList;
                }
            }

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(tbx_Quantity.Text, out int quantity);
            if (quantity <= 0)
            {
                MessageBox.Show("Incorrect Quantity typed","Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            AccountCheckoutSettings settings;

            try
            {
                settings = new AccountCheckoutSettings()
                {
                    UserLogin = tbx_UserName.Text,
                    UserPassword = tbx_Password.Text,
                    ProductToBuy = (FootsitesProduct)cbx_Products.SelectedValue,
                    BuyOptions = new ProductBuyOptions()
                    {
                        Quantity = quantity,
                        Size = cbx_Size.SelectedValue.ToString()
                    },
                    UserCcv2 = tbx_CCV2.Text
                };
            }
            catch (Exception)
            {
                MessageBox.Show("All required field is not filled correctly", "Error while adding task",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CheckoutTask task = new CheckoutTask()
            {
                CheckoutInfo = settings,
                MonitoringTokenSource =
                    CancellationTokenSource.CreateLinkedTokenSource(AppData.ApplicationGlobalTokenSource.Token),
            };

            AppData.Session.CurrentTasks.Add(task);
            task.StartMonitoring();

            MessageBox.Show("Checkout Task Added","Success", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
        }

        private void cbx_Websites_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbx_Products.IsEnabled = false;
            if(cbx_Websites.SelectedValue == null) return;

            cbx_Products.Items.Clear();

            var curStore = (FootSitesBotBase)cbx_Websites.SelectedValue;
            try
            {
                var lst = ReleasedProductsMonitor.Default.GetProductsList(curStore);
                foreach (var product in lst)
                {
                    cbx_Products.Items.Add(product);
                }
                cbx_Products.IsEnabled = true;
            }
            catch (Exception)
            {
                //ignored
            }
        }

        private void cbx_Products_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbx_Size.IsEnabled = false;
            if(cbx_Products.SelectedValue == null) return;
            cbx_Size.Items.Clear();

            if (cbx_Websites.SelectedValue is EastBayBot bot)
            {
                if (cbx_Products.SelectedValue is FootsitesProduct product)
                {
                    try
                    {
                        bot.GetProductSizes(product, CancellationToken.None);
                        if (product.ImageUrl != null)
                        {
                            BitmapImage bImage = new BitmapImage(new Uri(product.ImageUrl));
                            img_Product.Source = bImage;
                        }

                        txt_Prce.Content = product.Price + product.Currency;
                        txt_Status.Content = product.ReleaseTime != null && product.ReleaseTime > DateTime.UtcNow
                            ? $"Will Be Released on {product.ReleaseTime.Value.ToLocalTime():G}"
                            : "Already Released";
                        lnk_ProductUrl.NavigateUri = new Uri(product.Url);
                        Run run = new Run("Open");
                        lnk_ProductUrl.Inlines.Clear();
                        lnk_ProductUrl.Inlines.Add(run);
                        product.Sizes.ForEach(size => cbx_Size.Items.Add(size));
                        cbx_Size.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        //ignored
                    }
                }
            }
        }

        private void lnk_ProductUrl_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }


        private void btn_DeleteTask_OnClick(object sender, RoutedEventArgs e)
        {
            var index = tasksList.SelectedIndex;
            AppData.Session.CurrentTasks[index].MonitoringTokenSource.Cancel();
            AppData.Session.CurrentTasks.RemoveAt(index);
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

    public class ProxyItem
    {
        public string Proxy { get; set; }
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