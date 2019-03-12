// Decompiled with JetBrains decompiler
// Type: AlohaPOSAgents.Main
// Assembly: AlohaPOSAgents, Version=1.3.6.0, Culture=neutral, PublicKeyToken=null
// MVID: FF71D34C-5A65-400F-9BB4-3C379734484C
// Assembly location: D:\projects\tabsquare\POS-program\AlohaAgent\AlohaPOSAgents.exe

using log4net;
using log4net.Config;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace AlohaPOSAgents
{
  public class Main : Form
  {
    private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    public static bool is_debug = ConfigurationManager.AppSettings["DEBUG"] == "2606";
    private static readonly ASCIIEncoding EncodingAscii = new ASCIIEncoding();
    private static string _receiveMessage = string.Empty;
    private static bool _connect = true;
    private readonly SocketHelper _helper = new SocketHelper();
    private static SocketPermission _permission;
    private static Socket _sListener;
    private static IPEndPoint _ipEndPoint;
    private static Socket _listener;
    private IContainer components;
    private Label label1;
    private Label label3;
    private Label lblIberFunc;

    public Main()
    {
      XmlConfigurator.Configure();
      this.InitializeComponent();
      this.InitServerSocket(ConfigurationSettings.AppSettings["IP"], ConfigurationSettings.AppSettings["PORT"]);
    }

    private void Experiment()
    {
    }

    private void InitServerSocket(string ipAddress, string port)
    {
      try
      {
        Main._permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, "", -1);
        Main._sListener = (Socket) null;
        Main._permission.Demand();
        Main._ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), Convert.ToInt32(port));
        Main._sListener = new Socket(IPAddress.Parse(ipAddress).AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        Main._sListener.Bind((EndPoint) Main._ipEndPoint);
        Main._sListener.Listen(1024);
        Main._connect = true;
        AsyncCallback callback = new AsyncCallback(Main.AcceptCallback);
        Main._sListener.BeginAccept(callback, (object) Main._sListener);
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("Exception in InitServerSocket(): " + ex.Message);
      }
    }

    private static void AcceptCallback(IAsyncResult ar)
    {
      try
      {
        if (!Main._connect)
          return;
        byte[] buffer = new byte[1048576];
        Main._listener = (Socket) ar.AsyncState;
        Socket socket = Main._listener.EndAccept(ar);
        socket.NoDelay = false;
        object[] objArray = new object[2]
        {
          (object) buffer,
          (object) socket
        };
        socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Main.ReceiveCallback), (object) objArray);
        AsyncCallback callback = new AsyncCallback(Main.AcceptCallback);
        Main._listener.BeginAccept(callback, (object) Main._listener);
      }
      catch (Exception ex)
      {
        Main.Logger.InfoFormat("Error in AcceptCallBack(): " + ex.Message);
        int num = (int) MessageBox.Show("Error in AcceptCallBack(): " + ex.Message);
      }
    }

    public static void ReceiveCallback(IAsyncResult ar)
    {
      if (Main.is_debug)
        Main.Logger.InfoFormat("Begin receiving callback ReceiveCallback()");
      object[] asyncState = (object[]) ar.AsyncState;
      byte[] bytes = (byte[]) asyncState[0];
      Socket socket = (Socket) asyncState[1];
      int count = socket.EndReceive(ar);
      if (count <= 0)
        return;
      Main._receiveMessage = Main.EncodingAscii.GetString(bytes, 0, count);
      SocketHelper socketHelper = new SocketHelper();
      if (Main.is_debug)
        Main.Logger.InfoFormat("Finish initing SocketHelper");
      string s = socketHelper.ProcessMsg(Main._receiveMessage);
      socket.Send(Main.EncodingAscii.GetBytes(s));
      socket.Shutdown(SocketShutdown.Both);
      socket.Close();
      if (!Main.is_debug)
        return;
      Main.Logger.InfoFormat("Finish receiving callback ReceiveCallback() ");
    }

    private void MainFormClosed(object sender, FormClosedEventArgs e)
    {
    }

    private void label3_Click(object sender, EventArgs e)
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.label1 = new Label();
      this.label3 = new Label();
      this.lblIberFunc = new Label();
      this.SuspendLayout();
      this.label1.AutoSize = true;
      this.label1.Font = new Font("Microsoft Sans Serif", 15f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label1.Location = new Point(12, 9);
      this.label1.Name = "label1";
      this.label1.Size = new Size(554, 25);
      this.label1.TabIndex = 0;
      this.label1.Text = "PLEASE DO NOT CLOSE THIS  WINDOW. MINIMIZE ONLY";
      this.label3.AutoSize = true;
      this.label3.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.label3.Location = new Point(14, 85);
      this.label3.Name = "label3";
      this.label3.Size = new Size(182, 13);
      this.label3.TabIndex = 3;
      this.label3.Text = "ERROR LOGS in ~\\AlohaAgent\\Log";
      this.label3.Click += new EventHandler(this.label3_Click);
      this.lblIberFunc.AutoSize = true;
      this.lblIberFunc.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Underline, GraphicsUnit.Point, (byte) 0);
      this.lblIberFunc.Location = new Point(13, 45);
      this.lblIberFunc.Name = "lblIberFunc";
      this.lblIberFunc.Size = new Size(0, 20);
      this.lblIberFunc.TabIndex = 4;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(762, 106);
      this.Controls.Add((Control) this.lblIberFunc);
      this.Controls.Add((Control) this.label3);
      this.Controls.Add((Control) this.label1);
      this.Name = nameof (Main);
      this.Text = nameof (Main);
      this.FormClosed += new FormClosedEventHandler(this.MainFormClosed);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
