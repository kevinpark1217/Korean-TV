using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Korean_TV
{

    enum FolderType: int
    {
        Root=0, Default, Active, Complete, Contents
    }

    class Manage
    {
        const string path = @"T:\Korean TV";
        const string active = @"Active";
        const string complete = @"Complete";
        const string contents = @"Contents";
        public const int maxDays = 23;
        public const int maxHours = 6;

        public static String getPath(string type, FolderType status)
        {
            string dir = Path.Combine(path, type);
            switch ((int)status)
            {
                case 0:
                    return path;
                case 1:
                    return dir;
                case 2:
                    return Path.Combine(dir, active);
                case 3:
                    return Path.Combine(dir, complete);
                case 4:
                    return Path.Combine(dir, contents);
                default:
                    return null;
            }
        }

        public static void scrap(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            foreach(string file in files)
            {
                DateTime modify = File.GetLastWriteTime(file);
                if ((DateTime.Now - modify).Hours > maxHours)
                    File.Delete(file);
            }
        }

        public static void remove(string dir)
        {
            string[] folders = Directory.GetDirectories(dir);
            foreach(string folder in folders)
                remove(folder);

            string[] files = Directory.GetFiles(dir);
            foreach (string file in files)
            {
                DateTime creation = File.GetCreationTime(file);
                DateTime lastWrite = File.GetLastWriteTime(file);
                if ((DateTime.Now - creation).Days > maxDays || (DateTime.Now - lastWrite).Days > maxDays)
                    File.Delete(file);
            }

            if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                Directory.Delete(dir);
        }

        // Move any new completed download
        public static void move(string completeDir, string contentsDir, int naming)
        {
            if (!Directory.Exists(contentsDir))
                Directory.CreateDirectory(contentsDir);

            string[] downloaded = Directory.GetFiles(completeDir);
            foreach (string download in downloaded)
            {
                string title = Path.GetFileName(download);
                Item show = new Item(title, contentsDir);

                File.SetCreationTime(download, show.time);
                File.SetLastWriteTime(download, show.time);

                string folder = Path.Combine(contentsDir, show.title);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                /*if(show.season > 0)
                {
                    folder = Path.Combine(folder, "Season " + show.season);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                }*/

                string destination = Path.Combine(folder, show.getName(naming) + Path.GetExtension(title));
                if (File.Exists(destination))
                    File.Delete(destination);
                File.Move(download, destination);
            }
        }
        
        public static bool check(Item show, string contentsDir, int naming)
        {
            string[] folders = Directory.GetDirectories(contentsDir);
            foreach (string folder in folders)
            {
                //if (check(show, null, folder, naming))
                //    return true;
                string[] files = Directory.GetFiles(folder);
                foreach (string path in files)
                {
                    string file = Path.GetFileNameWithoutExtension(path);

                    if (show.getName(naming).Equals(file))
                        return true;
                }
            }

            return false;
        }

        public static String exisitingTitle(string contentsDir, string title)
        {
            string[] folders = Directory.GetDirectories(contentsDir);
            foreach (string folder in folders)
            {
                string folderName = new DirectoryInfo(folder).Name;
                if (title.Contains(folderName))
                    return folderName;
            }
            return title;
        }

    }
}
