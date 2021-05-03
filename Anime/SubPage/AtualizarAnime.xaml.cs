using Anime.Auxiliar;
using Anime.Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
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

namespace Anime.SubPage
{
    public partial class AtualizarAnimePage : UserControl
    {
        #region Variaveis
        private const string TAG = "AtualizarAnime";

        public delegate void onProgress(double value);
        public event onProgress OnProgress;

        private bool isInProgress = false;
        private readonly List<Error> erros = new List<Error>();
        private readonly List<string> letras = new List<string>();

        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };

        private bool miniaturas;
        private bool trailer;
        private bool sinopse;
        private bool episodios;
        private bool maturidade;
        private bool pontos;
        private bool tipo;
        private bool generos;

        #endregion

        public AtualizarAnimePage()
        {
            InitializeComponent();
            OnProgress += BuscarProgress;
            backgroundWorker.DoWork += DoWork;
            backgroundWorker.RunWorkerCompleted += RunWorkerCompleted;
        }

        #region Eventos

        private void BtnAtualizarTudo_Click(object sender, RoutedEventArgs e)
        {
            Atualizar();
        }

        private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
        {
            Atualizar(tbAnimeID.box.Text);
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            bool miniaturas = this.miniaturas;
            bool trailer = this.trailer;
            bool sinopse = this.sinopse;
            bool episodios = this.episodios;
            bool maturidade = this.maturidade;
            bool pontos = this.pontos;
            bool tipo = this.tipo;
            bool generos = this.generos;

            var allList = AnimeCollection.LoadFilesList(letras);
            double progress = 0;
            double childsCount = 0;
            foreach (var allListItem in allList.Values)
            {
                if (!isInProgress) break;
                foreach (AnimeCollection itemList in allListItem.Values)
                {
                    if (!isInProgress) break;
                    bool canSave = false;
                    childsCount += itemList.items.Count;

                    if (generos)
                        itemList.generos.Clear();

                    foreach (AnimeItem item in itemList.items.Values)
                    {
                        progress++;
                        OnProgress?.Invoke((progress / childsCount) * 100.0);
                        if (!isInProgress) break;

                        try
                        {
                            if (string.IsNullOrWhiteSpace(item.link))
                                throw new Exception("Link null");

                            using (WebClient client = new WebClient())
                            {
                                string htmlText = client.DownloadString(item.link);
                                var anime = new AnimeItem(htmlText);

                                if (miniaturas && !string.IsNullOrWhiteSpace(item.miniatura))
                                    item.miniatura = anime.miniatura;

                                if (trailer && !string.IsNullOrWhiteSpace(item.trailer))
                                    item.trailer = anime.trailer;

                                if (sinopse && !string.IsNullOrWhiteSpace(item.sinopse))
                                    item.sinopse = anime.sinopse;

                                if (episodios)
                                    item.episodios = anime.episodios;

                                if (maturidade && !string.IsNullOrWhiteSpace(item.maturidade))
                                    item.maturidade = anime.maturidade;

                                if (pontos)
                                    item.pontosBase = anime.pontosBase;

                                if (tipo && !string.IsNullOrWhiteSpace(item.tipo))
                                    item.tipo = anime.tipo;

                                if (generos && !string.IsNullOrWhiteSpace(item.GenerosToString()))
                                {
                                    foreach (string genero in item.generos)
                                        if (!itemList.generos.Contains(genero))
                                            itemList.generos.Add(genero);
                                }
                                canSave = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            erros.Add(new Error(ItemId: item.id, Descricao: ex.Message));
                            //lbErros.Dispatcher.Invoke(() => lbErros.Content = erros.Count);
                            Log.Erro(TAG, ex);
                            continue;

                        }
                    }

                    if (canSave)
                        AnimeCollection.SaveFile(itemList.Letra(), allListItem);
                }
            }
        }
        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            Log.Msg(TAG, "BuscarErro", "COMPLETE", "Erros", erros.Count);
            PararBusca();
            btnAtualizarTudo.Content = "Atualizar Tudo";
            btnAtualizar.Content = "Atualizar";
            if (erros.Count > 0)
            {
                try
                {
                    string FileName = "ërros" + ".json";
                    var json = JsonConvert.SerializeObject(erros);
                    File.WriteAllText(FileName, json);
                }
                catch (Exception ex)
                {
                    string msg = string.Format("Verificação concluida\nErros encontrados: {0}\nErro: {1}", erros.Count, ex.Message);
                    MessageBox.Show(msg, "Erro ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            MessageBox.Show("Erros encontrados: " + erros.Count, "Verificação concluida", MessageBoxButton.OK);
        }

        #endregion

        #region Metodos

        private void Atualizar()
        {
            string[] filtros = { "json" };
            letras.Clear();
            letras.AddRange(Import.GetArquivosDaPasta(Paths.FILES_PATH, filtros));
            SwitchInProgress();
        }

        private void Atualizar(string animeID)
        {
            if (string.IsNullOrWhiteSpace(animeID)) return;

            if (animeID.Length == 1)
            {
                var result = MessageBox.Show($"Atualizar todos os animes da letra {animeID}?", "Importante", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    AtualizarLetra(animeID);
                }
            }
            else
            {
                _AtualizarAnime(animeID);
            }
        }


        private void AtualizarLetra(string letra)
        {
            letras.Clear();
            letras.Add(Paths.FILES_PATH + letra + ".json");
            SwitchInProgress();
        }

        private void _AtualizarAnime(string animeID)
        {

        }


        private void SwitchInProgress()
        {
            if (isInProgress)
                PararBusca();
            else
                IniciarBusca();
        }

        private void IniciarBusca()
        {
            btnAtualizarTudo.Content = "Parar";
            btnAtualizar.Content = "Parar";
            isInProgress = true;

            CheckBool();

            if (AllUnchecked())
            {
                MessageBox.Show("Não tem nada marcado, zé ruela", "Rum", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            //lbErros.Content = 0;

            erros.Clear();
            progressBar.Value = 0;
            backgroundWorker.RunWorkerAsync();
        }

        private void CheckBool()
        {
            miniaturas = cbMiniatura.IsChecked.Value;
            trailer = cbTrailer.IsChecked.Value;
            sinopse = cbSinopse.IsChecked.Value;
            episodios = cbEpisodios.IsChecked.Value;
            maturidade = cbMaturidade.IsChecked.Value;
            pontos = cbPontos.IsChecked.Value;
            tipo = cbTipo.IsChecked.Value;
            generos = cbGeneros.IsChecked.Value;
        }

        private bool AllUnchecked()
        {
            return !miniaturas
               && !trailer
               && !sinopse
               && !episodios
               && !maturidade
               && !pontos
               && !tipo
               && !generos;
        }

        private void PararBusca()
        {
            backgroundWorker.CancelAsync();
            backgroundWorker.Dispose();
            btnAtualizarTudo.Content = "Parando";
            btnAtualizar.Content = "Parando";
            isInProgress = false;
        }

        private void BuscarProgress(double value)
        {
            //Log.Msg(TAG, "Buscando Erros", "% " + value.ToString("F"));

            //lbProgress.Dispatcher.Invoke(() => lbProgress.Content = value.ToString("F"));
            progressBar.Dispatcher.Invoke(() => progressBar.Value = value);
        }

        #endregion
    }
}
