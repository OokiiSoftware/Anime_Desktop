using Anime.Auxiliar;
using Anime.Ferramentas;
using Anime.Modelo;
using Anime.Pages;
using Firebase.Database.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anime.SubPage
{
    public partial class AnimeListPage : UserControl
    {
        #region Variaveis

        private const string TAG = "AnimeListPage";

        private Dictionary<string, AnimeCollection> currentList;
        private AnimeCollection currentAnimeCollection;

        private TestesPage testesPage;
        private double width = 700;
        private bool isCollectionList = false;

        #endregion

        public AnimeListPage()
        {
            InitializeComponent();
            SizeChanged += (s, e) => width = e.NewSize.Width;
            Init();
        }

        #region Eventos

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            if (tbLetra.box.Text.Trim().Length == 0) return;
            var letra = tbLetra.box.Text[0].ToString();
            Properties.Settings.Default.letra = letra;
            Properties.Settings.Default.Save();
            currentList = AnimeCollection.LoadFileList(letra);
            List(currentList);
        }
        
        private async void BtnPublicar_Click(object sender, RoutedEventArgs e)
        {
            var allListC = new Dictionary<string, AnimeCollection.Complemento>();
            var allListB = new Dictionary<string, AnimeCollection.Basico>();
            string[] filtros = { "json" };
            var letras = new List<string>();
            int childsCount = 0;

            try
            {
                letras.AddRange(Import.GetArquivosDaPasta(Paths.FILES_PATH, filtros));
            }
            catch (Exception ex)
            {
                Log.Erro(TAG, ex);
                MessageBox.Show(ex.Message, "ERRO ao ler a pasta de arquivos", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                foreach (string letraFile in letras)
                {
                    string letra = Path.GetFileNameWithoutExtension(letraFile);
                    var listTemp = AnimeCollection.LoadFileList(letra);
                    foreach (var key in listTemp.Keys)
                        if (!allListC.ContainsKey(key))
                        {
                            allListC.Add(key, new AnimeCollection.Complemento(listTemp[key]));
                            allListB.Add(key, new AnimeCollection.Basico(listTemp[key]));
                            childsCount += listTemp[key].items.Count;
                        }
                        else Log.Msg(TAG, "BtnPublicar", "Key duplicado", key);
                }
            }
            catch (Exception ex)
            {
                Log.Erro(TAG, ex);
                MessageBox.Show(ex.Message, "ERRO ao ler arquivos", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            

            Log.Msg(TAG, "BtnPublicar", "Load OK > PaisCount: " + allListC.Count, "ChildsCount: " + childsCount);

            try
            {
                await FirebaseOki.GetClient
                    .Child(FirebaseChild.ANIME)
                    .Child(FirebaseChild.BASICO)
                    .PutAsync(allListB);

                await FirebaseOki.GetClient
                    .Child(FirebaseChild.ANIME)
                    .Child(FirebaseChild.COMPLEMENTO)
                    .PutAsync(allListC);

                Log.Msg(TAG, "BtnPublicar", "Firebase Save", "ÖK");
                MessageBox.Show(string.Format("Animes: {0}\nRamificações: {1}", allListC.Count, childsCount), "Dados Salvos", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) {
                Log.Erro(TAG, ex);
                MessageBox.Show(ex.Message, "ERRO ao publicar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void BtnBuscarErros_Click(object sender, RoutedEventArgs e)
        {
            if (testesPage == null || !testesPage.IsVisible)
            {
                testesPage = new TestesPage();
                testesPage.Show();
            }
            else
            {
                MessageBox.Show("Esta janela ja está aberta", "Ops", MessageBoxButton.OK);
            }
        }

        #endregion

        #region Metodos

        private void Init()
        {
            var letra = Properties.Settings.Default.letra;

            tbQuant.box.Text = Properties.Settings.Default.quantList.ToString();
            tbLetra.box.Text = letra.ToString();
            currentList = AnimeCollection.LoadFileList(letra);
            List(currentList);
        }

        private void List(Dictionary<string, AnimeCollection> list = null, AnimeCollection animeICollection = null)
        {
            int i = GetQuantList;
            lbList.Items.Clear();
            isCollectionList = list != null;

            if (isCollectionList)
            {
                foreach (var itemList in list.Values.Reverse())
                {
                    i--;
                    var itemLayout = new AnimeICollectionLayout(itemList);
                    itemLayout.Tag = itemList;
                    itemLayout.MinWidth = width;
                    itemLayout.MouseDoubleClick += CollectionDoubleClick;
                    lbList.Items.Add(itemLayout);
                    if (i <= 0) break;
                }
            }
            else
            {
                if (animeICollection == null) return;
                foreach (var item in animeICollection.items.Values.Reverse())
                {
                    i--;
                    var itemLayout = new AnimeItemLayout(item);
                    itemLayout.MouseRightButtonUp += SingleItemClick;
                    itemLayout.MinWidth = width;
                    lbList.Items.Add(itemLayout);
                    if (i <= 0) break;
                }
            }
        }

        private void SingleItemClick(object sender, MouseButtonEventArgs e)
        {
            var letra = Properties.Settings.Default.letra;
            currentList = AnimeCollection.LoadFileList(letra);
            List(list: currentList);
        }

        private void CollectionDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var itemList = (sender as AnimeICollectionLayout).Tag as AnimeCollection;

            currentAnimeCollection = itemList;
            List(animeICollection: itemList);
        }

        private int GetQuantList
        {
            get
            {
                string quant = tbQuant.box.Text;
                int i = quant.Length == 0 ? 20 : Convert.ToInt32(quant);
                int j = Properties.Settings.Default.quantList;

                if (i != j)
                {
                    Properties.Settings.Default.quantList = i;
                    Properties.Settings.Default.Save();
                }

                return i;
            }
        }

        public void AddItem(AnimeItem item)
        {
            Log.Msg(TAG, "AddItem", item.id.Substring(0, item.id.IndexOf('_')));

            if (isCollectionList)
            {
                if (currentList == null) return;
                try
                {
                    var temp = currentList[item.id.Substring(0, item.id.IndexOf('_'))];
                    temp.items.Add(item.id, item);
                }
                catch (Exception)
                {
                    var t = new AnimeCollection
                    {
                        nome = item.nome,
                        nome2 = item.nome2
                    };

                    t.items.Add(item.id, item);
                    var itemLayout = new AnimeICollectionLayout(t);
                    itemLayout.Tag = t;
                    itemLayout.MinWidth = width;
                    itemLayout.MouseDoubleClick += CollectionDoubleClick;
                    lbList.Items.Insert(0, itemLayout);
                }
            }
            else
            {
                if (currentAnimeCollection == null) return;
                currentAnimeCollection.items.Add(item.id, item);
                var itemLayout = new AnimeItemLayout(item);
                itemLayout.MinWidth = width;
                itemLayout.MouseRightButtonUp += SingleItemClick;
                lbList.Items.Insert(0, itemLayout);
            }
        }

        public void AddItem(AnimeCollection item)
        {
            if (isCollectionList)
            {
                var itemLayout = new AnimeICollectionLayout(item);
                itemLayout.Tag = item;
                itemLayout.MinWidth = width;
                itemLayout.MouseDoubleClick += CollectionDoubleClick;
                lbList.Items.Insert(0, itemLayout);
            }
        }

        #endregion

    }
}