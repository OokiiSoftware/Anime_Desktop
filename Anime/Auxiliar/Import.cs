using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Windows;

namespace Anime.Auxiliar
{
    public class Import
    {
        private const string TAG = "Import";
        private static Dictionary<string, string> caracteresEspeciais;

        public static List<string> GetArquivosDaPasta(string pastaRaiz, string[] filtros = null, bool isRecursiva = true)
        {
            var arquivosEncontrados = new List<string>();

            if (string.IsNullOrWhiteSpace(pastaRaiz)) return arquivosEncontrados;
            if (filtros == null) filtros = new string[0];

            //define as opções para exibir as imagens da pasta raiz
            var opcaoDeBusca = isRecursiva ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            if (Directory.Exists(pastaRaiz))
                foreach (var filtro in filtros)
                    arquivosEncontrados.AddRange(Directory.GetFiles(pastaRaiz, string.Format("*.{0}", filtro), opcaoDeBusca));
            return arquivosEncontrados;
        }

        public static string GetFileText(string path)
        {
            try
            {
                if (!File.Exists(path))
                    throw new Exception();
                
                return File.ReadAllText(path);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string RemoverSimbolos(string value, bool copySimbolo = false, bool isPtBr = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            string temp = HttpUtility.HtmlDecode(value);

            foreach (var t in caracteresEspeciais)
                temp = temp.Replace(t.Key, t.Value);

            if (!isPtBr && temp.Contains("â"))
            {
                string caractere = temp.Substring(temp.IndexOf("â"), 3);
                if (copySimbolo)
                    Clipboard.SetText(caractere);

                if (caracteresEspeciais == null)
                    if (!LoadCaracteresEspeciais())
                        throw new Exception($"CaracteresEspecial.json não encontrado");

                if (!caracteresEspeciais.ContainsKey(caractere))
                {
                    var result = MessageBox.Show(
                    temp + "\n\nDeseja Ler os caracteres especiais novamente?",
                    "Caractere Especial",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Exclamation);

                    switch (result)
                    {
                        case MessageBoxResult.Cancel:
                            throw new Exception("Caractere Especial");
                        case MessageBoxResult.Yes:
                            LoadCaracteresEspeciais();
                            break;
                        case MessageBoxResult.No:
                            return temp;
                    }
                }

                if (!caracteresEspeciais.ContainsKey(caractere))
                    return RemoverSimbolos(value, copySimbolo);
                temp = temp.Replace(caractere, caracteresEspeciais[caractere]);

                return RemoverSimbolos(temp, copySimbolo);

            }
            return temp;
            //.Replace("\r", "")
            //.Replace("<i>", "")
            //.Replace("</i>", "")
            //.Replace("<br>", "")
            //.Replace("<br/>", "")
            //.Replace("<br />", "")
            //.Replace("â˜…", "★")
            //.Replace("â˜†", "☆")
            //.Replace("â™¥", "♥")
            //.Replace("â€“", "–");
        }

        public static bool LoadCaracteresEspeciais()
        {
            string CaracteresEspeciais = "CaracteresEspeciais.json";
            if (!File.Exists(CaracteresEspeciais))
                return false;

            try
            {
                string jsonS = File.ReadAllText(CaracteresEspeciais);
                caracteresEspeciais = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonS);
            }
            catch (Exception ex)
            {
                Log.Erro(TAG, ex);
            }

            return true;
        }

    }

    public class Paths
    {
        public const string FILES_PATH = "temp\\";
        public const string FILES = "files\\";
        public const string MINIATURAS = "fotos\\miniaturas\\";
        public const string FOTOS = "fotos\\capas\\";
        public const string HTML = "html\\";
    }

}
