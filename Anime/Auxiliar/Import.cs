using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anime.Auxiliar
{
    public class Import
    {
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
    }

    public class Paths
    {
        public const string FILES_PATH = "temp\\";
        public const string MINIATURAS = "fotos\\miniaturas\\";
        public const string FOTOS = "fotos\\capas\\";
        public const string HTML = "html\\";
    }
}
