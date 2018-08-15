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
            items.Add(new TaskItem() { Keywords = "nike air", Size = 12, Retries = "1", Status="Checking out" });
            items.Add(new TaskItem() { Keywords = "adidas", Size = 7, Retries = "3", Status = "Error" });
            items.Add(new TaskItem() { Keywords = "puma", Size = 8, Retries = "0", Status = "Done" });
            tasksList.ItemsSource = items;

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

    }

}
