using Anime.Auxiliar;
using Anime.Translator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Anime.Modelo
{
    public enum AnimeType { TV, MOVIE, ONA, OVA, SPECIAL, INDEFINIDO }

    public class AnimeItem
    {
        #region Variaveis
        public string id { get; set; }
        public string nome { get; set; }
        public string nome2 { get; set; }
        public string foto { get; set; }
        public string miniatura { get; set; }
        public string link { get; set; }
        public string trailer { get; set; }
        public string sinopse { get; set; }
        public string data { get; set; }
        public int episodios { get; set; }
        public string maturidade { get; set; }
        public double pontosBase { get; set; }
        public string tipo { get; set; } = AnimeType.INDEFINIDO.ToString();

        public List<string> generos = new List<string>();
        public List<string> links = new List<string>();

        #endregion

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
            public const string TRAILER = "<a class=\"iframe js-fancybox-video video-unit promotion\" href=\"";
            public const string LINK = "<meta property=\"og:url\" content=\"";
            public const string TEMP = "";
        }

        public AnimeItem() { }
        public AnimeItem(string htmlText)
        {
            int titulo_1_Int = htmlText.IndexOf(Parts.TITULO_1) + Parts.TITULO_1.Length;
            int titulo_2_Int = htmlText.IndexOf(Parts.TITULO_2) + Parts.TITULO_2.Length;

            int sinopse_int_init = htmlText.IndexOf(Parts.SINOPSE) + Parts.SINOPSE.Length;
            int sinopse_int_fim = htmlText.IndexOf("</p>", sinopse_int_init - Parts.SINOPSE.Length);

            int trailer_int = htmlText.IndexOf(Parts.TRAILER) + Parts.TRAILER.Length;
            int link_Int = htmlText.IndexOf(Parts.LINK) + Parts.LINK.Length;
            int miniatura_Int = htmlText.IndexOf(Parts.MINIATURA) + Parts.MINIATURA.Length;

            int tipo_Int = htmlText.IndexOf(Parts.TIPO) + Parts.TIPO.Length;
            int epis_Int = htmlText.IndexOf(Parts.EPIS) + Parts.EPIS.Length;
            int data_Int = htmlText.IndexOf(Parts.DATA) + Parts.DATA.Length;
            int ratting_Int = htmlText.IndexOf(Parts.RATTING) + Parts.RATTING.Length;
            int pontos_Int = htmlText.IndexOf(Parts.PONTOS) + Parts.PONTOS.Length + 3;//+3 (7">)

            List<int> generos_int = new List<int>
            {
                htmlText.IndexOf(Parts.GENEROS) + Parts.GENEROS.Length
            };
            int i = 0;
            while (true)
            {
                int position = htmlText.IndexOf("<span itemprop=\"genre\" style=\"display: none\">", generos_int[i]) + 45;
                if (position < 50) break;
                generos_int.Add(position);
                i++;
            }

            string titulo_1 = GetValue(titulo_1_Int, htmlText, '<');
            string titulo_2 = GetValue(titulo_2_Int, htmlText, '<');
            string pontos = GetValue(pontos_Int, htmlText, '<');
            string sinopse = htmlText.Substring(sinopse_int_init, sinopse_int_fim - sinopse_int_init);

            string epis = GetValue(epis_Int, htmlText, '<').Replace("\n", "").Trim();
            string data = GetValue(data_Int, htmlText, '<').Replace("\n", "").TrimStart().TrimEnd();
            string ratting = GetValue(ratting_Int, htmlText, '<').Replace("\n", "").TrimStart().TrimEnd();

            string trailer = GetValue(trailer_int, htmlText, '\"')/*.Replace("&amp;", "&")*/;
            string link = GetValue(link_Int, htmlText, '\"');
            string miniatura = GetValue(miniatura_Int, htmlText, '\"');
            string tipo = GetValue(tipo_Int, htmlText, '\"').ToUpper();

            if (sinopse.Contains("No synopsis"))
                sinopse = "";
            else
                sinopse = Import.RemoverSimbolos(sinopse, copySimbolo: true, isPtBr: false);
            //Log.Msg(TAG, "Iniciar", sinopse);
            sinopse = Traduzir(sinopse).Result;
            ratting = Traduzir(ratting).Result;

            int episodios;
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
                string temp = GetValue(generos_int[j], htmlText, '<');
                if (temp.ToLower().Contains("slice of life"))
                    temp = "Estilo de Vida";
                generosS += temp + ";";
            }
            //generosS.Remove(generosS.Length-1, 1);
            
            if (data.Contains(" to "))
                this.data = data.Substring(0, data.IndexOf(" to "));
            else
                this.data = data;

            this.nome = titulo_1;
            this.nome2 = titulo_2;
            this.pontosBase = pontosBase;
            this.episodios = episodios;
            this.miniatura = miniatura;
            this.maturidade = ratting;
            this.sinopse = sinopse;
            this.trailer = trailer;
            this.link = link;
            this.tipo = tipo;

            generosS = Traduzir(generosS).Result;
            generos.AddRange(generosS.Split(';'));
            this.generos.AddRange(generos);
        }

        public string GenerosToString()
        {
            string s = "";
            foreach (string d in generos)
                s += $"{d},";
            return s;
        }

        private string GetValue(int position, string text, char charFim)
        {
            string value = "";
            if (position >= 60)
                for (int i = position; i < position + 10000000; i++)
                {
                    if (text.Length < (i - 1) || text[i] == charFim) break;
                    value += text[i];
                }
            return Import.RemoverSimbolos(value, copySimbolo: true, isPtBr: false);
        }

        private async Task<string> Traduzir(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            string novoValue = await Tradutor.instance.Traduzir(value);
            if (string.IsNullOrWhiteSpace(novoValue))
            {
                var result = MessageBox.Show(value, "Erro ao traduzir: Deseja salvar mesmo assim?", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.No)
                    throw new Exception("Erro ao traduzir texto: " + value);
            }
            return novoValue;
        }

    }
    public class AnimeItemBasico
    {
        public AnimeItemBasico(AnimeItem item)
        {
            id = item.id;
            nome = item.nome;
            miniatura = item.miniatura;
            data = item.data;
            tipo = item.tipo;

            links.AddRange(item.links);

            if (!string.IsNullOrWhiteSpace(item.nome2))
                nome2 = item.nome2;
        }
        public string id { get; set; }
        public string nome { get; set; }
        public string nome2 { get; set; }
        public string miniatura { get; set; }
        public string data { get; set; }
        public string tipo { get; set; } = AnimeType.INDEFINIDO.ToString();

        public List<string> links = new List<string>();
    }
    public class AnimeItemComplemento
    {
        public AnimeItemComplemento(AnimeItem item)
        {
            id = item.id;
            link = item.link;
            episodios = item.episodios;
            pontosBase = item.pontosBase;

            if (!string.IsNullOrWhiteSpace(item.foto))
                foto = item.foto;
            if (!string.IsNullOrWhiteSpace(item.sinopse))
                sinopse = item.sinopse;
            if (!string.IsNullOrWhiteSpace(item.maturidade))
                maturidade = item.maturidade;
            if (!string.IsNullOrWhiteSpace(item.trailer))
                trailer = item.trailer;
        }
        public string id { get; set; }
        public string foto { get; set; }
        public string link { get; set; }
        public string trailer { get; set; }
        public int episodios { get; set; }
        public string sinopse { get; set; }
        public string maturidade { get; set; }
        public double pontosBase { get; set; }

    }

    public class AnimeCollection
    {
        public string id { get; set; }
        public string nome { get; set; }
        public string nome2 { get; set; }

        public List<string> links = new List<string>();

        private char letra;

        public Dictionary<string, AnimeItem> items { get; set; } = new Dictionary<string, AnimeItem>();
        
        public List<string> generos { get; set; } = new List<string>();
        public List<string> parentes { get; set; } = new List<string>();

        public char Letra()
        {
            return letra;
        }
        public int QuantChars()
        {
            int i = 0;
            foreach (var item in items.Values)
            {
                i += item.sinopse.Length;
            }
            return i;
        }

        public static Dictionary<string, Dictionary<string, AnimeCollection>> LoadFilesList(List<string> letras)
        {
            if (letras == null)
            {
                letras = new List<string>
                {
                    "a",
                    "b",
                    "c",
                    "d",
                    "e",
                    "f",
                    "g",
                    "h",
                    "i",
                    "j",
                    "k",
                    "l",
                    "m",
                    "n",
                    "o",
                    "p",
                    "q",
                    "r",
                    "s",
                    "t",
                    "u",
                    "v",
                    "w",
                    "x",
                    "y",
                    "z",
                    "_"
                };
            }

            var allList = new Dictionary<string, Dictionary<string, AnimeCollection>>();

            foreach (string letraFile in letras)
            {
                string letra = Path.GetFileNameWithoutExtension(letraFile);
                var listTemp = LoadFileList(letra);
                allList.Add(letraFile, listTemp);
            }

            return allList;
        }
        public static Dictionary<string, AnimeCollection> LoadFileList(string letra)
        {
            var list = new Dictionary<string, AnimeCollection>();
            string animeFileName = Paths.FILES_PATH + letra + ".json";
            try
            {
                if (File.Exists(animeFileName))
                {
                    var json = File.ReadAllText(animeFileName);
                    list = JsonConvert.DeserializeObject<Dictionary<string, AnimeCollection>>(json);
                    foreach (var temp in list.Values)
                        temp.letra = letra[0];
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception) { }
            return list;
        }

        public static void SaveFile(char letra, Dictionary<string, AnimeCollection> data)
        {
            string animeFileName = Paths.FILES_PATH + letra + ".json";
            try
            {
                if (!Directory.Exists(Paths.FILES_PATH))
                    Directory.CreateDirectory(Paths.FILES_PATH);

                var json = JsonConvert.SerializeObject(data);
                File.WriteAllText(animeFileName, json);
            }
            catch (Exception ex)
            {
                Log.Erro("AnimeCollection", ex);
            }
        }

    }
    public class AnimeCollectionBasico
    {
        public string id { get; set; }
        public string nome { get; set; }
        public string nome2 { get; set; }

        public List<string> links = new List<string>();
        public List<string> generos { get; set; } = new List<string>();

        public AnimeCollectionBasico(AnimeCollection list, string key)
        {
            id = key;
            items = new Dictionary<string, AnimeItemBasico>();
            foreach (var item in list.items.Values)
            {
                if (!items.ContainsKey(item.id))
                    items.Add(item.id, new AnimeItemBasico(item));
            }

            links.AddRange(list.links);
            generos = list.generos;
            nome = list.nome;
            nome2 = list.nome2;
        }

        public Dictionary<string, AnimeItemBasico> items { get; }
    }
    public class AnimeCollectionComplemento
    {
        public string id { get; set; }
        public List<string> parentes { get; set; } = new List<string>();


        public AnimeCollectionComplemento(AnimeCollection list, string key)
        {
            id = key;
            items = new Dictionary<string, AnimeItemComplemento>();
            foreach (var item in list.items.Values)
            {
                if (!items.ContainsKey(item.id))
                    items.Add(item.id, new AnimeItemComplemento(item));
            }
            parentes = list.parentes;
        }

        public Dictionary<string, AnimeItemComplemento> items { get; }
    }
}
