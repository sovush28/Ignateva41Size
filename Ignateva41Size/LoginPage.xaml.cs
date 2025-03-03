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

using System.Threading;

namespace Ignateva41Size
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            CaptchaStackP.Visibility = Visibility.Hidden;
            EnterCaptchaText.Visibility = Visibility.Hidden;
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;
            string captchaSubmitted = CaptchaTextBox.Text;

            
            string captchaGenerated = captchaWordOne.Text + captchaWordTwo.Text + captchaWordThree.Text + captchaWordFour.Text;

            if (login == "" || password == "")
            {
                MessageBox.Show("Есть пустые поля");
                DisableBtnFor10Seconds();
                CaptchaStackP.Visibility = Visibility.Visible;
                EnterCaptchaText.Visibility = Visibility.Visible;
                GenerateCaptcha();
                CaptchaTextBox.Text = "";
                return;
            }

            if (CaptchaStackP.Visibility == Visibility.Visible && captchaSubmitted != captchaGenerated)
            {
                MessageBox.Show("Неверная капча");
                CaptchaTextBox.Text = "";
                DisableBtnFor10Seconds();
                return;
            }

            User user = Ignateva41Entities.GetContext().User.ToList().
                Find(p => p.UserLogin == login && p.UserPassword == password);

            if (user != null)
            {
                Manager.MainFrame.Navigate(new ProductPage(user));
                LoginTextBox.Text = "";
                PasswordTextBox.Text = "";
                CaptchaTextBox.Text = "";
                CaptchaStackP.Visibility = Visibility.Hidden;
                EnterCaptchaText.Visibility = Visibility.Hidden;
            }

            else
            {
                MessageBox.Show("Введены неверные данные");
                LoginTextBox.Text = "";
                PasswordTextBox.Text = "";
                CaptchaTextBox.Text = "";
                DisableBtnFor10Seconds();

                CaptchaStackP.Visibility = Visibility.Visible;
                EnterCaptchaText.Visibility = Visibility.Visible;
            }

            GenerateCaptcha();
        }

        private void GuestLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new ProductPage(null));
            LoginTextBox.Text = "";
            PasswordTextBox.Text = "";
            CaptchaTextBox.Text = "";
        }

        private void GenerateCaptcha()
        {
            Random rand = new Random();

            char ch1 = (char)rand.Next('A', 'Z' + 1);
            captchaWordOne.Text = ch1.ToString();

            char ch2 = (char)rand.Next('A', 'Z' + 1);
            captchaWordTwo.Text = ch2.ToString();

            char ch3 = (char)rand.Next('A', 'Z' + 1);
            captchaWordThree.Text = ch3.ToString();

            char ch4 = (char)rand.Next('A', 'Z' + 1);
            captchaWordFour.Text = ch4.ToString();
        }

        private async void DisableBtnFor10Seconds()
        {
            LoginBtn.IsEnabled = false;
            LoginBtn.Opacity = 0.5;

            await Task.Delay(TimeSpan.FromSeconds(10));

            LoginBtn.IsEnabled = true;
            LoginBtn.Opacity = 1;
        }
    }
}
