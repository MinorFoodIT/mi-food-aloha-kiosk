// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.Utils
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using System;
using System.Text;

namespace AlohaPOSAgents
{
  public static class Utils
  {
    public static string Base64Encode(string plainText)
    {
      return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }

    public static string Base64Decode(string base64EncodedData)
    {
      return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
    }
  }
}
