using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EnviarEmailAsync
{
    class Email
    {
        public string De { get; set; }
        public string Para { get; set; }
        public string Assunto { get; set; }
        public string Mensagem { get; set; }
        public ConfiguracaoEmail Configuracao { get; set; }
    }

    class ConfiguracaoEmail
    {
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string Servidor { get; set; }
        public int Porta { get; set; }
    }
    class EnviarEmail
    {
        private SmtpClient _smtp { get; set; }
        private Email _email { get; set; }
        private MailMessage _mensagem { get; set; }

        public EnviarEmail(Email email)
        {
            _email = email;
            if (email == null)
                throw new Exception("Email é null, passar um objeto");
            else if (email != null && email.Configuracao == null)
                throw new Exception("Configuração de email é null, passar um objeto");
            else
                _mensagem = Build();
        }

        private MailMessage Build()
        {
            MailMessage mensagem = new MailMessage();

            //encode do email para não ter problemas com caracters
            mensagem.HeadersEncoding = Encoding.UTF8;
            mensagem.BodyEncoding = Encoding.UTF8;
            mensagem.SubjectEncoding = Encoding.UTF8;

            //de quem é o email
            mensagem.From = new MailAddress(_email.De);

            //para quem é o email
            //pode passar uma lista dividida por ';'   ex: email@email.com;email2@email.com;email3@email.com
            MailAddressCollection para = new MailAddressCollection();
            foreach (var email in _email.Para.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                para.Add(new MailAddress(email));
            }
            mensagem.To.Add(para.ToString());

            //assunto
            mensagem.Subject = _email.Assunto;
            //mensagem é um html
            mensagem.IsBodyHtml = true;
            //corpo do email
            mensagem.Body = _email.Mensagem;
            //prioridade do email
            mensagem.Priority = MailPriority.Normal;            

            //configurar o smtp
            _smtp = new SmtpClient()
            {
                Host = _email.Configuracao.Servidor,
                Port = _email.Configuracao.Porta,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(_email.Configuracao.Usuario, _email.Configuracao.Senha)
            };

            return mensagem;
        }

        public async Task<bool> EnviarAsync()
        {
            try
            {                
                using (_smtp)
                {
                    //adicionar a callback de envio do email
                    _smtp.SendCompleted += new SendCompletedEventHandler(EnviarCompleto);
                    using (_mensagem)
                        await _smtp.SendMailAsync(_mensagem);
                }               

                return true;
            }
            catch (ArgumentNullException aE)
            {
                Console.WriteLine("Falta de parametros na mensagem: {0}", aE.Message);
            }
            catch (SmtpException sE)
            {
                Console.WriteLine("SmtpException: {0}", sE.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;

        }

        public async Task EnviarSemRetornoAsync()
        {
            try
            {
                using (_smtp)
                {
                    //adicionar a callback de envio do email
                    _smtp.SendCompleted += new SendCompletedEventHandler(EnviarCompleto);
                    using (_mensagem)
                        await _smtp.SendMailAsync(_mensagem);
                }
                
            }
            catch (ArgumentNullException aE)
            {
                Console.WriteLine("Falta de parametros na mensagem: {0}", aE.Message);
            }
            catch (SmtpException sE)
            {
                Console.WriteLine("SmtpException: {0}", sE.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void CancelarEnvio()
        {
            if (_smtp != null)
            {
                _smtp.SendAsyncCancel();
            }
        }

        private void EnviarCompleto(object sender, AsyncCompletedEventArgs e)
        {
            _mensagem = e.UserState as MailMessage;

            if (!e.Cancelled && e.Error == null)
            {
                //adicionar log de envio no banco                
                Console.WriteLine("Email enviado com sucesso!");
            }
            else if (e.Error != null)
            {
                //adicionar log do erro no banco
                Console.WriteLine("Ocorreu um erro ao enviar o email: {0}", e.Error.ToString());
            }
            else if (e.Cancelled)
            {
                //adicionar log do cancelamento no banco
                Console.WriteLine("Envio de email cancelado!");
            }

            //limpar
            if (_mensagem != null)
                _mensagem.Dispose();
        }
    }
}
