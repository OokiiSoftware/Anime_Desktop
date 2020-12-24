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
        public string sinopse { get; set; }
        public string data { get; set; }
        public string maturidade { get; set; }
        public int episodios { get; set; }
        public double pontosBase { get; set; }
        public string tipo { get; set; } = AnimeType.INDEFINIDO.ToString();
    }
    public class AnimeBasico
    {
        public AnimeBasico(AnimeItem item)
        {
            id = item.id;
            nome = item.nome;
            nome2 = item.nome2;
            miniatura = item.miniatura;
            data = item.data;
        }
        public string id { get; set; }
        public string nome { get; set; }
        public string nome2 { get; set; }
        public string miniatura { get; set; }
        public string data { get; set; }
    }
    public class AnimeComplemento
    {
        public AnimeComplemento(AnimeItem item)
        {
            id = item.id;
            foto = item.foto;
            link = item.link;
            tipo = item.tipo;
            sinopse = item.sinopse;
            episodios = item.episodios;
        }
        public string id { get; set; }
        public string foto { get; set; }
        public string link { get; set; }
        public int episodios { get; set; }
        public string sinopse { get; set; }
        public string tipo { get; set; } = AnimeType.INDEFINIDO.ToString();
    }

    public class AnimeCollection
    {
        public class Complemento
        {
            public List<string> parentes { get; set; } = new List<string>();

            public Complemento(AnimeCollection list) {
                items = new Dictionary<string, AnimeComplemento>();
                foreach (var item in list.items.Values)
                {
                    if (!items.ContainsKey(item.id))
                        items.Add(item.id, new AnimeComplemento(item));
                }

                parentes = list.parentes;
            }

            public Dictionary<string, AnimeComplemento> items { get; }
        }
        public class Basico
        {
            public string nome { get; set; }
            public string nome2 { get; set; }
            public List<string> generos { get; set; } = new List<string>();

            public Basico(AnimeCollection list)
            {
                items = new Dictionary<string, AnimeBasico>();
                foreach (var item in list.items.Values)
                {
                    if (!items.ContainsKey(item.id))
                        items.Add(item.id, new AnimeBasico(item));
                }

                generos = list.generos;
                nome = list.nome;
                nome2 = list.nome2;
            }

            public Dictionary<string, AnimeBasico> items { get; }
        }

        public string nome { get; set; }
        public string nome2 { get; set; }
        public Dictionary<string, AnimeItem> items { get; set; } = new Dictionary<string, AnimeItem>();
        
        public List<string> generos { get; set; } = new List<string>();
        public List<string> parentes { get; set; } = new List<string>();

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
}
