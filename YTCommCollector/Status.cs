using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTCommCollector
{
  internal class Status
  {
    public string VideoId { get; set; } = string.Empty;
    public string? Title { get; set; } = string.Empty;
    public string? Url { get; set; } = string.Empty;
    public string Progress { get; set; } = string.Empty;

    public int Count
    {
      get
      {
        return ListComms.Count;
      }
    }

    public List<Comm> ListComms { get; set; } = new List<Comm>();

    public Status(string videoId)
    {
      VideoId = videoId;
      Title = String.Empty;
      Progress = "Stopped";
      Url = "https://www.youtube.com/watch?v=" + videoId;
    }

    public Boolean IsSelected { get; set; } = false;
  }
}
