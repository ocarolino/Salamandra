using Salamandra.Engine.Domain;
using Salamandra.Engine.Exceptions;
using Salamandra.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Salamandra.Engine.Services.Playlists
{
    // Based on: https://github.com/NateShoffner/M3U.NET/blob/master/M3U.NET/M3UFile.cs (2022-04-12)
    public class M3UPlaylistLoader
    {
        public List<PlaylistEntryInfo> Load(string filename)
        {
            List<PlaylistEntryInfo> entries = new List<PlaylistEntryInfo>();

            using (StreamReader reader = new StreamReader(filename))
            {
                var workingUri = new Uri(Path.GetDirectoryName(filename)!);

                string? line;
                var lineCount = 0;

                PlaylistEntryInfo? entry = null;

                while ((line = reader.ReadLine()) != null)
                {
                    if (lineCount == 0 && line != "#EXTM3U")
                        throw new PlaylistLoaderException("M3U header not found.");

                    if (line.StartsWith("#EXTINF:"))
                    {
                        if (entry != null)
                            throw new PlaylistLoaderException("Unexpected entry detected.");

                        var split = line.Substring(8, line.Length - 8).Split(new[] { ',' }, 2);

                        if (split.Length != 2)
                            throw new PlaylistLoaderException("Invalid track information.");

                        int seconds;
                        if (!int.TryParse(split[0], out seconds))
                            throw new PlaylistLoaderException("Invalid track duration.");

                        var title = split[1];

                        var duration = TimeSpan.FromSeconds(seconds);

                        entry = new PlaylistEntryInfo() { Duration = duration, FriendlyName = title };
                    }
                    else if (entry != null && !line.StartsWith("#")) // ignore comments
                    {
                        Uri path;

                        if (!Uri.TryCreate(line, UriKind.RelativeOrAbsolute, out path))
                            throw new PlaylistLoaderException("Invalid entry path.");

                        if (path.IsFile)
                            path = path.MakeAbsoluteUri(workingUri);

                        entry.Filename = path.ToString();

                        entries.Add(entry);

                        entry = null;
                    }

                    lineCount++;
                }
            }

            return entries;
        }
    }
}
