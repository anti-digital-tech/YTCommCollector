using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YTCommCollector
{
  internal static class YTTitleFetcher
  {
    static public async Task<string> GetTitleAsync(string apiKey, string videoId)
    {
      YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
      {
        ApiKey = apiKey
      });

      var searchRequest = youtubeService.Videos.List("snippet");
      searchRequest.Id = videoId;
      var searchResponse = await searchRequest.ExecuteAsync();
      var youTubeVideo = searchResponse.Items.FirstOrDefault();
      if (youTubeVideo != null)
      {
        return youTubeVideo.Snippet.Title;
      }
      return String.Empty;
    }
  }
}
