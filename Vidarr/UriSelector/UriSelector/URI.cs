using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace UriSelector
{
    class URI
    {
        private const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
        private const string VimeoLinkRegex = @"/https?:\/\/(?:www\.|player\.)?vimeo.com\/(?:channels\/(?:\w+\/)?|groups\/([^\/]*)\/videos\/|album\/(\d+)\/video\/|video\/|)(\d+)(?:$|\/|\?)/";


        public URI()
        {
        }

        public async Task<string>ExecuteGETRequest()
        {
            string url = "https://www.youtube.com/watch?v=Q3N7j8RsKxk";
            HttpWebRequest request = HttpWebRequest.CreateHttp(url);

            var ws = await request.GetResponseAsync();

            return ws.ResponseUri.ToString();
        }
        public static string GetYoutubeVideoId(string input)
        {
            var regex = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);

            foreach (Match match in regex.Matches(input))
            {
                foreach (var groupdata in match.Groups.Cast<Group>().Where(groupdata => !groupdata.ToString().StartsWith("http://") && !groupdata.ToString().StartsWith("https://") && !groupdata.ToString().StartsWith("youtu") && !groupdata.ToString().StartsWith("www.")))
                {
                    return groupdata.ToString();
                }
            }
            return string.Empty;
        }


        public static string GetYouTubeVideoTitle(string youtubeLinkUrl)
        {
            string response = ExecuteGETRequest(),
                     title = response.Substring(response.IndexOf("<title>\n") + 8);

            title = title.Substring(0, title.IndexOf("\n"));
            return title.Trim();
        }
    }
}
