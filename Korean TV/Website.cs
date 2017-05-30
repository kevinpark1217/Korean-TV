﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Korean_TV
{
    class Website
    {
        private static readonly Uri address = new Uri("https://twzoa.info/");
        private string type;
        private string listAddress;
        private int naming;
        private String phpsessid;
        private List<Item> list;

        public Website(string type, string listAddress, string phpsessid, int naming)
        {
            this.type = type;
            this.listAddress = listAddress;
            this.naming = naming;
            this.phpsessid = phpsessid;
            list = new List<Item>();
        }

        public static String login(string username, string password)
        {
            string postData = "a=login&id=" + username + "&pw=" + password;
            byte[] postArray = Encoding.UTF8.GetBytes(postData);

            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(address);
            WebReq.Method = "POST";
            WebReq.ContentType = "application/x-www-form-urlencoded";
            WebReq.ContentLength = postArray.Length;
            WebReq.CookieContainer = new CookieContainer();

            Stream PostData = WebReq.GetRequestStream();
            PostData.Write(postArray, 0, postArray.Length);
            PostData.Close();

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream ResponseStream = WebResp.GetResponseStream();
            StreamReader Stream = new StreamReader(ResponseStream, Encoding.UTF8);

            String cookie = WebReq.CookieContainer.GetCookieHeader(address);
            WebResp.Close();

            int separate = cookie.IndexOf('=');
            return cookie.Substring(separate + 1);
        }

        public static void logout(String phpsessid)
        {
            website("https://twzoa.info/?r=home&a=logout", phpsessid);
        }

        private bool commentExist(HtmlDocument html)
        {
            HtmlNode frame = html.GetElementbyId("bbsview").SelectSingleNode(".//iframe[@name='commentFrame']");
            String link = frame.GetAttributeValue("src", null);

            int page = 1;
            while(true)
            {
                Uri fullAddress = new Uri(address, WebUtility.HtmlDecode(link) + "&p=" + page);
                String data = website(fullAddress.ToString(), null);
                if (data == null) return false;

                html = new HtmlDocument();
                html.LoadHtml(data);
                HtmlNodeCollection comments = html.GetElementbyId("comment_box").SelectNodes("./div[@class='comment_list']");

                if (comments == null)
                    break;
                foreach (HtmlNode comment in comments)
                {
                    String user = comment.SelectSingleNode("./div[@class='info_box']/span[@class='name']/strong").InnerText.Trim();
                    if (user.Equals("Itanimulli"))
                        return true;
                }
                page++;
            }
            return false;
        }

        private void comment(String link, String id)
        {
            String text = "감사합니다~";
            Random rnd = new Random();
            int num = rnd.Next(3);
            for (int i = 0; i < num; i++)
                text += '~';
            
            String m = link.Substring(link.IndexOf("m=") + 2, link.IndexOf("&") - link.IndexOf("m=") - 2);
            link = link.Substring(link.IndexOf('&') + 1);
            String bid = link.Substring(link.IndexOf("bid=") + 4, link.IndexOf("&") - link.IndexOf("bid=") - 4);
            link = link.Substring(link.LastIndexOf('&') + 1);
            String uid = link.Substring(link.IndexOf("uid=") + 4);
            String metadata = "[" + m + "][" + uid + "][uid,comment,oneline,d_comment][rb_bbs_data][" + id + "][m:" + m + ",bid:" + bid + ",uid:" + uid + "]";

            string postData = "a=write&content=" + text + "&cync=" + metadata + "&m=comment";
            byte[] postArray = Encoding.UTF8.GetBytes(postData);
            
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(address);
            WebReq.Method = "POST";
            WebReq.ContentType = "application/x-www-form-urlencoded";
            WebReq.ContentLength = postArray.Length;
            WebReq.CookieContainer = new CookieContainer();
            WebReq.CookieContainer.Add(new Cookie("PHPSESSID", phpsessid, "/", "twzoa.info"));

            Stream PostData = WebReq.GetRequestStream();
            PostData.Write(postArray, 0, postArray.Length);
            PostData.Close();

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream ResponseStream = WebResp.GetResponseStream();
            StreamReader Stream = new StreamReader(ResponseStream, Encoding.UTF8);
            WebResp.Close();
        }

        public static void checkIn(String phpsessid)
        {
            string postData = "a=atdck&atd_text=비빕! 보봅! 안녕하세요~!&c=119&m=attend1";
            byte[] postArray = Encoding.UTF8.GetBytes(postData);
            
            HttpWebRequest WebReq = (HttpWebRequest)WebRequest.Create(address);
            WebReq.Method = "POST";
            WebReq.ContentType = "application/x-www-form-urlencoded";
            WebReq.ContentLength = postArray.Length;
            WebReq.CookieContainer = new CookieContainer();
            WebReq.CookieContainer.Add(new Cookie("PHPSESSID", phpsessid, "/", "twzoa.info"));

            Stream PostData = WebReq.GetRequestStream();
            PostData.Write(postArray, 0, postArray.Length);
            PostData.Close();

            HttpWebResponse WebResp = (HttpWebResponse)WebReq.GetResponse();
            Stream ResponseStream = WebResp.GetResponseStream();
            StreamReader Stream = new StreamReader(ResponseStream, Encoding.UTF8);

            WebResp.Close();
        }

        public bool retrieve(int minPage, int maxPage)
        {
            for (int page = minPage; page <= maxPage; page++)
            {
                Uri uri = new Uri(address, listAddress + page);
                String data = website(uri.ToString(), null);
                if (data != null)
                    parse(data);
            }

            if (list.Count == 0)
                return false;
            return true;
        }

        private static String website(String urlAddress, String phpsessid)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            if(phpsessid != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(new Cookie("PHPSESSID", phpsessid, "/", "twzoa.info"));
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);

                String data = readStream.ReadToEnd();
                response.Close();
                readStream.Close();
                return data;
            }
            return null;
        }

        private void parse(String data)
        {
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(data);

            HtmlNodeCollection contents = html.GetElementbyId("bbslist").SelectNodes(".//tbody/tr");
            foreach (HtmlNode element in contents)
            {
                HtmlNode idNode = element.SelectSingleNode("./td[@class='now']");
                if (idNode == null) continue;
                HtmlNode title = element.SelectSingleNode(".//div[@class='ellipsis']/a");
                String uri = new Uri(address, WebUtility.HtmlDecode(title.GetAttributeValue("href", null))).ToString();
                details(uri, idNode.InnerText.Trim());
            }
        }

        /*
        private string phpsessid()
        {
            var random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToLower();
            return new string(Enumerable.Repeat(chars, 26).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        */

        private void details(string link, string id)
        {
            String data = website(link, null);

            if (data == null) return;

            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(data);
            
            website(link + "&a=score&value=good", phpsessid); //Like
            if(!commentExist(html))
                comment(link, id); //Comment

            HtmlNode torrentLink = html.GetElementbyId("vContent").SelectSingleNode("./div[@class='attach']/ul/a");
            if (torrentLink == null)
                return;
            Uri uri = new Uri(new Uri(link), WebUtility.HtmlDecode(torrentLink.GetAttributeValue("href", null)));
            HtmlNode torrentName = html.GetElementbyId("bbsview").SelectSingleNode("./div[@class='viewbox']/div[@class='subject']/li[@class='title']");
            String name = WebUtility.HtmlDecode(torrentName.InnerText).Trim();

            HtmlNodeCollection contents = html.GetElementbyId("vContent").SelectNodes(".//div[@class='attach_list']/ul/li");
            if (contents != null && contents.Count > 1)
                return;

            Item show = new Item(name, Manage.getPath(type, FolderType.Contents));
            if (show.title == null || (DateTime.Now - show.time).Days > Manage.maxDays)
                return;

            //Console.WriteLine(show.title);
            show.phpsessid = phpsessid;
            show.torrent = uri.ToString();
            list.Add(show);
        }

        public void download()
        {
            for (int i = 0; i < list.Count; i++)
            {
                Item show = list.ElementAt(i);
                int j;
                for (j = 0; j < i; j++)
                    if (list.ElementAt(j).Equals(show))
                        break;
                if (i != j)
                    continue;

                if (!Manage.check(show, Manage.getPath(type, FolderType.Contents), naming))
                    torrent(show);
            }
        }

        private void torrent(Item show)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(show.torrent);
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.Add(new Cookie("PHPSESSID", show.phpsessid, "/", "twzoa.info"));
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            Stream sr = resp.GetResponseStream();
            
            string file = show.getName(naming) + ".torrent";
            string torrent = Path.Combine(Manage.getPath(type, FolderType.Root), file);
            FileStream fstream = File.Create(torrent);
            sr.CopyTo(fstream);
            sr.Close();
            fstream.Close();

            string destination = Path.Combine(Manage.getPath(type, FolderType.Default), file);
            if (File.Exists(destination))
                File.Delete(destination);
            File.Move(torrent, destination);
        }

    }
}
