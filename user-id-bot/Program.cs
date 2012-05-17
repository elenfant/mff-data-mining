using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Xml;
using HtmlAgilityPack;
using System.IO;


namespace ConsoleApplication1
{

    

    public class Film
    {
        string id;
        string nazev;
        string rok;
        int hodnoceni;
        public Film(string id, string nazev, int hodnoceni, string rok)
        {
            this.id = id;
            this.nazev = nazev;
            this.rok = rok;
            this.hodnoceni = hodnoceni;
        }

        public string get_vse()
        {
            string o = id + "\t;\t" + nazev + " " + rok + "\t;\t" + hodnoceni.ToString();
            return o;
        }

        public string Id
        {
            get { return id; }
        }
        public string Nazev
        {
            get { return nazev; }
        }

        public string Rok
        {
            get { return rok; }
        }
        public int Hodnoceni
        {
            get { return hodnoceni; }
        }


    }

    class Program
    {

        /// <summary>
        /// Returns the content of a given web adress as string.
        /// </summary>
        /// <param name="Url">URL of the webpage</param>
        /// <returns>Website content</returns>

        const string web = "http://www.csfd.cz";

        WebProxy wp = new WebProxy("210.101.131.232", 8080);
        
        const string proxy_list = "http://hidemyass.com/proxy-list/";



        private static string get_page(string url)
        {
            
            string result = null;

            try
            {
                WebClient client = new WebClient();
                result = client.DownloadString(url);
            }
            catch (Exception ex)
            {
                // handle error
                Console.WriteLine(ex.Message);
            }
            return result;
        }
        static void Main(string[] args)
        {


   /*         string[] proxys = new string[14]{"210.101.131.232","189.114.111.190","95.215.48.134","180.96.19.196","66.90.146.51","116.55.19.96","187.60.96.7","83.91.86.26","93.167.245.178","164.77.196.75",
                "187.60.96.7","80.64.168.217","112.65.219.72","61.134.121.237"};

            int[] ports = new int[14] { 8080, 8080, 8080, 3128, 1021, 808, 3128, 9100, 9100, 80, 3128, 8080, 80, 80 };
            */
            int idu = 340000;
       
            
            //int idprox = 0;

            string host = "";
            int port = 0;


            for (int strana = 3400; strana > 1000; --strana)
            {
                string url_strany = web + "/uzivatele/prehled/strana-" + strana.ToString() + "/";
                Console.WriteLine("strana " + strana.ToString());


          /*      try
                {*/

                    List<string> uziv = vysekej_adresy_uzivatelu(url_strany, host, port);


                    for (int i = 0; i < uziv.Count; ++i)
                    {
                        try
                        {
                            if (idu < 340001)   //tady si muzes nastavit, ktereho uzivatele jsi stahl naposledy
                            {

                                if (idu % 30 == 0)
                                {
                                    host = "";
                                    port = 0;
                                }


                                Console.WriteLine("zpracovavam uzivatele " + idu.ToString());

                                List<Film> filmy = new List<Film>();
                                string url_hodnoceni = web + uziv[i] + "hodnoceni/";

                                List<string> adresy = vycuc_stranky(url_hodnoceni, host, port);
                                foreach (string adresa in adresy)
                                {

                                    rozparsuj(adresa, filmy, host, port);
                                }
                                string path = "./data/uziv-" + idu.ToString() + ".txt";

                                /*   if (File.Exists(@path))
                                   {
                                       File.Delete(@path);
                                   } */
                                StreamWriter writer = new StreamWriter(@path);


                                foreach (Film f in filmy)
                                {
                                    writer.WriteLine(f.get_vse());
                                }

                                writer.Close();
                            }
                            idu--;
                        }

                        catch (Exception e)
                        {
                            ++i;
                            Console.WriteLine(e.Message);
                            System.Threading.Thread.Sleep(1000 * 60 * 5);   //vyjimka byva zpusobena tim, ze me nepusti na server, zkus to za 5 min znovu


                            /*         if (host.Length > 0)      //nejake hnusne, neprilis funkcni, vyhledani proxy serveru...
                                     {
                                         host = "";
                                         port = 0;
                                     }
                                     else
                                     {
                                         string prox = get_proxy(proxy_list);
                                         string[] pol = prox.Split(':');
                                         host = pol[0];
                                         port = Convert.ToInt32(pol[1]);

                                     }*/
                        }
                /*        if (idprox < 13)
                        {
                            ++idprox;
                        }
                        else
                        {
                            idprox = 0;
                        }*/
                    }
           /*     }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    System.Threading.Thread.Sleep(1000 * 60 * 5);
                    strana++;
                }*/
            }

        }

