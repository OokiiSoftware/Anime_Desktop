using Anime.Auxiliar;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Anime.Translator
{
    public class Tradutor
    {
        private const string TAG = "Translator";

        #region Variaveis

        public static class Languages
        {
            public const string English = "en";
            public const string Portuguese = "pt";
        }

        private static readonly HttpClient client = new HttpClient
        {
            DefaultRequestHeaders = { { "Ocp-Apim-Subscription-Key", "078912f349254a74bf3a51eb98cfe9c5" } }
        };

        public TimeSpan TranslationTime
        {
            get;
            private set;
        }

        public string TranslationSpeechUrl
        {
            get;
            private set;
        }

        public Exception Error
        {
            get;
            private set;
        }

        #endregion

        #region Public methods

        public async Task<string> Traduzir(string text, string toLanguage = "pt")
        {
            try
            {
                var encodedText = WebUtility.UrlEncode(text);

                var url = string.Format("https://api.microsofttranslator.com/V2/http.svc/Translate?to={1}&text={0}", text, toLanguage);

                string result = await client.GetStringAsync(url);
                return XElement.Parse(result).Value;
            }
            catch (Exception ex)
            {
                Log.Erro(TAG, ex);
                return null;
            }
        }

        #endregion

    }

}
