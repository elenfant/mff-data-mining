using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace mba
{
    class Program
    {
        const string path1 = "C:/Users/Fila/Documents/Visual Studio 2010/Projects/ConsoleApplication1/ConsoleApplication1/bin/Debug/data/";
        const string path2 = "D:/Matfyz/Data mining/data/";
        const string path3 = "ratings/best.txt";
        const string path4 = "ratings/number_of.txt";
        const string path5 = "ratings/all.txt";
        const string path6 = "ratings/averages50.txt";
        const string path7 = "ratings/best10.txt";
        const string path8 = "ratings/best25.txt";
        const string path9 = "ratings/best25_50.txt";
        const string path10 = "data/";

        
        static void Main(string[] args)
        {
            Dictionary<int, string> movie_names = new Dictionary<int, string>();
            Dictionary<int, string> movie_names2 = new Dictionary<int, string>();
            Dictionary<int, int[]> movie_ratings = new Dictionary<int, int[]>();
            Dictionary<int, int> number_of_ratings = new Dictionary<int, int>();
            Dictionary<int, int> best_ratings = new Dictionary<int, int>();
            Dictionary<int, int> numbers = new Dictionary<int, int>();
            Dictionary<int, double> avgs = new Dictionary<int, double>();
            const string path_out = "ratings/best25_50.txt";



            ReadFile_T1(path7, movie_names, numbers);
            Dictionary<int, int> cond_ids = Read_ids2(path7);
            Dictionary<int, int> effect_ids = Read_ids2(path9);
            const int conds = 22000;
            const int effects = 13000;
            const int trans = 78922;

            int[] num_of_c = get_num_of(path7, conds);
            int[] num_of_e = get_num_of(path9, effects);

            StreamWriter writer_r = new StreamWriter("ratings/best_rel.txt");
            StreamWriter writer_i = new StreamWriter("ratings/best_imp.txt");
            StreamWriter writer_c = new StreamWriter("ratings/best_comb.txt");
            
            StreamWriter writer_i025 = new StreamWriter("ratings/best_imp_025.txt");


            for (int i = 0; i < conds; ++i)
            {
                string path = "frequences/ft-L" + i.ToString()+".txt";
                if (File.Exists(path))
                {
                    int[] f_line = get_freq_line(path, effects);

                    choose_best(f_line, num_of_c, num_of_e, trans, i, writer_r, writer_i,writer_i025,writer_c,
                        movie_names, cond_ids, effect_ids);
                    if (i % 100 == 0)
                    {
                        Console.WriteLine(path);
                    }
                }
            }

            writer_i.Close();
            writer_r.Close();



      /*      int[,] frequences = new int[conds, effects];
            Frequency_table(path10,frequences, cond_ids, effect_ids);
            WriteFreq(frequences, cond_ids, effect_ids);

            */




     //       RevisitF();

  /*          ReadFile_T2(path5, movie_names2, avgs);
            ReadFile_T3(path8, movie_names, movie_names2, numbers);
            MakeOutput_T1(path_out, movie_names, numbers);

           // MakeOutput_T2(path_out, movie_names, avgs);

         //   int[,] test = new int [13600,21800];

           // string path = path1 + "uziv-1000.txt";
        string[] all_files = Directory.GetFiles(path1);
            
            int i = 1;
            int j = 1;
            foreach (string file_name in all_files)
            {
               Filter_best(j,file_name, movie_names, movie_ratings, number_of_ratings, best_ratings);
                if (i >= 100)
                {
                    Console.WriteLine("Zpracovan soubor " + file_name);
                    i = 1;
                }
                else
                {
                    ++i;
                }
                ++j;
            }
            all_files = Directory.GetFiles(path2);
            foreach (string file_name in all_files)
            {
                Filter_best(j, file_name, movie_names, movie_ratings, number_of_ratings, best_ratings);
                if (i >= 100)
                {
                    Console.WriteLine("Zpracovan soubor " + file_name);
                    i = 1;
                }
                else
                {
                    ++i;
                }
                ++j;
            }
         //   MakeOutput(movie_names, movie_ratings, number_of_ratings, best_ratings);
         

            */

        }



        //vybere nejlepsi pravidlo pro film
        static void choose_best(int[] f_line, int[] num_of_c, int[] num_of_e, int trans, int l,
            StreamWriter writer_r, StreamWriter writer_i, StreamWriter writer_i025, StreamWriter writer_c,  
            Dictionary<int, string> movie_names, Dictionary<int, int> cond_ids, Dictionary<int, int> effect_ids)
        {
            if (cond_ids.ContainsKey(l))
            {
                double[] reliabilities = new double[f_line.Length];
                double[] improvements = new double[f_line.Length];
                
                double[] supports = new double[f_line.Length];
                int max_r_id = -1;

                int max_i_id = -1;
                
                int max_i025_id = -1;

                int max_comb_id = -1;

                for (int i = 0; i < f_line.Length; ++i)
                {
                    reliabilities[i] = reliability(num_of_c[l], f_line[i]);
                    improvements[i] = improvement(trans, num_of_c[l], num_of_e[i], f_line[i]);
                    
                    supports[i] = support(trans, f_line[i]);

                    if (effect_ids.ContainsKey(i) && cond_ids[l] != effect_ids[i])
                    {
                        if (max_r_id >= 0)
                        {
                            if (reliabilities[i] > reliabilities[max_r_id])
                            {
                                max_r_id = i;
                            }
                            if (improvements[i] > improvements[max_i_id])
                            {
                                max_i_id = i;
                            }
                            
                            if (reliabilities[i] >= 0.25 && improvements[i] > improvements[max_i025_id])
                            {
                                max_i025_id = i;
                            }
                            if (reliabilities[i] * improvements[i] > reliabilities[max_comb_id] * improvements[max_comb_id])
                            {
                                max_comb_id = i;
                            }

                        }
                        else
                        {

                            max_r_id = i;
                            max_i_id = i;
                            max_comb_id = i;
                            
                            max_i025_id = i;
                        }
                    }

                }
                if (max_i025_id == 0)
                {
                    max_i025_id = max_i_id;
                }
                


                string line_r = cond_ids[l].ToString() + "\t;;\t" + movie_names[cond_ids[l]] + "\t;;\t" + "==>\t" + effect_ids[max_r_id].ToString() + "\t;;\t" + movie_names[effect_ids[max_r_id]] + "\t;;\t" +
                    Math.Round(supports[max_r_id], 2).ToString() + "\t;;\t" + Math.Round(reliabilities[max_r_id], 2).ToString() + "\t;;\t" + Math.Round(improvements[max_r_id], 2).ToString();

                writer_r.WriteLine(line_r);

                string line_i = cond_ids[l].ToString() + "\t;;\t" + movie_names[cond_ids[l]] + "\t;;\t" + "==>\t" + effect_ids[max_i_id].ToString() + "\t;;\t" + movie_names[effect_ids[max_i_id]] + "\t;;\t" +
                    Math.Round(supports[max_i_id], 2).ToString() + "\t;;\t" + Math.Round(reliabilities[max_i_id], 2).ToString() + "\t;;\t" + Math.Round(improvements[max_i_id], 2).ToString();
                writer_i.WriteLine(line_i);

                

                string line_i025 = cond_ids[l].ToString() + "\t;;\t" + movie_names[cond_ids[l]] + "\t;;\t" + "==>\t" + effect_ids[max_i025_id].ToString() + "\t;;\t" + movie_names[effect_ids[max_i025_id]] + "\t;;\t" +
                    Math.Round(supports[max_i025_id], 2).ToString() + "\t;;\t" + Math.Round(reliabilities[max_i025_id], 2).ToString() + "\t;;\t" + Math.Round(improvements[max_i025_id], 2).ToString();
                writer_i025.WriteLine(line_i025);

                string line_c = cond_ids[l].ToString() + "\t;;\t" + movie_names[cond_ids[l]] + "\t;;\t" + "==>\t" + effect_ids[max_comb_id].ToString() + "\t;;\t" + movie_names[effect_ids[max_comb_id]] + "\t;;\t" +
                    Math.Round(supports[max_comb_id], 2).ToString() + "\t;;\t" + Math.Round(reliabilities[max_comb_id], 2).ToString() + "\t;;\t" + Math.Round(improvements[max_comb_id], 2).ToString();
                writer_c.WriteLine(line_c);

            }
        }


        //nacte radek tab. cetnosti
        static int[] get_freq_line(string path, int effects)
        {
            int[] f_line = new int[effects];
            int i = 0;
            StreamReader reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                int f = Convert.ToInt32(line);
                f_line[i] = f;
                ++i;
            }
            return f_line;

        }

        //nacte pocty hodnoceni
        static int[] get_num_of(string path, int length)
        {
            int[] numbers = new int[length];
            StreamReader reader = new StreamReader(path);
            int i = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { ";;" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {

                    string count_s = pieces[2].Trim();
                    int count = Convert.ToInt32(count_s);


                    numbers[i] = count;
                    ++i;

                }
            }
            return numbers;
        }

        static double reliability(int num_of_a, int num_of_a_b)
        {
            double rel = (double)num_of_a_b / (double)num_of_a;
            return rel;

        }

        static double support(int trans, int num_of_a_b)
        {
            double sup = (double)num_of_a_b / (double)trans;
            return sup;
        }

        static double improvement(int trans,int num_of_a, int num_of_b, int num_of_a_b)
        {
            double p_a_b = (double)num_of_a_b / (double)trans;
            double p_a = (double)num_of_a / (double)trans;
            double p_b = (double)num_of_b / (double)trans;
            double imp = p_a_b / (p_a * p_b);
            return imp;
        }



        //spocte prumery z hodnoceni all
        static void ReadFile_T2(string path, Dictionary<int, string> movie_names, Dictionary<int, double> avgs)
        {
            StreamReader reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { ";;" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {
                    string id_s = pieces[0].Trim();
                    int id = Convert.ToInt32(id_s);
                    string name = pieces[1].Trim();
                    string ratings_s = pieces[2].Trim();
                    string[] ratings_s2 = ratings_s.Split('-');

                    int[] ratings = new int[6];
                    ratings[0] = Convert.ToInt32(ratings_s2[5].Trim());
                    ratings[1] = Convert.ToInt32(ratings_s2[4].Trim());
                    ratings[2] = Convert.ToInt32(ratings_s2[3].Trim());
                    ratings[3] = Convert.ToInt32(ratings_s2[2].Trim());
                    ratings[4] = Convert.ToInt32(ratings_s2[1].Trim());
                    ratings[5] = Convert.ToInt32(ratings_s2[0].Trim());

                    int count = 0;
                    int sum = 0;
                    for (int i = 0; i <= 5; ++i)
                    {
                        count += ratings[i];
                        sum += ratings[i] * i * 20;
                    }
                    double average = (double)sum / (double)count;

                    if (condition2(average))
                    {
                        movie_names.Add(id, name);
                        avgs.Add(id, average);
                    }
                }
            }
        }

        //nacte pocty hodnoceni z best souboru
        static void ReadFile_T1(string path, Dictionary<int, string> movie_names, Dictionary<int, int> numbers)
        {
            StreamReader reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { ";;" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {
                    string id_s = pieces[0].Trim();
                    int id = Convert.ToInt32(id_s);
                    string name = pieces[1].Trim();
                    string count_s = pieces[2].Trim();
                    int count = Convert.ToInt32(count_s);
                    if (condition(count))
                    {
                        movie_names.Add(id, name);
                        numbers.Add(id, count);
                    }
                }
            }
        }

        //nacte z best souboru hodnoceni tech, ktere jsou obsazeny v movie_names2
        static void ReadFile_T3(string path, Dictionary<int, string> movie_names, Dictionary<int, string> movie_names2, Dictionary<int, int> numbers)
        {
            StreamReader reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { ";;" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {
                    string id_s = pieces[0].Trim();
                    int id = Convert.ToInt32(id_s);
                    string name = pieces[1].Trim();
                    string count_s = pieces[2].Trim();
                    int count = Convert.ToInt32(count_s);
                    if (condition3(id,movie_names2))
                    {
                        movie_names.Add(id, name);
                        numbers.Add(id, count);
                    }
                }
            }
        }

        static bool condition(int x)
        {
           /* if (x >= 25)
            {

                return true;
            }
            else
            {
                return false;
            }*/
            return true;
        }

        static bool condition2(double x)
        {
            if (x >= 50)
            {

                return true;
            }
            else
            {
                return false;
            }
          //  return true;
        }
        static bool condition3(int id, Dictionary<int, string> movie_names)
        {
            if (movie_names.ContainsKey(id))
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        //nacte hodnoceni z dat
        static void ReadFile(string path, Dictionary<int, string> movie_names, Dictionary<int, int[]> movie_ratings, Dictionary<int, int> number_of_ratings, Dictionary<int, int> best_ratings)
        {
            
            StreamReader reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { "\t;\t" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {
                    string id_s = pieces[0].Trim();
                    int id = Convert.ToInt32(id_s);
                    string name = pieces[1].Trim();
                    string rating_s = pieces[2].Trim();
                    int rating = Convert.ToInt32(rating_s);
                    if (rating <= 5 && rating >= 0)
                    {
                        if (!movie_ratings.Keys.Contains(id))
                        {
                            movie_names.Add(id, name);
                            int[] rating_array = new int[6] { 0, 0, 0, 0, 0, 0 };
                            movie_ratings.Add(id, rating_array);

                            number_of_ratings.Add(id, 0);
                            best_ratings.Add(id, 0);


                        }

                        movie_ratings[id][rating]++;
                        number_of_ratings[id]++;
                        if (rating == 5)
                        {
                            best_ratings[id]++;
                        }
                    }
                }

            }
        }


        //vyber z dat jen nejlepsi hodnoceni
        static void Filter_best(int poradi, string path, Dictionary<int, string> movie_names, Dictionary<int, int[]> movie_ratings, Dictionary<int, int> number_of_ratings, Dictionary<int, int> best_ratings)
        {
            const string out_folder = "data/";
            
        //    string path_out = out_folder + "uziv-" + poradi.ToString() + ".txt"; 
            string path_out = out_folder+Path.GetFileName(path);
            StreamReader reader = new StreamReader(path);
            
            StreamWriter writer = new StreamWriter(path_out);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { "\t;\t" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {
                    string id_s = pieces[0].Trim();
                    int id = Convert.ToInt32(id_s);
                    string name = pieces[1].Trim();
                    string rating_s = pieces[2].Trim();
                    int rating = Convert.ToInt32(rating_s);
                    if (rating == 5)
                    {
                        writer.WriteLine(line);
                    }
                }

            }
            reader.Close();
            writer.Close();
        }


        //prumery
        static void MakeOutput_T2(string path, Dictionary<int, string> movie_names, Dictionary<int, double> avgs)
        {
            StreamWriter writer = new StreamWriter(path);
            foreach (int id in movie_names.Keys)
            {
                double average = Math.Round(avgs[id], 2);

                string line = id.ToString() + "\t;;\t" + movie_names[id] + "\t;;\t" + average.ToString();
                writer.WriteLine(line);
            }
            writer.Close();

        }

        //best
        static void MakeOutput_T1(string path, Dictionary<int, string> movie_names, Dictionary<int, int> numbers)
        {
            StreamWriter writer = new StreamWriter(path);
            foreach (int id in movie_names.Keys)
            {
                string line = id.ToString() + "\t;;\t" + movie_names[id] + "\t;;\t" + numbers[id].ToString();
                writer.WriteLine(line);
            }
            writer.Close();
            
        }

        //all, best, pocty
        static void MakeOutput(Dictionary<int, string> movie_names, Dictionary<int, int[]> movie_ratings, Dictionary<int, int> number_of_ratings, Dictionary<int, int> best_ratings)
        {
            const string all_ratings_file = "ratings/all.txt";
            const string best_ratings_file = "ratings/best.txt";
            const string number_of_ratings_file = "ratings/number_of.txt";
            StreamWriter writer_all = new StreamWriter(all_ratings_file);
            StreamWriter writer_best = new StreamWriter(best_ratings_file);
            StreamWriter writer_number_of = new StreamWriter(number_of_ratings_file);

            foreach (int id in movie_names.Keys)
            {
                int[] rats = movie_ratings[id];
                string all_line = id.ToString() + "\t;;\t" + movie_names[id] + "\t;;\t" + rats[5].ToString() + " - " + rats[4].ToString() + " - " + rats[3].ToString() + " - " + rats[2].ToString() + " - " + rats[1].ToString() + " - " + rats[0].ToString();
                writer_all.WriteLine(all_line);

                string best_line = id.ToString() + "\t;;\t" + movie_names[id] + "\t;;\t" + best_ratings[id].ToString();
                writer_best.WriteLine(best_line);

                string number_of_line = id.ToString() + "\t;;\t" + movie_names[id] + "\t;;\t" + number_of_ratings[id].ToString();
                writer_number_of.WriteLine(number_of_line);
            }

            writer_all.Close();
            writer_best.Close();
            writer_number_of.Close();
            
        }


        //tabulka cetnosti
        static void WriteFreq(int[,] frequences, Dictionary<int, int> cond_ids, Dictionary<int, int> effect_ids)
        {
            string folder_path = "frequences/";

            
            for (int i = 0; i < cond_ids.Count; ++i)
            {
                string path = folder_path + "ft-L" + i.ToString();
                StreamWriter writer = new StreamWriter(path);
                for (int j = 0; j < effect_ids.Count; ++j)
                {
                    if (j == 0)
                    {
                        writer.Write(frequences[i, j]);
                    }
                    else
                    {
                        writer.Write(";\t" + frequences[i, j]);
                    }

                }
                writer.Close();
            }
            
        }




        
        static Dictionary<int, int> Read_ids(string path)
        {
            Dictionary<int, int> ids = new Dictionary<int, int>();
            StreamReader reader = new StreamReader(path);
            int i = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { ";;" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {
                    string id_s = pieces[0].Trim();
                    int id = Convert.ToInt32(id_s);
                    ids.Add(id, i);
                    i++;

                }

            }
            return ids;
        }

        static Dictionary<int, int> Read_ids2(string path)
        {
            Dictionary<int, int> ids = new Dictionary<int, int>();
            StreamReader reader = new StreamReader(path);
            int i = 0;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { ";;" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {
                    string id_s = pieces[0].Trim();
                    int id = Convert.ToInt32(id_s);
                    ids.Add(i, id);
                    i++;

                }

            }
            return ids;
        }


        //nacti frekvencni tabulku
        static int[,] Frequency_table(string path, int[,] frequences, Dictionary<int, int> cond_ids, Dictionary<int, int> effect_ids)
        {
            int n = frequences.GetLength(0);
            for (int i = 0; i < n; ++i)
            {
                int m = frequences.GetLength(1);
                for (int j = 0; j < m; ++j)
                {
                    frequences[i, j] = 0;
                }

            }


            string[] all_files = Directory.GetFiles(path10);
            int k = 1;
            foreach (string file_name in all_files)
            {

                Freq_trans(file_name, cond_ids, effect_ids, frequences);
                if (k >= 100)
                {
                    Console.WriteLine("Zpracovavam " + Path.GetFileName(file_name));
                    k = 1;
                }
                else
                {
                    ++k;
                }
            }

            return frequences;
        }


        static void Freq_trans(string path, Dictionary<int, int> cond_ids, Dictionary<int, int> effect_ids, int[,] frequences)
        {
            List<int> movie_ids = new List<int>();
            StreamReader reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] pieces = line.Split(new string[1] { "\t;\t" }, System.StringSplitOptions.None);
                if (pieces.Length == 3)
                {
                    string id_s = pieces[0].Trim();
                    int id = Convert.ToInt32(id_s);
                    string name = pieces[1].Trim();
                    string rating_s = pieces[2].Trim();
                    int rating = Convert.ToInt32(rating_s);
                    movie_ids.Add(id);
                }
            }
            reader.Close();
            for (int i = 0; i < movie_ids.Count; ++i)
            {
                if (cond_ids.ContainsKey(movie_ids[i]))
                {
                    for (int j = 0; j < movie_ids.Count; ++j)
                    {
                        if (effect_ids.ContainsKey(movie_ids[j]))
                        {
                            int id_cond = cond_ids[movie_ids[i]];
                            int id_effect = effect_ids[movie_ids[j]];
                            frequences[id_cond, id_effect]++;
                        }
                    }
                }
            }

        }
        
    
    
    
    }
}
