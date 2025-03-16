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
        ///////////////////////////////////////// 257
        List<OrderProduct> selectedOrderProducts = new List<OrderProduct>();
        List<Product> selectedProducts = new List<Product>();
        private Order currentOrder = new Order();
        private OrderProduct currentOrderProduct = new OrderProduct();

        private User _currentUser;

        public OrderWindow(List<OrderProduct> selectedOrderProducts, List<Product> selectedProducts, string FIO)
        {
            InitializeComponent();

            DeleteBtn.Visibility = Visibility.Hidden;
            CancelSelectionBtn.Visibility = Visibility.Hidden;

            var currentPickUps = Ignateva41Entities.GetContext().PickUpPoint.ToList();
            PickupPointCombo.ItemsSource = currentPickUps;

            if (FIO == "" || FIO == null)
            {
                FIOTextBlock.Text = "Гость";
            }
            else
            {
                FIOTextBlock.Text = FIO;
            }

            foreach (var prod in selectedProducts)
            {
                prod.Quantity = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == prod.ProductArticleNumber)?.ProductCount ?? 1;
            }

            ShoeListView.ItemsSource = selectedProducts;

            //GenerateOrderNumber();

            //int maxOrderID = Ignateva41Entities.GetContext().Order.Max(o => o.OrderID);
            //int newOrderID = maxOrderID + 1;
            int newOrderID = GetNextOrderID();
            OrderNumberTB.Text = newOrderID.ToString();


            foreach(var product in selectedProducts)
            {
                var orderProduct = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);
                if (orderProduct != null)
                {
                    product.Quantity = orderProduct.ProductCount;
                }
                else
                {
                    product.Quantity = 1;
                }
            }

            this.selectedOrderProducts = selectedOrderProducts;
            this.selectedProducts = selectedProducts;

            OrderDatePicker.SelectedDate = DateTime.Now;
            SetDeliveryDate();

            // Update total cost initially
            UpdateTotalCost();
        }

        private int GetNextOrderID()
        {
            var sqlCommand = "SELECT IDENT_CURRENT('Order')";
            var nextID = Ignateva41Entities.GetContext().Database.SqlQuery<decimal>(sqlCommand).FirstOrDefault() + 1;
            return (int)nextID;
        }

        private void UpdateTotalCost()
        {
            decimal totalCost = 0;
            decimal discount = 0;

            foreach(var orderProduct in selectedOrderProducts)
            {
                var product = selectedProducts.FirstOrDefault(p => p.ProductArticleNumber == orderProduct.ProductArticleNumber);
                if (product == null) continue;

                //decimal prodCost = product.ProductCost;
                decimal discountPercent = product.ProductDiscountAmount ?? 0;

                totalCost += orderProduct.ProductCount * product.ProductCost;

                discount += orderProduct.ProductCount * product.ProductCost * (discountPercent / 100);
            }

            decimal totalWDiscount = totalCost - discount;

            OverallOrderCostTB.Text = totalCost.ToString("C"); // Format as currency
            TotalWDiscTB.Text = totalWDiscount.ToString("C");

            //foreach (var product in selectedProducts)
            //{
            //    var orderProduct = selectedOrderProducts.FirstOrDefault(op => op.ProductArticleNumber == product.ProductArticleNumber);
            //    if (orderProduct != null)
            //    {
            //        totalCost += product.ProductCost * orderProduct.ProductCount;
            //    }
            //}

        }

        //private void GenerateOrderNumber()
        //{
        //    try
        //    {
        //        int maxOrderID = Ignateva41Entities.GetContext().Order.Max(o => o.OrderID);
        //        int newOrderID = maxOrderID + 1;

        //        int? maxOrderCode = Ignateva41Entities.GetContext().Order.Max(o => (int?)o.OrderCode);
        //        int newOrderCode = maxOrderCode.HasValue ? maxOrderCode.Value + 1 : 1;

        //        OrderNumberTB.Text = newOrderID.ToString();
        //        currentOrder.OrderCode = newOrderCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка при генерации номера заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

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
            UpdateTotalCost();
            ShoeListView.Items.Refresh();
        }

        private void BtnMinus_Click(object sender, RoutedEventArgs e)
        {
            var prod = (sender as Button).DataContext as Product;

            if (prod.Quantity > 1)
            {
                // Уменьшаем количество товара
                prod.Quantity--;

                var selectedOP = selectedOrderProducts.FirstOrDefault(p => p.ProductArticleNumber == prod.ProductArticleNumber);
                if (selectedOP != null)
                {
                    selectedOP.ProductCount--;
                }
            }
            else
            {
                // Если количество равно 1, удаляем продукт из заказа
                //if (selectedProducts.Count == 1)
                //{
                //    // Если это последний продукт в заказе, предлагаем пользователю подтвердить отмену заказа
                //    if (MessageBox.Show("Отменить заказ?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                //    {
                //        this.Close(); // Закрываем окно заказа
                //        return;
                //    }
                //    else
                //    {
                //        return; // Прерываем выполнение метода, если пользователь отменил действие
                //    }
                //}

                // Удаляем продукт из списка выбранных продуктов
                selectedProducts.Remove(prod);

                // Удаляем соответствующий OrderProduct из списка
                selectedOrderProducts.RemoveAll(op => op.ProductArticleNumber == prod.ProductArticleNumber);
            }

            // Обновляем дату доставки, общую стоимость и перерисовываем ListView
            SetDeliveryDate();
            UpdateTotalCost();
            ShoeListView.Items.Refresh();
        }

        private void SetDeliveryDate()
        {
            bool areEnoughInStock = true;

            foreach (Product p in selectedProducts)
            {                
                if (p.ProductQuantityInStock < p.Quantity + 3)
                {
                    areEnoughInStock = false;
                    break;
                }
            }

            int deliveryDays = areEnoughInStock ? 3 : 6;

            if (OrderDatePicker.SelectedDate != null)
            {
                DeliveryDatePicker.Text = OrderDatePicker.SelectedDate.Value.AddDays(deliveryDays).ToString("d");
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (PickupPointCombo.SelectedIndex == -1)
            {
                errors.AppendLine("Выберите пункт выдачи.");
            }

            if (!OrderDatePicker.SelectedDate.HasValue)
            {
                errors.AppendLine("Выберите дату заказа.");
            }

            if (selectedProducts.Count == 0)
            {
                errors.AppendLine("Добавьте хотя бы один товар в заказ.");
            }

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (_currentUser == null)
            {
                currentOrder.OrderClient = "NULL";
            }
            else
            {
                //currentOrder.OrderClient = _currentUser.UserID;
            }

            try
            {
                var user = Ignateva41Entities.GetContext().User
                    .FirstOrDefault(u => u.UserSurname + " " + u.UserName + " " + u.UserPatronymic == FIOTextBlock.Text);

                int? newUserID = user?.UserID;
                if (FIOTextBlock.Text == "Гость")
                    newUserID = null;

                int prevOrderCode = Ignateva41Entities.GetContext().Order.Max(o => o.OrderCode);
                int newOrderCode = prevOrderCode + 1;

                var newOrder = new Order
                {
                    //OrderID = Convert.ToInt32(OrderNumberTB.Text),
                    OrderClient = newUserID.ToString(),
                    OrderPickupPoint = (PickupPointCombo.SelectedItem as PickUpPoint).PickUpPointID,
                    OrderStatus = "Новый",
                    OrderDate = OrderDatePicker.SelectedDate.Value,
                    OrderDeliveryDate = Convert.ToDateTime(DeliveryDatePicker.Text),
                    OrderCode = newOrderCode,
                    TotalCost = decimal.Parse(OverallOrderCostTB.Text.Replace("₽", "").Trim())
                };

                Ignateva41Entities.GetContext().Order.Add(newOrder);
                Ignateva41Entities.GetContext().SaveChanges();

                foreach (var orderProduct in selectedOrderProducts)
                {
                    orderProduct.OrderID = newOrder.OrderID;
                    Ignateva41Entities.GetContext().OrderProduct.Add(orderProduct);
                }

                Ignateva41Entities.GetContext().SaveChanges();
                MessageBox.Show("Заказ успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            if (ShoeListView.SelectedItems.Count > 0)
            {
                foreach (Product selectedProduct in ShoeListView.SelectedItems.Cast<Product>().ToList())
                {
                    selectedProducts.Remove(selectedProduct);
                    selectedOrderProducts.RemoveAll(op => op.ProductArticleNumber == selectedProduct.ProductArticleNumber);
                }

                SetDeliveryDate();
                UpdateTotalCost();
                ShoeListView.Items.Refresh();

                if (selectedProducts.Count == 0)
                {
                    SaveBtn.Visibility = Visibility.Hidden;
                    DeleteBtn.Visibility = Visibility.Hidden;
                    CancelSelectionBtn.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ShoeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ShoeListView.SelectedItems.Count > 0)
            {
                DeleteBtn.Visibility = Visibility.Visible;
                CancelSelectionBtn.Visibility = Visibility.Visible;
                SaveBtn.Visibility = Visibility.Hidden;
            }
            else
            {
                SaveBtn.Visibility = Visibility.Visible;
                DeleteBtn.Visibility = Visibility.Hidden;
                CancelSelectionBtn.Visibility = Visibility.Hidden;
            }
        }

        private void CancelSelectionBtn_Click(object sender, RoutedEventArgs e)
        {
            ShoeListView.SelectedItems.Clear();

            SaveBtn.Visibility = Visibility.Visible;
            DeleteBtn.Visibility = Visibility.Hidden;
            CancelSelectionBtn.Visibility = Visibility.Hidden;
        }

        private void OrderDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SetDeliveryDate();
        }
    }
}



    /*
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
    */