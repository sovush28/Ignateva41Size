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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ignateva41Size
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        public ProductPage(User user)
        {
            InitializeComponent();

            if (user!= null)
            {
                // в БД уже в каждом поле фамилии в конце пробел
                FIOTextBlock.Text = user.UserSurname + user.UserName + " " + user.UserPatronymic;

                switch (user.UserRole)
                {
                    case 1:
                        RoleTextBlock.Text = "Клиент"; break;
                    case 2:
                        RoleTextBlock.Text = "Менеджер"; break;
                    case 3:
                        RoleTextBlock.Text = "Администратор"; break;
                }
            }
            else
            {
                YouAuthAsText.Visibility = Visibility.Hidden;
                FIOTextBlock.Visibility = Visibility.Hidden;
                RoleTextBlock.Text = "Гость";
            }

            var currentProducts = Ignateva41Entities.GetContext().Product.ToList();
            ProductListView.ItemsSource = currentProducts;

            ComboDiscount.SelectedIndex = 0;
            UpdateProducts();
        }

        private void UpdateProducts()
        {
            var currentProducts = Ignateva41Entities.GetContext().Product.ToList();

            RecordsTotalText.Text= currentProducts.Count().ToString();

            switch (ComboDiscount.SelectedIndex)
            {
                case 0:
                    currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 0 &&
                    Convert.ToDouble(p.ProductDiscountAmount) <= 100)).ToList();
                    break;

                case 1:
                    currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 0 &&
                    Convert.ToDouble(p.ProductDiscountAmount) <= 9.99)).ToList();
                    break;

                case 2:
                    currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 10 &&
                    Convert.ToDouble(p.ProductDiscountAmount) <= 14.99)).ToList();
                    break;

                case 3:
                    currentProducts = currentProducts.Where(p => (Convert.ToDouble(p.ProductDiscountAmount) >= 15 &&
                    Convert.ToDouble(p.ProductDiscountAmount) <= 100)).ToList();
                    break;
            }

            currentProducts = currentProducts.Where(p => p.ProductName.ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            if (RButtonAsc.IsChecked.Value)
                currentProducts = currentProducts.OrderBy(p => p.ProductCost).ToList();

            if (RButtonDesc.IsChecked.Value)
                currentProducts = currentProducts.OrderByDescending(p => p.ProductCost).ToList();

            RecordsShownText.Text = currentProducts.Count().ToString();

            ProductListView.ItemsSource = currentProducts;
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void ComboDiscount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonAsc_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }

        private void RButtonDesc_Checked(object sender, RoutedEventArgs e)
        {
            UpdateProducts();
        }
    }
}
