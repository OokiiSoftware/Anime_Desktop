using System.Timers;
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
        private TestesPage testesPage;
        private LoginPage loginPage;
        private AtualizarAnimePage atualizarAnimePage;

        private bool loginSucess;
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

        private void BtnProcurarErros_Click(object sender, RoutedEventArgs e)
        {
            frame.Content = testesPage;
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            frame.Content = atualizarAnimePage;
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
                TimeSleep();
                var result = await FirebaseOki.Login(user, password);
                if (result == FirebaseLoginResult.SUCESS)
                    OnLoginSucess();
                else
                {
                    var result2 = MessageBox.Show(
                        "Não foi possivel fazer login automatico, deseja tentar novamente?",
                        "Auto Login", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                    if (result2 == MessageBoxResult.Yes)
                        Init();
                    else
                        LoginPopup();
                }
            }
        }

        private void LoginPopup()
        {
            loginPage = new LoginPage();
            var result = loginPage.ShowDialog() ?? false;
            if (result == true)
                OnLoginSucess();
            else
                Close();
        }

        private void OnLoginSucess()
        {
            loginSucess = true;

            testesPage = new TestesPage
            {
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
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
            atualizarAnimePage = new AtualizarAnimePage
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

        private void TimeSleep()
        {
            int seconds = 40;
            Timer timer = new Timer(seconds * 1000);
            timer.Elapsed += (s, e) => {
                if (!loginSucess) 
                {
                    var result = MessageBox.Show("Deseja continuar?", "Tempo exedido", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                    if (result != MessageBoxResult.Yes)
                        Dispatcher.Invoke(() => { Close(); });
                }
                
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        #endregion

    }
}
