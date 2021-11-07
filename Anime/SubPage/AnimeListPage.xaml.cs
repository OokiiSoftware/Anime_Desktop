using Anime.Auxiliar;
using Anime.Ferramentas;
using Anime.Modelo;
using Anime.Pages;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
            var allListC = new Dictionary<string, AnimeCollectionComplemento>();
            var allListB = new Dictionary<string, AnimeCollectionBasico>();
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
                var listT = JsonConvert.DeserializeObject<Dictionary<String, Dictionary<String, AnimeCollection>>>(File.ReadAllText(@"files\animes.json"));
                foreach(var lists in listT.Values)
                {
                    foreach(var key in lists.Keys)
                    {
                        allListC.Add(key, new AnimeCollectionComplemento(lists[key], key));
                        allListB.Add(key, new AnimeCollectionBasico(lists[key], key));
                        childsCount += lists[key].items.Count;
                    }
                }

                //Old
                /*foreach (string letraFile in letras)
                {
                    string letra = Path.GetFileNameWithoutExtension(letraFile);
                    var listTemp = AnimeCollection.LoadFileList(letra);
                    foreach (var key in listTemp.Keys)
                        if (!allListC.ContainsKey(key))
                        {
                            allListC.Add(key, new AnimeCollectionComplemento(listTemp[key], key));
                            allListB.Add(key, new AnimeCollectionBasico(listTemp[key], key));
                            childsCount += listTemp[key].items.Count;
                        }
                        else Log.Msg(TAG, "BtnPublicar", "Key duplicado", key);
                }*/
            }
            catch (Exception ex)
            {
                Log.Erro(TAG, ex);
                MessageBox.Show(ex.Message, "ERRO ao ler arquivos", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            

            Log.Msg(TAG, "BtnPublicar", "Load OK > PaisCount: " + allListC.Count, "ChildsCount: " + childsCount);

            voltar:
            try
            {
                File.WriteAllText(@"files\basico.json", JsonConvert.SerializeObject(allListB));
                File.WriteAllText(@"files\complemento.json", JsonConvert.SerializeObject(allListC));

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
                if (ex.Message.Contains("Auth token is expired"))
                {
                    var r = await FirebaseOki.AutoLogin();
                    if (r == FirebaseLoginResult.SUCESS)
                        goto voltar;
                }
                Log.Erro(TAG, ex);
                MessageBox.Show(ex.Message, "ERRO ao publicar", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCrunchyroll_Click(object sender, RoutedEventArgs e)
        {
            //SalvarNomes();
            //return;

            #region part 1
            /*string start = "https://www.";
            string end = "\"";
            string fileNameLoad = "Crunchyroll.html";
            string fileNameSave = "cLinks.txt";

            LoadHtml(start, end, fileNameLoad, fileNameSave);*/
            #endregion

            string animesFileName = "animes.json";
            string fileNameLoad = "links.txt";
            string fileNameSave = "animes.json";
            string fileNameLinks = "naoUsados.txt";
            string linkPart = "https://www.crunchyroll.com/pt-br/";

            UpdateLinks(animesFileName, fileNameLoad, fileNameSave, fileNameLinks, linkPart);
        }

        private void BtnFunimation_Click(object sender, RoutedEventArgs e)
        {
            #region part 1
            /*string start = "https://www.funimation.com/shows/";
            string end = "\"";
            string fileNameLoad = "Funimation.html";
            string fileNameSave = "fLinks.txt";

            LoadHtml(start, end, fileNameLoad, fileNameSave);*/
            #endregion

            string animesFileName =@"animes.json";
            string fileNameLoad = "fLinks.txt";
            string fileNameSave = "animesF.json";
            string fileNameLinks = "naoUsadosF.txt";
            string linkPart = "https://www.funimation.com/shows/";

            UpdateLinks(animesFileName, fileNameLoad, fileNameSave, fileNameLinks, linkPart);
        }

        /// <summary>
        /// Ler todos os animes e insere os links em seus respectivos animes
        /// </summary>
        /// <param name="fileNameLoad">Arquivo .txt com os links</param>
        /// <param name="fileNameSave">Nome do arquivo .json a ser salvo</param>
        /// <param name="linkPart">Parte comum que tem em todos os links</param>
        /// <param name="fileNameLinks">Arquivo .txt onde será salvo os links não usados</param>
        /// <param name="animesFileName">Arquivo que contém todos os animes</param>
        private void UpdateLinks(string animesFileName, string fileNameLoad, string fileNameSave, string fileNameLinks, string linkPart)
        {
            animesFileName = Paths.FILES + animesFileName;
            fileNameLinks = Paths.FILES + fileNameLinks;
            fileNameLoad = Paths.FILES + fileNameLoad;
            fileNameSave = Paths.FILES + fileNameSave;

            int igualFilhos = 0;
            int igualPai = 0;

            string[] links = File.ReadAllLines(fileNameLoad);
            List<string> linksUsados = new List<string>();

            List<string> nLinks = new List<string>();
            foreach (var link in links)
            {
                var temp = ToLink(link.Replace(linkPart, ""));
                nLinks.Add(temp);
            }

            void addLink(string value)
            {
                if (!linksUsados.Contains(value))
                    linksUsados.Add(value);
            };

            var bw = new BackgroundWorker();
            bw.DoWork += (s, e2) =>
            {
                var listT = JsonConvert.DeserializeObject<Dictionary<String, Dictionary<String, AnimeCollection>>>(File.ReadAllText(animesFileName));
                
                foreach (var list in listT.Values)
                {
                    foreach (var collection in list.Values)
                    {
                        string linkTemp = "";

                        var n1 = ToLink(collection.nome);
                        var n2 = ToLink(collection.nome2);

                        string linkIgual = null;

                        for (int i = 0; i < nLinks.Count; i++)
                        {
                            var link = nLinks[i];

                            if (NomeValido(n1))
                            {
                                if (link.Equals(n1))
                                {
                                    linkIgual = links[i];
                                    break;
                                }
                            }

                            if (NomeValido(n2))
                            {
                                if (link.Equals(n2))
                                {
                                    linkIgual = links[i];
                                    break;
                                }
                            }

                        }

                        if (linkIgual != null)
                        {
                            igualPai++;
                            collection.links.Add(linkIgual);
                            addLink(linkIgual);
                            linkTemp = linkIgual;
                            //Log.Msg(TAG, "Crunchyroll", "OK", item.nome);
                        }

                        foreach (var anime in collection.items.Values)
                        {
                            var n3 = ToLink(anime.nome);
                            var n4 = ToLink(anime.nome2);

                            string linkIgual2 = null;

                            for (int i = 0; i < nLinks.Count; i++)
                            {
                                var link = nLinks[i];
                                if (links[i].Equals(linkTemp))
                                    continue;

                                if (NomeValido(n3))
                                {
                                    if (link.Equals(n3))
                                    {
                                        linkIgual2 = links[i];
                                        break;
                                    }
                                }

                                if (NomeValido(n4))
                                {
                                    if (link.Equals(n4))
                                    {
                                        linkIgual2 = links[i];
                                        break;
                                    }
                                }

                            }

                            if (linkIgual2 != null)
                            {
                                anime.links.Add(linkIgual2);
                                addLink(linkIgual2);
                                linkTemp = linkIgual2;
                                igualFilhos++;
                                //Log.Msg(TAG, "Crunchyroll", "OK", item.nome);
                            }
                        }

                    }
                }

                try
                {
                    var json = JsonConvert.SerializeObject(listT);
                    File.WriteAllText(fileNameSave, json);
                }
                catch (Exception ex)
                {
                    Log.Erro("AnimeCollection", ex);
                }
            };
            bw.RunWorkerCompleted += (s, e2) =>
            {
                List<string> linksNaoUsados = new List<string>();
                for (int i = 0; i < nLinks.Count; i++)
                {
                    var link = links[i];
                    if (!linksUsados.Contains(link))
                        linksNaoUsados.Add(links[i]);
                }

                File.WriteAllLines(fileNameLinks, linksNaoUsados);

                Log.Msg(TAG, "UpdateLinks", igualFilhos, igualPai);
                MessageBox.Show($"Pais: {igualPai}\nFilhos: {igualFilhos}", "Concluido", MessageBoxButton.OK);

            };
            bw.RunWorkerAsync();
            bw.Dispose();
        }

        string ToLink(string value)
        {
            if (value == null)
                return "";
            if (value.Length == 0)
                return "";

            return value.ToLower()
                .Replace("0", "")
                .Replace("1", "")
                .Replace("2", "")
                .Replace("3", "")
                .Replace("4", "")
                .Replace("5", "")
                .Replace("6", "")
                .Replace("7", "")
                .Replace("8", "")
                .Replace("9", "")

                .Replace("à", "a")
                .Replace("â", "a")
                .Replace("ã", "a")
                .Replace("é", "e")
                .Replace("è", "e")
                .Replace("ō", "o")
                .Replace("ó", "o")
                .Replace("ä", "a")
                .Replace("î", "i")
                .Replace("ï", "i")
                .Replace("³", "3")
                .Replace("×", "x")
                .Replace("¹", "1")

                .Replace(" ", "")
                .Replace("-", "")
                .Replace("␣", "")
                .Replace("˜", "")
                .Replace("%", "")
                .Replace("©", "")
                .Replace("ω", "")
                .Replace("º", "")
                .Replace("ž", "")
                .Replace("¤", "")
                .Replace("¼", "")
                .Replace("ª", "")
                .Replace("™", "")
                .Replace("€", "")
                .Replace("|", "")
                .Replace("$", "")
                .Replace("„", "")
                .Replace("œ", "")
                .Replace("¿", "")
                .Replace("¶", "")
                .Replace("¨", "")
                .Replace(" ", "")
                .Replace("ß", "")
                .Replace("∬", "")
                .Replace("＋", "")

                .Replace("+", "")
                .Replace("&", "")
                .Replace(".", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("=", "")
                .Replace(",", "")
                .Replace("'", "")
                .Replace("?", "")
                .Replace("*", "")
                .Replace("\"", "")
                .Replace("\"", "")
                .Replace("＊", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("/", "")
                .Replace(";", "")
                .Replace("⅙", "")
                .Replace("~", "")
                .Replace("_", "")
                .Replace("#", "")
                .Replace("±", "")
                .Replace("°", "")
                .Replace("@", "")

                .Replace(":", "")
                .Replace("!", "")
                .Replace("♭", "")
                .Replace("’", "")
                .Replace("♥", "")
                .Replace("♡", "")
                .Replace("–", "")
                .Replace("−", "")
                .Replace("—", "")
                .Replace("―", "")
                .Replace("†", "")
                .Replace("…", "")
                .Replace("♪", "")
                .Replace("∞", "")
                .Replace("√", "")
                .Replace("“", "")
                .Replace("”", "")
                .Replace("★", "")
                .Replace("☆", "")
                .Replace("◯", "")
                .Replace("∽", "")
                .Replace("△", "");
        }
        bool NomeValido(string n1)
        {
            return !string.IsNullOrEmpty(n1);
        }

        /// <summary>
        /// Percorre links de Funimation e add no anime se este tiver o mesmo nome que o link da crunchy
        /// </summary>
        private void AddLinksIguais()
        {
            string animesFileName = @"files\naoUsados.txt";
            string fileNameLoad = @"files\animes.json";
            string[] links = File.ReadAllLines(animesFileName);
            var listT = JsonConvert.DeserializeObject<Dictionary<String, Dictionary<String, AnimeCollection>>>(File.ReadAllText(fileNameLoad));

            foreach(var link in links)
                foreach(var value in listT.Values)
                {
                    foreach(var collection in value.Values)
                    {
                        var nLink = link.Replace("https://www.funimation.com/shows/", "https://www.crunchyroll.com/pt-br/");
                        if (collection.links.Contains(nLink))
                        {
                            collection.links.Add(link);
                        }
                        foreach(var anime in collection.items.Values)
                        {
                            if (anime.links.Contains(nLink))
                            {
                                anime.links.Add(link);
                            }
                        }
                    }
                }


            File.WriteAllText(@"files\animesNew.json", JsonConvert.SerializeObject(listT));
        }

        /// <summary>
        /// Abre basicos e insere no geral os links e ids
        /// </summary>
        private void Temp()
        {
            string animesFileName = @"files\basico.json";
            string fileNameLoad = @"files\animes.json";
            var list = JsonConvert.DeserializeObject<Dictionary<string, AnimeCollection>>(File.ReadAllText(animesFileName));
            var listT = JsonConvert.DeserializeObject<Dictionary<String, Dictionary<String, AnimeCollection>>>(File.ReadAllText(fileNameLoad));


            foreach (var value in listT.Values)
            {
                foreach (var key in value.Keys)
                {
                    if (list.ContainsKey(key))
                    {
                        value[key].id = key;
                        if (value[key].links.Count == 0)
                        {
                            value[key].links.AddRange(list[key].links);
                        }

                        foreach (var keyFilho in value[key].items.Keys)
                        {
                            if (list[key].items.ContainsKey(keyFilho))
                                if (value[key].items[keyFilho].links.Count == 0)
                                {
                                    value[key].items[keyFilho].links.AddRange(list[key].items[keyFilho].links);
                                }
                        }
                    }
                }
            }

            File.WriteAllText(@"files\animesNew.json", JsonConvert.SerializeObject(listT));
        }

        private void SalvarNomes()
        {
            var lis = AnimeCollection.LoadFilesList(null);
            List<string> nomes = new List<string>();
            List<string> novosNomes = new List<string>();
            foreach(var list in lis.Values)
            {
                foreach(var collection in list.Values)
                {
                    string nome1 = collection.nome;
                    string nome2 = collection.nome2;

                    if (NomeValido(nome1) && !nomes.Contains(nome1))
                    {
                        nomes.Add(nome1);
                    }
                    if (NomeValido(nome2) && !nomes.Contains(nome2))
                    {
                        nomes.Add(nome2);
                    }

                    foreach(var anime in collection.items.Values)
                    {
                        string nome3 = anime.nome;
                        string nome4 = anime.nome2;

                        if (NomeValido(nome3) && !nomes.Contains(nome3))
                        {
                            nomes.Add(nome3);
                        }
                        if (NomeValido(nome4) && !nomes.Contains(nome4))
                        {
                            nomes.Add(nome4);
                        }
                    }
                }
            }

            foreach(var nome in nomes)
            {
                string temp = ToLink(nome)
                    .Replace(" ", "")
                    .Replace("a", "")
                    .Replace("b", "")
                    .Replace("c", "")
                    .Replace("d", "")
                    .Replace("e", "")
                    .Replace("f", "")
                    .Replace("g", "")
                    .Replace("h", "")
                    .Replace("i", "")
                    .Replace("j", "")
                    .Replace("k", "")
                    .Replace("l", "")
                    .Replace("m", "")
                    .Replace("n", "")
                    .Replace("o", "")
                    .Replace("p", "")
                    .Replace("q", "")
                    .Replace("r", "")
                    .Replace("s", "")
                    .Replace("t", "")
                    .Replace("u", "")
                    .Replace("v", "")
                    .Replace("w", "")
                    .Replace("x", "")
                    .Replace("y", "")
                    .Replace("z", "");

                if(!novosNomes.Contains(temp))
                    novosNomes.Add(temp);
            }

            File.WriteAllLines(@"files\nomes.txt", novosNomes);
        }

        /// <summary>
        /// Ler um arquivo .html e salva em um .txt todos os links encontrados
        /// </summary>
        /// <param name="start">Parte inicial do link</param>
        /// <param name="end">Parte final do link</param>
        /// <param name="fileNameLoad">Nome do arquivo .html</param>
        /// <param name="fileNameSave">Nome do arquivo .txt</param>
        private void LoadHtml(string start, string end, string fileNameLoad, string fileNameSave)
        {
            string[] data = File.ReadAllLines(fileNameLoad);
            if (data == null)
            {
                MessageBox.Show("data == null", "ERRO", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<string> links = new List<string>();
            foreach (var linha in data)
            {
                if (linha.Contains(start) && linha.Contains(end))
                {
                    int i = linha.IndexOf(start);
                    int k = linha.IndexOf(end, i);
                    links.Add(linha.Substring(i, k - i));
                }
            }
            try
            {
                if (!Directory.Exists(Paths.FILES))
                    Directory.CreateDirectory(Paths.FILES);

                string fileName = Paths.FILES + fileNameSave;
                File.WriteAllLines(fileName, links);
            }
            catch (Exception ex)
            {
                Log.Erro("AnimeCollection", ex);
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

        private string[] GetData()
        {
            try
            {
                return File.ReadAllLines("Crunchyroll.html");
            }
            catch
            {
                return null;
            }
        }

        #endregion

    }
}