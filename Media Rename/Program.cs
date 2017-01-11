using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Media_Rename
{
    class Program
    {
        static void Main(string[] args)
        {
            fileFinder(@"T:\Korean TV\News\Contents");
            fileFinder(@"T:\Korean TV\Drama\Contents");
            fileFinder(@"T:\Korean TV\Variety\Contents");
        }

        static void fileFinder(string path)
        {
            string[] folders = Directory.GetDirectories(path);
            foreach(string folder in folders)
                fileFinder(folder);

            string[] files = Directory.GetFiles(path);
            foreach (string file in files)
                fileRemove(file);
        }

        static void fileRemove(string file)
        {
            DateTime creation = File.GetCreationTime(file);
            DateTime lastWrite = File.GetLastWriteTime(file);
            if ((DateTime.Now - creation).Days < 2 || (DateTime.Now - lastWrite).Days < 2)
                File.Delete(file);
        }

        static void fileRename(string file)
        {
            string name = Path.GetFileName(file);

            //Change time divider
            /*Match modifier = Regex.Match(name, @"\d\d\d\d.\d\d?.\d\d?");
            if (!modifier.Success)
                return;
            int startDash = modifier.Value.IndexOf('.');
            int endDash = modifier.Value.LastIndexOf('.');
            int year = Convert.ToInt32(modifier.Value.Substring(0, 4));
            int month = Convert.ToInt32(modifier.Value.Substring(startDash + 1, endDash - startDash - 1));
            int day = Convert.ToInt32(modifier.Value.Substring(endDash + 1));
            name = name.Substring(0, modifier.Index) + year + '.' + month.ToString("D2") + '.' + day.ToString("D2") + name.Substring(modifier.Index + modifier.Length);*/

            //Add season and episode padding
            /*Match modifier = Regex.Match(name, @"S\d\d?E\d\d?");
            if (!modifier.Success)
                return;
            Match seasonMatch = Regex.Match(modifier.Value, @"S\d\d?");
            Match episodeMatch = Regex.Match(modifier.Value, @"E\d\d?");
            int season = Convert.ToInt32(Regex.Match(seasonMatch.Value, @"\d\d?").Value);
            int episode = Convert.ToInt32(Regex.Match(episodeMatch.Value, @"\d\d?").Value);
            name = name.Substring(0, modifier.Index) + "S" + season.ToString("D2") + "E" + episode.ToString("D2") + name.Substring(modifier.Index + modifier.Length);*/

            //Fix Date Created and Modified
            /*Match modifier = Regex.Match(name, @"\d\d\d\d.\d\d?.\d\d?");
            if (!modifier.Success)
                return;
            int startDash = modifier.Value.IndexOf('.');
            int endDash = modifier.Value.LastIndexOf('.');
            int year = Convert.ToInt32(modifier.Value.Substring(0, 4));
            int month = Convert.ToInt32(modifier.Value.Substring(startDash + 1, endDash - startDash - 1));
            int day = Convert.ToInt32(modifier.Value.Substring(endDash + 1));
            DateTime time = new DateTime(year, month, day);
            File.SetCreationTime(file, time);
            File.SetLastWriteTime(file, time);*/

            File.Move(file, Path.Combine(Path.GetDirectoryName(file), name));
        }
    }
}
