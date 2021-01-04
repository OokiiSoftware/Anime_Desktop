using Anime.Auxiliar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Anime.Modelo
{
    public enum AnimeType { TV, MOVIE, ONA, OVA, SPECIAL, INDEFINIDO }

    public class AnimeItem
    {
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

            if (!string.IsNullOrWhiteSpace(item.nome2))
                nome2 = item.nome2;
        }
        public string id { get; set; }
        public string nome { get; set; }
        public string nome2 { get; set; }
        public string miniatura { get; set; }
        public string data { get; set; }
        public string tipo { get; set; } = AnimeType.INDEFINIDO.ToString();
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
        public string nome { get; set; }
        public string nome2 { get; set; }
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
        public string nome { get; set; }
        public string nome2 { get; set; }
        public List<string> generos { get; set; } = new List<string>();

        public AnimeCollectionBasico(AnimeCollection list)
        {
            items = new Dictionary<string, AnimeItemBasico>();
            foreach (var item in list.items.Values)
            {
                if (!items.ContainsKey(item.id))
                    items.Add(item.id, new AnimeItemBasico(item));
            }

            generos = list.generos;
            nome = list.nome;
            nome2 = list.nome2;
        }

        public Dictionary<string, AnimeItemBasico> items { get; }
    }
    public class AnimeCollectionComplemento
    {
        public List<string> parentes { get; set; } = new List<string>();

        public AnimeCollectionComplemento(AnimeCollection list)
        {
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
