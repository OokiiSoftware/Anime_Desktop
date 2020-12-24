using Anime.Auxiliar;
using Anime.Modelo;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using System.Web;
using Anime.Translator;

namespace Anime.Pages
{
    public partial class AutoLoadAnimeData : UserControl
    {
        #region Variaveis

        private const string TAG = "AutoLoadAnimeData";

        public delegate void onAddItem(AnimeCollection item);
        public event onAddItem OnAddItem;

        private readonly Tradutor translator = new Tradutor();

        private Dictionary<string, AnimeCollection> currentList;

        class Parts
        {
            public const string TITULO_1 = "<h1 class=\"title-name h1_bold_none\"><strong>";
            public const string TITULO_2 = "<p class=\"title-english title-inherit\">";
            public const string SINOPSE = "<p itemprop=\"description\">";
            public const string MINIATURA = "<meta property=\"og:image\" content=\"";
            public const string TIPO = "<a href=\"https://myanimelist.net/topanime.php?type=";
            public const string EPIS = "<span class=\"dark_text\">Episodes:</span>";
            public const string DATA = "<span class=\"dark_text\">Aired:</span>";
            public const string RATTING = "<span class=\"dark_text\">Rating:</span>";
            public const string PONTOS = "<span itemprop=\"ratingValue\" class=\"score-label score-";
            public const string GENEROS = "<span itemprop=\"genre\" style=\"display: none\">";
            public const string TEMP = "";
            public const string LINK = "<meta property=\"og:url\" content=\"";
        }

        #endregion

        public AutoLoadAnimeData()
        {
            InitializeComponent();
            Init();
        }

        #region Eventos

        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            Iniciar();
        }

