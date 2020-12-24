using System.Windows;
using Anime.Auxiliar;
using Anime.Pages;
using Anime.SubPage;

namespace Anime
{
    public partial class MainWindow : Window
    {
        private AnimeListPage animeList;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = e.NewSize.Width;
            double metade = width / 2 - 15;
            gdList.Width = metade;
            pgAdd.Width = metade;
        }

        private async void Init()
        {
            string user = Properties.Settings.Default.user;
            string password = Properties.Settings.Default.password;
            Log.Msg("Main", user, password);
            if (user.Trim().Equals("") || password.Trim().Equals(""))
            {
                LoginPopup();
            }
            else
            {
                var result = await FirebaseOki.Login(user, password);
                if (result == FirebaseLoginResult.SUCESS)
                    OnLoginSucess();
                else
                    LoginPopup();
            }
        }

        private void LoginPopup()
        {
            var login = new LoginPage();
            var result = login.ShowDialog() ?? false;
            if (result == true)
                OnLoginSucess();
            else
                Close();
        }

        private void OnLoginSucess()
        {
            animeList = new AnimeListPage();

            animeList.HorizontalAlignment = HorizontalAlignment.Stretch;

            animeList.Margin = new Thickness(0);

            pgAdd.OnAddItem += (item) => animeList.AddItem(item);
            gdList.Children.Add(animeList);
        }
    }
}
