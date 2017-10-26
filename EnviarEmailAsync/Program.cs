using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
  Servidor: smtps.uol.com.br
    Porta: 587
 */

namespace EnviarEmailAsync
{
    class Program
    {
        static void Main(string[] args)
        {
            //configuraçoes do smtp
            ConfiguracaoEmail cfg = new ConfiguracaoEmail()
            {
                Usuario = "julio.com@uol.com.br",
                Senha = "2diol",
                Porta = 587,
                Servidor = "smtps.uol.com.br"
            };

            //configurar o email a ser enviado
            Email email = new Email()
            {
                Configuracao = cfg,
                De = cfg.Usuario,
                Para = "jcschincariolfh@gmail.com",
                Assunto = "Teste de envio",
                Mensagem = "<html>" +
                            "<header><title>Titulo</title></header>" +
                            "<body><h1>Teste H1</h1></body>" +
                            "</html>"
            };

            try
            {
                var enviar = new EnviarEmail(email);
                Task<bool> retornoEnviar = enviar.EnviarAsync();

                //pegar o retorno fora do async se precisar
                bool enviado = retornoEnviar.Result;
                if(enviado)
                    Console.WriteLine("Enviado.");
                else
                    Console.WriteLine("Não enviado.");
            }
            catch (Exception e)
            {
                Console.WriteLine("Erro ao inicializar EnviarEmail: {0}", e.Message);
                throw;
            }

            Console.WriteLine("Executando outra ação enquanto envia o email.");
            for (int i = 1; i <= 10000; i++)
            {
                //if(i == 500) //testar o cancelamento
                //    enviar.CancelarEnvio();
                Console.Write("\r{0} %", i);
            }

            Console.Read();

        }
    }
}
