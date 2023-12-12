using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace JMIAL
{
    public class Lyrics
    {
          public string GetLyrics(string song, string artist, Returner style, bool readCache)
        {
            string address = "http://www.azlyrics.com/lyrics/" + Regex.Replace(artist.Replace(" ", ""), @"[^\w\.@-]", "") + "/" + Regex.Replace(song.Replace(" ", ""), @"[^\w\.@-]", "") + ".html";
            address = address.ToLower();

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "lyrics")))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "lyrics"));
            }

            if (style == Returner.Html)
            {
                if (readCache)
                {
                    if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "lyrics", $"{artist} - {song}.html")))
                    {
                        return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "lyrics", $"{artist} - {song}.html"));
                    }
                }

                string contents = DownloadCode(address);
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "lyrics", $"{artist} - {song}.html"), contents);
                return contents;
            }
            else
            {
                if (readCache)
                {
                    if (File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "lyrics", $"{artist} - {song}.txt")))
                    {
                        return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "lyrics", $"{artist} - {song}.txt"));
                    }
                }

                string contents = DownloadCode(address);
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "lyrics", $"{artist} - {song}.txt"), StripHtml(contents));
                return StripHtml(contents);
            }
        }

        private string DownloadCode(string address)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string html = client.DownloadString(address);
                    html = html.Substring(html.IndexOf("<!-- Usage of azlyrics.com content by any third-party lyrics provider is prohibited by our licensing agreement. Sorry about that. -->"));
                    html = html.Substring(html.IndexOf(">") + 2);
                    html = html.Substring(0, html.IndexOf("<!-- MxM banner -->"));

                    return html;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string StripHtml(string html)
        {
            try
            {
                html = Regex.Replace(html, "<.*?>", " ");
                html = html.Replace("<br>", Environment.NewLine + " ");
                html = html.Replace("<br />", Environment.NewLine + " ");

                while (html.Contains(Environment.NewLine + Environment.NewLine + Environment.NewLine))
                {
                    html = html.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                }

                while (html.StartsWith(Environment.NewLine))
                {
                    html = html.Substring(2);
                }

                html = html.Replace("\"", Environment.NewLine + "\"");
                html = html.Replace(Environment.NewLine + "\" + Environment.NewLine", "\" + Environment.NewLine");

                return html.Trim();
            }
            catch (Exception ex)
            {
                return "Error parsing information!" + Environment.NewLine + ex.InnerException.ToString();
            }
        }

        public enum Returner
        {
            Html = 0,
            Text = 1
        }
    }
}
