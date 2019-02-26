using Shim.Entities;
using System.Collections.Generic;

namespace Shim.Library
{
  public class Entry
  {
    public Agent Target { get; set; }
    public LogType Type { get; set; }
    public string Value { get; set; }
  }

  public sealed class HistoryManager
  {
    public static List<Entry> Entries;

    public static void Reset()
    {
      Entries = new List<Entry>();
    }

    public static void LogMessage(string message, Agent target = null)
    {
      Entries.Add(new Entry()
      {
        Type = LogType.Message,
        Value = message,
        Target = target ?? null
      });
    }

    public static void LogItem(Item item, Agent agent, bool isLoss = false)
    {
      Entries.Add(new Entry()
      {
        Type = (isLoss) ? LogType.ItemLoss : LogType.ItemGain,
        Target = agent,
        Value = item.Name
      });
    }

    public static void LogTrait(Trait item, Agent agent, bool isRemoved = false)
    {
      Entries.Add(new Entry()
      {
        Type = (isRemoved) ? LogType.TraitRemoved : LogType.TraitAdded,
        Target = agent,
        Value = item.Name
      });
    }
  }
}
