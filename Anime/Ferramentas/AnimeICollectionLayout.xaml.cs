using Anime.Modelo;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Anime.Ferramentas
{
    public partial class AnimeICollectionLayout : UserControl
    {
        public AnimeICollectionLayout()
        {
            InitializeComponent();
        }
        public AnimeICollectionLayout(AnimeCollection item)
        {
            InitializeComponent();
            nome.Text = item.nome;
            nome2.Text = item.nome2;
            filhos.Text += item.items.Count.ToString();
            if (item.parentes != null)
                foreach (string s in item.parentes)
                    parentes.Text += s + ", ";
            if (item.generos != null)
                foreach (string s in item.generos)
                generos.Text += s + ", ";

            try
            {
                var items = item.items.Values.ToList();

                Uri uri;
                var child = items[0];
                id.Text = child.id;
                if (string.IsNullOrWhiteSpace(child.foto))
                    uri = new Uri(child.miniatura);
                else
                    uri = new Uri(child.foto);
                    foto.Source = new BitmapImage(uri);
            }
            catch(Exception ex)
            {
                Console.WriteLine("AnimeItemLayout: " + ex.Message);
            }

            if (id.Text.Trim().Length == 0) id.Visibility = Visibility.Collapsed;
            if (nome2.Text.Trim().Length == 0) nome2.Visibility = Visibility.Collapsed;
            if (generos.Text.Trim().Length == 0) generos.Visibility = Visibility.Collapsed;
        }
    }
}
