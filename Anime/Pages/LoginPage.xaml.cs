using Anime.Auxiliar;
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

namespace Anime.Pages
{
    public partial class LoginPage : Window
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            log.Content = "";
            string user = tbUser.box.Text;
            string password = tbPassword.password.Password;
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                log.Content = "Preencha todos os campos.";
                return;
            }

            var resuult = await FirebaseOki.Login(user, password);
            if (resuult != FirebaseLoginResult.SUCESS)
                log.Content = resuult.ToString();
            else
            {
                Properties.Settings.Default.user = user;
                Properties.Settings.Default.password = password;
                Properties.Settings.Default.Save();
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
