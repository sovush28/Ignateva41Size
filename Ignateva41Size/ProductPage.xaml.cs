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
        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>(); // ?
        List<Product> selectedProducts = new List<Product>(); // ?

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

            OrderBtn.Visibility = Visibility.Hidden;

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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ProductListView.SelectedIndex >= 0)
            {
                var prod = ProductListView.SelectedItem as Product;

                // Проверяем, есть ли товар уже в списке selectedProducts
                var existingProduct = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
                if (existingProduct != null)
                {
                    // Если товар уже есть в списке, увеличиваем его количество
                    existingProduct.Quantity++;
                }
                else
                {
                    // Если товара нет в списке, добавляем его с количеством 1
                    prod.Quantity = 1; // Устанавливаем начальное количество
                    selectedProducts.Add(prod);
                }

                // Проверяем, есть ли товар уже в списке selectedOrderProducts
                var existingOrderProduct = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == prod.ProductArticleNumber);
                if (existingOrderProduct != null)
                {
                    // Если товар уже есть в списке, увеличиваем его количество
                    existingOrderProduct.ProductCount++;
                }
                else
                {
                    // Если товара нет в списке, добавляем его с количеством 1
                    var newOrderProd = new OrderProduct
                    {
                        OrderID = GenerateOrderID(), // Генерируем уникальный ID заказа
                        ProductArticleNumber = prod.ProductArticleNumber,
                        ProductCount = 1
                    };
                    selectedOrderProducts.Add(newOrderProd);
                }

                // Показываем кнопку "View Order"
                OrderBtn.Visibility = Visibility.Visible;
            }
        }

        private int GenerateOrderID()
        {
            var maxOrderID = Ignateva41Entities.GetContext().Order.Max(o => o.OrderID);
            return maxOrderID + 1;
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedProducts = selectedProducts.Distinct().ToList();
            OrderWindow orderWindow = new OrderWindow(selectedOrderProducts, selectedProducts, FIOTextBlock.Text);
            orderWindow.ShowDialog();
        }
    }
}
