using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Quaver_First_Place_Stats
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static Stats stats = new Stats();
        struct Average // Not a good name but don't know what else to call this tbh
        {
            // Lowest, highest, and mean values
            public double low, mean, high;

            // The maps associated with the corresponding values
            public Map lowMap, highMap;
        }
        struct Stats
        {
            // Individually displayed values
            public int TotalCount;
            public int Duplicates;
            public Score Oldest;
            public int TotalMarvellous, TotalPerfect, TotalGreat;

            // Table values
            public Average Performance, Difficulty, Accuracy, Length, BPM, Scroll, Combo, Misses, LNPercent, Rice, Noodle, Plays;

            // Breakdown values
            public Dictionary<string, int> Grade, Charter, Artist, Tags;
        }
        struct Score
        {
            public DateTime Date;
            public double PerformanceRating;
            public double Accuracy;
            public int MaxCombo;
            public int MarvelousCount;
            public int PerfectCount;
            public int GreatCount;
            public int MissCount;
            public string Grade;
            public int ScrollSpeed;
            public Map Map;

            public Score (DateTime date, double performanceRating, double accuracy, int maxCombo, 
                int marvelousCount, int perfectCount, int greatCount, int missCount, string grade, int scrollSpeed, Map map)
            {
                Date = date;
                PerformanceRating = performanceRating;
                Accuracy = accuracy;
                MaxCombo = maxCombo;
                MarvelousCount = marvelousCount;
                PerfectCount = perfectCount;
                GreatCount = greatCount;
                MissCount = missCount;
                Grade = grade;
                ScrollSpeed = scrollSpeed;
                Map = map;
            }
        }
        struct Map
        {
            public string ID; //Is actually the MD5 hash of the map, but serves the same purpose as an ID
            public string Charter;
            public string Artist;
            public string Title;
            public List<string> Tags;
            public string DifficultyName;
            public int Length;
            public double BPM;
            public double DifficultyRating;
            public int RiceCount;
            public int NoodleCount;
            public double LNPercent;
            public int MaxCombo;
            public int PlayCount;

            public Map(string id, string charter, string artist, string title, List<string> tags, string difficultyName, 
                int length, double bpm, double difficultyRating, int riceCount, int noodleCount, double lnPercent, int maxCombo, int playCount)
            {
                ID = id;
                Charter = charter;
                Artist = artist;
                Title = title;
                Tags = tags;
                DifficultyName = difficultyName;
                Length = length;
                BPM = bpm;
                DifficultyRating = difficultyRating;
                RiceCount = riceCount;
                NoodleCount = noodleCount;
                LNPercent = lnPercent;
                MaxCombo = maxCombo;
                PlayCount = playCount;
            }
        }

        static async Task Main()
        {
            // Sets the console window size to 90% of its maximum
            Console.SetWindowSize((int)(Console.LargestWindowWidth * 0.9), (int)(Console.LargestWindowHeight * 0.9));

            // Asks the user to input their user ID
            // Does not check if the ID is valid because im lazy, so if they input something bad it will probably crash
            string userID = GetUserID();

            // Displays the name and rank of the user (may add more info later)
            // Makes a single API call to get the info
            await DisplayUserInfo(userID);

            // Calls the API to get Json files of all the maps the user has first place on
            // Returns in batches of (maximum) 50 maps
            List<string> firstPlaceJsonBatches = await GetFirstPlaces(userID);

            // Combines and parses all json batches into one list of scores
            List<Score> firstPlaceScores = GetScoreInfos(firstPlaceJsonBatches);

            // Calculates all the relevant stats and stores it in the stats static variable
            AssignAttributeStats(firstPlaceScores);

            // Displays stats about the user's first places
            DisplayFirstPlaceStats();
        }
        static void DisplayFirstPlaceStats()
        {
            int padding = 32;
            int variablePadding = Console.LargestWindowWidth / 4;


            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nTotal First Places: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.TotalCount);

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nDuplicate First Places: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Duplicates + "\n");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("(these are errors which should not exist, but are counted in the total first places because they also show on the website)");
            
            Console.Write("\n\n\n\n");


            // Draw table column headings
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("".PadLeft(padding) + "Highest".PadRight(variablePadding));

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Mean".PadRight(Console.LargestWindowWidth / 4));

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Lowest");

            Console.WriteLine("\n");


            DrawTableRow(padding, variablePadding, "Performance Rating",
                stats.Performance.high.ToString("#0.##"), stats.Performance.mean.ToString("#0.##"), stats.Performance.low.ToString("#0.##"),
                stats.Performance.highMap, stats.Performance.lowMap);
            DrawTableRow(padding, variablePadding, "Difficulty Rating (1.0x)",
                stats.Difficulty.high.ToString("#0.##"), stats.Difficulty.mean.ToString("#0.##"), stats.Difficulty.low.ToString("#0.##"),
                stats.Difficulty.highMap, stats.Difficulty.lowMap);
            DrawTableRow(padding, variablePadding, "Accuracy",
                stats.Accuracy.high.ToString("#.##") + "%", stats.Accuracy.mean.ToString("#.##") + "%", stats.Accuracy.low.ToString("#.##") + "%",
                stats.Accuracy.highMap, stats.Accuracy.lowMap);
            DrawTableRow(padding, variablePadding, "Length (1.0x)",
                new DateTime(0).AddMilliseconds(stats.Length.high).ToLongTimeString(), 
                new DateTime(0).AddMilliseconds(stats.Length.mean).ToLongTimeString(), 
                new DateTime(0).AddMilliseconds(stats.Length.low).ToLongTimeString(),
                stats.Length.highMap, stats.Length.lowMap);
            DrawTableRow(padding, variablePadding, "BPM (1.0x)",
                stats.BPM.high.ToString("#.##"), stats.BPM.mean.ToString("#.##"), stats.BPM.low.ToString("#.##"),
                stats.BPM.highMap, stats.BPM.lowMap);
            DrawTableRow(padding, variablePadding, "Scroll Speed",
                (stats.Scroll.high / 10.0).ToString("#.#"), (stats.Scroll.mean / 10.0).ToString("#.#"), (stats.Scroll.low / 10.0).ToString("#.#"),
                stats.Scroll.highMap, stats.Scroll.lowMap);
            DrawTableRow(padding, variablePadding, "Max Combo",
                stats.Combo.high.ToString("#"), stats.Combo.mean.ToString("#"), stats.Combo.low.ToString("#"),
                stats.Combo.highMap, stats.Combo.lowMap);
            DrawTableRow(padding, variablePadding, "Miss Count",
                stats.Misses.high.ToString("#0"), stats.Misses.mean.ToString("#0"), stats.Misses.low.ToString("#0"),
                stats.Misses.highMap, stats.Misses.lowMap);
            DrawTableRow(padding, variablePadding, "LN Percentage",
                stats.LNPercent.high.ToString("#0.##") + "%", stats.LNPercent.mean.ToString("#0.##") + "%", stats.LNPercent.low.ToString("#0.##") + "%",
                stats.LNPercent.highMap, stats.LNPercent.lowMap);
            DrawTableRow(padding, variablePadding, "Rice Count",
                stats.Rice.high.ToString("#0"), stats.Rice.mean.ToString("#0"), stats.Rice.low.ToString("#0"),
                stats.Rice.highMap, stats.Rice.lowMap);
            DrawTableRow(padding, variablePadding, "Noodle Count",
                stats.Noodle.high.ToString("#0"), stats.Noodle.mean.ToString("#0"), stats.Noodle.low.ToString("#0"),
                stats.Noodle.highMap, stats.Noodle.lowMap);
            DrawTableRow(padding, variablePadding, "Playcount",
                stats.Plays.high.ToString("#0"), stats.Plays.mean.ToString("#0"), stats.Plays.low.ToString("#0"),
                stats.Plays.highMap, stats.Plays.lowMap);


            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n\nOverall MA: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;    
            Console.Write(((double)stats.TotalMarvellous / stats.TotalPerfect).ToString("#0.##"));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" (");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(stats.TotalMarvellous);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" : ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(stats.TotalPerfect);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(")\n");

            Console.Write("Overall PA: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(((double)stats.TotalPerfect / stats.TotalGreat).ToString("#0.##"));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" (");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(stats.TotalPerfect);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" : ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(stats.TotalGreat);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(")\n\n");

            Console.Write("Oldest First Place: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(stats.Oldest.Map.Title);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write(" [" + stats.Oldest.Map.DifficultyName + "]");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" on ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Oldest.Date + "\n\n");


            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Grade Counts\n");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("X".PadLeft(4));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Grade["X"] + "\n");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("SS".PadLeft(4));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Grade["SS"] + "\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("S".PadLeft(4));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Grade["S"] + "\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("A".PadLeft(4));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Grade["A"] + "\n");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("B".PadLeft(4));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Grade["B"] + "\n");

            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("C".PadLeft(4));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Grade["C"] + "\n");

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("D".PadLeft(4));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Grade["D"] + "\n");

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("F".PadLeft(4));
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(": ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(stats.Grade["F"] + "\n");


            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nCharters >3\n");
            List<KeyValuePair<string, int>> charters = stats.Charter.OrderByDescending(x => x.Value).ToList();
            foreach (KeyValuePair<string, int> charter in charters)
            {
                if (charter.Value < 3)
                {
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(charter.Value.ToString().PadLeft(4));
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(": ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(charter.Key + "\n");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nArtists >3\n");
            List<KeyValuePair<string, int>> artists = stats.Artist.OrderByDescending(x => x.Value).ToList();
            foreach (KeyValuePair<string, int> artist in artists)
            {
                if (artist.Value < 3)
                {
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(artist.Value.ToString().PadLeft(4));
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(": ");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(artist.Key + "\n");
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nTags >5\n");
            List<KeyValuePair<string, int>> tags = stats.Tags.OrderByDescending(x => x.Value).ToList();
            foreach (KeyValuePair<string, int> tag in tags)
            {
                if (tag.Value < 5)
                {
                    continue;
                }

                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.Write(tag.Value.ToString().PadLeft(4));
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(": ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write(tag.Key + "\n");
            }


            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\n\n");
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
            Console.ReadKey(true);


            // highest performance rating, average performance rating, lowest performance rating
            // highest difficulty rating, average difficulty rating, lowest difficulty rating
            // highest acc, average acc, lowest acc
            // highest length, average length, lowest length
            // highest bpm, average bpm, lowest bpm
            // highest scroll speed, average scroll speed, lowest scroll speed
            // highest max combo, average max combo, lowest max combo
            // highest misscount, average misscount, lowest misscount (0, obviously)
            // highest ln percentage, average ln percentage, lowest ln percentage
            // highest rice count, average rice count, lowest rice count
            // highest ln count, average ln count, lowest ln count
            // highest playcount, average playcount, lowest playcount

            // overall ratio (MA and PA)
            // oldest first place

            // grade breakdown
            // charter breakdown
            // artist breakdown
            // tags breakdown

        }
        static void DrawTableRow(int padding, int variablePadding, string name, string highValue, string meanValue, string lowValue, Map highMap, Map lowMap)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write((name + ": ").PadLeft(padding));
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(highValue.PadRight(variablePadding));
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write(meanValue.PadRight(variablePadding));
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write(lowValue);

            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" ".PadLeft(padding) + highMap.Title);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(" [" + highMap.DifficultyName + "]");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" ".PadLeft(2 * variablePadding - 3 - highMap.Title.Length - highMap.DifficultyName.Length) + lowMap.Title);
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(" [" + lowMap.DifficultyName + "]");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("\n\n");
        }
        static void AssignAttributeStats(List<Score> scores)
        {
            HashSet<string> mapIDs = new HashSet<string>();

            int count = scores.Count;
            stats.TotalCount = count;

            double performanceTotal = 0;
            double difficultyTotal = 0;
            double accuracyTotal = 0;
            double lengthTotal = 0;
            double bpmTotal = 0;
            double scrollTotal = 0;
            int comboTotal = 0;
            int missTotal = 0;
            double lnPercentTotal = 0;
            int riceTotal = 0;
            int noodleTotal = 0;
            int playsTotal = 0;

            stats.Performance.low = double.MaxValue;
            stats.Difficulty.low = double.MaxValue;
            stats.Accuracy.low = double.MaxValue;
            stats.Length.low = double.MaxValue;
            stats.BPM.low = double.MaxValue;
            stats.Scroll.low = double.MaxValue;
            stats.Combo.low = double.MaxValue;
            stats.Misses.low = double.MaxValue;
            stats.LNPercent.low = double.MaxValue;
            stats.Rice.low = double.MaxValue;
            stats.Noodle.low = double.MaxValue;
            stats.Plays.low = double.MaxValue;

            stats.Oldest = scores[0];
            stats.Grade = new Dictionary<string, int> { 
                {"X", 0}, {"SS", 0},{"S", 0}, {"A", 0}, {"B", 0}, {"C", 0}, {"D", 0}, {"F", 0} };
            stats.Charter = new Dictionary<string, int>();
            stats.Artist = new Dictionary<string, int>();
            stats.Tags = new Dictionary<string, int>();


            for (int index = 0; index < scores.Count; index++)
            {
                Score score = scores[index];

                // Check if the score is a glitched duplicate score
                if (mapIDs.Contains(score.Map.ID))
                {
                    stats.Duplicates++;
                    scores.RemoveAt(index);
                    index--;
                    continue;
                }
                mapIDs.Add(score.Map.ID);


                // Check if the score is a new oldest score
                if (score.Date < stats.Oldest.Date)
                {
                    stats.Oldest = score;
                }


                // Add 1 to the counter for the grade of the score
                stats.Grade[score.Grade]++;

                // Add 1 to the counter for the charter of the map
                if (!stats.Charter.ContainsKey(score.Map.Charter))
                {
                    stats.Charter.Add(score.Map.Charter, 0);
                }
                stats.Charter[score.Map.Charter]++;

                // Add 1 to the counter for the artist of the map
                if (!stats.Artist.ContainsKey(score.Map.Artist))
                {
                    stats.Artist.Add(score.Map.Artist, 0);
                }
                stats.Artist[score.Map.Artist]++;

                // Add 1 to the counters for each tag
                foreach (string tag in score.Map.Tags)
                {
                    if (!stats.Tags.ContainsKey(tag))
                    {
                        stats.Tags.Add(tag, 0);
                    }
                    stats.Tags[tag]++;
                }


                // Add marv/perf/great counts to the total (for calculating MA/PA later)
                stats.TotalMarvellous += score.MarvelousCount;
                stats.TotalPerfect += score.PerfectCount;
                stats.TotalGreat += score.GreatCount;


                double performance = score.PerformanceRating;
                double difficulty = score.Map.DifficultyRating;
                double accuracy = score.Accuracy;
                int length = score.Map.Length;
                double bpm = score.Map.BPM;
                double scroll = score.ScrollSpeed;
                int combo = score.MaxCombo;
                int misses = score.MissCount;
                double lnPercent = score.Map.LNPercent;
                int rice = score.Map.RiceCount;
                int noodle = score.Map.NoodleCount;
                int plays = score.Map.PlayCount;

                performanceTotal += performance;
                difficultyTotal += difficulty;
                accuracyTotal += accuracy;
                lengthTotal += length;
                bpmTotal += bpm;
                scrollTotal += scroll;
                comboTotal += combo;
                missTotal += misses;
                lnPercentTotal += lnPercent;
                riceTotal += rice;
                noodleTotal += noodle;
                playsTotal += plays;


                if (performance < stats.Performance.low)
                { stats.Performance.low = performance; stats.Performance.lowMap = score.Map; }
                if (performance > stats.Performance.high)
                { stats.Performance.high = performance; stats.Performance.highMap = score.Map; }

                if (difficulty < stats.Difficulty.low)
                { stats.Difficulty.low = difficulty; stats.Difficulty.lowMap = score.Map; }
                if (difficulty > stats.Difficulty.high)
                { stats.Difficulty.high = difficulty; stats.Difficulty.highMap = score.Map; }

                if (accuracy < stats.Accuracy.low)
                { stats.Accuracy.low = accuracy; stats.Accuracy.lowMap = score.Map; }
                if (accuracy > stats.Accuracy.high)
                { stats.Accuracy.high = accuracy; stats.Accuracy.highMap = score.Map; }

                if (length < stats.Length.low)
                { stats.Length.low = length; stats.Length.lowMap = score.Map; }
                if (length > stats.Length.high)
                { stats.Length.high = length; stats.Length.highMap = score.Map; }


                if (bpm < stats.BPM.low)
                { stats.BPM.low = bpm; stats.BPM.lowMap = score.Map; }
                if (bpm > stats.BPM.high)
                { stats.BPM.high = bpm; stats.BPM.highMap = score.Map; }

                if (scroll < stats.Scroll.low)
                { stats.Scroll.low = scroll; stats.Scroll.lowMap = score.Map; }
                if (scroll > stats.Scroll.high)
                { stats.Scroll.high = scroll; stats.Scroll.highMap = score.Map; }

                if (combo < stats.Combo.low)
                { stats.Combo.low = combo; stats.Combo.lowMap = score.Map; }
                if (combo > stats.Combo.high)
                { stats.Combo.high = combo; stats.Combo.highMap = score.Map; }

                if (misses < stats.Misses.low)
                { stats.Misses.low = misses; stats.Misses.lowMap = score.Map; }
                if (misses > stats.Misses.high)
                { stats.Misses.high = misses; stats.Misses.highMap = score.Map; }


                if (lnPercent < stats.LNPercent.low)
                { stats.LNPercent.low = lnPercent; stats.LNPercent.lowMap = score.Map; }
                if (lnPercent > stats.LNPercent.high)
                { stats.LNPercent.high = lnPercent; stats.LNPercent.highMap = score.Map; }

                if (rice < stats.Rice.low)
                { stats.Rice.low = rice; stats.Rice.lowMap = score.Map; }
                if (rice > stats.Rice.high)
                { stats.Rice.high = rice; stats.Rice.highMap = score.Map; }

                if (noodle < stats.Noodle.low)
                { stats.Noodle.low = noodle; stats.Noodle.lowMap = score.Map; }
                if (noodle > stats.Noodle.high)
                { stats.Noodle.high = noodle; stats.Noodle.highMap = score.Map; }

                if (plays < stats.Plays.low)
                { stats.Plays.low = plays; stats.Plays.lowMap = score.Map; }
                if (plays > stats.Plays.high)
                { stats.Plays.high = plays; stats.Plays.highMap = score.Map; }
            }

            stats.Performance.mean = performanceTotal / count;
            stats.Difficulty.mean = difficultyTotal / count;
            stats.Accuracy.mean = accuracyTotal / count;
            stats.Length.mean = lengthTotal / count;
            stats.BPM.mean = bpmTotal / count;
            stats.Scroll.mean = scrollTotal / count;
            stats.Combo.mean = comboTotal / count;
            stats.Misses.mean = missTotal / count;
            stats.LNPercent.mean = lnPercentTotal / count;
            stats.Rice.mean = riceTotal / count;
            stats.Noodle.mean = noodleTotal / count;
            stats.Plays.mean = playsTotal / count;
        }
        static List<Score> GetScoreInfos(List<string> firstPlacesJsons)
        {
            // Combines all batches of scores into one list

            List<Score> scores = new List<Score>();
            foreach (string json in firstPlacesJsons)
            {
                // Parse the score information from the json response
                List<Score> partialScores = ParseScores(json);
                scores.AddRange(partialScores);
            }

            return scores;
        }
        static async Task<List<string>>GetFirstPlaces(string userID)
        {
            List<string> firsts = new List<string>();
            string response = null;

            int page = 0;
            while (response == null || response.Split('{').Length > 2)
            {
                string requestURL = "https://api.quavergame.com/v2/user/" + userID + "/scores/1/firstplace?page=" + page;

                response = await CallAPI(requestURL);
                firsts.Add(response);

                page++;
            }

            // Remove the last response (which is always null)
            firsts.RemoveAt(firsts.Count - 1);
            return firsts;
        }
        static async Task<string> GetUserInfo(string userID)
        {
            string requestURL = "https://api.quavergame.com/v2/user/" + userID;
            return await CallAPI(requestURL);
        }
        static async Task DisplayUserInfo(string userID)
        {
            string userInfo = await GetUserInfo(userID);
            string username = ParseString(userInfo, "username");
            int rank = ParseInt(userInfo, "global");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\nUser Selected: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(username + "\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Rank: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(rank + "\n\n");

            Console.ForegroundColor= ConsoleColor.Gray;
        }
        static string GetUserID()
        {
            Console.WriteLine("Enter User ID: ");
            Console.Write("(e.g. in 'https://quavergame.com/user/1241', '1241' is the user ID for Zoobin4)");
            Console.SetCursorPosition(15, Console.CursorTop - 1);

            string user_id = Console.ReadLine(); // Too lazy to check if the user ID is valid
            Console.SetCursorPosition(0, Console.CursorTop + 1);
            return user_id;
        }
        static List<Score> ParseScores(string json)
        {
            List<Score> scores = new List<Score>();

            // Splits the json into a format of 'score, map, score, map, score... etc'
            // Have to do the splits manually to avoid it splitting incorrectly at songs, artists, charters, etc with the '{' character
            List<string> splitJson = new List<string>();
            int splitIndex = json.IndexOf("{\"id\"");
            while (splitIndex != -1)
            {
                // Add the current split to the list
                string split = json.Substring(0, splitIndex);
                splitJson.Add(split);

                // Remove everything up to the start of the next split and one character of the split identifier
                json = json.Remove(0, splitIndex + 1);

                // Re-find the index of the start of the next split
                // (now that things have been removed the previous split index marks the start of the current split and the new split index marks the end)
                splitIndex = json.IndexOf("{\"id\"");
            }
            // Add the last split to the list
            // Since the index of the next split doesn't exist, the last split doesn't get added automatically
            splitJson.Add(json);


            int index = 1;
            while (index < splitJson.Count - 1)
            {
                // Parse the score and map information individually
                Score score = ParseScore(splitJson[index]);
                Map map = ParseMap(splitJson[index + 1]);

                // Add the associated map info to the score
                score.Map = map;

                scores.Add(score);
                index += 2;
            }

            return scores;
        }
        static Score ParseScore(string json)
        {
            DateTime timestamp = ParseTime(json, "timestamp");
            double performanceRating = ParseDouble(json, "performance_rating");
            double accuracy = ParseDouble(json, "accuracy");
            int maxCombo = ParseInt(json, "max_combo");
            int marvelousCount = ParseInt(json, "count_marvelous");
            int perfectCount = ParseInt(json, "count_perfect");
            int greatCount = ParseInt(json, "count_great");
            int missCount = ParseInt(json, "count_miss");
            string grade = ParseString(json, "grade");
            int scrollSpeed = ParseInt(json, "scroll_speed");

            // Returns score information with empty map data (to be filled later)
            return new Score(timestamp, performanceRating, accuracy, maxCombo, marvelousCount, perfectCount, greatCount, missCount, grade, scrollSpeed, new Map());
        }
        static Map ParseMap(string json)
        {
            string id = ParseString(json, "md5");
            string charter = ParseString(json, "creator_username");
            string artist = ParseString(json, "artist");
            string title = ParseString(json, "title");
            List<string> tags = ParseListString(json, "tags");
            string difficultyName = ParseString(json, "difficulty_name");
            int length = ParseInt(json, "length");
            double bpm = ParseDouble(json, "bpm");
            double difficultyRating = ParseDouble(json, "difficulty_rating");
            int riceCount = ParseInt(json, "count_hitobject_normal");
            int noodleCount = ParseInt(json, "count_hitobject_long");
            double lnPercent = ParseDouble(json, "long_note_percentage");
            int maxCombo = ParseInt(json, "max_combo");
            int playCount = ParseInt(json, "play_attempts");

            return new Map (id, charter, artist, title, tags, difficultyName, length, bpm, difficultyRating, riceCount, noodleCount, lnPercent, maxCombo, playCount);
        }
        static DateTime ParseTime(string json, string target)
        {
            // Find location of target data
            int targetIndex = json.IndexOf("\"" + target + "\":");

            // Remove everything before the target data
            json = json.Substring(targetIndex + target.Length + 4);

            // Remove everything after the end of the target data
            int endIndex = json.IndexOf(",");
            json = json.Substring(0, endIndex - 2);


            // Split into date and time (format is 'date T time' for some reason, dunno why "T" is the splitter)
            string date = json.Split('T')[0];
            string time = json.Split('T')[1];

            // Split the date into parts (format is 'yyyy-mm-dd')
            string[] dateSplit = date.Split('-');
            int year = int.Parse(dateSplit[0]);
            int month = int.Parse(dateSplit[1]);
            int day = int.Parse(dateSplit[2]);

            // Split the time into parts (format is 'hh:mm:ss.ddd')
            string[] timeSplit = time.Split(':');
            int hour = int.Parse(timeSplit[0]);
            int minute = int.Parse(timeSplit[1]);

            // Split into seconds and milliseconds
            string[] secondSplit = timeSplit[2].Split('.');
            int second = int.Parse(secondSplit[0]);
            int millisecond = 0;
            // Sometimes the millisecond value isn't there for some reason, so this avoids a crash in that case
            if (secondSplit.Length != 1)
            {
                millisecond = int.Parse(secondSplit[1]);
            }

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }
        static List<string> ParseListString(string json, string target)
        {
            // Find location of target data
            int targetIndex = json.IndexOf("\"" + target + "\":");

            // Remove everything before the target data
            json = json.Substring(targetIndex + target.Length + 4);

            // Remove everything after the end of the target data
            int endIndex = json.IndexOf("\"");
            json = json.Substring(0, endIndex);


            // Removes all commas (tags in quaver are split by spaces, but many people add commas unnecessarily)
            json = json.Replace(",", "");

            // Split the tags by space to obtain a list
            return new List<string>(json.Split(' '));
        }
        static string ParseString(string json, string target)
        {
            int targetIndex = json.IndexOf("\"" + target + "\":"); // Find location of target data
            json = json.Substring(targetIndex + target.Length + 3); // Remove everything before the target
            int endIndex = json.IndexOf(","); // Find end of target data (won't work if the target is the last line and hence doesn't have a comma at the end)
            string data = json.Substring(0, endIndex + 1).Replace("\"", "").Replace(",", ""); // Remove any quotes and commas
            return data;
        }
        static int ParseInt(string json, string target)
        {
            int targetIndex = json.IndexOf("\"" + target + "\":"); // Find location of target data
            json = json.Substring(targetIndex + target.Length + 3); // Remove everything before the target
            int endIndex = json.IndexOf(","); // Find end of target data (won't work if the target is the last line and hence doesn't have a comma at the end)
            int data = int.Parse(json.Substring(0, endIndex));
            return data;
        }
        static double ParseDouble(string json, string target)
        {
            int targetIndex = json.IndexOf("\"" + target + "\":"); // Find location of target data
            if (targetIndex == -1)
            {

            }
            json = json.Substring(targetIndex + target.Length + 3); // Remove everything before the target
            int endIndex = json.IndexOf(","); // Find end of target data (won't work if the target is the last line and hence doesn't have a comma at the end)
            double data = double.Parse(json.Substring(0, endIndex));
            return data;
        }
        static async Task<string> CallAPI(string request)
        {
            try
            {
                // Get a response from the API
                HttpResponseMessage response = await client.GetAsync(request);

                // Throw an exception if the call was unsuccessful for any reason, otherwise continue
                response.EnsureSuccessStatusCode();

                // Return the content of the response as a string
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                // Check if the error message contains the error code for rate limiting (429)
                if (e.Message.Contains("429"))
                {
                    Console.WriteLine("\nAPI rate limit reached.");
                }
                else
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message: {0} ", e.Message);
                }

                // Wait 30 seconds before retrying the request
                // If the error was a rate limit, then it should take 1 minute to refresh the limits
                int timer = 30;
                while (timer > 0)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write("Waiting " + timer + " seconds before retrying...   ");
                    Thread.Sleep(1000);
                    timer--;
                }
                // Make the timer end on 0 because I think thats more satisfying
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.WriteLine("Waiting 0 seconds before retrying...   ");

                // Re-attempt the request
                return await CallAPI(request);
            }

        }
    }
}
