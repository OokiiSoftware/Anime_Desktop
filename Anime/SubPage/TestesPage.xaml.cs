using Anime.Auxiliar;
using Anime.Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Anime.SubPage
{
    public partial class TestesPage : UserControl
    {
        #region Variaveis
        const string TAG = "TestesPage";

        public delegate void onProgress(double value);
        public event onProgress OnProgress;

        private bool isInProgress = false;
        private bool incluirVariaveis;
        private bool incluirMiniaturas;
        private bool consertarProblemas;
        private bool incluirFotos;
        private bool salvarFotos;
        private bool salvarMiniaturas;
        private bool sobrescreverFotos;

        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker { WorkerSupportsCancellation = true };

        private readonly List<Error> erros = new List<Error>();

        #endregion

        public TestesPage()
        {
            InitializeComponent();
            OnProgress += BuscarErrosProgress;
            backgroundWorker.DoWork += DoWork;
            backgroundWorker.RunWorkerCompleted += RunWorkerCompleted;
        }

        #region Eventos

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            backgroundWorker.CancelAsync();
            backgroundWorker.Dispose();
            isInProgress = false;
        }

        private void BtnIniciar_Click(object sender, RoutedEventArgs e)
        {
            SwitchInProgress();
        }

        private void CbMiniaturas_Checked(object sender, RoutedEventArgs e)
        {
            cbSalvarMiniaturas.Visibility = Visibility.Visible;
            cbOverride.Visibility = Visibility.Visible;
        }

        private void CbMiniaturas_Unchecked(object sender, RoutedEventArgs e)
        {
            cbSalvarMiniaturas.Visibility = Visibility.Collapsed;
            if (!cbFotos.IsChecked.Value)
                cbOverride.Visibility = Visibility.Collapsed;
        }

        private void CbFotos_Checked(object sender, RoutedEventArgs e)
        {
            cbSalvarFotos.Visibility = Visibility.Visible;
            cbOverride.Visibility = Visibility.Visible;
        }

        private void CbFotos_Unchecked(object sender, RoutedEventArgs e)
        {
            cbSalvarFotos.Visibility = Visibility.Collapsed;
            if (!cbMiniaturas.IsChecked.Value)
                cbOverride.Visibility = Visibility.Collapsed;
        }

        private void CbVariaveis_Checked(object sender, RoutedEventArgs e)
        {
            cbCorrigir.Visibility = Visibility.Visible;
        }

        private void CbVariaveis_Unchecked(object sender, RoutedEventArgs e)
        {
            cbCorrigir.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Metodos

        private void SwitchInProgress()
        {
            if (isInProgress)
                PararBusca();
            else
                IniciarBusca();
        }

        private void IniciarBusca()
        {
            btnIniciar.Content = "Parar";
            isInProgress = true;

            incluirVariaveis = cbVariaveis.IsChecked.Value;
            incluirMiniaturas = cbMiniaturas.IsChecked.Value;
            sobrescreverFotos = cbOverride.IsChecked.Value;
            incluirFotos = cbFotos.IsChecked.Value;
            salvarMiniaturas = cbSalvarMiniaturas.IsChecked.Value;
            salvarFotos = cbSalvarFotos.IsChecked.Value;
            consertarProblemas = cbCorrigir.IsChecked.Value;

            erros.Clear();
            progressBar.Value = 0;
            backgroundWorker.RunWorkerAsync();
        }
        private void PararBusca()
        {
            backgroundWorker.CancelAsync();
            backgroundWorker.Dispose();
            btnIniciar.Content = "Parando";
            isInProgress = false;
        }

        private void _DoWork(object sender, DoWorkEventArgs e)
        {
            Log.Msg(TAG, "BuscarErros", "Init");

            bool incluirVariaveis = this.incluirVariaveis;
            bool incluirMiniaturas = this.incluirMiniaturas;
            bool sobrescreverFotos = this.sobrescreverFotos;
            bool incluirFotos = this.incluirFotos;
            bool salvarFotos = this.salvarFotos;
            bool salvarMiniaturas = this.salvarMiniaturas;
            bool consertarProblemas = this.consertarProblemas;

            var allList = new Dictionary<string, AnimeCollection>();
            string[] filtros = { "json" };
            var letras = Import.GetArquivosDaPasta(Paths.FILES_PATH, filtros);
            int childsCount = 0;

            foreach (string letraFile in letras)
            {
                string letra = Path.GetFileNameWithoutExtension(letraFile);
                var listTemp = AnimeCollection.LoadFileList(letra);
                foreach (var key in listTemp.Keys)
                    if (!allList.ContainsKey(key))
                    {
                        allList.Add(key, listTemp[key]);
                        childsCount += listTemp[key].items.Count;
                    }
                    else Log.Msg(Name, "BtnPublicar", "Key duplicado", key);
            }

            Log.Msg(TAG, "BuscarErros", "Load OK > PaisCount: " + allList.Count, "ChildsCount: " + childsCount);

            double progress = 0;
            foreach (var itemList in allList.Values)
            {
                foreach (var item in itemList.items.Values)
                {
                    if (!isInProgress) break;
                    try
                    {
                        string erro = "";

                        if (incluirVariaveis)
                        {
                            if (string.IsNullOrWhiteSpace(item.nome))
                                erro += "nome vazio";
                            if (string.IsNullOrWhiteSpace(item.id))
                                erro += ";id vazio";
                            if (string.IsNullOrWhiteSpace(item.miniatura))
                                erro += ";miniatura vazio";
                            if (string.IsNullOrWhiteSpace(item.data))
                                erro += ";data vazio";
                            if (string.IsNullOrWhiteSpace(item.tipo))
                                erro += ";tipo vazio";
                            if (string.IsNullOrWhiteSpace(item.foto))
                                erro += ";foto vazio";
                            if (string.IsNullOrWhiteSpace(item.link))
                                erro += ";link vazio";
                            if (string.IsNullOrWhiteSpace(item.sinopse))
                                erro += ";sinopse vazio";

                            if (consertarProblemas)
                            {
                                if (string.IsNullOrWhiteSpace(item.nome2) || item.nome.Equals(item.nome2))
                                    item.nome2 = null;

                                item.nome = Import.RemoverSimbolos(item.nome);
                                item.nome2 = Import.RemoverSimbolos(item.nome2);
                                item.sinopse = Import.RemoverSimbolos(item.sinopse);
                                item.maturidade = Import.RemoverSimbolos(item.maturidade);
                                item.tipo = Import.RemoverSimbolos(item.tipo);

                                if (item.tipo.ToLower().Equals("music"))
                                    itemList.items.Remove(item.id);
                            }
                        }

                        if (incluirMiniaturas)
                        {
                            if (salvarMiniaturas)
                            {
                                string temp = SalvarFoto(item.miniatura, Paths.MINIATURAS, item.id, sobrescreverFotos).Result;
                                if (temp != null)
                                    erro += ";miniatura > " + temp;
                            }
                            else
                            {
                                string temp = TestarUrl(item.miniatura);
                                if (temp != null)
                                    erro += ";miniatura > " + temp;
                            }
                        }
                        if (!string.IsNullOrEmpty(item.foto) && incluirFotos)
                        {
                            if (salvarFotos)
                            {
                                string temp = SalvarFoto(item.foto, Paths.FOTOS, item.id, sobrescreverFotos).Result;
                                if (temp != null)
                                    erro += ";foto > " + temp;
                            }
                            else
                            {
                                string temp = TestarUrl(item.foto);
                                if (temp != null)
                                    erro += ";foto > " + temp;
                            }
                        }

                        if (!string.IsNullOrEmpty(erro))
                            throw new Exception(erro);

                    }
                    catch (Exception ex)
                    {
                        erros.Add(new Error(ItemId: item.id, Descricao: ex.Message));
                        lbErros.Dispatcher.Invoke(() => lbErros.Content = erros.Count);
                        Log.Erro(TAG, ex);
                        continue;
                    }
                    progress++;
                    OnProgress?.Invoke((progress / childsCount) * 100.0);
                }

                if (consertarProblemas)
                {
                    AnimeCollection.SaveFile(itemList.Letra(), allList);
                }
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            Log.Msg(TAG, "BuscarErros", "Init");

            bool incluirVariaveis = this.incluirVariaveis;
            bool incluirMiniaturas = this.incluirMiniaturas;
            bool sobrescreverFotos = this.sobrescreverFotos;
            bool incluirFotos = this.incluirFotos;
            bool salvarFotos = this.salvarFotos;
            bool salvarMiniaturas = this.salvarMiniaturas;
            bool consertarProblemas = this.consertarProblemas;

            var allList = new Dictionary<string, Dictionary<string, AnimeCollection>>();
            string[] filtros = { "json" };
            var letras = Import.GetArquivosDaPasta(Paths.FILES_PATH, filtros);
            int childsCount = 0;

            foreach (string letraFile in letras)
            {
                string letra = Path.GetFileNameWithoutExtension(letraFile);
                var listTemp = AnimeCollection.LoadFileList(letra);
                allList.Add(letraFile, listTemp);

                foreach (var key in listTemp.Keys)
                    if (!allList.ContainsKey(key))
                    {
                        childsCount += listTemp[key].items.Count;
                    }
                    else Log.Msg(Name, "BtnPublicar", "Key duplicado", key);
            }

            Log.Msg(TAG, "BuscarErros", "Load OK > PaisCount: " + allList.Count, "ChildsCount: " + childsCount);

            double progress = 0;
            foreach (var allListItem in allList.Values)
            {
                if (!isInProgress) break;
                foreach (var itemList in allListItem.Values)
                {
                    if (!isInProgress) break;
                    foreach (var item in itemList.items.Values)
                    {
                        if (!isInProgress) break;
                        try
                        {
                            string erro = "";

                            if (incluirVariaveis)
                            {
                                if (consertarProblemas)
                                {
                                    if (string.IsNullOrWhiteSpace(item.nome2) || item.nome.Equals(item.nome2))
                                        item.nome2 = null;

                                    item.nome = Import.RemoverSimbolos(item.nome);
                                    item.nome2 = Import.RemoverSimbolos(item.nome2);
                                    item.sinopse = Import.RemoverSimbolos(item.sinopse);
                                    item.maturidade = Import.RemoverSimbolos(item.maturidade);
                                    item.tipo = Import.RemoverSimbolos(item.tipo);

                                    if (item.tipo.ToLower().Equals("music") || item.tipo.ToLower().Equals("airing"))
                                    {
                                        MessageBox.Show($"{item.id}", "Tipo Desconhecido", MessageBoxButton.OK, MessageBoxImage.Warning);
                                    }

                                    if (item.miniatura.Contains("apple-touch-icon-256"))
                                        item.miniatura = string.Empty;

                                    item.sinopse = item.sinopse.Replace("\n[Escrito por MAL Rewrite]", string.Empty);

                                    string test = "\n(Fonte:";
                                    if (item.sinopse.Contains(test))
                                    {
                                        //Log.Msg(TAG, "consertarProblemas", "Fonte");
                                        string sinopse = item.sinopse;
                                        int indexInit = sinopse.IndexOf(test);
                                        if (indexInit > 0)
                                        {
                                            int indexFim = sinopse.IndexOf(")", indexInit +1);

                                            if (indexFim > 1)
                                            {
                                                string fonte = sinopse.Substring(indexInit, indexFim - indexInit);
                                                item.sinopse = item.sinopse.Replace(fonte, string.Empty);
                                                Log.Msg(TAG, "consertarProblemas", fonte);
                                            }
                                        }
                                        
                                    }
                                    
                                    if (!string.IsNullOrWhiteSpace(item.trailer) && item.trailer.Contains("youtube"))
                                    {
                                        string inicio = "https://www.youtube.com/embed/";
                                        string fim = "?";
                                        string trailer = item.trailer;

                                        int inicioPosition = trailer.IndexOf(inicio) + inicio.Length;
                                        int fimPosition = trailer.IndexOf(fim, inicioPosition);

                                        trailer = trailer.Substring(inicioPosition, fimPosition - inicioPosition);

                                        item.trailer = trailer;
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(item.nome))
                                        erro += "nome vazio";
                                    if (string.IsNullOrWhiteSpace(item.id))
                                        erro += ";id vazio";
                                    if (string.IsNullOrWhiteSpace(item.miniatura))
                                        erro += ";miniatura vazio";
                                    if (string.IsNullOrWhiteSpace(item.data))
                                        erro += ";data vazio";
                                    if (string.IsNullOrWhiteSpace(item.tipo))
                                        erro += ";tipo vazio";
                                    if (string.IsNullOrWhiteSpace(item.foto))
                                        erro += ";foto vazio";
                                    if (string.IsNullOrWhiteSpace(item.link))
                                        erro += ";link vazio";
                                    if (string.IsNullOrWhiteSpace(item.sinopse))
                                        erro += ";sinopse vazio";
                                }
                            }

                            if (incluirMiniaturas)
                            {
                                if (salvarMiniaturas)
                                {
                                    string temp = SalvarFoto(item.miniatura, Paths.MINIATURAS, item.id, sobrescreverFotos).Result;
                                    if (temp != null)
                                        erro += ";miniatura > " + temp;
                                }
                                else
                                {
                                    string temp = TestarUrl(item.miniatura);
                                    if (temp != null)
                                        erro += ";miniatura > " + temp;
                                }
                            }

                            if (!string.IsNullOrEmpty(item.foto) && incluirFotos)
                            {
                                if (salvarFotos)
                                {
                                    string temp = SalvarFoto(item.foto, Paths.FOTOS, item.id, sobrescreverFotos).Result;
                                    if (temp != null)
                                        erro += ";foto > " + temp;
                                }
                                else
                                {
                                    string temp = TestarUrl(item.foto);
                                    if (temp != null)
                                        erro += ";foto > " + temp;
                                }
                            }

                            if (!string.IsNullOrEmpty(erro))
                                throw new Exception(erro);
                        }
                        catch (Exception ex)
                        {
                            erros.Add(new Error(ItemId: item.id, Descricao: ex.Message));
                            lbErros.Dispatcher.Invoke(() => lbErros.Content = erros.Count);
                            Log.Erro(TAG, ex);
                            continue;
                        }

                        progress++;
                        OnProgress?.Invoke((progress / childsCount) * 100.0);
                    }

                    if (consertarProblemas)
                        AnimeCollection.SaveFile(itemList.Letra(), allListItem);
                    
                }
                
            }
        }

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar.Value = 0;
            Log.Msg(TAG, "BuscarErro", "COMPLETE", "Erros", erros.Count);
            PararBusca();
            btnIniciar.Content = "Iniciar";
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

        private void BuscarErrosProgress(double value)
        {
            //Log.Msg(TAG, "Buscando Erros", "% " + value.ToString("F"));

            lbProgress.Dispatcher.Invoke(() => lbProgress.Content = value.ToString("F"));
            progressBar.Dispatcher.Invoke(() => progressBar.Value = value);
        }

        private string TestarUrl(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (response.StatusCode == HttpStatusCode.OK)
                    response.Close();
                else
                    throw new Exception("link inválido");
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<string> SalvarFoto(string url, string directory, string fileName, bool sobrescrever)
        {
            try
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string ext = ".jpg";
                string itemFolder = fileName[0].ToString() + "\\";
                string filePath = directory + itemFolder + fileName + ext;

                if (File.Exists(filePath) && !sobrescrever)
                    return null;

                var webClient = new WebClient();
                await webClient.DownloadFileTaskAsync(url, filePath);
                webClient.Dispose();

                return null;
            }
            catch (Exception ex)
            {
                Log.Erro(TAG, ex);
                return ex.Message;
            }
        }

        #endregion

    }
}
