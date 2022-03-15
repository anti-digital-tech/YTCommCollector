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
    public string Title { get; set; } = string.Empty;
    public int Progress { get; set; } = 0;

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
      Progress = 0;
    }

    public Boolean IsSelected { get; set; } = false;
  }
}
