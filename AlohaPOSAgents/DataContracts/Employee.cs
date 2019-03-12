// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.DataContracts.Employee
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using AlohaPOSAgents.Interfaces;

namespace AlohaPOSAgents.DataContracts
{
  internal class Employee : IAlohaObjects
  {
    private readonly string _firstName;
    private readonly string _id;
    private readonly string _lastName;

    public Employee(string id, string firstName, string lastName)
    {
      this._id = id;
      this._firstName = firstName;
      this._lastName = lastName;
    }

    public string ToJsonString()
    {
      return string.Format("{id:{0}, name:{1}}", (object) this._id, (object) (this._firstName + this._lastName));
    }
  }
}
