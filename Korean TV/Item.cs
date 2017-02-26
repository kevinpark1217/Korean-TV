﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TVDBSharp;
using TVDBSharp.Models;

namespace Korean_TV
{
    class Item
    {
        public String title;
        public String episodeTitle;
        public int season;
        public int episode;
        public DateTime time;

        private int naming = -1;
        public String phpsessid;
        public String torrent;

        public Item(string file, string contentsDir)
        {
            if (Regex.IsMatch(file, @"(?i:\.720p-NEXT)")) nextParse(file);
            //else if (Regex.IsMatch(file, @"(?i:.720p-CineBus)")) cinebusParse(file);
            else if (Regex.IsMatch(file, @"(?i:\.((720)|(450))p-Unknown)")) unknownParse(file);
            else if (Regex.IsMatch(file, @"(?i:\.720p-SolKae™)")) solkaeParse(file);
            else return;

            if (title == null)
                return;
            title = Manage.exisitingTitle(contentsDir, title);
        }

        private void solkaeParse(String file)
        {
            file = Regex.Replace(file, @"(?i:\.HDTV\.x264)", "");
            file = Regex.Replace(file, @"(?i:\.720p-SolKae™)", ".720p-NEXT");

            nextParse(file);
        }

        private void unknownParse(String file)
        {
            file = Regex.Replace(file, @"(?i:((\.)?HDTV\.H264))", "");
            file = Regex.Replace(file, @"(?i:\.((720)|(450))p-Unknown)", ".720p-NEXT");

            nextParse(file);
        }

        private void cinebusParse(String file)
        {
            file = file.Substring(0, Regex.Match(file, @"(?i:\.720p-CineBus)").Index);
            file = Regex.Replace(file, @"(?i:\.H264\.AAC)", "");
            Match dateMatch = Regex.Match(file, @"(\d\d\d\d\d\d\.)");
            string date = dateMatch.Value;
            int year = Convert.ToInt32(date.Substring(0, 2)) + 2000;
            int month = Convert.ToInt32(date.Substring(2, 2));
            int day = Convert.ToInt32(date.Substring(4, 2));
            time = new DateTime(year, month, day);
            file = file.Substring(dateMatch.Index + dateMatch.Length);

            int startIndex = file.IndexOf('「');
            int endIndex = file.IndexOf('」');
            if (startIndex >= 0 && endIndex >=0)
            {
                episodeTitle = file.Substring(startIndex + 1, endIndex - startIndex - 1);
                file = file.Substring(0, startIndex) + file.Substring(endIndex + 1);
            }

            title = file.Trim();
            naming = 1;
        }

        private void nextParse(String file)
        {
            file = Regex.Replace(file, @"[\[]+(.*?)+[\]] ", "");
            file = Regex.Replace(file, @"(?i:\.END)", "");
            file = file.Substring(0, Regex.Match(file, @"(?i:\.720p-NEXT)").Index);
            file = file.Trim();
            
            Match dateMatch = Regex.Match(file, @"(\.\d\d\d\d\d\d)");
            string date = dateMatch.Value;
            int year = Convert.ToInt32(date.Substring(1, 2)) + 2000;
            int month = Convert.ToInt32(date.Substring(3, 2));
            int day = Convert.ToInt32(date.Substring(5, 2));
            time = new DateTime(year, month, day);

            title = file.Substring(0, dateMatch.Index);
            if (dateMatch.Index + dateMatch.Length != file.Length)
                episodeTitle = file.Substring(dateMatch.Index + dateMatch.Length + 1).Trim();

            Match episodeMatch = Regex.Match(title, @"(\.E\d\d\d?\d?)");
            if (episodeMatch.Success)
            {
                episode = Convert.ToInt32(episodeMatch.Value.Substring(2));
                title = title.Substring(0, episodeMatch.Index);
            }
            else
                naming = 1;

            Match seasonMatch = Regex.Match(title, @"\s(시즌)?\d?\d(?(1)|$)");
            if (seasonMatch.Success)
            {
                season = Convert.ToInt32(Regex.Match(seasonMatch.Value, @"\d?\d").Value);
                if (String.IsNullOrEmpty(episodeTitle))
                    episodeTitle = title.Substring(seasonMatch.Index + seasonMatch.Length).Trim();
                else
                    episodeTitle += title.Substring(seasonMatch.Index + seasonMatch.Length);
                title = title.Substring(0, seasonMatch.Index);
            }

            Match uselessMatch = Regex.Match(title, @"(\d부(\s|(작 특집 )))|(-\s)");
            if (uselessMatch.Success)
                title = title.Substring(uselessMatch.Index + uselessMatch.Length);

            Match partMatch = Regex.Match(title, @"(\s\d부)$");
            if (partMatch.Success)
            {
                if (String.IsNullOrEmpty(episodeTitle))
                    episodeTitle = partMatch.Value.Trim();
                else
                    episodeTitle += partMatch.Value;
                title = title.Substring(0, partMatch.Index);
            }

            Match special = Regex.Match(title, @"(기획\s)|(특집\s)");
            title = title.Substring(special.Index + special.Length);
            title = title.Replace("특집다큐", "").Trim();
        }

        private bool isMatch()
        {
            TVDB db = new TVDB(getKey());
            List<Show> results = db.Search(title, 1);

            if (results.Count > 0)
            {
                Show show = results.ElementAt(0);
                List<Episode> episodes = show.Episodes;
                foreach (Episode episode in episodes)
                {
                    if (episode.EpisodeNumber == this.episode && episode.FirstAired == time)
                    {
                        season = episode.SeasonNumber;
                        //title = show.Name;
                        //episodeTitle = episode.Title;
                        return true;
                    }
                }
            }
            return false;
        }

        private String getKey()
        {
            List<String> keys = new List<string> { "3B93AA4377EF6427", "CBDB4A364D00EC28", "5600F962B88AF44D", "37C4E8B68F2D3ACB", "8B6F308A1E238266" };

            Random random = new Random();
            int index = random.Next(keys.Count);
            return keys[index];
        }

        public String getName(int naming)
        {
            try { if (isMatch()) naming = 0; }
            catch (System.Xml.XmlException) { naming = 0; }

            if (this.naming != -1)
                naming = this.naming;

            switch (naming)
            {
                case 0:
                    if (String.IsNullOrEmpty(episodeTitle))
                        return title + " - " + "S" + season.ToString("D2") + "E" + episode.ToString("D2");
                    return title + " - " + "S" + season.ToString("D2") + "E" + episode.ToString("D2") + " - " + episodeTitle;
                case 1:
                    string date = time.Year + "." + time.Month.ToString("D2") + "." + time.Day.ToString("D2");
                    if (String.IsNullOrEmpty(episodeTitle))
                        return title + " - " + date;
                    return title + " - " + date + " - " + episodeTitle;
                default:
                    return null;
            }
        }

        public override bool Equals(object obj)
        {
            Item show = (Item)obj;
            if(title.Equals(show.title) && DateTime.Equals(time, show.time) )
                if ( (episode != 0 && episode == show.episode && season == show.season) || (episodeTitle == null && show.episodeTitle == null) || (episodeTitle.Equals(show.episodeTitle)) )
                    return true;
            return false;
        }

    }
}
