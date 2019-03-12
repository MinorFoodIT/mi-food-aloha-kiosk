// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.DataContracts.Check
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using AlohaPOSAgents.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AlohaPOSAgents.DataContracts
{
  public class Check : IAlohaObjects
  {
    public int id;
    public List<OrderItem> orderItems;
    public int tableId;

    public string ToJsonString()
    {
      return JsonConvert.SerializeObject((object) this);
    }
  }
}
