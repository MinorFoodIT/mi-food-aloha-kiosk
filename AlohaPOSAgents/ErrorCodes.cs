// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.ErrorCodes
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

namespace AlohaPOSAgents
{
  public static class ErrorCodes
  {
    public enum Error
    {
      SystemError = 10001, // 0x00002711
      SokcetMessageInvalid = 10002, // 0x00002712
      InvalidParamError = 10003, // 0x00002713
      ParameterKeyInvalid = 10004, // 0x00002714
      ParameterValueInvalid = 10005, // 0x00002715
      DataSentToPosInvalid = 10006, // 0x00002716
      AppKeyInvalid = 10007, // 0x00002717
      PasswordInvalid = 10008, // 0x00002718
    }
  }
}
