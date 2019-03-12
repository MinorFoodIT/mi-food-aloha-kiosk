// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.SocketHelper
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using AlohaFOHLib;
using AlohaPOSAgents.DataContracts;
using AlohaPOSAgents.Interfaces;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POSAgent.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace AlohaPOSAgents
{
  internal class SocketHelper : ISocketHelper
  {
    private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private static IberDepot PIberDepotInterface = (IberDepot) null;
    private static IIberFuncs2 IberFunc = (IIberFuncs2) null;
    public bool is_debug = ConfigurationManager.AppSettings["DEBUG"] == "2606";
    private const string LICENSE = "1J210T2L3V5N1V0@4H2?18324;5=5H4J2R4X0U3R2445154R4R4Z2P1I5Q2Z3W5G241M1L59";
    protected JavaScriptSerializer _javaScriptSerializer;
    private string _mstrResponse;
    private string _parsedData;
    private bool _caughtException;
    private int termId;
    private int tableNumSpecific;
    private int dineIn;

    public JavaScriptSerializer JavaScriptSerializer
    {
      get
      {
        if (this._javaScriptSerializer == null)
        {
          this._javaScriptSerializer = new JavaScriptSerializer();
          this._javaScriptSerializer.RegisterConverters((IEnumerable<JavaScriptConverter>) new DynamicJsonConverter[1]
          {
            new DynamicJsonConverter()
          });
        }
        return this._javaScriptSerializer;
      }
    }

    public bool IsSettlePayment
    {
      get
      {
        return ConfigurationManager.AppSettings[nameof (IsSettlePayment)].ToLower().Equals("1");
      }
    }

    public string CRMStoreCode
    {
      get
      {
        return ConfigurationManager.AppSettings[nameof (CRMStoreCode)].ToString();
      }
    }

    public string CRMEndPoint
    {
      get
      {
        return ConfigurationManager.AppSettings[nameof (CRMEndPoint)].ToString();
      }
    }

    public bool IsOrderClosedIfNotTally
    {
      get
      {
        return ConfigurationManager.AppSettings[nameof (IsOrderClosedIfNotTally)].ToLower().Equals("1");
      }
    }

    public string KioskTable
    {
      get
      {
        return ConfigurationManager.AppSettings[nameof (KioskTable)].ToString();
      }
    }

    public bool IsLogoutEnabled
    {
      get
      {
        return ConfigurationManager.AppSettings[nameof (IsLogoutEnabled)].ToLower().Equals("true");
      }
    }

    public int PaymentTenderId
    {
      get
      {
        return int.Parse(ConfigurationManager.AppSettings[nameof (PaymentTenderId)]);
      }
    }

    [DllImport("ole32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    [return: MarshalAs(UnmanagedType.Interface)]
    private static extern object CoGetClassObject(
      [MarshalAs(UnmanagedType.LPStruct), In] Guid rclsid,
      SocketHelper.CLSCTX dwClsContext,
      IntPtr pServerInfo,
      [MarshalAs(UnmanagedType.LPStruct), In] Guid riid);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern int LogonUser(
      string lpszUsername,
      string lpszDomain,
      string lpszPassword,
      int dwLogonType,
      int dwLogonProvider,
      out IntPtr phToken);

    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern int ImpersonateLoggedOnUser(IntPtr hToken);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern int RevertToSelf();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern int CloseHandle(IntPtr hObject);

    public SocketHelper()
    {
      this.termId = Convert.ToInt32(ConfigurationManager.AppSettings["TERM_ID"]);
      this.dineIn = int.Parse(ConfigurationSettings.AppSettings["DineIn"]);
      try
      {
        SocketHelper.PIberDepotInterface = (IberDepot) new IberDepotClass();
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat(">>>>>>>>> EXCEPTION with initializing IberDepotClass object: ", (object) ex.Message.ToString());
        int num = (int) MessageBox.Show(">>>>>>>>> EXCEPTION with initializing IberDepotClass object: ", ex.Message.ToString());
      }
      SocketHelper.IClassFactory2 classObject = SocketHelper.CoGetClassObject(typeof (IberFuncsClass).GUID, SocketHelper.CLSCTX.CLSCTX_LOCAL_SERVER, IntPtr.Zero, typeof (SocketHelper.IClassFactory2).GUID) as SocketHelper.IClassFactory2;
      try
      {
        try
        {
          if (int.Parse(ConfigurationSettings.AppSettings["IsLicenseRequired"]) == 1)
            SocketHelper.IberFunc = classObject.CreateInstanceLic((object) null, (object) null, typeof (IIberFuncs2).GUID, "1J210T2L3V5N1V0@4H2?18324;5=5H4J2R4X0U3R2445154R4R4Z2P1I5Q2Z3W5G241M1L59") as IIberFuncs2;
          else
            SocketHelper.IberFunc = (IIberFuncs2) classObject.CreateInstance((object) null, typeof (IIberFuncs2).GUID);
        }
        catch (Exception ex)
        {
          SocketHelper.IberFunc = (IIberFuncs2) classObject.CreateInstance((object) null, typeof (IIberFuncs2).GUID);
        }
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(">>>>>>>>> EXCEPTION with initializing IberFunc object: ", ex.Message.ToString());
        throw ex;
      }
    }

    public string GetTableList()
    {
      return string.Empty;
    }

    public List<OrderItem> GetItemsInCheck(int checkId)
    {
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("Start GetItemsInCheck()");
      List<OrderItem> orderItemList = new List<OrderItem>();
      SocketHelper.Logger.InfoFormat("Check ID {0}", (object) checkId);
      IberObject berObject1 = SocketHelper.PIberDepotInterface.FindObjectFromId(540, checkId).First();
      IberEnum berEnum = (IberEnum) null;
      try
      {
        berEnum = berObject1.GetEnum(542);
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat("Error when get entries in Check: {0}", (object) ex.StackTrace);
      }
      if (berEnum != null)
      {
        OrderItem orderItem = new OrderItem();
        for (int index = 1; index <= berEnum.Count; ++index)
        {
          IberObject berObject2 = berEnum.get_Item(index);
          int longVal1 = berObject2.GetLongVal("TYPE");
          int longVal2 = berObject2.GetLongVal("LEVEL");
          if (this.is_debug)
          {
            SocketHelper.Logger.InfoFormat("================================");
            SocketHelper.Logger.InfoFormat("TYPE {0}", (object) longVal1);
            SocketHelper.Logger.InfoFormat("LEVEL {0}", (object) longVal2);
            SocketHelper.Logger.InfoFormat("DATA {0}", (object) berObject2.GetLongVal("DATA"));
            SocketHelper.Logger.InfoFormat("PRICE {0}", (object) berObject2.GetDoubleVal("PRICE"));
            SocketHelper.Logger.InfoFormat("DISP_NAME {0}", (object) berObject2.GetStringVal("DISP_NAME"));
          }
          string stringVal = berObject2.GetStringVal("MOD_CODE");
          if (!stringVal.Contains("12") && !stringVal.Contains("8") && !stringVal.Contains("13"))
          {
            if (longVal2 == 0 && longVal1 != 104 && longVal1 == 0)
            {
              orderItemList.Add(orderItem);
              orderItem = new OrderItem()
              {
                plu = berObject2.GetLongVal("DATA"),
                name = berObject2.GetStringVal("DISP_NAME"),
                price = berObject2.GetDoubleVal("PRICE"),
                modifiers = new List<Modifier>()
              };
              orderItem.ui_type = longVal1 != 0 ? 0 : 1;
            }
            if (longVal2 == 1 && longVal1 == 0)
            {
              Modifier modifier = new Modifier()
              {
                id = berObject2.GetLongVal("DATA"),
                name = berObject2.GetStringVal("DISP_NAME"),
                price = berObject2.GetDoubleVal("PRICE")
              };
              orderItem.modifiers.Add(modifier);
            }
          }
        }
        orderItemList.Add(orderItem);
        orderItemList.RemoveAt(0);
      }
      return orderItemList;
    }

    public List<OrderItem> GetItemsWithTotalTaxInCheck(int checkId)
    {
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("Start GetItemsInCheck");
      List<OrderItem> orderItemList = new List<OrderItem>();
      SocketHelper.Logger.InfoFormat("Check ID {0}", (object) checkId);
      IberObject berObject1 = SocketHelper.PIberDepotInterface.FindObjectFromId(540, checkId).First();
      IberEnum berEnum = (IberEnum) null;
      try
      {
        berEnum = berObject1.GetEnum(542);
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat("Error when get entries in Check: {0}", (object) ex.StackTrace);
      }
      if (berEnum != null)
      {
        OrderItem orderItem = new OrderItem();
        for (int index = 1; index <= berEnum.Count; ++index)
        {
          IberObject berObject2 = berEnum.get_Item(index);
          int longVal1 = berObject2.GetLongVal("TYPE");
          int longVal2 = berObject2.GetLongVal("LEVEL");
          SocketHelper.Logger.InfoFormat("================================");
          SocketHelper.Logger.InfoFormat("TYPE {0}", (object) longVal1);
          SocketHelper.Logger.InfoFormat("LEVEL {0}", (object) longVal2);
          SocketHelper.Logger.InfoFormat("DATA {0}", (object) berObject2.GetLongVal("DATA"));
          SocketHelper.Logger.InfoFormat("PRICE {0}", (object) berObject2.GetDoubleVal("PRICE"));
          SocketHelper.Logger.InfoFormat("DISP_NAME {0}", (object) berObject2.GetStringVal("DISP_NAME"));
          string stringVal = berObject2.GetStringVal("MOD_CODE");
          if (!stringVal.Contains("12") && !stringVal.Contains("8") && !stringVal.Contains("13"))
          {
            if (longVal2 == 0 && longVal1 != 104)
            {
              orderItemList.Add(orderItem);
              orderItem = new OrderItem()
              {
                plu = berObject2.GetLongVal("DATA"),
                name = berObject2.GetStringVal("DISP_NAME"),
                price = berObject2.GetDoubleVal("PRICE"),
                modifiers = new List<Modifier>()
              };
              orderItem.ui_type = longVal1 != 0 ? 0 : 1;
            }
            if (longVal2 == 1)
            {
              switch (longVal1)
              {
                case 0:
                  Modifier modifier1 = new Modifier()
                  {
                    id = berObject2.GetLongVal("DATA"),
                    name = berObject2.GetStringVal("DISP_NAME"),
                    price = berObject2.GetDoubleVal("PRICE")
                  };
                  orderItem.modifiers.Add(modifier1);
                  continue;
                case 1:
                  Modifier modifier2 = new Modifier()
                  {
                    id = -1,
                    name = berObject2.GetStringVal("DISP_NAME"),
                    price = berObject2.GetDoubleVal("PRICE")
                  };
                  orderItem.modifiers.Add(modifier2);
                  continue;
                default:
                  continue;
              }
            }
          }
        }
        orderItemList.Add(orderItem);
        orderItemList.RemoveAt(0);
      }
      return orderItemList;
    }

    public void AddItems(
      int tableId,
      int checkId,
      int orderType,
      List<OrderItem> orderItems,
      string orderNo)
    {
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("[Naveen] [AddItems] Selected CheckID {0} on table {1}", (object) checkId, (object) tableId);
      int itemId = -1;
      try
      {
        itemId = int.Parse(ConfigurationManager.AppSettings["PluOpenItem"].ToString());
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat("ERROR: Missing PLUOpenItem Code");
      }
      List<int> intList = new List<int>();
      string empty = string.Empty;
      if (orderNo != null)
      {
        SocketHelper.Logger.InfoFormat("[Naveen] [AddItems] Sending Kiosk Running OrderNo {0}", (object) orderNo);
        try
        {
          int entryInCheck = this.CreateEntryInCheck(checkId, itemId, "Kiosk " + orderNo, 0.0, (List<Modifier>) null, 1);
          intList.Add(entryInCheck);
        }
        catch (Exception ex)
        {
          SocketHelper.Logger.InfoFormat("[Error] Could not send OrderNo {0} as Open Item {1} {2} {3}", (object) orderNo, (object) itemId, (object) ex.Message, (object) ex.StackTrace.ToString());
        }
      }
      try
      {
        foreach (OrderItem orderItem in orderItems)
        {
          int plu = orderItem.plu;
          string name = orderItem.name;
          for (int index = 0; index < orderItem.quantity; ++index)
          {
            try
            {
              if (plu == itemId)
              {
                if (this.is_debug)
                  SocketHelper.Logger.InfoFormat("CheckId == PluOpenItem >>>>>>>>>>> CheckId {0}, ItemID {1}, Item Name {2}, Item Price {3}, OrderItem.Count {4}", (object) checkId, (object) plu, (object) "", (object) -999999999.99, (object) orderItem.modifiers.Count);
                int entryInCheck = this.CreateEntryInCheck(checkId, plu, "", -999999999.99, orderItem.modifiers, orderItem.quantity);
                intList.Add(entryInCheck);
              }
              else
              {
                if (this.is_debug)
                  SocketHelper.Logger.InfoFormat("ItemId != PluOpenItem >>>>>>>>>  CheckId {0}, ItemID {1}, Item Name {2}, Item Price {3}, OrderItem.Count {4}", (object) checkId, (object) plu, (object) "", (object) -999999999.99, (object) orderItem.modifiers.Count);
                int entryInCheck = this.CreateEntryInCheck(checkId, plu, "", -999999999.99, orderItem.modifiers, orderItem.quantity);
                intList.Add(entryInCheck);
              }
            }
            catch (Exception ex)
            {
              foreach (int EntryId in intList)
                SocketHelper.IberFunc.VoidItem(this.termId, checkId, EntryId, 2);
              this.FinishAddItems(tableId, this.dineIn);
              throw ex;
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw;
      }
      finally
      {
        this.FinishAddItems(tableId, this.dineIn);
        if (this.IsLogoutEnabled)
          this.Logout();
      }
      Thread.Sleep(1000);
    }

    public void Logout()
    {
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("[Debug] LOGOUT() TermId " + (object) this.termId);
      SocketHelper.IberFunc.LogOut(this.termId);
    }

    public AlohaPOSAgents.DataContracts.Check OpenTable(
      int tableNum,
      string tableName,
      int numGuests)
    {
      SocketHelper.Logger.InfoFormat("Start Open Table");
      AlohaPOSAgents.DataContracts.Check check = new AlohaPOSAgents.DataContracts.Check();
      int TableId = -1;
      try
      {
        if (this.is_debug)
          SocketHelper.Logger.InfoFormat("[Debug] AddTable: termId " + (object) this.termId + ", QueueId " + (object) 0 + ", tableNum" + (tableNum != 0 ? (object) "" : (object) tableName) + " numOfGuests " + (object) numGuests);
        TableId = SocketHelper.IberFunc.AddTable(this.termId, 0, tableNum, tableNum != 0 ? "" : tableName, numGuests);
        if (this.is_debug)
          SocketHelper.Logger.InfoFormat(">>>>>>>>>> [Debug] TableId:" + (object) TableId);
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat(">>>>>>>>>> Exception in OpenTable()");
      }
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("[Debug] AddCheck: termId " + (object) this.termId + ", TableId " + (object) TableId);
      int num = SocketHelper.IberFunc.AddCheck(this.termId, TableId);
      check.tableId = TableId;
      check.id = num;
      SocketHelper.Logger.InfoFormat("Open Table OK. TableID :" + (object) TableId + " CHeckID " + (object) num);
      return check;
    }

    public string ProcessMsg(string message)
    {
      this._caughtException = false;
      this._parsedData = message;
      this._mstrResponse = this.FuncNavigator(this._parsedData).ToJsonString();
      SocketHelper.Logger.InfoFormat("Final Response to Service: " + this._mstrResponse);
      return this._mstrResponse;
    }

    public static string SerializeObject(object responseObj)
    {
      return JsonConvert.SerializeObject(responseObj);
    }

    private Result FuncNavigator(string parsedData)
    {
      Result result = new Result();
      try
      {
        JObject jobject = JObject.Parse(parsedData);
        AlohaPOSAgents.DataContracts.Check check1 = new AlohaPOSAgents.DataContracts.Check();
        SocketHelper.Logger.InfoFormat("\r\n\r\n");
        SocketHelper.Logger.InfoFormat("REQUEST IN JSON: {0}", (object) parsedData);
        switch (jobject["function"].ToString())
        {
          case "OpenTable":
            if (this.is_debug)
              SocketHelper.Logger.InfoFormat("OpenTable()");
            SocketHelper.Logger.InfoFormat("Tabble Num: {0}", (object) jobject["table_num"]);
            SocketHelper.Logger.InfoFormat("Table {0}", (object) jobject["table_name"]);
            SocketHelper.Logger.InfoFormat("Num Guests {0}", (object) jobject["num_guests"]);
            AlohaPOSAgents.DataContracts.Check check2 = this.OpenTable(Convert.ToInt32((object) jobject["table_num"]), jobject["table_name"].ToString(), Convert.ToInt32((object) jobject["num_guests"]));
            result.data = (object) check2;
            break;
          case "AddItems":
            if (this.is_debug)
            {
              SocketHelper.Logger.InfoFormat("AddItems()");
              SocketHelper.Logger.InfoFormat("Table {0}", (object) jobject["table_name"]);
              SocketHelper.Logger.InfoFormat("Num Guests {0}", (object) jobject["num_guests"]);
              SocketHelper.Logger.InfoFormat("Tabble Num: {0}", (object) jobject["table_num"]);
            }
            JArray jarray1 = (JArray) jobject["order_items"];
            check1.orderItems = jarray1.ToObject<List<OrderItem>>();
            AlohaPOSAgents.DataContracts.Check check3 = this.AddItemsProcessor(Convert.ToInt32((object) jobject["table_num"]), jobject["table_name"].ToString(), Convert.ToInt32((object) jobject["num_guests"]), check1.orderItems);
            result.data = (object) check3;
            break;
          case "AddItemsForKiosk":
            int num = 1;
            bool flag = false;
            string orderNo = "1";
            if (jobject["table_name"] != null)
              orderNo = jobject["table_name"].ToString();
            string empty1 = string.Empty;
            if (jobject["terminal"] != null)
              empty1 = jobject["terminal"].ToString();
            string empty2 = string.Empty;
            if (jobject["order_type"] != null)
              empty2 = jobject["order_type"].ToString();
            string empty3 = string.Empty;
            if (jobject["membership_id"] != null)
              empty3 = jobject["membership_id"].ToString();
            string empty4 = string.Empty;
            if (jobject["payment"] != null)
              empty4 = jobject["payment"].ToString();
            double order_amount = 0.0;
            if (jobject["order_amount"] != null)
              order_amount = double.Parse(jobject["order_amount"].ToString());
            string empty5 = string.Empty;
            if (jobject["surcharges"] != null)
              jobject["surcharges"].ToString().Trim();
            string payment_media_id = string.Empty;
            if (jobject["payment_type_media_id"] != null)
              payment_media_id = jobject["payment_type_media_id"].ToString().Trim();
            JArray jarray2 = (JArray) jobject["order_items"];
            check1.orderItems = jarray2.ToObject<List<OrderItem>>();
            SocketHelper.Logger.InfoFormat("");
            SocketHelper.Logger.InfoFormat("");
            SocketHelper.Logger.InfoFormat("AddItemsForKiosk");
            SocketHelper.Logger.InfoFormat("Table Name:" + orderNo);
            SocketHelper.Logger.InfoFormat("Terminal:" + empty1);
            SocketHelper.Logger.InfoFormat("Num Guests " + (object) num);
            SocketHelper.Logger.InfoFormat("Order Type: " + empty2);
            SocketHelper.Logger.InfoFormat("Total Amount: " + (object) jobject["order_amount"]);
            SocketHelper.Logger.InfoFormat("Payment Tender ID: " + (object) jobject["payment_type_media_id"]);
            SocketHelper.Logger.InfoFormat("Order Items: {0} ", (object) SocketHelper.SerializeObject((object) check1.orderItems));
            SocketHelper.Logger.InfoFormat("DicData: {0} ", (object) SocketHelper.SerializeObject((object) jobject));
            List<OrderItem> orderItems = check1.orderItems;
            string str1 = string.Empty;
            try
            {
              check1 = this.SendOrderForKiosk(parsedData, empty1, orderNo, order_amount, empty4, payment_media_id, check1.orderItems);
            }
            catch (Exception ex)
            {
              flag = true;
              str1 = ex.Message;
              if (!ex.Message.Equals("The order is closed but the order amount is not tally"))
              {
                SocketHelper.Logger.InfoFormat("ERROR Message {0}", (object) ex.Message);
                SocketHelper.Logger.InfoFormat("ERROR StackTrace {0}", (object) ex.StackTrace);
                result.result = false;
                result.message += str1;
                if (!this._caughtException)
                  result = this.ErrorProcessor(ex, parsedData);
                return result;
              }
            }
            try
            {
              if (!string.IsNullOrEmpty(empty3))
              {
                if (!this.SendCRM(empty3, check1.id, orderNo, order_amount, parsedData, orderItems))
                {
                  string str2 = str1 + "The points could not be earned due to the error to CRM endpoint!!!";
                  return new Result()
                  {
                    result = false,
                    message = str2,
                    data = (object) check1.id
                  };
                }
              }
            }
            catch (Exception ex)
            {
              SocketHelper.Logger.InfoFormat("ERROR Message {0}", (object) ex.Message);
              SocketHelper.Logger.InfoFormat("ERROR StackTrace {0}", (object) ex.StackTrace);
              result.result = false;
              result.message += ex.Message;
              return result;
            }
            if (flag)
              return new Result()
              {
                result = false,
                message = str1,
                data = (object) check1.id
              };
            return new Result()
            {
              result = true,
              message = "The order has been sent through POS successfully!",
              data = (object) check1.id
            };
          case "GetTables":
            if (this.is_debug)
              SocketHelper.Logger.InfoFormat("GetTables()");
            List<AlohaPOSAgents.DataContracts.Table> tables = this.GetTables();
            result.data = (object) tables;
            break;
          case "GetCustomTables":
            if (this.is_debug)
              SocketHelper.Logger.InfoFormat("GetCustomTables()");
            List<AlohaPOSAgents.DataContracts.Table> customTables = this.GetCustomTables();
            result.data = (object) customTables;
            break;
          case "GetItems":
            if (this.is_debug)
              SocketHelper.Logger.InfoFormat("GetItems() Table Num: {0}", (object) jobject["table_num"]);
            List<OrderItem> checksInTable1 = this.GetChecksInTable(Convert.ToInt32((object) jobject["table_num"]), jobject["table_name"].ToString(), 0);
            result.data = (object) checksInTable1;
            break;
          case "GetBills":
            if (this.is_debug)
              SocketHelper.Logger.InfoFormat("GetBills() Table Num: {0}", (object) jobject["table_num"]);
            List<OrderItem> checksInTable2 = this.GetChecksInTable(Convert.ToInt32((object) jobject["table_num"]), jobject["table_name"].ToString(), 1);
            result.data = (object) checksInTable2;
            break;
        }
        SocketHelper.Logger.InfoFormat("RESPOSNE IN JSON: {0}", (object) result.ToJsonString());
        return result;
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat("ERROR Message {0}", (object) ex.Message);
        SocketHelper.Logger.InfoFormat("ERROR StackTrace {0}", (object) ex.StackTrace);
        result.result = false;
        result.message = ex.Message;
        if (!this._caughtException)
          result = this.ErrorProcessor(ex, parsedData);
        return result;
      }
    }

    private bool SendCRM(
      string membership_id,
      int checkId,
      string orderNo,
      double order_amount,
      string parsedData,
      List<OrderItem> orderItems)
    {
      try
      {
        DateTime now = DateTime.Now;
        string newValue1 = Guid.NewGuid().ToString();
        string newValue2 = now.ToString("yyyyMMddHHmmss");
        string newValue3 = "010555108801300";
        string newValue4 = now.ToString("yyyyMMdd");
        string crmStoreCode = this.CRMStoreCode;
        string newValue5 = newValue2;
        string newValue6 = "2";
        string newValue7 = orderNo;
        string newValue8 = "200";
        string newValue9 = order_amount.ToString();
        string newValue10 = "0";
        order_amount.ToString();
        string newValue11 = membership_id;
        string newValue12 = crmStoreCode + newValue5 + newValue7;
        string empty = string.Empty;
        if (orderItems != null)
        {
          foreach (OrderItem orderItem in orderItems)
          {
            empty += this.GenerateItemDetailSection(orderItem.plu, orderItem.quantity, orderItem.price, orderItem.name);
            if (orderItem.modifiers.Count > 0)
            {
              foreach (Modifier modifier in orderItem.modifiers)
              {
                try
                {
                  if (modifier != null)
                  {
                    if (modifier.price > 0.0 && modifier.id > 0)
                      empty += this.GenerateItemDetailSection(modifier.id, modifier.quantity, modifier.price, modifier.name);
                    else if (modifier.price > 0.0)
                    {
                      if (modifier.plu > 0)
                        empty += this.GenerateItemDetailSection(modifier.plu, modifier.quantity, modifier.price, modifier.name);
                    }
                  }
                }
                catch (Exception ex)
                {
                  SocketHelper.Logger.InfoFormat(">>>> [Error] Problem with earning modifiers: {0}", (object) (ex.Message + ex.StackTrace.ToString()));
                }
              }
            }
          }
        }
        else
          SocketHelper.Logger.InfoFormat("[Error] NULL!!!!!!!!!!");
        string content = "\r\n                        <? xml version= '1.0' encoding= 'UTF-8' ?>\r\n                        <soap:Envelope xmlns:soap= 'http://schemas.xmlsoap.org/soap/envelope/'\r\n                        xmlns:xsd= 'http://www.w3.org/2001/XMLSchema'\r\n                        xmlns:xsi= 'http://www.w3.org/2001/XMLSchema-instance' >\r\n                          <soap:Body>\r\n                            <RequestService02 xmlns= 'http://tempuri.org/' >\r\n                              <RegMsg02>\r\n                                <ReqHdr>\r\n                                  <ActCd> ENPCF02 </ActCd>\r\n                                  <ReqID> {ReqID} </ReqID>\r\n                                  <ReqDt> {ReqDt} </ReqDt>\r\n                                  <TxID>  {TxID} </TxID>\r\n                                </ReqHdr>\r\n                                <TrnHdr>\r\n                                  <DOB> {DOB} </DOB>\r\n                                  <StrCd> {StrCd} </StrCd>\r\n                                  <TrnMd> 1 </TrnMd>\r\n                                  <TrnDt> {TrnDt} </TrnDt>\r\n                                  <TerID> {terID} </TerID>\r\n                                  <ChkNo> {ChkNo} </ChkNo>\r\n                                  <StfID> {stfID} </StfID>\r\n                                  <TtlAmt> {TtlAmt} </TtlAmt>\r\n                                  <DisAmt> {DisAmt} </DisAmt>\r\n                                  <DueAmt> {TtlAmt} </DueAmt>\r\n                                  <Ref1> {Ref1} </Ref1>\r\n                                  <Ref2> {Ref2} </Ref2>\r\n                                  <Ref3> CF Singha Complex </Ref3>\r\n                                </TrnHdr>\r\n                                <TrnDtl>\r\n                                  <Items>                        \r\n\t\t\t\t                       {ItemDetailsPlaceHolder}\r\n\t\t                          </Items>\r\n                                  <Promos />\r\n                                  <Comps />\r\n                                </TrnDtl>\r\n                              </RegMsg02>\r\n                            </RequestService02>\r\n                          </soap:Body>\r\n                        </soap:Envelope>                    \r\n                ".Replace("{ReqID}", newValue1).Replace("{ReqDt}", newValue2).Replace("{TxID}", newValue3).Replace("{DOB}", newValue4).Replace("{StrCd}", crmStoreCode).Replace("{TrnDt}", newValue5).Replace("{terID}", newValue6).Replace("{ChkNo}", newValue7).Replace("{stfID}", newValue8).Replace("{TtlAmt}", newValue9).Replace("{DisAmt}", newValue10).Replace("{TtlAmt}", newValue9).Replace("{Ref1}", newValue11).Replace("{Ref2}", newValue12).Replace("{ItemDetailsPlaceHolder}", empty);
        SocketHelper.Logger.InfoFormat("CRM Request >>> " + content);
        using (HttpClient httpClient = new HttpClient())
        {
          httpClient.BaseAddress = new Uri("http://prc-uat.minordigital.com/atg");
          try
          {
            JObject jobject = new JObject();
            StringContent stringContent = new StringContent(content, Encoding.UTF8, "application/xml");
            HttpResponseMessage result;
            try
            {
              result = httpClient.PostAsync(this.CRMEndPoint, (HttpContent) stringContent).Result;
            }
            catch (Exception ex)
            {
              SocketHelper.Logger.InfoFormat("[Error] Problem with sending CRM: " + ex.Message + ex.StackTrace.ToString());
              throw;
            }
            string str = this.ReformatAPIResponse(result);
            SocketHelper.Logger.InfoFormat(" CRM Response after format >>>> " + str);
            return str.Contains("<ResMsg>Success</ResMsg>");
          }
          catch (Exception ex)
          {
            throw;
          }
        }
      }
      catch (Exception ex)
      {
        throw;
      }
    }

    private string GenerateItemDetailSection(int p1, int p2, double p3, string p4)
    {
      return "<Item>\r\n\t\t\t\t                             <ItmID> {ItemId} </ItmID>\r\n\t\t\t\t                             <PntID />\r\n\t\t\t\t                             <ItmQTY> {ItemQuantity} </ItmQTY>\r\n\t\t\t\t                             <ItmAmt> {ItemAmount} </ItmAmt>\r\n\t\t\t\t                             <ItmDist>0</ItmDist>\r\n\t\t\t\t                             <ItmCatID>0</ItmCatID>\r\n\t\t\t\t                             <ItmCatName></ItmCatName>\r\n\t\t\t\t                             <ItmEntID>0</ItmEntID>\r\n\t\t\t\t                             <ItmName> {ItemName} </ItmName>\r\n\t\t\t\t                             <PntName />\r\n\t\t\t\t                        </Item>\r\n                                        ".Replace("{ItemId}", string.Concat((object) p1)).Replace("{ItemQuantity}", string.Concat((object) p2)).Replace("{ItemAmount}", string.Concat((object) (p3 * (double) p2))).Replace("{ItemName}", p4);
    }

    private string ReformatAPIResponse(HttpResponseMessage createOrderResponse)
    {
      return createOrderResponse.Content.ReadAsStringAsync().Result.Replace("\\", "").Trim('"');
    }

    private List<OrderItem> GetChecksInTable(
      int tableNo,
      string tableName,
      int viewMode)
    {
      List<OrderItem> orderItemList = new List<OrderItem>();
      int tableId = tableNo > 0 ? this.FindTableToken(tableNo) : this.FindCustomTableToken(tableName);
      SocketHelper.Logger.InfoFormat("Table ID: {0}", (object) tableId);
      if (tableId <= 0)
        return orderItemList;
      int latestCheckId = this.FindLatestCheckId(tableId);
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("Find the Last Check from table: {0}", (object) latestCheckId);
      if (viewMode == 0)
        orderItemList = latestCheckId > 0 ? this.GetItemsInCheck(latestCheckId) : orderItemList;
      if (viewMode == 1)
        orderItemList = latestCheckId > 0 ? this.GetItemsInCheck(latestCheckId) : orderItemList;
      return orderItemList;
    }

    public AlohaPOSAgents.DataContracts.Check SendOrderForKiosk(
      string requestInJSon,
      string terminal,
      string orderNo,
      double order_amount,
      string payment,
      string payment_media_id,
      List<OrderItem> orderItems)
    {
      int NumGuests = 1;
      string kioskTable = this.KioskTable;
      int num1 = int.Parse(kioskTable);
      AlohaPOSAgents.DataContracts.Check check = new AlohaPOSAgents.DataContracts.Check();
      this.tableNumSpecific = num1;
      this.JavaScriptSerializer.Deserialize(requestInJSon, typeof (object));
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("Start Add Items Processer");
      int num2 = 0;
      int checkId = 0;
      bool flag = false;
      try
      {
        num2 = SocketHelper.IberFunc.AddTable(this.termId, 0, num1, num1 != 0 ? "" : kioskTable, NumGuests);
      }
      catch (Exception ex)
      {
        if (ex.Message.Contains("0xC0068032"))
        {
          SocketHelper.Logger.InfoFormat(">>>> [Debug] Table In Use: {0}", (object) num1);
          num2 = num1 != 0 ? this.FindTableToken(num1) : this.FindCustomTableToken(kioskTable);
          SocketHelper.Logger.InfoFormat(">>>> [Debug] Retrieved Table Id : {0}", (object) num2);
          flag = true;
        }
      }
      SocketHelper.Logger.InfoFormat("Table opened successuflly >>>>> TableID : {0}", (object) num2);
      if (flag)
        checkId = this.FindLatestCheckId(num2);
      if (checkId == 0)
      {
        if (this.is_debug)
          SocketHelper.Logger.InfoFormat(string.Format("[Debug] AddCheck() termId {0}, tableId {1}", (object) this.termId, (object) num2));
        checkId = SocketHelper.IberFunc.AddCheck(this.termId, num2);
      }
      check.tableId = num2;
      check.id = checkId;
      SocketHelper.Logger.InfoFormat("Check opened successfully >>>>>  CheckId : {0}", (object) checkId);
      try
      {
        this.AddItems(num2, checkId, this.dineIn, orderItems, orderNo);
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat("Error sending items:" + ex.Message + ex.StackTrace.ToString());
        throw ex;
      }
      if (this.IsSettlePayment)
      {
        if (!this.PerformSettlement(checkId, order_amount, payment_media_id))
        {
          SocketHelper.Logger.InfoFormat("The order is closed but the order amount is not tally");
          throw new Exception("The order is closed but the order amount is not tally");
        }
        SocketHelper.Logger.InfoFormat("The order is closed successfully!!!");
      }
      return check;
    }

    private AlohaPOSAgents.DataContracts.Check AddItemsProcessor(
      int tableNum,
      string tableName,
      int numGuests,
      List<OrderItem> orderItems)
    {
      AlohaPOSAgents.DataContracts.Check check = new AlohaPOSAgents.DataContracts.Check();
      this.tableNumSpecific = tableNum;
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("Start Add Items Processer");
      int num = 0;
      int checkId = 0;
      bool flag = false;
      try
      {
        if (tableNum == 0)
        {
          num = this.FindCustomTableToken(tableName);
          if (num > 0)
            flag = true;
        }
        else
        {
          if (this.is_debug)
            SocketHelper.Logger.InfoFormat(string.Format(" [Debug] AddTable() terminalID {0}, queueId {1}, tableNum {2}, tableName {3}, guests {4}, ", (object) this.termId, (object) 0, (object) tableNum, tableNum != 0 ? (object) "" : (object) tableName, (object) numGuests));
          num = SocketHelper.IberFunc.AddTable(this.termId, 0, tableNum, tableNum != 0 ? "" : tableName, numGuests);
        }
      }
      catch (Exception ex)
      {
        if (ex.Message.Contains("0xC0068032"))
        {
          SocketHelper.Logger.InfoFormat(">>>> [Debug] Table In Use: {0}", (object) tableNum);
          num = tableNum != 0 ? this.FindTableToken(tableNum) : this.FindCustomTableToken(tableName);
          SocketHelper.Logger.InfoFormat(">>>> [Debug] Retrieved Table Id : {0}", (object) num);
          flag = true;
        }
      }
      SocketHelper.Logger.InfoFormat("Table opened successuflly >>>>> TableID : {0}", (object) num);
      if (flag)
        checkId = this.FindLatestCheckId(num);
      if (checkId == 0)
      {
        if (this.is_debug)
          SocketHelper.Logger.InfoFormat(string.Format("[Debug] AddCheck() termId {0}, tableId {1}", (object) this.termId, (object) num));
        checkId = SocketHelper.IberFunc.AddCheck(this.termId, num);
      }
      check.tableId = num;
      check.id = checkId;
      SocketHelper.Logger.InfoFormat("Check opened successfully >>>>>  CheckId : {0}", (object) checkId);
      this.AddItems(num, checkId, this.dineIn, orderItems, (string) null);
      return check;
    }

    private bool PerformSettlement(int checkId, double order_amount, string payment_media_id)
    {
      try
      {
        int int32 = Convert.ToInt32(ConfigurationManager.AppSettings["TERM_ID"]);
        int paymentTenderId = this.PaymentTenderId;
        try
        {
          paymentTenderId = int.Parse(payment_media_id);
        }
        catch (Exception ex)
        {
          SocketHelper.Logger.Info((object) "[Error] Can not cast payment_type_id!!!!");
        }
        string CardId = "";
        double checkBalance = SocketHelper.IberFunc.GetCheckBalance(int32, checkId);
        SocketHelper.Logger.InfoFormat("[Debug] The total amount from POS: " + (object) checkBalance);
        SocketHelper.Logger.InfoFormat("[Debug] The tender: " + (object) paymentTenderId);
        bool flag = true;
        if (order_amount == checkBalance)
        {
          SocketHelper.Logger.InfoFormat("[Debug] Balance matched ---> " + (object) checkBalance);
        }
        else
        {
          SocketHelper.Logger.InfoFormat("[Debug] Balance NOT matched Tabsquare {0} <> POS {1} ", (object) order_amount, (object) checkBalance);
          flag = false;
        }
        if (flag || !flag && this.IsOrderClosedIfNotTally)
        {
          SocketHelper.IberFunc.ApplyPayment(int32, checkId, paymentTenderId, checkBalance, 0.0, CardId, "1220", "", "");
          Thread.Sleep(1000);
          Thread.Sleep(500);
          SocketHelper.IberFunc.CloseCheck(int32, checkId);
          Thread.Sleep(500);
          SocketHelper.IberFunc.CloseTable(int32, checkId);
          Thread.Sleep(500);
        }
        if (!flag)
          return flag;
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat("Exception in PerformSettlement(): " + ex.Message);
        SocketHelper.Logger.InfoFormat("Exception: " + ex.Message + ex.StackTrace.ToString());
        throw ex;
      }
      return true;
    }

    private Result ErrorProcessor(Exception ex, string parsedData)
    {
      Result result = new Result()
      {
        result = false,
        message = ex.Message
      };
      try
      {
        SocketHelper.Logger.InfoFormat("Start Process Exception");
        if (!this._caughtException)
        {
          SocketHelper.Logger.InfoFormat("2");
          SocketHelper.Logger.InfoFormat(ex.Message);
          if (ex.Message.Contains("0xC0068007"))
          {
            this.Login();
            return this.FuncNavigator(parsedData);
          }
          if (ex.Message.Contains("0xC0068008"))
            return this.FuncNavigator(parsedData);
          if (ex.Message.Contains("0xC006800C"))
          {
            SocketHelper.Logger.InfoFormat("Error - Not Clocked In");
            this.ClockIn();
            return this.FuncNavigator(parsedData);
          }
          if (ex.Message.Contains("0xC0068028"))
          {
            string format = "Error with ordering item - Modifier not allowed for parent item. Order is not closed yet !!!" + result.message;
            SocketHelper.Logger.InfoFormat(format);
            return result;
          }
          if (ex.Message.Contains("0xC0068029"))
          {
            string format = "Error with ordering item - Modifier requirements not met. Order is not closed yet !!!" + result.message;
            SocketHelper.Logger.InfoFormat(format);
            return result;
          }
          if (ex.Message.Contains("0xC006802A"))
          {
            string format = "Error with ordering item - Item is not an open item. Order is not closed yet !!!" + result.message;
            SocketHelper.Logger.InfoFormat(format);
            return result;
          }
          if (ex.Message.Contains("0xC0068027"))
          {
            string format = "Error with ordering item - Invalid ModCode. Order is not closed yet !!!" + result.message;
            SocketHelper.Logger.InfoFormat(format);
            return result;
          }
          if (ex.Message.Contains("0xC0068032"))
          {
            string format = "Error with ordering item - Table is in use !!!" + result.message;
            SocketHelper.Logger.InfoFormat(format);
            return result;
          }
          if (ex.Message.Contains("0xC0068026"))
          {
            string format = "Error with ordering item - Unavailable Item. Order is not closed yet !!!" + result.message;
            SocketHelper.Logger.InfoFormat(format);
            return result;
          }
          if (ex.Message.Contains("0xC006801E"))
          {
            string format = "Error with ordering item - Employee not assigned to drawer !!!" + result.message;
            SocketHelper.Logger.InfoFormat(format);
            return result;
          }
        }
        this._caughtException = true;
        return result;
      }
      catch (Exception ex1)
      {
        result.result = false;
        result.message = ex1.Message;
        return result;
      }
    }

    private void FinishAddItems(int tableId, int orderModeId)
    {
      try
      {
        if (this.is_debug)
          SocketHelper.Logger.InfoFormat(string.Format("[Debug] OrderItems() termId {0}, tableId {1}, orderModeId {2}", (object) this.termId, (object) tableId, (object) orderModeId));
        SocketHelper.IberFunc.OrderItems(this.termId, tableId, orderModeId);
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat(">>>> Exception in OrderItems() TermId {0} TableId {1} OrderModeId {2} {3}", (object) this.termId, (object) tableId, (object) orderModeId, (object) ex.Message);
      }
    }

    private int CreateEntryInCheck(
      int checkId,
      int itemId,
      string itemName,
      double price,
      List<Modifier> modifers,
      int itemQuantity)
    {
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("ADDING MAIN ITEM");
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("[Debug] BeginItem() termId {0}, checkId {1}, itemId {2}, itemName {3}, price {4}", (object) this.termId, (object) checkId, (object) itemId, (object) itemName, (object) price);
      int entryId = SocketHelper.IberFunc.BeginItem(this.termId, checkId, itemId, itemName, price);
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("[Debug] EntryId {0}", (object) entryId);
      try
      {
        int.Parse(ConfigurationManager.AppSettings["PluOpenItem"].ToString());
      }
      catch (Exception ex)
      {
        SocketHelper.Logger.InfoFormat(">>>>>>> ERROR >>>>>> Missing PLUOpenItem Code in configuration file app.config");
      }
      SocketHelper.Logger.InfoFormat("[Debug] Processing ordering items");
      if (modifers != null)
      {
        foreach (Modifier modifer in modifers)
        {
          for (int index = 0; index < modifer.quantity / itemQuantity; ++index)
          {
            if (modifer.id > 0 && modifer.price > 0.0)
              this.CreateModiferInEntry(entryId, modifer.id, modifer.name, -999999999.99, modifer);
            else if (modifer.price > 0.0 && modifer.plu > 0)
              this.CreateModiferInEntry(entryId, modifer.plu, "", modifer.price, modifer);
          }
        }
      }
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("Term ID : {0}", (object) this.termId);
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("[Debug] EndItem() termId " + (object) this.termId);
      SocketHelper.IberFunc.EndItem(this.termId);
      if (modifers != null)
      {
        foreach (Modifier modifer in modifers)
        {
          if (modifer.id <= 0 && modifer.plu <= 0 || modifer.price <= 0.0)
            this.ApplySpecialMessage(checkId, entryId, modifer.name);
        }
      }
      return entryId;
    }

    private void CreateModiferInEntry(
      int entryId,
      int modifierId,
      string name,
      double price,
      Modifier parent)
    {
      if (SocketHelper.IberFunc == null)
        return;
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("ADDING MODIFER ITEM");
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("[Debug] ModItem() EntryId : {0}, ModifierId : {1}, ModifierName : {2}, Modifer Price : {3} ", (object) entryId, (object) modifierId, (object) name, (object) price);
      int ParentEntryId = SocketHelper.IberFunc.ModItem(this.termId, entryId, modifierId, name, price, 0);
      if (parent.modifiers.Count <= 0)
        return;
      foreach (Modifier modifier in parent.modifiers)
      {
        if (modifier != null)
        {
          if (this.is_debug)
            SocketHelper.Logger.InfoFormat("ADDING SUB-MODIFER ITEM");
          if (this.is_debug)
            SocketHelper.Logger.InfoFormat("[Debug] ModItem() Parent EntryId : {0}, ModifierId : {1}, ModifierName : {2}, Modifer Price : {3} ", (object) ParentEntryId, (object) modifier.id, (object) modifier.name, (object) modifier.price);
          SocketHelper.IberFunc.ModItem(this.termId, ParentEntryId, modifier.id, modifier.name, modifier.price, 0);
        }
      }
    }

    private void ApplySpecialMessage(int checkId, int entryId, string message)
    {
      SocketHelper.Logger.InfoFormat("Start Apply special Request");
      SocketHelper.Logger.InfoFormat("Special Message - {0}", (object) message);
      if (SocketHelper.IberFunc == null)
        return;
      ((IIberFuncs20) SocketHelper.IberFunc).ApplySpecialMessage(this.termId, checkId, entryId, message);
    }

    public int Login()
    {
      int int32 = Convert.ToInt32(ConfigurationSettings.AppSettings["STAFF_ID"]);
      SocketHelper.Logger.InfoFormat("Start Login");
      return SocketHelper.IberFunc.LogIn(this.termId, int32, "", "");
    }

    public void ClockIn()
    {
      SocketHelper.Logger.InfoFormat("Start ClockIn");
      int JobCodeId = -1;
      try
      {
        JobCodeId = int.Parse(ConfigurationManager.AppSettings["JobCode"].ToString());
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("Missing JobCode!");
      }
      SocketHelper.Logger.InfoFormat(string.Format("[Debug] ClockIn() termId{0}, jobCode {1}", (object) this.termId, (object) JobCodeId));
      SocketHelper.IberFunc.ClockIn(this.termId, JobCodeId);
    }

    private List<AlohaPOSAgents.DataContracts.Table> GetTables()
    {
      List<AlohaPOSAgents.DataContracts.Table> tableList = new List<AlohaPOSAgents.DataContracts.Table>();
      IberEnum berEnum = SocketHelper.PIberDepotInterface.GetEnum(30);
      for (IberObject berObject = berEnum.First(); !berObject.Equals((object) null); berObject = berEnum.Next())
      {
        AlohaPOSAgents.DataContracts.Table table = new AlohaPOSAgents.DataContracts.Table()
        {
          Id = berObject.GetStringVal("ID"),
          Desc = berObject.GetStringVal("DESC")
        };
        tableList.Add(table);
      }
      return tableList;
    }

    public static List<IberObject> GetEmployeeList()
    {
      List<IberObject> berObjectList = new List<IberObject>();
      try
      {
        IberEnum berEnum = SocketHelper.PIberDepotInterface.GetEnum(7);
        for (IberObject berObject = berEnum.First(); !berObject.Equals((object) null); berObject = berEnum.Next())
        {
          berObjectList.Add(berObject);
          berObject.GetStringVal("ID");
          berObject.GetStringVal("FIRSTNAME");
          berObject.GetStringVal("LASTNAME");
        }
        return berObjectList;
      }
      catch (Exception ex)
      {
        return berObjectList;
      }
    }

    private int FindTableToken(int tableNo)
    {
      if (this.is_debug)
        SocketHelper.Logger.InfoFormat("[Debug] Start Find Table Token by Table NO");
      List<IberObject> employeeList = SocketHelper.GetEmployeeList();
      int num = 0;
      for (int index1 = 0; index1 < employeeList.Count; ++index1)
      {
        IberEnum berEnum;
        try
        {
          berEnum = employeeList[index1].GetEnum(501);
        }
        catch (Exception ex)
        {
          continue;
        }
        for (int index2 = 1; index2 <= berEnum.Count; ++index2)
        {
          IberObject berObject = berEnum.get_Item(index2);
          if (berObject.GetLongVal("TABLEDEF_ID") == tableNo)
          {
            num = berObject.GetLongVal("ID");
            SocketHelper.Logger.InfoFormat("FOUND TABLEID - {0}", (object) num);
          }
        }
      }
      return num;
    }

    private int RetrievedTableIdForUnOwned(int tableNo)
    {
      List<IberObject> employeeList = SocketHelper.GetEmployeeList();
      for (int index1 = 0; index1 < employeeList.Count; ++index1)
      {
        IberEnum berEnum;
        try
        {
          berEnum = employeeList[index1].GetEnum(501);
        }
        catch (Exception ex)
        {
          continue;
        }
        for (int index2 = 1; index2 <= berEnum.Count; ++index2)
        {
          IberObject berObject = berEnum.get_Item(index2);
          SocketHelper.Logger.InfoFormat("TABLE_DEF_ID - {0}", (object) berObject.GetLongVal("TABLEDEF_ID"));
          SocketHelper.Logger.InfoFormat("SOURCE_TABLE_ID - {0}", (object) berObject.GetLongVal("SOURCE_TABLE_ID"));
          SocketHelper.Logger.InfoFormat("TABLE ID - {0}", (object) berObject.GetLongVal("ID"));
          if (berObject.GetLongVal("TABLEDEF_ID") == tableNo)
            return berObject.GetLongVal("ID");
        }
      }
      return 0;
    }

    private int FindCustomTableToken(string tableName)
    {
      SocketHelper.Logger.InfoFormat("Start Find Custom Table Token by Table Name");
      List<IberObject> employeeList = SocketHelper.GetEmployeeList();
      SocketHelper.Logger.InfoFormat("Finish Get Employee List");
      SocketHelper.Logger.InfoFormat("No of Employees: {0}", (object) employeeList.Count);
      SocketHelper.Logger.InfoFormat("Table Name: {0}", (object) tableName);
      for (int index1 = 0; index1 < employeeList.Count; ++index1)
      {
        IberEnum berEnum;
        try
        {
          berEnum = employeeList[index1].GetEnum(501);
        }
        catch (Exception ex)
        {
          continue;
        }
        for (int index2 = 1; index2 <= berEnum.Count; ++index2)
        {
          IberObject berObject = berEnum.get_Item(index2);
          if (string.CompareOrdinal(berObject.GetStringVal("NAME"), tableName) == 0)
            return berObject.GetLongVal("ID");
        }
      }
      return 0;
    }

    private int FindLatestCheckId(int tableId)
    {
      try
      {
        IberEnum berEnum = SocketHelper.PIberDepotInterface.FindObjectFromId(520, tableId).First().GetEnum(526);
        if (berEnum.Count <= 0)
          return 0;
        IberObject berObject = berEnum.get_Item(berEnum.Count);
        if (this.is_debug)
          SocketHelper.Logger.InfoFormat("CHECK ID: {0}", (object) berObject.GetLongVal("ID"));
        return berObject.GetLongVal("ID");
      }
      catch (Exception ex)
      {
        return 0;
      }
    }

    private List<int> GetAllCheckIdByTableId(int tableId)
    {
      return new List<int>();
    }

    private List<AlohaPOSAgents.DataContracts.Table> GetCustomTables()
    {
      List<AlohaPOSAgents.DataContracts.Table> tableList = new List<AlohaPOSAgents.DataContracts.Table>();
      SocketHelper.Logger.InfoFormat("Start Get CustomTables");
      List<IberObject> employeeList = SocketHelper.GetEmployeeList();
      SocketHelper.Logger.InfoFormat("Finish Get Employee List");
      SocketHelper.Logger.InfoFormat("No of Employees: {0}", (object) employeeList.Count);
      for (int index1 = 0; index1 < employeeList.Count; ++index1)
      {
        IberEnum berEnum;
        try
        {
          berEnum = employeeList[index1].GetEnum(501);
        }
        catch (Exception ex)
        {
          continue;
        }
        for (int index2 = 1; index2 <= berEnum.Count; ++index2)
        {
          IberObject berObject = berEnum.get_Item(index2);
          SocketHelper.Logger.InfoFormat("TABLE_DEF_ID - {0}", (object) berObject.GetLongVal("TABLEDEF_ID"));
          SocketHelper.Logger.InfoFormat("TABLE ID - {0}", (object) berObject.GetLongVal("ID"));
          if (berObject.GetLongVal("TABLEDEF_ID") == 0)
          {
            AlohaPOSAgents.DataContracts.Table table = new AlohaPOSAgents.DataContracts.Table()
            {
              EmployeeId = employeeList[index1].GetLongVal("ID"),
              Name = berObject.GetStringVal("NAME")
            };
            tableList.Add(table);
          }
        }
      }
      return tableList;
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("B196B28F-BAB4-101A-B69C-00AA00341D07")]
    [ComImport]
    private interface IClassFactory2
    {
      [return: MarshalAs(UnmanagedType.Interface)]
      object CreateInstance([MarshalAs(UnmanagedType.Interface), In] object unused, [MarshalAs(UnmanagedType.LPStruct), In] Guid iid);

      void LockServer(int fLock);

      IntPtr GetLicInfo();

      [return: MarshalAs(UnmanagedType.BStr)]
      string RequestLicKey([MarshalAs(UnmanagedType.U4), In] int reserved);

      [return: MarshalAs(UnmanagedType.Interface)]
      object CreateInstanceLic([MarshalAs(UnmanagedType.Interface), In] object pUnkOuter, [MarshalAs(UnmanagedType.Interface), In] object pUnkReserved, [MarshalAs(UnmanagedType.LPStruct), In] Guid iid, [MarshalAs(UnmanagedType.BStr), In] string bstrKey);
    }

    [System.Flags]
    private enum CLSCTX : uint
    {
      CLSCTX_INPROC_SERVER = 1,
      CLSCTX_INPROC_HANDLER = 2,
      CLSCTX_LOCAL_SERVER = 4,
      CLSCTX_INPROC_SERVER16 = 8,
      CLSCTX_REMOTE_SERVER = 16, // 0x00000010
      CLSCTX_INPROC_HANDLER16 = 32, // 0x00000020
      CLSCTX_RESERVED1 = 64, // 0x00000040
      CLSCTX_RESERVED2 = 128, // 0x00000080
      CLSCTX_RESERVED3 = 256, // 0x00000100
      CLSCTX_RESERVED4 = 512, // 0x00000200
      CLSCTX_NO_CODE_DOWNLOAD = 1024, // 0x00000400
      CLSCTX_RESERVED5 = 2048, // 0x00000800
      CLSCTX_NO_CUSTOM_MARSHAL = 4096, // 0x00001000
      CLSCTX_ENABLE_CODE_DOWNLOAD = 8192, // 0x00002000
      CLSCTX_NO_FAILURE_LOG = 16384, // 0x00004000
      CLSCTX_DISABLE_AAA = 32768, // 0x00008000
      CLSCTX_ENABLE_AAA = 65536, // 0x00010000
      CLSCTX_FROM_DEFAULT_CONTEXT = 131072, // 0x00020000
      CLSCTX_INPROC = CLSCTX_INPROC_HANDLER | CLSCTX_INPROC_SERVER, // 0x00000003
      CLSCTX_SERVER = CLSCTX_REMOTE_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_INPROC_SERVER, // 0x00000015
      CLSCTX_ALL = CLSCTX_SERVER | CLSCTX_INPROC_HANDLER, // 0x00000017
    }
  }
}
