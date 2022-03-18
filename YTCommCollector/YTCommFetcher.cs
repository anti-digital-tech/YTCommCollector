using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Windows;

namespace YTCommCollector
{
  internal class YTCommFetcher
  {
    YouTubeService youtubeService_;
    delegate void DelegateProcess();
    DelegateProcess delegateProcess_;
    Window parent_;
    public YTCommFetcher(string keyApi, Window parent, Action updateDisplay)
    {
      youtubeService_ = new YouTubeService(new BaseClientService.Initializer()
      {
        ApiKey = keyApi
      });
      parent_ = parent;
      delegateProcess_ = new DelegateProcess(updateDisplay);
    }
    public async Task FetchReplyCommentsAsync(Status status, string parentId, int seqNum, int childNum, string? nextPageToken)
    {
      var request = youtubeService_.Comments.List("snippet");
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
          status.ListComms.Add(comm);
          childNum++;
        }
        catch { }

      }

      parent_.Dispatcher.Invoke(delegateProcess_);

      if (response.NextPageToken != null)
      {
        await FetchReplyCommentsAsync(status, parentId, seqNum, childNum, response.NextPageToken);
      }
    }
    public async Task FetchCommentsAsync(Status status, int seqNum, string? nextPageToken)
    {
      var request = youtubeService_.CommentThreads.List("snippet");

      request.VideoId = status.VideoId;
      request.Order = CommentThreadsResource.ListRequest.OrderEnum.Time;
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
          status.ListComms.Add(comm);

          if (item.Snippet.TotalReplyCount > 0)
          {
            await FetchReplyCommentsAsync(status, parentId, seqNum, 1, null);
          }
          seqNum++;
        }
        catch { }
      }

      parent_.Dispatcher.Invoke(delegateProcess_);

      if (response.NextPageToken != null)
      {
        await FetchCommentsAsync(status, seqNum, response.NextPageToken);
      }
      status.Progress = "Completed";
    }
    static public string CorrectVideoId(string videoId)
    {
      if (string.IsNullOrEmpty(videoId)) return String.Empty;
      return videoId.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) ?
        Regex.Replace(videoId, @"^.+v=([^&]+).*", @"$1", RegexOptions.IgnoreCase | RegexOptions.Singleline) : videoId;
    }

    public Task FetchAsync(List<Status> listStatuses)
    {
      var tasks = new List<Task>();
      foreach(var status in listStatuses)
      {
        status.Progress = "Running";
        var task = Task.Run(() => FetchCommentsAsync(status, 1, null));
        tasks.Add(task);
      }
      return Task.WhenAll(tasks);
    }
  }
}