        /*  //nejake hnusne, neprilis funkcni, vyhledani proxy serveru...
        private static string get_proxy(string url)
        {
            HtmlWeb htmlWeb = new HtmlWeb();

            // Creates an HtmlDocument object from an URL
            HtmlDocument document = htmlWeb.Load(url);



            // Targets a specific node
            HtmlNode tabulka = document.DocumentNode.SelectSingleNode(@"//table[@id=""listtable""]");
            HtmlNodeCollection trs = tabulka.SelectNodes(@".//tr");

            Random rand = new Random();
            int i = rand.Next(10);
            i += 2;
            HtmlNode tr = trs[i];

            string adresa = "";
            HtmlNodeCollection tds = tr.SelectNodes(@".//td");
            HtmlNode ip_ad = tds[1];
            HtmlNodeCollection spans = ip_ad.SelectNodes(@".//span");
            int poc = spans.Count;
            for (int j = 1; j < poc; ++j)
            {
                HtmlNode sn = spans[j];
                if (!sn.Attributes.Contains("style") || sn.Attributes["style"].Value != "display:none")
                {
                    string cislo = sn.InnerText;
                    if (cislo == ".")
                    {
                        adresa += sn.InnerText;
                    }
                    else if (cislo.Length > 0 && cislo[0] == '.')
                    {
                        adresa += sn.InnerText;
                    }
                    else if (adresa.Length > 0 && adresa[adresa.Length - 1] == '.')
                    {
                        adresa += sn.InnerText;
                    }
                    else
                    {
                        if (adresa.Length > 0)
                        {
                            adresa += "." + sn.InnerText;
                        }
                        else
                        {
                            adresa += sn.InnerText;
                        }
                    }
                }
            }
            
            HtmlNode port_n = tds[2];
            string port = port_n.InnerText;
            return adresa + ":" + port;

        }   */


        private static List<string> vysekej_adresy_uzivatelu(string url,string prox,int por)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            
            // Creates an HtmlDocument object from an URL
            HtmlDocument document;
            if (prox.Length == 0)
            {
                document = htmlWeb.Load(url);
            }
            else
            {
                document = htmlWeb.Load(url, prox, por, "", "");
            }
            
            

            // Targets a specific node
            HtmlNode tabulka = document.DocumentNode.SelectSingleNode(@"//table[@class=""ui-table-list""]");
            HtmlNodeCollection trs = tabulka.SelectNodes(@".//tr");
            List<string> adresy = new List<string>();
            foreach (HtmlNode n in trs)
            {
                HtmlNodeCollection tds = n.SelectNodes(@".//td");
                int pocet = vysekej_pocet(tds);
                if (pocet > 100)
                {
                    HtmlNode t = n.SelectSingleNode(@".//td[@class=""nick""]");
                    HtmlNode a = t.SelectSingleNode(@".//a");
                    string adresa = a.Attributes["href"].Value;
                    adresy.Add(adresa);
                }
            }
            return adresy;

        }
        private static int vysekej_pocet(HtmlNodeCollection ns)
        {
            if (ns.Count > 3)
            {
                string s = ns[3].InnerText;
                string[] ss = s.Split(' ');
                s = ss[0];
                try
                {
                    int p = Convert.ToInt32(s);
                    return p;
                }
                catch
                {
                    return 0;
                }
                
            }
            else
            {
                return 0;
            }

        }

