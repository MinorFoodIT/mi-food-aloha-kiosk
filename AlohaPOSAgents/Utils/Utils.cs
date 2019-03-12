// Decompiled with JetBrains decompiler
// Type: Tabsquare.Utils.Utils
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tabsquare.Utils
{
  internal class Utils
  {
    public static string FormatUTF8(string itemName)
    {
      Decoder decoder = Encoding.UTF8.GetDecoder();
      byte[] bytes = Encoding.UTF8.GetBytes(itemName);
      char[] chars = new char[decoder.GetCharCount(bytes, 0, ((IEnumerable<byte>) bytes).Count<byte>())];
      decoder.GetChars(bytes, 0, ((IEnumerable<byte>) bytes).Count<byte>(), chars, 0);
      string empty = string.Empty;
      foreach (char ch in chars)
        empty += (string) (object) ch;
      return empty;
    }
  }
}
