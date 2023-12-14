using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.ComponentModel.Design;

namespace JMIAL
{
    public class LyricsHelper
    {
        public enum Returner
        {
            Html = 0,
            Text = 1
        }

        public string StripHtml(string html)
        {
            try
            {
                html = Regex.Replace(html, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", string.Empty, RegexOptions.IgnoreCase);
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

                return WebUtility.HtmlDecode(html.Trim());
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
    public interface ILyricsProvider
    {
        string GetLyrics(string song, string artist, LyricsHelper.Returner style);
        string GetProviderName() {
            return "Null";
        }
    }
    public class AZLyrics : ILyricsProvider
    {
        LyricsHelper LyricsHelper = new LyricsHelper();
        public string GetLyrics(string song, string artist, LyricsHelper.Returner style)
        {
            string address = "http://www.azlyrics.com/lyrics/" + Regex.Replace(artist.Replace(" ", ""), @"[^\w\.@-]", "") + "/" + Regex.Replace(song.Replace(" ", ""), @"[^\w\.@-]", "") + ".html";
            address = address.ToLower();

            if (style == LyricsHelper.Returner.Html)
            {
                string contents = DownloadCode(address);
                return contents;
            }
            else
            {
                string contents = DownloadCode(address);
                return LyricsHelper.StripHtml(contents);
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

        public string GetProviderName() {
            return "AZLyrics";
        }
    }
    public class GeniusLyrics : ILyricsProvider
    {
        LyricsHelper LyricsHelper = new LyricsHelper();
        public string GetLyrics(string song, string artist, LyricsHelper.Returner style)
        {
            string address = "https://genius.com/" + artist.Trim().Replace(" ", "-").ToLower().Replace("$", "") + "-" + song.Trim().Replace(" ", "-").ToLower().Replace("$", "") + "-lyrics";
            address = address.ToLower();

            if (style == LyricsHelper.Returner.Html)
            {
                return DownloadCode(address);
            }
            else if (style == LyricsHelper.Returner.Text)
            {
                return LyricsHelper.StripHtml(DownloadCode(address));
            }
            else
            {
                return "There was an error retreiving lyrics from Genius.";
            }

        }
        private string DownloadCode(string address)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string html = client.DownloadString(address);
                    html = html.Substring(html.IndexOf("class=\"Lyrics__Container"));
                    html = html.Substring(html.IndexOf(">") + 1);
                    html = html.Substring(0, html.IndexOf("</span></div>"));

                    return html;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string GetProviderName()
        {
            return "Genius";
        }
    }
    public class MusixMatch : ILyricsProvider
    {
        LyricsHelper LyricsHelper = new LyricsHelper();
        public string GetLyrics(string song, string artist, LyricsHelper.Returner style)
        {
            string address = "https://www.musixmatch.com/lyrics/" + artist.Trim().Replace(" ", "-").ToUpper().Replace("$", "") + "/" + song.Trim().Replace(" ", "-").Replace("$", "");
            address = address.ToLower();

            if (style == LyricsHelper.Returner.Html)
            {
                return DownloadCode(address);
            }
            else if (style == LyricsHelper.Returner.Text)
            {
                return LyricsHelper.StripHtml(DownloadCode(address));
            }
            else
            {
                return "There was an error retreiving lyrics from Musixmatch.";
            }
        }
        private string DownloadCode(string address)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string html = client.DownloadString(address);
                    html = html.Substring(html.IndexOf("<span><p class=\"mxm-lyrics__content "));
                    html = html.Substring(html.IndexOf(">") + 1);
                    html = html.Substring(0, html.IndexOf("</span></p></div>") - 6);

                    return html;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string GetProviderName()
        {
            return "MusixMatch";
        }
    }
    public class LyricsCom : ILyricsProvider
    {
        LyricsHelper LyricsHelper = new LyricsHelper();
        public string GetLyrics(string song, string artist, LyricsHelper.Returner style)
        {
            string address = "https://www.lyrics.com/artist/" + artist.Trim().Replace(" ", "-").ToUpper().Replace("$", "");
            address = address.ToLower();

            if (style == LyricsHelper.Returner.Html)
            {
                return DownloadCode(address, song);
            }
            else if (style == LyricsHelper.Returner.Text)
            {
                return LyricsHelper.StripHtml(DownloadCode(address, song));
            }
            else
            {
                return "There was an error retreiving lyrics from lyrics.com.";
            }
        }
        private string DownloadCode(string address, string query)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string html = client.DownloadString(address);
                    html = html.Substring(html.IndexOf("<div class=\"tdata-ext\">"));
                    html = html.Substring(html.IndexOf(">") + 1);
                    html = html.Substring(0, html.IndexOf("<div class=\"callout comments-area\">"));
                    address = "https://www.lyrics.com/lyric/" + FindMostMatchingUrl(html, query.Replace(" ","+"));
                    html = client.DownloadString(address);
                    html = html.Substring(html.IndexOf("<pre id=\"lyric-body-text\""));
                    html = html.Substring(html.IndexOf(">") + 1);
                    html = html.Substring(0, html.IndexOf("</pre>"));
                    return html;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        static string FindMostMatchingUrl(string html, string userInput)
        {
            var urlMatches = Regex.Matches(html, @"<a\s+href=""/lyric/(.*?)""");
            var urls = urlMatches.Cast<Match>().Select(match => match.Groups[1].Value).ToList();
            var mostMatchingUrl = GetMostMatchingUrl(urls, userInput);

            return mostMatchingUrl;
        }
        static string GetMostMatchingUrl(List<string> urls, string userInput)
        {
            var mostMatchingUrl = urls.OrderByDescending(url => GetSimilarity(url, userInput)).FirstOrDefault();
            return mostMatchingUrl;
        }
        static double GetSimilarity(string str1, string str2)
        {
            int commonLength = str1.Zip(str2, (c1, c2) => c1 == c2).Count(common => common);
            double similarity = (2.0 * commonLength) / (str1.Length + str2.Length);
            return similarity;
        }
        public string GetProviderName()
        {
            return "Lyrics.com";
        }
    }
}