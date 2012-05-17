using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.Xml;
using HtmlAgilityPack;
using System.IO;


namespace user_id_bot
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
        const string web = "http://www.csfd.cz";

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
            string host = "";
            int port = 0;
			TextWriter output = System.Console.Out;
			
			/* get the last accessed user and page */
			//TODO: get the last accessed user and page, from file
				
			/* parse users until number of their ratings drop below 100 */
			for (int page = 1; /* condition intentionally left empty */ ; page++)
			{
				string pageUrl;
				if (page == 1)
				{
					pageUrl = web + "/uzivatele/prehled/podle-rating/";
				}
				else
				{
                	pageUrl = web + "/uzivatele/prehled/podle-rating/strana-" + page.ToString() + "/";
				}
                output.WriteLine("Processing page " + page.ToString() + ".");
				
                List<string> users = null;
				/* repeat until the request succeeds */
                do
                {
                    try
                    {
                        users = getUsersURLs(pageUrl, host, port);
                    }
                    catch (Exception e)
                    {
                        output.WriteLine(e.Message);
                        System.Threading.Thread.Sleep(1000 * 60 * 5);
                    }
                } while (users == null);			

				bool stopProcessing = false;
                for (int i = 0; i < users.Count; ++i)
                {
                    try
                    {
						string userID = getUserID(users[i]);
                        output.Write("Processing user " + userID);

                        List<Film> movies = new List<Film>();
                        string userReviewsBaseURL = web + users[i] + "hodnoceni/";

                        List<string> reviewsPagesURL = getReviewsPagesURLs(userReviewsBaseURL, host, port);
                        foreach (string reviewsURL in reviewsPagesURL)
                        {
                            getReviews(reviewsURL, movies, host, port);
							output.Write('.');
                        }
						output.WriteLine(" finished.");
						
						/* ensure that the user reviewed at least 100 movies
						 * otherwise stop processing since we process users by their reviews count */
						if (movies.Count < 100)
						{
							stopProcessing = true;
							break;
						}

						/* write results to file identified by userID */
						string dataFile = "./data/" + userID + ".txt";
                        StreamWriter writer = new StreamWriter(@dataFile);
                        foreach (Film f in movies)
                        {
                            writer.WriteLine(f.get_vse());
                        }
                        writer.Close();
                    }
                    catch (Exception e)
                    {
                        output.WriteLine(e.Message);
						//vyjimka byva zpusobena tim, ze me nepusti na server, zkus to za 5 min znovu
                        System.Threading.Thread.Sleep(1000 * 60 * 5);
                    }
                }

				if (stopProcessing)
				{
					break;
				}
			}
		}
		
        private static List<string> getUsersURLs(string url,string prox,int por)
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
                if (pocet >= 100)
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

        private static List<string> getReviewsPagesURLs(string url, string host, int port)
        {
            HtmlWeb htmlWeb = new HtmlWeb();

            HtmlDocument document;
            if (host.Length == 0)
            {
                document = htmlWeb.Load(url);
            }
            else
            {
                document = htmlWeb.Load(url, host, port, "", "");
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
            return adresy;
        }

		/*
		 * userURL pattern: /uzivatel/121600-drsan40/
		 */
		private static string getUserID(string userURL)
		{
			string[] urlParts = userURL.Split('/');
			return urlParts[2];		
		}

        private static void getReviews(string reviewsURL, List<Film> movies, string host, int port)
        {

            HtmlWeb htmlWeb = new HtmlWeb();
            HtmlDocument document;
            if (host.Length == 0)
            {
                document = htmlWeb.Load(reviewsURL);
            }
            else
            {
                
                document = htmlWeb.Load(reviewsURL, host, port, "", "");
            }

			
            HtmlNode tabulka = document.DocumentNode.SelectSingleNode(@"//table[@class=""ui-table-list""]");
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
                movies.Add(f);
            }          
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
