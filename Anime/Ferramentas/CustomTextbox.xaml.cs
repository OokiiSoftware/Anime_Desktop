using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public enum TextImput { Text, Number, Data }
    public partial class CustomTextbox : UserControl
    {
        public string Hint { get; set; }
        public TextWrapping TextWrapping { get; set; } = TextWrapping.NoWrap;
        public TextImput TextImput { get; set; } = TextImput.Text;
        public int MaxLines { get; set; } = 200;
        public int MaxLength { get; set; } = 20000;
        public bool IsReadOnly { get; set; } = false;
        public bool AcceptsReturn { get; set; } = false;
        public bool IsPassword { get; set; }

        private readonly Regex regexNumber = new Regex("[^0-9-]+");
        private readonly Regex regexData = new Regex("[^0-9-ai]+");

        public CustomTextbox()
        {
            InitializeComponent();
            hint.Content = Hint;
            MouseDown += (s, e) => {
                if (IsPassword)
                    password.Focus();
                else
                    box.Focus();
            };
            if (IsPassword)
            {
                password.MaxLength = MaxLength;
                password.PreviewTextInput += Box_PreviewTextInput;
            }
            else
            {
                box.MaxLines = MaxLines;
                box.MaxLength = MaxLength;
                box.IsReadOnly = IsReadOnly;
                box.TextWrapping = TextWrapping;
                box.AcceptsReturn = AcceptsReturn;
                box.PreviewTextInput += Box_PreviewTextInput;
            }
            SetVisibility();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            hint.Content = Hint;
            if (IsPassword)
            {
                password.MaxLength = MaxLength;
            }
            else
            {
                box.MaxLines = MaxLines;
                box.MaxLength = MaxLength;
                box.IsReadOnly = IsReadOnly;
                box.TextWrapping = TextWrapping;
                box.AcceptsReturn = AcceptsReturn;
            }
            
            box.Foreground =
            hint.Foreground =
            password.Foreground = Foreground;

            SetVisibility();
        }

        private void SetVisibility()
        {
            if (IsPassword)
            {
                password.Visibility = Visibility.Visible;
                box.Visibility = Visibility.Collapsed;
            }
            else
            {
                password.Visibility = Visibility.Collapsed;
                box.Visibility = Visibility.Visible;
            }
        }

        private void Box_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            switch (TextImput)
            {
                case TextImput.Number:
                    if (regexNumber.IsMatch(e.Text))
                        e.Handled = true;
                    break;
                case TextImput.Data:
                    if (regexData.IsMatch(e.Text))
                        e.Handled = true;
                    break;
                default:
                    break;
            }
        }
    }
}
