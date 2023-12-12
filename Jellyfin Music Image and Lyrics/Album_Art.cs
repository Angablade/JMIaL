using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
namespace JMIAL
{
    public class AlbumArt
    {
        public string GetAlbumArtUrl(string keyword)
        {
            try
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "networklog.xcfr"));
            }
            catch (Exception)
            {
            }

            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile($"https://www.google.com/search?q={keyword.Replace(" ", "+")}&tbm=isch", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "networklog.xcfr"));
                }

                string htmlContent = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "networklog.xcfr"));

                if (!string.IsNullOrEmpty(htmlContent) && htmlContent.Contains("<img"))
                {
                    htmlContent = htmlContent.Substring(htmlContent.IndexOf("<img") + 5);

                    htmlContent = htmlContent.Substring(htmlContent.IndexOf("<img"));
                    htmlContent = htmlContent.Substring(htmlContent.IndexOf("src=") + 5);
                    htmlContent = htmlContent.Substring(0, htmlContent.IndexOf("\""));

                    return htmlContent;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error: {ex.Message}");
                return null;
            }
            finally
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "networklog.xcfr"));
            }
        }
    }

}
