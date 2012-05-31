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

    public class Movie
    {
        string id;
        string nazev;
        string rok;
        int hodnoceni;
        public Movie(string id, string nazev, int hodnoceni, string rok)
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
	
	internal class Parser
	{
        const string web = "http://www.csfd.cz";
        string host = "";
        int port = 0;
		TextWriter output = System.Console.Out;
		
		/* get the last accessed user and page */
		//TODO: get the last accessed user and page, from file

        private string get_page(string url)
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
		
		public void parse()
		{
			/* parse users until number of their ratings drop to 100 or below */
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
		output.WriteLine();
                output.WriteLine("Processing page " + page.ToString() + ".");
		output.WriteLine("------------------------------------------------------------------------------");
				
                List<string> users = null;
                users = getUsersURLs(pageUrl, host, port);

				bool stopProcessing = false;
				int failedCount = 0;
                for (int i = 0; i < users.Count; ++i)
                {
					
					string userID = getUserID(users[i]);
                    output.Write("Processing user " + userID);

                    List<Movie> movies = new List<Movie>();
                    string userReviewsBaseURL = web + users[i] + "hodnoceni/";

                    List<string> reviewsPagesURL = getReviewsPagesURLs(userReviewsBaseURL, host, port);
                    foreach (string reviewsURL in reviewsPagesURL)
                    {
                        getReviews(reviewsURL, movies, host, port);
						output.Write('.');
                    }
					
					/* ensure that the user reviewed more than 100 movies
					 * otherwise stop processing since we process users by their reviews count */
					if (movies.Count <= 100)
					{
						failedCount++;
						output.WriteLine("user has only " + movies.Count.ToString() + " reviews.\n!");
						output.WriteLine(failedCount.ToString() + ". user with low review count!");
						if (failedCount >= 100)
						{
							output.WriteLine("Too many users with low reviews count, breaking now.");
							stopProcessing = true;
							break;
						}
					}
					else
					{
						/* write results to file identified by userID */
						string dataFile = "./data/" + userID + ".txt";
                				StreamWriter writer = new StreamWriter(@dataFile);
				                foreach (Movie f in movies)
				                {
				                        writer.WriteLine(f.get_vse());
				                }
                    				writer.Close();
						output.WriteLine(" finished with " + movies.Count.ToString() + " saved reviews.");
					}
                }

				if (stopProcessing)
				{
					break;
				}
			}			
		}
		
        private List<string> getUsersURLs(string url, string host, int por)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
            
            /* Creates an HtmlDocument object from an URL */
            HtmlDocument HtmlDoc = null;
			do {
				try {
		            if (host.Length == 0)
		            {
		                HtmlDoc = htmlWeb.Load(url);
		            }
		            else
		            {
		                HtmlDoc = htmlWeb.Load(url, host, por, "", "");
		            }            				
				} catch (Exception ex) {
					processException(ex);
				}
			} while (HtmlDoc == null);

			/* Finds users with number of reviews higher than 100 and saves their profile URL */
            HtmlNode usersTable = HtmlDoc.DocumentNode.SelectSingleNode(@"//table[@class=""ui-table-list""]");
            HtmlNodeCollection trNodes = usersTable.SelectNodes(@".//tr");
            List<string> usersURLs = new List<string>();
            foreach (HtmlNode tr in trNodes)
            {
                HtmlNodeCollection tdNodes = tr.SelectNodes(@".//td");
                int numReviews = getReviewCount(tdNodes);
                if (numReviews > 100)
                {
                    HtmlNode tdNickClass = tr.SelectSingleNode(@".//td[@class=""nick""]");
                    HtmlNode anchor = tdNickClass.SelectSingleNode(@".//a");
                    string userURL = anchor.Attributes["href"].Value;
                    usersURLs.Add(userURL);
                }
            }
			
            return usersURLs;
        }

		//TODO DONE, only add some comments		
		private void processException(Exception e)
		{
			output.WriteLine(e.Message);
			//vyjimka byva zpusobena tim, ze me nepusti na server, zkus to za 5 min znovu
			System.Threading.Thread.Sleep(1000 * 60 * 5);
		}
		
		//TODO DONE, only add some comments
        private static int getReviewCount(HtmlNodeCollection tdNodes)
        {
            if (tdNodes.Count > 3)
            {
                string[] numReviewsParts = tdNodes[3].InnerText.Split(' ');
                string numReviews = numReviewsParts[0];
                try
                {
                    return Convert.ToInt32(numReviews);
                }
                catch
                {
                    return 0;
                }
			}
            // else
            return 0;
        }

		//TODO DONE, only add some comments
        private List<string> getReviewsPagesURLs(string baseURL, string host, int port)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
			HtmlDocument HtmlDoc = null;			
			do {
				try {
		            if (host.Length == 0)
		            {
		                HtmlDoc = htmlWeb.Load(baseURL);
		            }
		            else
		            {
		                HtmlDoc = htmlWeb.Load(baseURL, host, port, "", "");
		            }			
				} catch (Exception ex) {
					processException(ex);
				}
			} while (HtmlDoc == null);
			
			
            HtmlNode paginatorDiv = HtmlDoc.DocumentNode.SelectSingleNode(@"//div[@class=""paginator text""]");
            HtmlNodeCollection pagesAnchors = paginatorDiv.SelectNodes(@".//a");
            List<string> pagesURLs = new List<string>();
            HtmlNode lastPageAnchor = pagesAnchors[pagesAnchors.Count - 2];
            string lastPageURL = lastPageAnchor.Attributes["href"].Value;
            string lastPageNumber = lastPageURL.Split('-').Last();
            lastPageNumber = lastPageNumber.Substring(0, lastPageNumber.Length - 1);
            int pageCount = Convert.ToInt32(lastPageNumber);
            for (int i = 1; i <= pageCount; ++i)
            {
                string pageURL = baseURL + "strana-" + i.ToString() + "/";
                pagesURLs.Add(pageURL);
            }
            return pagesURLs;
        }

		/*
		 * userURL pattern: /uzivatel/121600-drsan40/
		 */
		//TODO DONE, only add some comments
		private static string getUserID(string userURL)
		{
			string[] urlParts = userURL.Split('/');
			return urlParts[2];		
		}

        //TODO DONE, only add some comments
		private void getReviews(string reviewsURL, List<Movie> movies, string host, int port)
        {
            HtmlWeb htmlWeb = new HtmlWeb();
			HtmlDocument HtmlDoc = null;
			do {
				try {
			        if (host.Length == 0)
			        {
			            HtmlDoc = htmlWeb.Load(reviewsURL);
			        }
			        else
			        {
			            HtmlDoc = htmlWeb.Load(reviewsURL, host, port, "", "");
			        }
				} catch (Exception ex) {
					processException(ex);
				}
			} while (HtmlDoc == null);
			
            HtmlNode reviewsTable = HtmlDoc.DocumentNode.SelectSingleNode(@"//table[@class=""ui-table-list""]");
            HtmlNodeCollection trNodes =  reviewsTable.SelectNodes(@".//tr");
            for (int i = 0; i < trNodes.Count;i++ )
            {
				///TODO: SKLIPEK 170(leva)*160*110*97
                HtmlNode tr = trNodes[i];
                
                HtmlNode ratingImg = tr.SelectSingleNode(@".//*[@class=""rating""]");
                if (ratingImg == null)
                {
                    continue;
                }
                int rating = getRating(ratingImg);
                HtmlNode movieAnchor = tr.SelectSingleNode(@".//a[@class=""film c2""] | .//a[@class=""film c1""] | .//a[@class=""film c3""]");
                if (movieAnchor == null)
                {
                    continue;
                }
                string movieID = getMovieID(movieAnchor);
                string movieName = getMovieName(movieAnchor);
                HtmlNode movieYearSpan = tr.SelectSingleNode(@".//span[@class=""film-year""]");
                if (movieYearSpan == null)
                {
                    continue;
                }
                string movieYear = getMovieYear(movieYearSpan);
                movies.Add(new Movie(movieID, movieName, rating, movieYear));
            }
        }

		//TODO DONE, only add some comments
        private static int getRating(HtmlNode ratingImg)
        {
            int rating = 0;
            if (ratingImg.Name == "img")
            {
                string stars = ratingImg.Attributes["alt"].Value;
                rating = stars.Length;
            }
            return rating;
        }

		//TODO DONE, only add some comments
		private static string getMovieID(HtmlNode movieAnchor)
        {
            string movieUrl = movieAnchor.Attributes["href"].Value;
            string[] UrlParts = movieUrl.Split('/');
            if (UrlParts.Length > 1)
            {
                string lastPart = UrlParts[UrlParts.Length - 2];
                string[] movieIDs = lastPart.Split('-');
                if (movieIDs.Length > 0)
                {
                    return movieIDs[0];
                }
                else
                {
                    return "n/a";
                }
            }
            else
            {
                return "n/a";
            }
        }

		//TODO DONE, only add some comments
		private static string getMovieName(HtmlNode movieAnchor)
        {
            return movieAnchor.InnerText;
            
        }
		
		//TODO DONE, only add some comments
        private static string getMovieYear(HtmlNode movieYearSpan)
        {
            string movieYear = movieYearSpan.InnerText;
            movieYear = movieYear.Trim();
          	// movieYear = movieYear.Substring(1, movieYear.Length - 2);
            return movieYear;
        }

	}

    class Program
    {
        static void Main(string[] args)
        {
			if (!File.Exists("./data/"))
			{
				System.IO.Directory.CreateDirectory("./data/");
			}
			Parser csfdParser = new Parser();
			csfdParser.parse();
		}		
    }
	
}