        private async void BtnTraduzir_Click(object sender, RoutedEventArgs e)
        {
            string text = tbLinks.box.Text;
            if (text.Trim().Length == 0) return;


            string traducao = await translator.Traduzir(text);

            //string traducao = translator.Translate(text);
            //string traducao2 = translator.Translate2(text);
            //string traducao3 = translator.Translate3(text);

            //string result = string.Format("1: {0}\n2: {1}\n3: {2}", traducao, traducao2, traducao3);
            MessageBox.Show(traducao, "Resultado", MessageBoxButton.OK);
        }
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true);
            if (!isText) return;

            //var text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
            if (tbLinks.box.Text.Length > 0 && tbLinks.box.SelectionStart == tbLinks.box.Text.Length)
            {
                tbLinks.box.Text += "\n";
                tbLinks.box.SelectionStart = tbLinks.box.Text.Length;
                tbLinks.box.SelectionLength = 0;
            }
        }

        #endregion

        #region Metodos

        private void Init()
        {
            DataObject.AddPastingHandler(tbLinks.box, OnPaste);

            tbId.box.Text = Properties.Settings.Default.currentID.ToString();
            tbLetra.box.Text = Properties.Settings.Default.letra;
            tbLinks.box.Text = Properties.Settings.Default.currentLinks;
        }

        private async void Iniciar()
        {
            if (tbLetra.box.Text.Length == 0)
            {
                MessageBox.Show("O campo de texto 'Letra' está vazio", "Ops", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (tbId.box.Text.Length == 0)
            {
                MessageBox.Show("O campo de texto 'ID' está vazio", "Ops", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Properties.Settings.Default.letra = tbLetra.box.Text;
            Properties.Settings.Default.currentID = Convert.ToInt32(tbId.box.Text);
            Properties.Settings.Default.Save();

            try
            {
                char letra = tbLetra.box.Text[0];
                currentList = AnimeCollection.LoadFileList(letra.ToString());

                //string[] filtros = { "html" };
                List<string> filesPath = new List<string>(tbLinks.box.Text.Split('\n'));// Import.GetArquivosDaPasta(Paths.HTML, filtros, isRecursiva: false);
                List<string> erros = new List<string>();
                filesPath.RemoveAll(e => string.IsNullOrWhiteSpace(e));

                Log.Msg(TAG, "Iniciar", "Init");
                foreach (var itemPath in filesPath)
                {
                    using (WebClient client = new WebClient())
                    {
                        string itemText = client.DownloadString(itemPath);
                        try
                        {
                            //string itemText = Import.GetFileText(itemPath);
                            if (itemText == null) continue;

                            int titulo_1_Int = itemText.IndexOf(Parts.TITULO_1) + Parts.TITULO_1.Length;
                            int titulo_2_Int = itemText.IndexOf(Parts.TITULO_2) + Parts.TITULO_2.Length;

                            int sinopse_int_init = itemText.IndexOf(Parts.SINOPSE) + Parts.SINOPSE.Length;
                            int sinopse_int_fim = itemText.IndexOf("</p>", sinopse_int_init - Parts.SINOPSE.Length);

                            int link_Int = itemText.IndexOf(Parts.LINK) + Parts.LINK.Length;
                            int miniatura_Int = itemText.IndexOf(Parts.MINIATURA) + Parts.MINIATURA.Length;

                            int tipo_Int = itemText.IndexOf(Parts.TIPO) + Parts.TIPO.Length;
                            int epis_Int = itemText.IndexOf(Parts.EPIS) + Parts.EPIS.Length;
                            int data_Int = itemText.IndexOf(Parts.DATA) + Parts.DATA.Length;
                            int ratting_Int = itemText.IndexOf(Parts.RATTING) + Parts.RATTING.Length;
                            int pontos_Int = itemText.IndexOf(Parts.PONTOS) + Parts.PONTOS.Length + 3;//+3 (7">)

                            List<int> generos_int = new List<int>();
                            generos_int.Add(itemText.IndexOf(Parts.GENEROS) + Parts.GENEROS.Length);
                            int i = 0;
                            while (true)
                            {
                                int position = itemText.IndexOf("<span itemprop=\"genre\" style=\"display: none\">", generos_int[i]) + 45;
                                if (position < 50) break;
                                generos_int.Add(position);
                                i++;
                            }

                            string titulo_1 = GetValue(titulo_1_Int, itemText, '<');
                            string titulo_2 = GetValue(titulo_2_Int, itemText, '<');
                            string pontos = GetValue(pontos_Int, itemText, '<');
                            string sinopse = itemText.Substring(sinopse_int_init, sinopse_int_fim - sinopse_int_init);

                            string epis = GetValue(epis_Int, itemText, '<').Replace("\n", "").Trim();
                            string data = GetValue(data_Int, itemText, '<').Replace("\n", "").TrimStart().TrimEnd();
                            string ratting = GetValue(ratting_Int, itemText, '<').Replace("\n", "").TrimStart().TrimEnd();

                            string link = GetValue(link_Int, itemText, '\"');
                            string miniatura = GetValue(miniatura_Int, itemText, '\"');
                            string tipo = GetValue(tipo_Int, itemText, '\"').ToUpper();

                            if (sinopse.Contains("No synopsis"))
                                sinopse = "";
                            else
                                sinopse = RemoverSimbolos(sinopse);
                            //Log.Msg(TAG, "Iniciar", sinopse);
                            sinopse = await Traduzir(sinopse);
                            ratting = await Traduzir(ratting);

                            int episodios = 0;
                            try
                            {
                                episodios = Convert.ToInt32(epis);
                            }
                            catch (Exception)
                            {
                                episodios = -1;
                            }
                            double pontosBase = 0;
                            try
                            {
                                pontosBase = Convert.ToDouble(pontos);
                            }
                            catch (Exception) { }

                            List<string> generos = new List<string>();
                            string generosS = "";

                            for (int j = 0; j < generos_int.Count; j++)
                            {
                                string temp = GetValue(generos_int[j], itemText, '<');
                                if (temp.ToLower().Contains("slice of life"))
                                    temp = "Estilo de Vida";
                                generosS += temp + ";";
                            }
                            //generosS.Remove(generosS.Length-1, 1);
                            generosS = await Traduzir(generosS);
                            generos.AddRange(generosS.Split(';'));

                            var anime = new AnimeItem
                            {
                                nome = titulo_1,
                                nome2 = titulo_2,
                                tipo = tipo,
                                miniatura = miniatura,
                                link = link,
                                maturidade = ratting,
                                sinopse = sinopse,
                                episodios = episodios,
                                pontosBase = pontosBase
                            };

                            if (data.Contains(" to "))
                                anime.data = data.Substring(0, data.IndexOf(" to "));
                            else
                                anime.data = data;

                            SavePai(titulo_1, titulo_2, generos);
                            Save(anime);
                        }
                        catch (Exception ex)
                        {
                            erros.Add(ex.Message);
                            Log.Erro(TAG, ex);
                            continue;
                        }
                    }

                }
                Log.Msg(TAG, "Iniciar", "OK");

                if (erros.Count > 0)
                {
                    foreach (string erro in erros)
                    {
                        var result = MessageBox.Show(erro, "Erro encontrado", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                        if (result == MessageBoxResult.Cancel) break;
                    }
                    var result2 = MessageBox.Show("Erros: " + erros.Count, "Deseja salvar?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result2 != MessageBoxResult.Yes) return;
                }

                AnimeCollection.SaveFile(letra, currentList);
                OnAddItem?.Invoke(currentList[letra + tbId.box.Text]);

                tbLinks.box.Text = "";
                int id = Convert.ToInt32(tbId.box.Text) + 1;
                string novoID = id.ToString("0000");
                tbId.box.Text = novoID;

                Properties.Settings.Default.currentID = id;
                Properties.Settings.Default.letra = string.Empty;
                Properties.Settings.Default.Save();
            }
            catch(Exception ex)
            {
                Log.Erro(TAG, ex);
                MessageBox.Show(ex.Message, "ERRO Geral", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePai(string nome, string nome2, List<string> generos)
        {
            var item = new AnimeCollection {
                nome = nome,
                nome2 = nome2,
            };
            //var parentes = tbParentes.box.Text.Split(',');
            //foreach (string g in parentes)
            //    if (g.Trim().Length > 0)
            //        item.parentes.Add(g.Trim());
            foreach (string g in generos)
                if (g.Trim().Length > 0)
                {
                    item.generos.Add(g.TrimStart().TrimEnd());
                }

            char letra = tbLetra.box.Text[0];
            var itemID = letra + tbId.box.Text;
            if (!currentList.ContainsKey(itemID))
            {
                currentList.Add(itemID, new AnimeCollection());
                currentList[itemID].items = item.items;
            }

            if (item.generos.Count > 0)
                currentList[itemID].generos = item.generos;
            if (item.parentes.Count > 0)
                currentList[itemID].parentes = item.parentes;

            if (string.IsNullOrWhiteSpace(currentList[itemID].nome))
                currentList[itemID].nome = item.nome;

            if (string.IsNullOrWhiteSpace(currentList[itemID].nome2))
                currentList[itemID].nome2 = item.nome2;

            if (currentList[itemID].parentes != null && currentList[itemID].parentes.Count == 0)
                currentList[itemID].parentes = null;
        }

        private void Save(AnimeItem item)
        {
            char letra = tbLetra.box.Text[0];
            string paiID = letra + tbId.box.Text;

            if (!currentList.ContainsKey(paiID))
                currentList.Add(paiID, new AnimeCollection());

            #region Data
            string[] dataS = item.data.Split(' ');
            string mesTemp = "";
            string mes = "";
            string dia = "";
            string ano = "";

            switch (dataS.Length)
            {
                case 1:
                    ano = dataS[0].Trim();
                    break;
                case 2:
                    mesTemp = dataS[0].Trim();
                    ano = dataS[1].Trim();
                    break;
                case 3:
                    mesTemp = dataS[0].Trim();
                    dia = dataS[1].Replace(",", "").Trim();
                    ano = dataS[2].Trim();
                    break;
            }

            switch (mesTemp.ToLower())
            {
                case "jan":
                    mes = "01";
                    break;
                case "feb":
                    mes = "02";
                    break;
                case "mar":
                    mes = "03";
                    break;
                case "apr":
                    mes = "04";
                    break;
                case "mai":
                    mes = "05";
                    break;
                case "jun":
                    mes = "06";
                    break;
                case "jul":
                    mes = "07";
                    break;
                case "aug":
                    mes = "08";
                    break;
                case "sep":
                    mes = "09";
                    break;
                case "oct":
                    mes = "10";
                    break;
                case "nov":
                    mes = "11";
                    break;
                case "dec":
                    mes = "12";
                    break;
            }

            item.data = ano;
            if (mes.Length > 0)
                item.data += "-" + mes;
            if (dia.Length > 0)
            {
                if (dia.Length == 1)
                    item.data += "-" + "0" + dia;
                else
                    item.data += "-" + dia;
            }
            #endregion

            string itemID = paiID + "_" + (currentList[paiID].items.Count + 1);
            item.id = itemID;
            currentList[paiID].items.Add(itemID, item);
        }

        string GetValue(int position, string text, char charFim)
        {
            string value = "";
            if (position >= 60)
                for (int i = position; i < position + 10000000; i++)
                {
                    if (text.Length < (i - 1) || text[i] == charFim) break;
                    value += text[i];
                }
            return RemoverSimbolos(value);
        }

        private string RemoverSimbolos(string value)
        {
            return HttpUtility.HtmlDecode(value)
                .Replace("\r", "")
                .Replace("<i>", "")
                .Replace("</i>", "")
                .Replace("<br>", "")
                .Replace("<br/>", "")
                .Replace("<br />", "")
                .Replace("â™¥>", "♥")
                .Replace("â˜†", "☆");
        }

        private async Task<string> Traduzir(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            string novoValue = await translator.Traduzir(value);
            if (string.IsNullOrWhiteSpace(novoValue))
            {
                var result = MessageBox.Show(value, "Erro ao traduzir: Deseja salvar mesmo assim?", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.No)
                    throw new Exception("Erro ao traduzir texto: " + value);
            }
            return novoValue;
        }

        #endregion
    }
}
