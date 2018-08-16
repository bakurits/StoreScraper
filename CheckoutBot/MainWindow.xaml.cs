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

            List<TaskItem> items = new List<TaskItem>();
            items.Add(new TaskItem() { Keywords = "nike air", Size = 12, Retries = "1", Status="Checking out", ListImage="/images/list_progress.png" });
            items.Add(new TaskItem() { Keywords = "adidas", Size = 7, Retries = "3", Status = "Error", ListImage = "/images/list_error.png" });
            items.Add(new TaskItem() { Keywords = "puma", Size = 8, Retries = "0", Status = "Done", ListImage = "/images/list_done.png" });
            tasksList.ItemsSource = items;

            List<ProfileItem> profiles = new List<ProfileItem>();
            profiles.Add(new ProfileItem() { ProfileName = "steve1", Name = "Steve Vue", CreditCard = "**** 0005", Date = "08/16/2018" });
            profiles.Add(new ProfileItem() { ProfileName = "steve2", Name = "Steve Vue", CreditCard = "**** 1025", Date = "08/16/2018"  });
            profiles.Add(new ProfileItem() { ProfileName = "steve3", Name = "Steve Vue", CreditCard = "**** 1234", Date = "08/16/2018" });
            profileList.ItemsSource = profiles;

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

      
    }

    public class TaskItem
    {
        public string Keywords { get; set; }

        public int Size { get; set; }

        public string Retries { get; set; }
        public string Status { get; set; }
        public string ListImage { get; set; }

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
