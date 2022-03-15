using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace YTCommCollector
{
  internal class Engine
  {
    string keyApi = string.Empty;
    public Engine(string keyApi)
    {
      this.keyApi = keyApi;
    }
    public async Task FetchReplyComment(List<Comm> listComms, YouTubeService youtubeService, string parentId, int seqNum, int childNum, string? nextPageToken)
    {
      var request = youtubeService.Comments.List("snippet");
      request.TextFormat = CommentsResource.ListRequest.TextFormatEnum.PlainText;
      request.MaxResults = 50;
      request.ParentId = parentId;
      request.PageToken = nextPageToken;

      var response = await request.ExecuteAsync();

      foreach (var item in response.Items)
      {
        try
        {
          Comm comm = new Comm();
          comm.SeqNum = seqNum;
          comm.ChildNum = childNum;
          comm.Text = item.Snippet.TextDisplay;
          comm.CountGood = item.Snippet.LikeCount;
          comm.Author = item.Snippet.AuthorDisplayName;
          comm.CreatedAt = item.Snippet.PublishedAt;
          listComms.Add(comm);
          childNum++;
        }
        catch { }

      }

      if (response.NextPageToken != null)
      {
        await FetchReplyComment(listComms, youtubeService, parentId, seqNum, childNum, response.NextPageToken);
      }
    }
    public async Task FetchComment(List<Comm> listComms, string videoId, YouTubeService youtubeService, int seqNum, string? nextPageToken)
    {
      var request = youtubeService.CommentThreads.List("snippet");
      request.VideoId = videoId;
      request.Order = CommentThreadsResource.ListRequest.OrderEnum.Relevance;
      request.TextFormat = CommentThreadsResource.ListRequest.TextFormatEnum.PlainText;
      request.MaxResults = 100;
      request.PageToken = nextPageToken;

      var response = await request.ExecuteAsync();
      foreach (var item in response.Items)
      {
        try
        {
          Comm comm = new Comm();
          comm.SeqNum = seqNum;
          comm.ChildNum = 0;
          comm.Text = item.Snippet.TopLevelComment.Snippet.TextDisplay;
          comm.CountGood = item.Snippet.TopLevelComment.Snippet.LikeCount;
          comm.Author = item.Snippet.TopLevelComment.Snippet.AuthorDisplayName;
          comm.CreatedAt = item.Snippet.TopLevelComment.Snippet.PublishedAt;
          comm.CountReply = item.Snippet.TotalReplyCount;
          string parentId = item.Snippet.TopLevelComment.Id;
          listComms.Add(comm);

          if (item.Snippet.TotalReplyCount > 0)
          {
            await FetchReplyComment(listComms, youtubeService, parentId, seqNum, 1, null);
          }
          seqNum++;
        }
        catch { }
      }

      if (response.NextPageToken != null)
      {
        await FetchComment(listComms, videoId, youtubeService, seqNum, response.NextPageToken);
      }
    }

    public async void Fetch()
    {
      string urlTest = "https://www.youtube.com/watch?time_continue=12&v=-8dHivCHeEI&feature=emb_logo";
      string videoId = Regex.Replace(urlTest, @".+v=([^&]+).*", @"$1", RegexOptions.IgnoreCase | RegexOptions.Singleline);
      var youtubeService = new YouTubeService(new BaseClientService.Initializer()
      {
        ApiKey = this.keyApi
      });

      Status status = new Status(videoId);

      //await FetchComment(status.ListComms, videoId, youtubeService, 1, null);

      /*
            foreach(var info in commentList)
            {
                string line;
                if (info.ChildNo == 0)
                    line = string.Format("{0:0000}\t{1}\t{2}\t{3}\t{4}\t{5}", info.ParentNo, info.Text.Replace("\n", ""), info.LikeCount, info.AuthorName, info.PublishedAt, info.ReplyCount);
                else
                    line = string.Format("{0:0000}-{1:000}\t{2}\t{3}\t{4}\t{5}", info.ParentNo, info.ChildNo, info.Text.Replace("\n", ""), info.LikeCount, info.AuthorName, info.PublishedAt);
                System.Diagnostics.Debug.WriteLine(line);
            }
      */
    }
  }
}
