using System.Windows;
using Anime.Auxiliar;
using Anime.Pages;
using Anime.SubPage;

namespace Anime
{
    public partial class MainWindow : Window
    {
        #region Variaveis
        private AnimeListPage animeList;
        private AnimeAddPage pgAdd;
        private AutoLoadAnimeData loadAnimeData;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        #region Eventos

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double width = e.NewSize.Width;
            double metade = width / 2 - 15;
            gdList.Width = metade;
            frame.Width = metade;
        }

        private void BtnAutoAdd_Click(object sender, RoutedEventArgs e)
        {
            frame.Content = loadAnimeData;
        }

        private void BtnManualAdd_Click(object sender, RoutedEventArgs e)
        {
            frame.Content = pgAdd;
        }

        #endregion

        #region Metodos

        private async void Init()
        {
            string user = Properties.Settings.Default.user;
            string password = Properties.Settings.Default.password;

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
            loadAnimeData = new AutoLoadAnimeData
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
            animeList = new AnimeListPage
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            pgAdd = new AnimeAddPage
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };

            loadAnimeData.OnAddItem += (item) => animeList.AddItem(item);
            pgAdd.OnAddItem += (item) => animeList.AddItem(item);

            gdList.Children.Add(animeList);
            frame.Content = loadAnimeData;
        }

        #endregion
    }
}
