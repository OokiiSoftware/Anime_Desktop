using Anime.Modelo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Anime.Ferramentas
{
    public partial class AnimeItemLayout : UserControl
    {
        public AnimeItemLayout()
        {
            InitializeComponent();
        }
        public AnimeItemLayout(AnimeItem item)
        {
            InitializeComponent();
            nome.Text = item.nome;
            nome2.Text = item.nome2;
            id.Text = item.id;
            parentes.Text = "";// item.link;
            sinopse.Text = "";// item.sinopse;
            data.Text = item.data;
            tipo.Text = item.tipo;
            eps.Text = item.episodios.ToString();

            try
            {
                Uri uri;
                if (string.IsNullOrWhiteSpace(item.foto))
                    uri = new Uri(item.miniatura);
                else
                    uri = new Uri(item.foto);
                    foto.Source = new BitmapImage(uri);
            }
            catch(Exception ex)
            {
                Console.WriteLine("AnimeItemLayout: " + ex.Message);
            }

            if (nome2.Text.Trim().Length == 0) nome2.Visibility = Visibility.Collapsed;
            if (sinopse.Text.Trim().Length == 0) sinopse.Visibility = Visibility.Collapsed;
            if (data.Text.Trim().Length == 0) data.Visibility = Visibility.Collapsed;
            if (tipo.Text.Trim().Length == 0) tipo.Visibility = Visibility.Collapsed;
        }
    }
}
