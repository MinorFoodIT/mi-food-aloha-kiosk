// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.DataContracts.Result
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using Newtonsoft.Json;

namespace AlohaPOSAgents.DataContracts
{
  public class Result
  {
    public string message = "";
    public bool result = true;
    public object data;

    public string ToJsonString()
    {
      return JsonConvert.SerializeObject((object) this);
    }
  }
}
