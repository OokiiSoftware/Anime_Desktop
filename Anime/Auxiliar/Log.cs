using System;

namespace Anime.Auxiliar
{
    public static class Log
    {
        private const string TAG = "Log";
        public static bool ENVIAR_DADOS = true;

        public static void Erro_(string tag, string titulo, string mensagem = null, string dados = null, bool enviarErro = true)
        {
            Console.WriteLine(string.Format("LOG | {0}: {1}: {2}: {3}", tag, titulo, mensagem == null ? "" : mensagem, dados == null ? "" : dados));
        }

        /// <summary>
        /// Antes de usar o 'dadoAux' veja antes como deve ser usado.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="exeption"></param>
        /// <param name="dadoAux">Deve ser usado em métodos com mais de um (1) tratamento de erro pra indicar em qual tratamento o erro ocorreu.</param>
        /// <param name="enviarErro"></param>
        public static async void Erro(string tag, Exception exeption, dynamic dadoAux = null, bool enviarErro = true)
        {
            string msg = "";
            string titulo = "";
            string mensagem = "";
            string dados = "";
            string dadosAux = "";
            if (exeption != null)
            {
                titulo = "Método: " + exeption.TargetSite.Name;
                mensagem = "Message: " + exeption.Message;

                if (exeption.HelpLink != null) dados = "\nHelpLink: " + exeption.HelpLink;
                if (exeption.Source != null) dados += "\nSource: " + exeption.Source;
                if (exeption.StackTrace != null) dados += "\nStackTrace: " + exeption.StackTrace;

                msg = "\nClasse: " + tag;
                msg += "\n" + titulo;
                msg += "\n" + mensagem;
                msg += dados;
            }
            if (dadoAux != null)
            {
                dadosAux = dadoAux.ToString();
                msg += "\nDadoAuxiliar: " + dadosAux;
            }
            Console.WriteLine(string.Format("ERRO | {0}", msg));

            if (ENVIAR_DADOS && enviarErro)
            {
                //var mac = Import.Get.EnderecoMac();
                //if (mac == null) mac = "123456789";

                //ErroLog log = new ErroLog(mac, tag, titulo, mensagem, dados);
                //await log.Enviar();

                //Msg(TAG, "Erro", "Log Enviado");
            }
        }

        public static void Msg(string tag, string titulo, dynamic value0 = null, dynamic value1 = null, dynamic value2 = null, dynamic value3 = null)
        {
            string msg = "";
            if (value0 != null)
                msg += ": " + value0;
            if (value1 != null)
                msg += ": " + value1;
            if (value2 != null)
                msg += ": " + value2;
            if (value3 != null)
                msg += ": " + value3;
            Console.WriteLine(string.Format("LOG | {0}: {1}: {2}", tag, titulo, msg));
        }
    }

}
