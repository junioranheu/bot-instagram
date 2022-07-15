using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace BotAnheuInsta
{
    public partial class FormBotAnheuInsta : Form
    {
        // Para que o Selenium funcione é necessário instalar:
        // 01 - Selenium.WebDriver;
        // 02 - Selenium.WebDriver.ChromeDriver;

        // Wards:
        // Ideia baseada em: https://www.youtube.com/watch?v=nb_gbWzGpPM&feature=youtu.be&t=1184;
        // Incognito: https://stackoverflow.com/questions/38643340/selenium-webdriver-chrome-c-sharp-unable-to-launch-selenium-browser-in-inc;
        // Como selecionar um input, classe, etc.: https://saucelabs.com/resources/articles/selenium-tips-css-selectors;
        // Buscar elemento com timer: https://stackoverflow.com/questions/6992993/selenium-c-sharp-webdriver-wait-until-element-is-present;
        // Clicar em algum elemento (usando Javascript): https://stackoverflow.com/questions/35259012/click-function-on-selenium-not-working-in-c-sharp;

        // Variáveis constantes;
        const string login = "xxx", senha = "IssoEApenasUmTeste@";
        const int qtdScrollParaBaixoBusca = 8;

        public FormBotAnheuInsta()
        {
            InitializeComponent();

            // Aviso escondido;
            lblAviso.Visible = false;
        }

        private void BtnSair_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnIniciar_Click(object sender, EventArgs e)
        {
            // Inciciar variáveis;
            lblAviso.Text = "";
            string hashtagBuscada = "naruto";

            // Iniciar conexão entre o Selenium e navegador, e abrir o Instagram;
            ChromeOptions options = new ChromeOptions();
            options.AddArguments("--incognito");
            ChromeDriver driver = new ChromeDriver(Environment.CurrentDirectory, options);
            driver.Navigate().GoToUrl("http://instagram.com");

            // Aguardar 1 segundo para iniciar o processo;
            Thread.Sleep(1000);
            try
            {
                // PARTE 01: Login;
                IWebElement elementoLogin = BuscarElemento(driver, By.CssSelector("input[name='username']"), 20);
                elementoLogin.SendKeys(login);

                IWebElement elementoSenha = BuscarElemento(driver, By.CssSelector("input[name='password']"), 20);
                elementoSenha.SendKeys(senha);
                Aviso("Nome de usuário e senha preenchidos");

                IWebElement elementoBotaoLogin = BuscarElemento(driver, By.CssSelector("button[type='submit']"), 20);
                ClicarElemento(driver, elementoBotaoLogin);

                // PARTE 02: Clicar para não receber notificações;
                Thread.Sleep(2000);
                Aviso("Realizando login e clicando para não receber notificações...");     
                IWebElement elementoNotificacao = BuscarElemento(driver, By.CssSelector("._a9_1"), 20);
                ClicarElemento(driver, elementoNotificacao);
                Thread.Sleep(1000);
                Aviso("Botão de não receber notificações clicado...");

                // PARTE 03: Busca da hashtag;
                Thread.Sleep(5000);
                Aviso("Login realizado com sucesso\nBuscando por #" + hashtagBuscada + "\nAguarde uns instantes...");
                IWebElement elentoCaixaBusca = BuscarElemento(driver, By.CssSelector("._aawh"), 20);
                elentoCaixaBusca.SendKeys(hashtagBuscada);
                driver.Navigate().GoToUrl("http://instagram.com/explore/tags/" + hashtagBuscada + "/");
                Aviso("Carregando resultados\nAguarde uns instantes...");

                // Esperar mais alguns segundos pra carregar as imagens;
                Thread.Sleep(3000);

                // Dar scroll para baixo x vezes para carregar mais itens;
                // Cada scroll demora mais 2 segundos
                for (int i = 0; i < qtdScrollParaBaixoBusca; i++)
                {
                    Aviso("Carregando resultados\nScroll " + (i + 1).ToString() + " de " + qtdScrollParaBaixoBusca + "\nAguarde uns instantes...");
                    ScrollMaximoPraBaixo(driver);
                    Thread.Sleep(2000);
                }

                // Finalmente buscar e armazenar os links das fotos encontradas;
                // Explicação dessa parte no link a seguir: https://youtu.be/nb_gbWzGpPM?t=1704;
                IList<IWebElement> listaItensEncontrados = driver.FindElements(By.TagName("a"));
                List<string> listaURL = new List<string>();
                foreach (var item in listaItensEncontrados)
                {
                    string link = item.GetAttribute("href");
                    listaURL.Add(link);
                }

                Aviso("Foram encontradas " + listaURL.Count + " fotos\nPreparando para curtir e comentar...");

                // PARTE 03: Navegar na lista de URLs para realizar os comentários;
                int j = 1;
                foreach (var url in listaURL)
                {
                    // Ir até a foto;
                    driver.Navigate().GoToUrl(url);
                    Thread.Sleep(3000);

                    // Checar se a foto já está curtida: https://stackoverflow.com/questions/63907587/python-selenium-how-do-i-check-if-i-liked-a-post-on-instagram-or-not;
                    // Se a foto tiver curtida, pule a foto;
                    var elementoChecarFotoCurtida = BuscarElemento(driver, By.CssSelector("._aamw button._abl- div._abl_ span svg"), 20);
                    string cor = elementoChecarFotoCurtida.GetAttribute("fill");
                    if (cor != "#ed4956")
                    {
                        // Curtir foto;
                        IWebElement elementoCurtir = BuscarElemento(driver, By.CssSelector("._aamw button._abl-"), 20);
                        ClicarElemento(driver, elementoCurtir);

                        // Comentar (pro comentário funcionar, foi necessário usar duas vezes o Click(): https://stackoverflow.com/questions/53502191/send-a-instagram-comment-using-python-with-selenium/);
                        string comentario = GerarComentario();

                        try
                        {
                            IWebElement elementoCaixaComentario = BuscarElemento(driver, By.ClassName("_aaoc"), 3);
                            elementoCaixaComentario.Click();
                            elementoCaixaComentario = BuscarElemento(driver, By.ClassName("_aaoc"), 20);
                            elementoCaixaComentario.Click();
                            EscreverComentario(elementoCaixaComentario, comentario);

                            // Enviar comentário;
                            IWebElement elementoBotaoPublicar = BuscarElemento(driver, By.CssSelector("button[type='submit']"), 20);
                            ClicarElemento(driver, elementoBotaoPublicar);
                        }
                        catch (Exception)
                        {
                            Aviso("Essa última foto não pode ser curtida e/ou comentada porque haviam restrições");
                        }
                        finally
                        {         
                            j++;
                            Aviso("Foram encontradas " + listaURL.Count + " fotos\nFotos comentadas e curtidas: " + j.ToString());
                        }
                 
                        Thread.Sleep(1000);
                    }
                }

                Aviso("Processo finalizado\nForam encontradas " + listaURL.Count + " fotos\nFotos comentadas e curtidas: " + j.ToString() + "");
                driver.Close();
            }
            catch (Exception ex)
            {
                Aviso("Houve um erro durante o processo\n-> " + ex.Message);
            }
        }

        // Escrever comentário de forma "humana";
        private void EscreverComentario(IWebElement elemento, string comentario)
        {
            foreach (char letra in comentario)
            {
                elemento.SendKeys(letra.ToString());
                Thread.Sleep(100);
            }
        }

        // Gerar comentário aleatório;
        private string GerarComentario()
        {
            //var listaComentarios = new List<string> {
            //    "Olá! Quer aprender um pouco mais sobre xxx? Me siga aqui :)",
            //    "Foto da hora! Quer aprender um pouco mais sobre xxx? Me siga aqui :)",
            //    "Gostei da foto! Quer aprender um pouco mais sobre xxx? Me siga aqui :)",
            //    "Oi, né? Quer aprender um pouco mais sobre xxx? Me siga aqui :)" 
            //};

            var listaComentarios = new List<string> {
                "Isso é apenas um #teste 01",
                "Isso é apenas um #teste 02",
                "Isso é apenas um #teste 03",
                "Isso é apenas um #teste 04",
            };

            var random = new Random();
            int i = random.Next(listaComentarios.Count);
            string comentarioAleatorio = listaComentarios[i];

            return comentarioAleatorio;
        }

        // Dar um scroll pra baixo na tela (Instagram) usando Javascript;
        private void ScrollMaximoPraBaixo(IWebDriver driver)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
        }

        // Clicar em algum elemento (usando Javascript);
        private void ClicarElemento(IWebDriver driver, IWebElement elemento)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].click();", elemento);
        }

        // Buscar elemento;
        private IWebElement BuscarElemento(IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds >= 10)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                IWebElement elemento = wait.Until(drv => drv.FindElement(by));
                return elemento;
            }

            return driver.FindElement(by);
        }

        private void Aviso(string aviso)
        {
            lblAviso.Text = aviso;
            lblAviso.Visible = true;
            Application.DoEvents();
        }

        // Permitir mover o programa;
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case 0x84:
                    base.WndProc(ref m);
                    if ((int)m.Result == 0x1)
                        m.Result = (IntPtr)0x2;
                    return;
            }

            base.WndProc(ref m);
        }
    }
}
