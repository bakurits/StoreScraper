﻿using System;
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

            shippingAddress_country.SelectedValue = Countries.UnitedStated;
            billingAddress_country.SelectedValue = Countries.UnitedStated;
            shippingAddress_state.SelectedValue = States.Alabama;
            billingAddress_state.SelectedValue = States.Alabama;

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

        private void handleVisaSelect(object sender, RoutedEventArgs e)
        {
            mastercardCheckBox.IsChecked = false;
            americanexpressCheckBox.IsChecked = false;
        }

        private void handleMastercardSelect(object sender, RoutedEventArgs e)
        {
            visaCheckBox.IsChecked = false;
            americanexpressCheckBox.IsChecked = false;
        }

        private void handleAmericanexpressSelect(object sender, RoutedEventArgs e)
        {
            mastercardCheckBox.IsChecked = false;
            visaCheckBox.IsChecked = false;
        }

        private void handleShippingCountrySelect(object sender, RoutedEventArgs e)
        {
            if (shippingAddress_country.SelectedValue == null)
            {
                return;
            }

            Countries shippingCountry;
            Enum.TryParse<Countries>(shippingAddress_country.SelectedValue.ToString(), out shippingCountry);
            
            if (shippingCountry != Countries.UnitedStated)
            {
                shippingState.Visibility = Visibility.Hidden;
            }
            else
            {
                shippingState.Visibility = Visibility.Visible;
            }
        }

        private void handleBillingCountrySelect(object sender, RoutedEventArgs e)
        {
            if (billingAddress_country.SelectedValue == null)
            {
                return;
            }

            Countries shippingCountry;
            Enum.TryParse<Countries>(billingAddress_country.SelectedValue.ToString(), out shippingCountry);

            if (shippingCountry != Countries.UnitedStated)
            {
                billingState.Visibility = Visibility.Hidden;
            }
            else
            {
                billingState.Visibility = Visibility.Visible;
            }
        }


        private void addProfile(object sender, RoutedEventArgs e)
        {

            Countries shippingCountry;
            Enum.TryParse<Countries>(shippingAddress_country.SelectedValue.ToString(), out shippingCountry);
            Countries billingCountry;
            Enum.TryParse<Countries>(billingAddress_country.SelectedValue.ToString(), out billingCountry);


            
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

            States shipping_state;
            if (shippingState.Visibility != Visibility.Hidden)
            {
                Enum.TryParse<States>(shippingAddress_state.SelectedValue.ToString(), out shipping_state);
                shippingAddress.State = shipping_state;
            }
            States billing_state;
            if (billingState.Visibility != Visibility.Hidden)
            {
                Enum.TryParse<States>(billingAddress_state.SelectedValue.ToString(), out billing_state);
                billingAddress.State = billing_state;
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

            var creditCard = new CardInfo()
            {
                CartNumber = cardNumber.Text,
                CSC = cardCSC.Text,
                ValidUntil = new DateTime(int.Parse(cardExpYear.Text), int.Parse(cardExpMonth.Text), 1),
                CardHolderName = cardHolder.Text,
                Type = ccType
            };

            Profile profile = new Profile()
            {
                Name = profileName.Text,
                ShippingAddress = shippingAddress,
                BillingAddress = billingAddress,
                CreditCard = creditCard,
                ListShippingName = shippingAddress.FirstName + " " + shippingAddress.LastName,
                ListCardLastDigits = "**** " + creditCard.CartNumber.Substring(creditCard.CartNumber.Length - 4),
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


}
