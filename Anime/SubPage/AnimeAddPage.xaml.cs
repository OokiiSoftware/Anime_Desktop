using Anime.Modelo;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anime.SubPage
{
    public partial class AnimeAddPage : UserControl
    {
        #region Variaveis
        public delegate void onAddItem(AnimeItem item);
        public event onAddItem OnAddItem;

        private const int EPS_ERRO = -10;
        private Dictionary<string, AnimeCollection> currentList;
        #endregion

        public AnimeAddPage()
        {
            InitializeComponent();
            Init();
        }

        #region Events

        private void UserControl_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                switch (e.Key)
                {
                    case Key.D1:
                    case Key.NumPad1:
                        cbTipo.SelectedIndex = 0;
                        break;
                    case Key.D2:
                    case Key.NumPad2:
                        cbTipo.SelectedIndex = 1;
                        break;
                    case Key.D3:
                    case Key.NumPad3:
                        cbTipo.SelectedIndex = 2;
                        break;
                    case Key.D4:
                    case Key.NumPad4:
                        cbTipo.SelectedIndex = 3;
                        break;
                    case Key.D5:
                    case Key.NumPad5:
                        cbTipo.SelectedIndex = 4;
                        break;
                    case Key.D6:
                    case Key.NumPad6:
                        cbTipo.SelectedIndex = 5;
                        break;
                    case Key.Up:
                        if (cbTipo.SelectedIndex - 1 >= 0)
                            cbTipo.SelectedIndex--;
                        break;
                    case Key.Down:
                        if (cbTipo.SelectedIndex + 1 <= 5)
                            cbTipo.SelectedIndex++;
                        break;
                }
            }
        }

        private void DatePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var value = e.NewValue;
            if (value == null) return;
            string dateTame = value.ToString().Split(' ')[0];
            string[] date = dateTame.Split('/');

            tbData.box.Text = string.Format("{0}-{1}-{2}", date[2], date[1], date[0]);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void BtnLimparTudo_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
            tbLetra.box.Text = "";
            tbNomePai.box.Text = "";
            tbNomePai2.box.Text = "";
            tbGeneros.box.Text = "";
            tbId.box.Text = "";
        }

        #endregion

        #region Metodos

        private void Init()
        {
            cbTipo.Items.Add(AnimeType.TV);
            cbTipo.Items.Add(AnimeType.MOVIE);
            cbTipo.Items.Add(AnimeType.ONA);
            cbTipo.Items.Add(AnimeType.OVA);
            cbTipo.Items.Add(AnimeType.SPECIAL);
            cbTipo.Items.Add(AnimeType.INDEFINIDO);
            cbTipo.SelectedIndex = 0;
        }

        private void SavePai()
        {
            var item = new AnimeCollection
            {
                nome = tbNomePai.box.Text,
                nome2 = tbNomePai2.box.Text,
            };
            var generos = tbGeneros.box.Text.Split(',');
            var parentes = tbParentes.box.Text.Split(',');
            foreach (string g in parentes)
                if (g.Trim().Length > 0)
                    item.parentes.Add(g.Trim());
            foreach (string g in generos)
                if (g.Trim().Length > 0)
                    item.generos.Add(g.TrimStart().TrimEnd());

            char letra = tbLetra.box.Text[0];
            currentList = AnimeCollection.LoadFileList(letra.ToString());

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

            if (!string.IsNullOrWhiteSpace(item.nome))
                currentList[itemID].nome = item.nome;

            if (!string.IsNullOrWhiteSpace(item.nome2))
                currentList[itemID].nome2 = item.nome2;

            if (currentList[itemID].parentes != null && currentList[itemID].parentes.Count == 0)
                currentList[itemID].parentes = null;
        }

        private void Save()
        {
            log.Content = "";

            var item = CriarObj();
            if (!Verificar(item)) return;

            SavePai();

            char letra = tbLetra.box.Text[0];
            string paiID = letra + tbId.box.Text;

            if (!currentList.ContainsKey(paiID))
                currentList.Add(paiID, new AnimeCollection());

            string itemID = paiID + "_" + (currentList[paiID].items.Count + 1);
            item.id = itemID;
            currentList[paiID].items.Add(itemID, item);
            AnimeCollection.SaveFile(letra, currentList);
            LimparCampos();
            OnAddItem?.Invoke(item);
        }

        private void LimparCampos()
        {
            tbNome.box.Text =
            tbNome2.box.Text =
            tbFoto.box.Text =
            tbMini.box.Text =
            tbEps.box.Text =
            tbLink.box.Text =
            tbSinopse.box.Text =
            tbParentes.box.Text =
            tbData.box.Text = "";
            cbTipo.SelectedIndex = 0;

            datePicker.Value = null;
            datePicker.CalendarDisplayMode = CalendarMode.Year;
        }

        private bool Verificar(AnimeItem item)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tbId.box.Text)) throw new Exception("Id");
                if (string.IsNullOrWhiteSpace(tbLetra.box.Text)) throw new Exception("Letra");
                if (string.IsNullOrWhiteSpace(item.nome)) throw new Exception("nome");
                if (string.IsNullOrWhiteSpace(item.miniatura)) throw new Exception("miniatura");
                if (string.IsNullOrWhiteSpace(item.link)) throw new Exception("link");
                if (string.IsNullOrWhiteSpace(item.data)) throw new Exception("data");
                if (item.episodios == EPS_ERRO) throw new Exception("episodios");

                if (string.IsNullOrWhiteSpace(item.nome2)) item.nome2 = null;

                if (string.IsNullOrWhiteSpace(item.foto)) {
                    item.foto = null;
                    var result = MessageBox.Show("Sem Foto\nDeseja continuar?", "Aviso", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No) throw new Exception("foto");
                }
                if (item.episodios == 0) {
                    item.foto = null;
                    var result = MessageBox.Show("Sem Episódios\nDeseja continuar?", "Aviso", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.No) throw new Exception("episodios");
                }
            }
            catch (Exception ex)
            {
                string text = "Verificar: " + ex.Message + " vazio";
                Console.WriteLine(text);
                log.Content = text;
                return false;
            }
            return true;
        }

        private AnimeItem CriarObj()
        {
            var item = new AnimeItem
            {
                nome = tbNome.box.Text,
                nome2 = tbNome2.box.Text,
                foto = tbFoto.box.Text,
                miniatura = tbMini.box.Text,
                link = tbLink.box.Text,
                sinopse = tbSinopse.box.Text.Replace("\r", ""),
                data = tbData.box.Text,
                tipo = cbTipo.SelectedItem.ToString()
            };
            try
            {
                if (tbEps.box.Text == "")
                    item.episodios = 0;
                else
                    item.episodios = Convert.ToInt32(tbEps.box.Text);
            }
            catch (Exception)
            {
                Console.WriteLine("CriarObj");
                item.episodios = EPS_ERRO;
            }
            return item;
        }

        #endregion

    }
}
