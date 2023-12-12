
using JMIAL;
using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;

class Program
{

    static void Main(string[] args)
    {
        string directory = null;
        bool includeArtist = false;
        bool includeLyrics = false;
        bool overwriteall = false;
        bool verbose = false;
        string query = "album art";
        string aquery = "artist";
        int timeoutval = 0;
        bool showHelp = false;
        bool silent = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-?":  //Help menu listing commands.
                case "-h":
                case "-help":
                    showHelp = true;
                    break;

                case "-d":  //Lidarr based media library location on disk that is specified.
                case "-dir":
                    if (i + 1 < args.Length)
                    {
                        directory = args[i + 1];
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing directory path after -d or -dir flag.");
                        return;
                    }
                    break;

                case "-a":  //The flag to set to include artist photo downloads when scanning library.
                case "-artist":
                    includeArtist = true;
                    break;

                case "-l":  //The flag to set when wanting to download lyrics for the music in library.
                case "-lyrics":
                    includeLyrics = true;
                    break;

                case "-o":  //The flag to set to overight all files instead of skipping existing files.
                case "-overwrite":
                    overwriteall = true;
                    break;

                case "-v":  //The flag to set to enter verbose logging mode.
                case "-verbose":
                    verbose = true;
                    break;

                case "-q":  //The search query that will be included with the artist name and album name when searching for album art.
                case "-query":
                    if (i + 1 < args.Length)
                    {
                        query = args[i + 1];
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing search query after -q or -query flag.");
                        return;
                    }
                    break;

                case "-p": //The search query that will be included with the artist name when searching for artists photo.
                case "-pquery":
                    if (i + 1 < args.Length)
                    {
                        aquery = args[i + 1];
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing search aquery after -aq or -aquery flag.");
                        return;
                    }
                    break;

                case "-t":  //The timeout intervel that is set between each image downloaded.
                case "-timeout":
                    if (i + 1 < args.Length)
                    {
                        if (int.TryParse(args[i + 1], out int result))
                        { timeoutval = result; }
                        i++;
                    }
                    else
                    {
                        Console.WriteLine("Error: Missing number value after -t or -timeout flag.");
                        return;
                    }
                    break;

                case "-s":  //The flag to set to overight all files instead of skipping existing files.
                case "-silent":
                    silent = true;
                    break;

                default:
                    Console.WriteLine($"Error: Unknown command-line argument {args[i]}");
                    return;
            }
        }
        if (silent != true) {
            Console.WriteLine("************************************************"); 
            Console.WriteLine("*     .---.                                    *"); 
            Console.WriteLine("*     |   |                              .---. *"); 
            Console.WriteLine("*     '---' __  __   ___   .--.          |   | *"); 
            Console.WriteLine("*     .---.|  |/  `.'   `. |__|          |   | *"); 
            Console.WriteLine("*     |   ||   .-.  .-.   '.--.          |   | *"); 
            Console.WriteLine("*     |   ||  |  |  |  |  ||  |   __     |   | *"); 
            Console.WriteLine("*     |   ||  |  |  |  |  ||  | .:--.'.  |   | *"); 
            Console.WriteLine("*     |   ||  |  |  |  |  ||  |/ |   \\|  |   | *"); 
            Console.WriteLine("*     |   ||  |  |  |  |  ||  |`\" __ ||  |   | *"); 
            Console.WriteLine("*     |   ||__|  |__|  |__||__| .'.''| | |   | *"); 
            Console.WriteLine("*  __.'   '                    / /   | |_`---' *"); 
            Console.WriteLine("* |      '                     \\ \\._,\\'/       *"); 
            Console.WriteLine("* |____.'                       `--'  `\"`      *"); 
            Console.WriteLine("************************************************"); 
            Console.WriteLine("*   The Jellyfin Music Images and lyrics tool  *"); 
            Console.WriteLine("************************************************"); 
            Console.WriteLine("|");
            if(showHelp == true)
            {
                DisplayHelp(silent);
                return;
            }
            
        
            Console.WriteLine($"|       Directory : {directory ?? "Not specified"}"); 
            Console.WriteLine($"| Include Artists : {includeArtist}"); 
            Console.WriteLine($"|  Include Lyrics : {includeLyrics}"); 
            Console.WriteLine($"| Overwrite files : {overwriteall}"); 
            if (verbose == true)
            {
                Console.WriteLine($"|    Verbose mode : {verbose}"); 
                Console.WriteLine($"|  Album subquery : {query}"); 
                Console.WriteLine($"| Artist subquery : {aquery}"); 
                Console.WriteLine($"|  Timeout length : {timeoutval}"); 
            }
            Console.WriteLine("|");
        };

        if (string.IsNullOrEmpty(directory))
        {
            if (silent != true) { Console.WriteLine("| Error: Directory not specified. Exiting program."); };
            Environment.Exit(1);
        }

        ScanAndExecute(directory, includeLyrics, includeArtist, overwriteall, verbose, query, aquery, timeoutval, silent);
    }

    static void ScanAndExecute(string currentPath, Boolean includelyrics, Boolean includeartists, Boolean overwrite, bool verbose, string query, string aquery, int tout, bool silent)
    {
        AlbumArt art = new AlbumArt();
        Lyrics Lyr = new Lyrics();

        string[] Folders = Directory.GetDirectories(currentPath);
        foreach (var Folder in Folders)
        {
            if (silent != true) { Console.Write("|->" + Folder); };
            string[] subFolders = Directory.GetDirectories(Folder);

            if (includeartists)
            {
                try
                {
                    string folderPath = Path.Combine(Folder, "folder.jpg");

                    if (!overwrite && !File.Exists(folderPath))
                    {
                        if (verbose == true) { if (silent != true) { Console.Write(" {Search:" + Folder.Substring(currentPath.Length + 1) + "+" + aquery + "} "); }; };
                        DownloadFileAsync(art.GetAlbumArtUrl(Folder.Substring(currentPath.Length + 1) + "+" + aquery), folderPath, tout).Wait();
                    }
                    else if (overwrite)
                    {
                        if (verbose == true) { if (silent != true) { Console.Write(" {Search:" + Folder.Substring(currentPath.Length + 1) + "+" + aquery + "} "); }; };
                        DownloadFileAsync(art.GetAlbumArtUrl(Folder.Substring(currentPath.Length + 1) + "+" + aquery), folderPath, tout).Wait();
                    }

                    if (silent != true) { Console.WriteLine(" [OK]"); };
                }
                catch (Exception)
                {
                    if (silent != true) { Console.WriteLine(" [X]"); };
                }
            }
            else
            {
                if (silent != true)
                {
                    Console.WriteLine("");
                }


                foreach (var subFolder in subFolders)
                {
                    try
                    {
                        string filePath = Path.Combine(subFolder, "cover.jpg");
                        if (silent != true) { Console.Write("|   |->" + subFolder.Substring(Folder.Length + 1)); };
                        if (!overwrite && !File.Exists(filePath))
                        {
                            if (verbose == true) { if (silent != true) { Console.Write(" {Search:" + subFolder.Substring(currentPath.Length + 1).Replace("\\", "+").Replace("/", "+") + "+" + query + "} "); }; };
                            DownloadFileAsync(art.GetAlbumArtUrl(subFolder.Substring(currentPath.Length + 1).Replace("\\", "+").Replace("/", "+") + "+" + query), Path.Combine(subFolder, "cover.jpg"), tout).Wait();
                        }
                        else if (overwrite)
                        {
                            if (verbose == true) { if (silent != true) { Console.Write(" {Search:" + subFolder.Substring(currentPath.Length + 1).Replace("\\", "+").Replace("/", "+") + "+" + query + "} "); }; };
                            DownloadFileAsync(art.GetAlbumArtUrl(subFolder.Substring(currentPath.Length + 1).Replace("\\", "+").Replace("/", "+") + "+" + query), Path.Combine(subFolder, "cover.jpg"), tout).Wait();
                        }
                        if (silent != true) { Console.WriteLine(" [OK]"); };
                    }
                    catch (Exception)
                    {
                        if (silent != true) { Console.WriteLine(" [X]"); };
                    }

                    if (includelyrics == true)
                    {
                        string[] filelist = Directory.GetFiles(subFolder);
                        foreach (var file in filelist)
                        {
                            if (includelyrics == true)
                            {
                                if (HasAudioExtension(file))
                                {
                                    try
                                    {
                                        if (silent != true)
                                        {
                                            Console.Write("|   |   |->" + file.Substring(subFolder.Length + 1));
                                            AudioTagReader.AudioTags ptags = new AudioTagReader.AudioTags();
                                            ptags = AudioTagReader.ReadTags(file);
                                            if (ptags.Artist != null)
                                            {
                                                if (verbose == true)
                                                {
                                                    if (silent != true) { Console.Write(" {Search:" + ptags.Artist + " - " + ptags.Title + "} "); };
                                                    using (StreamWriter writer = new StreamWriter(Path.ChangeExtension(file, ".lyrs")))
                                                    {
                                                        writer.Write(Lyr.GetLyrics(ptags.Title, ptags.Artist, Lyrics.Returner.Text, false));
                                                    }
                                                    if (silent != true)
                                                    {
                                                        Console.WriteLine(" [OK]");
                                                    };
                                                }
                                            }
                                            else
                                            {
                                                if (verbose == true)
                                                {
                                                    if (silent != true)
                                                    {
                                                        Console.Write(" {File contains no tags!} ");
                                                    };
                                                };

                                                if (silent != true)
                                                {
                                                    Console.WriteLine(" [X]");
                                                };
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        if (silent != true)
                                        {
                                            Console.WriteLine(" [X]");
                                        };
                                    }
                                }
                            }
                        }

                        if (silent != true)
                        {
                            Console.WriteLine("|   |");
                        };
                    }
                }
                if (silent != true) { Console.WriteLine("|"); };
            }
        } 
    }

    static bool HasAudioExtension(string filePath)
    {
        string[] audioExtensions = { ".mp3", ".flac", ".aac", ".ogg", ".wma", ".aiff", ".m4a", ".opus" };
        string fileExtension = Path.GetExtension(filePath);
        return Array.Exists(audioExtensions, ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
    }

    static ImageCodecInfo GetEncoderInfo(ImageFormat format)
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
        return codecs.FirstOrDefault(codec => codec.FormatID == format.Guid);
    }

    static async Task DownloadFileAsync(string fileUrl, string savePath, int timeoutval)
    {
        _ = Task.Delay(timeoutval);
        using (HttpClient client = new HttpClient())
        {
            using (HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var fileStream = System.IO.File.Create(savePath))
                {
                    await stream.CopyToAsync(fileStream);
                    fileStream.Close();
                }
            }
        }
        Task.Delay(timeoutval);
    }

    static void DisplayHelp(bool silent)
    {
        if (silent != true)
        {
            Console.WriteLine("|-> -?, -h, -help   Display this help message"); };
            Console.WriteLine("|-> -d, -dir        Lidarr based media library location on disk that is specified."); 
            Console.WriteLine("|-> -a, -artist     Include artist photo downloads when scanning the library."); 
            Console.WriteLine("|-> -l, -lyrics     Download lyrics for the music in the library.");
            Console.WriteLine("|-> -o, -overwrite  Overwrite all files instead of skipping existing files.");
            Console.WriteLine("|-> -v, -verbose    Enter verbose logging mode.");
            Console.WriteLine("|-> -q, -query      Search query for album art (default: 'album art').");
            Console.WriteLine("|-> -p, -pquery     Search query for artist photo (default: 'artist').");
            Console.WriteLine("|-> -t, -timeout    Timeout interval between each image download.");
        }
    }

