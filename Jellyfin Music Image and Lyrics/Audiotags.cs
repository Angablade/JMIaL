using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace JMIAL
{
    public class AudioTagReader
    {
        public class AudioTags
        {
            public string? Artist { get; set; }
            public string? Title { get; set; }
        }

        public static AudioTags ReadTags(string filePath)
        {
            string extension = GetFileExtension(filePath);

            switch (extension.ToLower())
            {
                case ".mp3":
                    return ReadMp3Tags(filePath);
                case ".flac":
                    return ReadFlacTags(filePath);
                case ".aac":
                    return ReadAacTags(filePath);
                case ".ogg":
                    return ReadOggTags(filePath);
                case ".wma":
                    return ReadWmaTags(filePath);
                case ".aiff":
                    return ReadAiffTags(filePath);
                case ".m4a":
                    return ReadM4aTags(filePath);
                case ".opus":
                    return ReadOpusTags(filePath);
                default:
                    return null;
            }
        }
        public static string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath);
        }

        public static AudioTags ReadMp3Tags(string filePath)
        {
            AudioTags tags = new AudioTags();
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] header = new byte[10];
                    fileStream.Read(header, 0, 10);

                    string tag = Encoding.ASCII.GetString(header, 0, 3);

                    if (tag.Equals("ID3", StringComparison.Ordinal))
                    {
                        int tagSize = ((header[6] & 0x7F) << 21) | ((header[7] & 0x7F) << 14) | ((header[8] & 0x7F) << 7) | (header[9] & 0x7F);

                        byte[] tagData = new byte[tagSize];
                        fileStream.Read(tagData, 0, tagSize);
                        string artist = Encoding.ASCII.GetString(tagData, 33, 30).Trim();
                        string title = Encoding.ASCII.GetString(tagData, 3, 30).Trim();
                        return new AudioTags { Artist = artist, Title = title };
                    }
                    else
                    {
                        return new AudioTags { Artist = null, Title = null };
                    }
                }
            }
            catch (Exception ex)
            {
                return new AudioTags { Artist = null, Title = ex.Message };
            }
        }

        public static AudioTags ReadFlacTags(string filePath)
        {
            AudioTags tags = new AudioTags();

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] flacHeader = new byte[4];
                    fileStream.Read(flacHeader, 0, 4);

                    if (Encoding.ASCII.GetString(flacHeader, 0, 4) != "fLaC")
                    {
                        Console.WriteLine("Not a valid FLAC file.");
                        return tags; // Return empty tags
                    }

                    while (true)
                    {
                        byte[] blockHeader = new byte[4];
                        fileStream.Read(blockHeader, 0, 4);

                        byte blockType = (byte)(blockHeader[0] & 0x7F); 
                        int blockSize = (blockHeader[1] << 16) | (blockHeader[2] << 8) | blockHeader[3]; 

                        byte[] metadataBlock = new byte[blockSize];
                        fileStream.Read(metadataBlock, 0, blockSize);

                        if (blockType == 4)
                        {
                            int vendorLength = BitConverter.ToInt32(metadataBlock, 0);
                            string vendorString = Encoding.ASCII.GetString(metadataBlock, 4, vendorLength);

                            int commentsCount = BitConverter.ToInt32(metadataBlock, 4 + vendorLength);
                            int offset = 8 + vendorLength;

                            for (int i = 0; i < commentsCount; i++)
                            {
                                int commentLength = BitConverter.ToInt32(metadataBlock, offset);
                                string comment = Encoding.UTF8.GetString(metadataBlock, offset + 4, commentLength);
                                offset += 4 + commentLength;

                                if (comment.StartsWith("ARTIST=", StringComparison.OrdinalIgnoreCase))
                                {
                                    tags.Artist = comment.Substring(7);
                                }
                                else if (comment.StartsWith("TITLE=", StringComparison.OrdinalIgnoreCase))
                                {
                                    tags.Title = comment.Substring(6);
                                }
                            }
                        }

                        fileStream.Seek(blockSize, SeekOrigin.Current);
                        if ((blockType & 0x80) != 0)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading FLAC tags: {ex.Message}");
            }

            return tags;
        }

        public static AudioTags ReadAacTags(string filePath)
        {
            AudioTags tags = new AudioTags();

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] ftypHeader = new byte[4];
                    fileStream.Read(ftypHeader, 0, 4);

                    if (Encoding.ASCII.GetString(ftypHeader, 0, 4) != "ftyp")
                    {
                        Console.WriteLine("Not a valid AAC file.");
                        return tags;
                    }

                    while (fileStream.Position < fileStream.Length)
                    {
                        byte[] atomHeader = new byte[8];
                        fileStream.Read(atomHeader, 0, 8);

                        uint atomSize = BitConverter.ToUInt32(ReadLittleEndianBytes(new MemoryStream(atomHeader, 0, 4), 4), 0);
                        string atomType = Encoding.ASCII.GetString(atomHeader, 4, 4);

                        if (atomType == "moov")
                        {
                            fileStream.Seek(4, SeekOrigin.Current);
                            byte[] udtaHeader = new byte[4];
                            fileStream.Read(udtaHeader, 0, 4);

                            if (Encoding.ASCII.GetString(udtaHeader, 0, 4) == "udta")
                            {
                                fileStream.Seek(4, SeekOrigin.Current);
                                byte[] metaHeader = new byte[4];
                                fileStream.Read(metaHeader, 0, 4);

                                if (Encoding.ASCII.GetString(metaHeader, 0, 4) == "meta")
                                {
                                    fileStream.Seek(4, SeekOrigin.Current);
                                    byte[] ilstHeader = new byte[4];
                                    fileStream.Read(ilstHeader, 0, 4);

                                    if (Encoding.ASCII.GetString(ilstHeader, 0, 4) == "ilst")
                                    {
                                        while (fileStream.Position < fileStream.Length)
                                        {
                                            byte[] entryHeader = new byte[8];
                                            fileStream.Read(entryHeader, 0, 8);

                                            uint entrySize = BitConverter.ToUInt32(ReadLittleEndianBytes(new MemoryStream(entryHeader, 0, 4), 4), 0);
                                            string entryType = Encoding.ASCII.GetString(entryHeader, 4, 4);

                                            byte[] entryData = new byte[entrySize - 8];
                                            fileStream.Read(entryData, 0, (int)entrySize - 8);
                                            if (entryType == "\xA9nam")
                                            {
                                                tags.Title = Encoding.UTF8.GetString(entryData);
                                            }
                                            else if (entryType == "\xA9ART")
                                            {
                                                tags.Artist = Encoding.UTF8.GetString(entryData);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        fileStream.Seek(atomSize - 8, SeekOrigin.Current);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading AAC tags: {ex.Message}");
            }

            return tags;
        }

        public static AudioTags ReadOggTags(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] oggSHeader = new byte[4];
                    fileStream.Read(oggSHeader, 0, 4);

                    if (Encoding.ASCII.GetString(oggSHeader, 0, 4) != "OggS")
                    {
                        return new AudioTags { Artist = null, Title = null};
                    }

                    fileStream.Seek(28, SeekOrigin.Begin);
                    byte[] commentLengthBytes = new byte[4];
                    fileStream.Read(commentLengthBytes, 0, 4);
                    int commentLength = BitConverter.ToInt32(commentLengthBytes, 0);
                    byte[] commentField = new byte[commentLength];
                    fileStream.Read(commentField, 0, commentLength);
                    string vorbisComment = Encoding.UTF8.GetString(commentField);
                    string[] commentLines = vorbisComment.Split('\0');

                    AudioTags result = new AudioTags();
                    foreach (string commentLine in commentLines)
                    {
                        if (commentLine.StartsWith("ARTIST=", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Artist = commentLine.Substring(7);
                        }
                        else if (commentLine.StartsWith("TITLE=", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Title = commentLine.Substring(6);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                return new AudioTags { Artist = null, Title = null };
            }
        }

        public static AudioTags ReadWmaTags(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] asfHeader = new byte[16];
                    fileStream.Read(asfHeader, 0, 16);

                    if (!IsAsfHeader(asfHeader))
                    {
                        return new AudioTags { Artist = null, Title = null };
                    }

                    fileStream.Seek(30, SeekOrigin.Begin);
                    byte[] tagObjectSizeBytes = new byte[8];
                    fileStream.Read(tagObjectSizeBytes, 0, 8);
                    long tagObjectSize = BitConverter.ToInt64(tagObjectSizeBytes, 0);
                    byte[] tagObject = new byte[tagObjectSize];
                    fileStream.Read(tagObject, 0, (int)tagObjectSize);
                    string asfTagString = Encoding.Unicode.GetString(tagObject);
                    string[] tagLines = asfTagString.Split('\0');

                    AudioTags result = new AudioTags();
                    foreach (string tagLine in tagLines)
                    {
                        if (tagLine.StartsWith("WM/AlbumArtist=", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Artist = tagLine.Substring("WM/AlbumArtist=".Length);
                        }
                        else if (tagLine.StartsWith("WM/Title=", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Title = tagLine.Substring("WM/Title=".Length);
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                return new AudioTags { Artist = null, Title = null };
            }
        }

        public static AudioTags ReadAiffTags(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] formHeader = new byte[4];
                    fileStream.Read(formHeader, 0, 4);

                    if (Encoding.ASCII.GetString(formHeader, 0, 4) != "FORM")
                    {
                        return new AudioTags {Artist = null, Title = null };
                    }

                    byte[] formSizeBytes = new byte[4];
                    fileStream.Read(formSizeBytes, 0, 4);
                    int formSize = BitConverter.ToInt32(formSizeBytes, 0);

                    byte[] aiffSignature = new byte[4];
                    fileStream.Read(aiffSignature, 0, 4);

                    if (Encoding.ASCII.GetString(aiffSignature, 0, 4) != "AIFF")
                    {
                        return new AudioTags {Artist = null, Title = null };
                    }

                    while (fileStream.Position < formSize)
                    {
                        byte[] chunkHeader = new byte[4];
                        fileStream.Read(chunkHeader, 0, 4);
                        int chunkSize = BitConverter.ToInt32(chunkHeader, 0);

                        if (Encoding.ASCII.GetString(chunkHeader, 0, 4) == "COMM")
                        {
                            byte[] commChunk = new byte[chunkSize];
                            fileStream.Read(commChunk, 0, chunkSize);
                            int channels = BitConverter.ToInt16(commChunk, 0);
                            int frames = BitConverter.ToInt32(commChunk, 2);
                            int bitsPerSample = BitConverter.ToInt16(commChunk, 12);
                            int remainingSize = formSize - (int)fileStream.Position;
                            byte[] remainingChunks = new byte[remainingSize];
                            fileStream.Read(remainingChunks, 0, remainingSize);
                            string nameChunk = Encoding.UTF8.GetString(remainingChunks);
                            string[] nameLines = nameChunk.Split('\0');

                            AudioTags result = new AudioTags();
                            foreach (string nameLine in nameLines)
                            {
                                if (nameLine.StartsWith("Artist=", StringComparison.OrdinalIgnoreCase))
                                {
                                    result.Artist = nameLine.Substring("Artist=".Length);
                                }
                                else if (nameLine.StartsWith("Title=", StringComparison.OrdinalIgnoreCase))
                                {
                                    result.Title = nameLine.Substring("Title=".Length);
                                }
                            }

                            return result;
                        }
                        fileStream.Seek(chunkSize, SeekOrigin.Current);
                    }
                }
            }
            catch (Exception ex)
            {
                return new AudioTags { Artist = null, Title = null };
            }

            return new AudioTags {Artist = null, Title = null };
        }


        public static AudioTags ReadM4aTags(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] ftypHeader = new byte[4];
                    fileStream.Read(ftypHeader, 0, 4);

                    if (Encoding.ASCII.GetString(ftypHeader, 0, 4) != "ftyp")
                    {
                        return new AudioTags {Artist = null, Title = null };
                    }

                    while (fileStream.Position < fileStream.Length)
                    {
                        byte[] atomHeader = new byte[8];
                        fileStream.Read(atomHeader, 0, 8);

                        int atomSize = BitConverter.ToInt32(atomHeader, 0);
                        string atomType = Encoding.ASCII.GetString(atomHeader, 4, 4);

                        if (atomType == "moov")
                        {
                            fileStream.Seek(4, SeekOrigin.Current);
                            byte[] udtaHeader = new byte[4];
                            fileStream.Read(udtaHeader, 0, 4);

                            if (Encoding.ASCII.GetString(udtaHeader, 0, 4) == "udta")
                            {
                                fileStream.Seek(4, SeekOrigin.Current);
                                byte[] metaHeader = new byte[4];
                                fileStream.Read(metaHeader, 0, 4);

                                if (Encoding.ASCII.GetString(metaHeader, 0, 4) == "meta")
                                {
                                    fileStream.Seek(4, SeekOrigin.Current);
                                    byte[] ilstHeader = new byte[4];
                                    fileStream.Read(ilstHeader, 0, 4);

                                    if (Encoding.ASCII.GetString(ilstHeader, 0, 4) == "ilst")
                                    {
                                        while (fileStream.Position < fileStream.Length)
                                        {
                                            byte[] entryHeader = new byte[8];
                                            fileStream.Read(entryHeader, 0, 8);
                                            int entrySize = BitConverter.ToInt32(entryHeader, 0);
                                            string entryType = Encoding.ASCII.GetString(entryHeader, 4, 4);
                                            byte[] entryData = new byte[entrySize - 8];
                                            fileStream.Read(entryData, 0, entrySize - 8);

                                            AudioTags result = new AudioTags();
                                            if (entryType == "©nam")
                                            {
                                                result.Title = Encoding.UTF8.GetString(entryData);
                                            }
                                            else if (entryType == "©ART")
                                            {
                                                result.Artist = Encoding.UTF8.GetString(entryData);
                                            }

                                            if (!string.IsNullOrEmpty(result.Title) && !string.IsNullOrEmpty(result.Artist))
                                            {
                                                return result;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        fileStream.Seek(atomSize - 8, SeekOrigin.Current);
                    }
                }
            }
            catch (Exception ex)
            {
                return new AudioTags { Artist = null, Title = null };
            }

            return new AudioTags { Artist = null, Title = null };
        }

        public static AudioTags ReadOpusTags(string filePath)
        {
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] oggSHeader = new byte[4];
                    fileStream.Read(oggSHeader, 0, 4);

                    if (Encoding.ASCII.GetString(oggSHeader, 0, 4) != "OggS")
                    {
                        return new AudioTags { Artist = null, Title = null };
                    }

                    fileStream.Seek(26, SeekOrigin.Current);
                    int segmentTableSize = fileStream.ReadByte();
                    fileStream.Seek(segmentTableSize, SeekOrigin.Current);
                    int packetType = fileStream.ReadByte();

                    if (packetType == 1)
                    {
                        byte[] vorbisHeader = new byte[4];
                        fileStream.Read(vorbisHeader, 0, 4);

                        if (Encoding.ASCII.GetString(vorbisHeader, 0, 4) == "Opus")
                        {
                            int vorbisCommentSize = fileStream.ReadByte();
                            byte[] vorbisCommentField = new byte[vorbisCommentSize];
                            fileStream.Read(vorbisCommentField, 0, vorbisCommentSize);
                            string vorbisComment = Encoding.UTF8.GetString(vorbisCommentField);
                            string[] commentLines = vorbisComment.Split('\0');

                            var tags = new AudioTags();

                            foreach (string commentLine in commentLines)
                            {
                                if (commentLine.StartsWith("ARTIST=", StringComparison.OrdinalIgnoreCase))
                                {
                                    tags.Artist = commentLine.Substring(7);
                                }
                                else if (commentLine.StartsWith("TITLE=", StringComparison.OrdinalIgnoreCase))
                                {
                                    tags.Title = commentLine.Substring(6);
                                }
                            }

                            return tags;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new AudioTags { Artist = null, Title = null };
            }

            return new AudioTags { Artist = null, Title = null };
        }


        public static bool IsAsfHeader(byte[] header)
        {
            string headerString = Encoding.ASCII.GetString(header, 0, 16);
            return headerString.StartsWith("ASF ") && headerString.EndsWith("ASFER\x01\x02\x00\x00\x00");
        }

        public static byte[] ReadLittleEndianBytes(Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, count);
            Array.Reverse(buffer);
            return buffer;
        }
    }

}
