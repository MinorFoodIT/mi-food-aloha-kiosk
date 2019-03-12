// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.Interfaces.ISocketHelper
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using AlohaPOSAgents.DataContracts;
using System.Collections.Generic;

namespace AlohaPOSAgents.Interfaces
{
  internal interface ISocketHelper
  {
    List<OrderItem> GetItemsInCheck(int checkId);

    Check OpenTable(int tableNum, string tableName, int numGuests);

    void AddItems(
      int tableId,
      int checkId,
      int orderType,
      List<OrderItem> orderItems,
      string orderNo);

    void Logout();
  }
}
