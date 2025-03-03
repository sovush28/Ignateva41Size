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

namespace Ignateva41Size
{
    /// <summary>
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        List<Product> selectedProducts = new List<Product>();
        private Order currentOrder = new Order();
        private OrderProduct currentOrderProduct = new OrderProduct();

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO)
        {
            InitializeComponent();

            DeleteBtn.Visibility = Visibility.Hidden;
            CancelSelectionBtn.Visibility = Visibility.Hidden;

            var currentPickUps = Ignateva41Entities.GetContext().PickUpPoint.ToList();
            PickupPointCombo.ItemsSource = currentPickUps;

            FIOTextBlock.Text = FIO;
            if (FIOTextBlock.Text == null)
                FIOTextBlock.Text = "Гость";

            foreach (var prod in selectedProducts)
            {
                prod.Quantity = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == prod.ProductArticleNumber)?.ProductCount ?? 1;
            }

            ShoeListView.ItemsSource = selectedProducts;

            GenerateOrderNumber();

            SetDeliveryDate();
            UpdateTotalCost();

            //InitializeComponent();

            //var currentPickUps = Ignateva41Entities.GetContext().PickUpPoint.ToList();
            //PickupPointCombo.ItemsSource = currentPickUps;
            ////PickupPointCombo.SelectedIndex = 0;

            //FIOTextBlock.Text = FIO;
            //OrderNumberTB.Text = selectedOrderProducts.First().OrderID.ToString();

            //ShoeListView.ItemsSource = selectedProducts;
            //foreach(Product p in selectedProducts)
            //{
            //    p.Quantity = 1;
            //    foreach(OrderProduct q in selectedOrderProducts)
            //    {
            //        if (p.ProductArticleNumber == q.ProductArticleNumber)
            //            p.Quantity = q.ProductCount; // количество продуктов для отображения
            //    }
            //}

            //this.selectedOrderProducts = selectedOrderProducts;
            //this.selectedProducts = selectedProducts;

            //OrderDatePicker.Text = DateTime.Now.ToString();
            //SetDeliveryDate();
        }

        private void UpdateTotalCost()
        {
            decimal totalCost = 0;

            foreach (var product in selectedProducts)
            {
                var orderProduct = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);
                if (orderProduct != null)
                {
                    totalCost += product.ProductCost * orderProduct.ProductCount;
                }
            }

            OverallOrderCostTB.Text = totalCost.ToString() + " руб.";
        }

        private void GenerateOrderNumber()
        {
            try
            {
                // Retrieve the maximum OrderID from the database
                int maxOrderID = Ignateva41Entities.GetContext().Order.Max(o => o.OrderID);
                int newOrderID = maxOrderID + 1;

                // Retrieve the maximum OrderCode from the database
                int? maxOrderCode = Ignateva41Entities.GetContext().Order.Max(o => (int?)o.OrderCode);
                int newOrderCode = maxOrderCode.HasValue ? maxOrderCode.Value + 1 : 1;

                // Set the generated values to the respective fields
                OrderNumberTB.Text = newOrderID.ToString();
                currentOrder.OrderCode = newOrderCode; // Pre-fill the OrderCode for saving
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации номера заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPlus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;
            prod.Quantity++;

            var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
            if (selectedOP != null)
            {
                selectedOP.ProductCount++;
            }

            SetDeliveryDate();
            ShoeListView.Items.Refresh();
            UpdateTotalCost();

            //var prod = (sender as Button).DataContext as Product;
            //prod.Quantity++;

            //var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
            //int index = selectedOrderProducts.IndexOf(selectedOP);

            //selectedOrderProducts[index].ProductCount++; // 

            //SetDeliveryDate();

            //ShoeListView.Items.Refresh();
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;

            if (prod.Quantity > 1)
            {
                prod.Quantity--;

                var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
                if (selectedOP != null)
                {
                    selectedOP.ProductCount--;
                }
            }
            else
            {
                if (selectedProducts.Count == 1)
                {
                    if (MessageBox.Show("Отменить заказ?", "Внимание!",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        this.Close();
                    }
                    else
                    {
                        return;
                    }
                }

                selectedProducts.Remove(prod);
                selectedOrderProducts.RemoveAll(op => op.ProductArticleNumber == prod.ProductArticleNumber);
            }

            SetDeliveryDate();
            ShoeListView.Items.Refresh();
            UpdateTotalCost();

            //var prod = (sender as Button).DataContext as Product;

            //if (prod.Quantity > 1)
            //{
            //    prod.Quantity--;

            //    var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
            //    int index = selectedOrderProducts.IndexOf(selectedOP);

            //    selectedOrderProducts[index].ProductCount--; // 
            //}
            //else
            //{
            //    if (selectedProducts.Count == 1)
            //    {
            //        if (MessageBox.Show("Отменить оформление заказа?", "Внимание!",
            //            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            //        {
            //            selectedProducts.Clear();
            //        }
            //        else return;
            //    }
            //    selectedProducts.Remove(prod); //// ?????
            //}

            //SetDeliveryDate();

            //ShoeListView.Items.Refresh();

        }

        private void SetDeliveryDate()
        {
            bool areEnoughInStock = true;

            foreach(Product p in selectedProducts)
            {
                if (p.ProductQuantityInStock < 3)
                {
                    areEnoughInStock = false;
                    //break;
                }

            }

            int deliveryDays = 3;

            if (!areEnoughInStock)
            {
                deliveryDays = 6;
            }

            if (OrderDatePicker.Text != "")
                DeliveryDatePicker.Text = OrderDatePicker.SelectedDate.Value.AddDays(deliveryDays).ToString();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (PickupPointCombo.SelectedIndex == -1)
            {
                errors.AppendLine("Выберите пункт выдачи.");
            }
            if (selectedProducts.Count == 0)
            {
                errors.AppendLine("Добавьте хотя бы один товар в заказ.");
            }

            // Display validation errors if any
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var user = Ignateva41Entities.GetContext().User
                    .FirstOrDefault(u => u.UserSurname + u.UserName + " " + u.UserPatronymic == FIOTextBlock.Text);

                //if (user == null)
                //{
                //    MessageBox.Show("Клиент с указанным ФИО не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                int? newUserID = user.UserID;
                if (FIOTextBlock.Text == "Гость")
                    newUserID = null;

                var newOrder = new Order
                {
                    OrderID = Convert.ToInt32(OrderNumberTB.Text),
                    OrderClient = newUserID==null ? "NULL" : newUserID.ToString(), //??
                    OrderPickupPoint = (PickupPointCombo.SelectedItem as PickUpPoint).PickUpPointID,
                    OrderStatus = "Новый",
                    OrderDate = Convert.ToDateTime(OrderDatePicker.Text),
                    OrderDeliveryDate = Convert.ToDateTime(DeliveryDatePicker.Text),
                    OrderCode = currentOrder.OrderCode
                };

                // Add the order to the context
                Ignateva41Entities.GetContext().Order.Add(newOrder);

                // Add order products to the context
                foreach (var orderProduct in selectedOrderProducts)
                {
                    orderProduct.OrderID = newOrder.OrderID; // Link each product to the current order
                    Ignateva41Entities.GetContext().OrderProduct.Add(orderProduct);
                }

                Ignateva41Entities.GetContext().SaveChanges();
                MessageBox.Show("Заказ успешно сохранен");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }


            //StringBuilder errors = new StringBuilder();

            //if (PickupPointCombo.SelectedIndex == 0)
            //    errors.AppendLine("Выберите пункт выдачи");

            //if (errors.Length > 0)
            //{
            //    MessageBox.Show(errors.ToString());
            //    return;
            //}
            //else
            //{
            //    //?????????????????????????
            //    currentOrder.OrderClient = Ignateva41Entities.GetContext().User.Where(p => Convert.ToString(p.UserSurname + p.UserName + " " + p.UserPatronymic) == FIOTextBlock.Text);
            //    foreach(Product p in selectedProducts)
            //    {
            //        //сохранить каждый в ордерпродукт отдельно
            //    }
            //    //статус заказа новый!!!

            //}

            ////currentOrder.ClientFIO = ClientTB.Text; // ??????????
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            //удаление продукта из заказа
        }

        private void ShoeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShoeListView.SelectedItems.Count > 0)
            {
                DeleteBtn.Visibility = Visibility.Visible;
                CancelSelectionBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Hidden;
            }
        }

        private void CancelSelectionBtn_Click(object sender, RoutedEventArgs e)
        {
            ShoeListView.SelectedItems.Clear();

            SaveBtn.Visibility = Visibility.Visible;
            DeleteBtn.Visibility = Visibility.Hidden;
            CancelSelectionBtn.Visibility = Visibility.Hidden;

        }
    }
}
