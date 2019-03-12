// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.AlohaError
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

namespace AlohaPOSAgents
{
  public static class AlohaError
  {
    public const string StaffAlreadyLogin = "0xC0068002";
    public const string NoOneLoggedIn = "0xC0068007";
    public const string AlreadyClockedIn = "0xC0068008";
    public const string TableInUse = "0xC0068032";
    public const string NotClockedIn = "0xC006800C";
    public const string ModNotAuthForParentItem = "0xC0068028";
    public const string ModReqsNotMet = "0xC0068029";
    public const string ItemIsNOTOpenItem = "0xC006802A";
    public const string InvalidModCode = "0xC0068027";
    public const string UnavailableItem = "0xC0068026";
    public const string CheckNotFullyTendered = "0xC0068018";
    public const string EmpNotAssignedToDrawer = "0xC006801E";
    public const string OrderAmountNotTally = "The order is closed but the order amount is not tally";
    public const string CRMSuccess = "<ResMsg>Success</ResMsg>";
  }
}
