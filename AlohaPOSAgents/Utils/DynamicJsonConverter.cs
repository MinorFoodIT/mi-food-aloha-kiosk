// Decompiled with JetBrains decompiler
// Type: POSAgent.Utils.DynamicJsonConverter
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace POSAgent.Utils
{
  public sealed class DynamicJsonConverter : JavaScriptConverter
  {
    public override object Deserialize(
      IDictionary<string, object> dictionary,
      Type type,
      JavaScriptSerializer serializer)
    {
      if (dictionary == null)
        throw new ArgumentNullException(nameof (dictionary));
      if (!(type == typeof (object)))
        return (object) null;
      return (object) new DynamicJsonConverter.DynamicJsonObject(dictionary);
    }

    public override IDictionary<string, object> Serialize(
      object obj,
      JavaScriptSerializer serializer)
    {
      throw new NotImplementedException();
    }

    public override IEnumerable<Type> SupportedTypes
    {
      get
      {
        return (IEnumerable<Type>) new ReadOnlyCollection<Type>((IList<Type>) new List<Type>((IEnumerable<Type>) new Type[1]
        {
          typeof (object)
        }));
      }
    }

    private sealed class DynamicJsonObject : DynamicObject
    {
      private readonly IDictionary<string, object> _dictionary;

      public DynamicJsonObject(IDictionary<string, object> dictionary)
      {
        if (dictionary == null)
          throw new ArgumentNullException(nameof (dictionary));
        this._dictionary = dictionary;
      }

      public override string ToString()
      {
        StringBuilder sb = new StringBuilder("{");
        this.ToString(sb);
        return sb.ToString();
      }

      private void ToString(StringBuilder sb)
      {
        bool flag1 = true;
        foreach (KeyValuePair<string, object> keyValuePair in (IEnumerable<KeyValuePair<string, object>>) this._dictionary)
        {
          if (!flag1)
            sb.Append(",");
          flag1 = false;
          object obj1 = keyValuePair.Value;
          string key = keyValuePair.Key;
          if (obj1 is string)
            sb.AppendFormat("{0}:\"{1}\"", (object) key, obj1);
          else if (obj1 is IDictionary<string, object>)
            new DynamicJsonConverter.DynamicJsonObject((IDictionary<string, object>) obj1).ToString(sb);
          else if (obj1 is ArrayList)
          {
            sb.Append(key + ":[");
            bool flag2 = true;
            foreach (object obj2 in (ArrayList) obj1)
            {
              if (!flag2)
                sb.Append(",");
              flag2 = false;
              if (obj2 is IDictionary<string, object>)
                new DynamicJsonConverter.DynamicJsonObject((IDictionary<string, object>) obj2).ToString(sb);
              else if (obj2 is string)
                sb.AppendFormat("\"{0}\"", obj2);
              else
                sb.AppendFormat("{0}", obj2);
            }
            sb.Append("]");
          }
          else
            sb.AppendFormat("{0}:{1}", (object) key, obj1);
        }
        sb.Append("}");
      }

      public override bool TryGetMember(GetMemberBinder binder, out object result)
      {
        if (!this._dictionary.TryGetValue(binder.Name, out result))
        {
          result = (object) null;
          return true;
        }
        result = DynamicJsonConverter.DynamicJsonObject.WrapResultObject(result);
        return true;
      }

      public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
      {
        if (indexes.Length != 1 || indexes[0] == null)
          return base.TryGetIndex(binder, indexes, out result);
        if (!this._dictionary.TryGetValue(indexes[0].ToString(), out result))
        {
          result = (object) null;
          return true;
        }
        result = DynamicJsonConverter.DynamicJsonObject.WrapResultObject(result);
        return true;
      }

      private static object WrapResultObject(object result)
      {
        IDictionary<string, object> dictionary = result as IDictionary<string, object>;
        if (dictionary != null)
          return (object) new DynamicJsonConverter.DynamicJsonObject(dictionary);
        ArrayList source = result as ArrayList;
        if (source == null || source.Count <= 0)
          return result;
        if (!(source[0] is IDictionary<string, object>))
          return (object) new List<object>(source.Cast<object>());
        return (object) new List<object>((IEnumerable<object>) source.Cast<IDictionary<string, object>>().Select<IDictionary<string, object>, DynamicJsonConverter.DynamicJsonObject>((Func<IDictionary<string, object>, DynamicJsonConverter.DynamicJsonObject>) (x => new DynamicJsonConverter.DynamicJsonObject(x))));
      }
    }
  }
}
