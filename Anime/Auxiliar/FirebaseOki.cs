using Firebase.Auth;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anime.Auxiliar
{
    public enum FirebaseLoginResult
    {
        /// <summary>
        /// "UnknownEmailAddress" Endereço de email não encontrado
        /// </summary>
        EMAIL_NAO_ENCONTRADO,
        /// <summary>
        /// "Undefined" Erro na conexão com o banco de dados
        /// </summary>
        ERRO_DE_CONEXAO,
        /// <summary>
        /// "WrongPassword" Senha incorreta
        /// </summary>
        SENHA_INCORRETA,
        PERMISSAO_NEGADA,
        /// <summary>
        /// Ultima versão do sistema no banco de dados diferente da ultima versão do sistema no Storage
        /// </summary>
        ERRO_DB_STORAGE,
        ERROR,
        SUCESS,
        CANCEL
    }

    public class FirebaseChild
    {
        public const string TESTE = "teste";
        public const string ANIME = "anime";
        public const string ANIMES = "animes";
        public const string BASICO = "basico";
        public const string COMPLEMENTO = "complemento";
    }

    public class FirebaseOki
    {
        private const string TAG = "FirebaseOki";
        private static Dictionary<string, string> _firebaseProjectData;

        public static Dictionary<string, string> FirebaseProjectData
        {
            get
            {
                if (_firebaseProjectData == null)
                {
                    string result = Encoding.UTF8.GetString(Properties.Resources.firebase_empresa_dados);
                    _firebaseProjectData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                }
                return _firebaseProjectData;
            }
        }

        public static FirebaseClient GetClient { get; private set; }
        public static FirebaseStorage GetStorage => new FirebaseStorage(FirebaseProjectData[Const.STORAGE]);
        public static string GetApiKey => FirebaseProjectData[Const.API_KEY];
        public static string GetDatabaseURL => FirebaseProjectData[Const.DATABASE];
        public static string GetCompanhiaNome => FirebaseProjectData[Const.NOME];

        public static void SetFirebaseClient(FirebaseClient client)
        {
            GetClient = client;
        }
        public static async Task<FirebaseLoginResult> Login(string login, string senha)
        {
            if (login.Trim().Length == 0 || senha.Trim().Length == 0)
                return FirebaseLoginResult.CANCEL;

            try
            {
                var authProvider = new FirebaseAuthProvider(new FirebaseConfig(GetApiKey));
                var auth = await authProvider.SignInWithEmailAndPasswordAsync(login, senha);

                var firebase = new FirebaseClient(GetDatabaseURL,
                  new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(auth.FirebaseToken) });

                SetFirebaseClient(firebase);
                authProvider.Dispose();

                return FirebaseLoginResult.SUCESS;
            }
            catch (FirebaseAuthException ex)
            {
                switch (ex.Reason.ToString())
                {
                    case CodError.EMAIL_NAO_ENCONTRADO:
                        return FirebaseLoginResult.EMAIL_NAO_ENCONTRADO;
                    case CodError.ERRO_DE_CONEXAO:
                        return FirebaseLoginResult.ERRO_DE_CONEXAO;
                    case CodError.SENHA_INCORRETA:
                        return FirebaseLoginResult.SENHA_INCORRETA;
                }

                Log.Erro(TAG, ex);
                //Log.Erro(TAG, "VerificarCredenciais 1", e.Reason.ToString());
                return FirebaseLoginResult.ERROR;
            }
            catch (Exception ex)
            {
                if (ex.Message.Equals(CodError.PERMISSAO_NEGADA))
                    return FirebaseLoginResult.PERMISSAO_NEGADA;
                Log.Erro(TAG, ex);
                return FirebaseLoginResult.ERROR;
            }
        }
        


        public static class CodError
        {
            public const string EMAIL_NAO_ENCONTRADO = "UnknownEmailAddress";
            public const string ERRO_DE_CONEXAO = "Undefined";
            public const string SENHA_INCORRETA = "WrongPassword";
            public const string PERMISSAO_NEGADA = "Permissão negada";
        }

    }
}
