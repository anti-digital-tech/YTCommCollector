using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YTCommCollector
{
  internal class Comm
  {
    public int SeqNum { set; get; } = 0;
    public int ChildNum { set; get; } = 0;
    public string Link { set; get; } = string.Empty;

    public DateTime? CreatedAt { set; get; } = null;

    public string Text { set; get; } = string.Empty;
    public string Author { set; get; } = string.Empty;
    public long? CountGood { set; get; } = null;
    public long? CountReply { set; get; } = null;
  }
}