        private static List<string> vycuc_stranky(string url,string prox,int por)
        {
            HtmlWeb htmlWeb = new HtmlWeb();

            // Creates an HtmlDocument object from an URL
           // HtmlDocument document = htmlWeb.Load(url, prox, por, "", "");
            HtmlDocument document;
            if (prox.Length == 0)
            {
                document = htmlWeb.Load(url);
            }
            else
            {
                document = htmlWeb.Load(url, prox, por, "", "");
            }
            HtmlNode d = document.DocumentNode.SelectSingleNode(@"//div[@class=""paginator text""]");
            HtmlNodeCollection stranky = d.SelectNodes(@".//a");
            List<string> adresy = new List<string>();
            HtmlNode n = stranky[stranky.Count - 2];
            string adrlast = n.Attributes["href"].Value;
            string[] ss = adrlast.Split('-');
            string s = ss.Last();
            s = s.Substring(0, s.Length - 1);
            int posledni = Convert.ToInt32(s);
            for (int i = 1; i <= posledni; ++i)
            {
                string adr = url + "strana-" + i.ToString() + "/";
                adresy.Add(adr);
            }

            /*
            foreach (HtmlNode n in stranky)
            {
                if (n.InnerText != "následující &gt;&gt;")
                {
                    adresy.Add(n.Attributes["href"].Value);
                }
            }*/
            return adresy;
        }

        private static List<Film> rozparsuj(string url,List<Film> filmy,string prox,int por)
        {

            HtmlWeb htmlWeb = new HtmlWeb();

            // Creates an HtmlDocument object from an URL
           // HtmlDocument document = htmlWeb.Load(url_source,prox,por,"","");

            HtmlDocument document;
            if (prox.Length == 0)
            {
                document = htmlWeb.Load(url);
            }
            else
            {
                
                document = htmlWeb.Load(url, prox, por, "", "");
            }

            // Targets a specific node
            HtmlNode tabulka = document.DocumentNode.SelectSingleNode(@"//table[@class=""ui-table-list""]");
            HtmlNode telo = tabulka.SelectSingleNode(@"//tbody");

            HtmlNodeCollection trs =  tabulka.SelectNodes(@".//tr");


            for (int i = 0; i < trs.Count;i++ )
            {
                HtmlNode n = trs[i];
                
                HtmlNode hodnoceni_n = n.SelectSingleNode(@".//*[@class=""rating""]");
                if (hodnoceni_n == null)
                {
                    continue;
                }
                int hodnoceni = vysekej_hodnoceni(hodnoceni_n);
                HtmlNode odkaz_n = n.SelectSingleNode(@".//a[@class=""film c2""] | .//a[@class=""film c1""] | .//a[@class=""film c3""]");
                if (odkaz_n == null)
                {
                    continue;
                }
                string id = vysekej_id(odkaz_n);
                string nazev = vysekej_nazev(odkaz_n);
                HtmlNode rok_n = n.SelectSingleNode(@".//span[@class=""film-year""]");
                if (rok_n == null)
                {
                    continue;
                }
                string rok = vysekej_rok(rok_n);
                Film f = new Film(id, nazev, hodnoceni, rok);
                filmy.Add(f);

            }
            return filmy;
            
        }
        private static int vysekej_hodnoceni(HtmlNode n)
        {
            int hodn = 0;
            if (n.Name == "img")
            {
                string hvezdy = n.Attributes["alt"].Value;
                hodn = hvezdy.Length;
            }
            return hodn;
        }
        private static string vysekej_id(HtmlNode n)
        {
            string odkaz = n.Attributes["href"].Value;
            string[] casti = odkaz.Split('/');
            if (casti.Length > 1)
            {
                string last = casti[casti.Length - 2];
                string[] cs = last.Split('-');
                if (cs.Length > 0)
                {
                    string id = cs[0];
                    return id;
                }
                else
                {
                    return "nic";
                }
            }
            else
            {
                return "nic";
            }
        }

        private static string vysekej_nazev(HtmlNode n)
        {
            return n.InnerText;
            
        }
        private static string vysekej_rok(HtmlNode n)
        {
            string r = n.InnerText;
            r = r.Trim();
          //  r = r.Substring(1, r.Length - 2);
            return r;
        }

    }
}
