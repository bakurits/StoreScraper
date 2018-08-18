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
using CheckoutBot.Models.Shipping;
using CheckoutBot.Models.Payment;
using CheckoutBot.Models;

namespace CheckoutBot
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<Profile> profiles = new List<Profile>();


        public MainWindow()
        {
            InitializeComponent();

            List<TaskItem> items = new List<TaskItem>();
            items.Add(new TaskItem() { Keywords = "nike air", Size = 12, Retries = "1", Status="Checking out", ListImage="/images/list_progress.png" });
            items.Add(new TaskItem() { Keywords = "adidas", Size = 7, Retries = "3", Status = "Error", ListImage = "/images/list_error.png" });
            items.Add(new TaskItem() { Keywords = "puma", Size = 8, Retries = "0", Status = "Done", ListImage = "/images/list_done.png" });
            tasksList.ItemsSource = items;

           /* List<ProfileItem> profiles = new List<ProfileItem>();
            profiles.Add(new ProfileItem() { ProfileName = "steve1", Name = "Steve Vue", CreditCard = "**** 0005", Date = "08/16/2018" });
            profiles.Add(new ProfileItem() { ProfileName = "steve2", Name = "Steve Vue", CreditCard = "**** 1025", Date = "08/16/2018"  });
            profiles.Add(new ProfileItem() { ProfileName = "steve3", Name = "Steve Vue", CreditCard = "**** 1234", Date = "08/16/2018" });
            profileList.ItemsSource = profiles;*/

            List<TaskItem> successfulItems = new List<TaskItem>();
            successfulItems.Add(new TaskItem() { Keywords = "nike air", Size = 12, Retries = "1", Status = "Done", ListImage = "/images/list_done.png" });
            successfulItems.Add(new TaskItem() { Keywords = "adidas", Size = 7, Retries = "3", Status = "Done", ListImage = "/images/list_done.png" });
            successfulItems.Add(new TaskItem() { Keywords = "puma", Size = 8, Retries = "0", Status = "Done", ListImage = "/images/list_done.png" });
            successfulTaks.ItemsSource = successfulItems;
            List<TokenItem> tokenItems = new List<TokenItem>();
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
            tokenItems.Add(new TokenItem() { Site = "https://footlocker.com", Token = "grIUWHSUHA:sadiajsw98equwSNAsamcnasub" });
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


        }


        private void handleAccountType(object sender, RoutedEventArgs e)
        {
            if (userRadioBtn.IsChecked == true)
            {
                accComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                accComboBox.Visibility = Visibility.Hidden;
            }
        }

        private void addProfile(object sender, RoutedEventArgs e)
        {

            Countries shippingCountry;
            Enum.TryParse<Countries>(shippingAddress_country.SelectedValue.ToString(), out shippingCountry);
            Countries billingCountry;
            Enum.TryParse<Countries>(billingAddress_country.SelectedValue.ToString(), out billingCountry);


            States shippingState;
            Enum.TryParse<States>(shippingAddress_state.SelectedValue.ToString(), out shippingState);
            States billingState;
            Enum.TryParse<States>(billingAddress_state.SelectedValue.ToString(), out billingState);


            var shippingAddress = new ShippinInfo()
            {
                City = shippingAddress_city.Text,
                ZipCode = shippingAddress_zip.Text,
                AddressLine1 = shippingAddress_address1.Text,
                AddressLine2 = shippingAddress_address2.Text,
                Country = shippingCountry,
                State = shippingState
            };

            var billingAddress = new ShippinInfo()
            {
                City = billingAddress_city.Text,
                ZipCode = billingAddress_zip.Text,
                AddressLine1 = billingAddress_address1.Text,
                AddressLine2 = billingAddress_address2.Text,
                Country = billingCountry,
                State = billingState
            };


            var creditCard = new CardInfo()
            {
                CartNumber = cardNumber.Text,
                CSC = cardCSC.Text,
                ValidUntil = new DateTime(int.Parse(cardExpYear.Text), int.Parse(cardExpMonth.Text), 1),
                CardHolderName = cardHolder.Text
            };

            Profile profile = new Profile()
            {
                Name = profileName.Text,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                CreditCard = creditCard,
                ListShippingName = shippingAddress_firstName.Text,
                ListCardLastDigits = "**** " + creditCard.CartNumber.Substring(creditCard.CartNumber.Length - 4),
                DateCreated = new DateTime()

        };

            profiles.Add(profile);

            profileList.ItemsSource = profiles;
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


    public class AddressContainer
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string ZIP { get; set; }
        public string Apartment { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
    }

    public class CardContainer
    {
        public string Number { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string CSC { get; set; }
    }

    public class ProfileItem
    {
        public string ProfileName { get; set; }
        public string Name { get; set; }
        public string CreditCard { get; set; }
        public string Date { get; set; }
    }


    public class TokenItem
    {
        public string Site { get; set; }
        public string Token { get; set; }



    }


}
