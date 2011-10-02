using System.Collections.Generic;
using System.Net;
using System.Text;
using IHI.Server.Habbos;
using IHI.Server.WebAdmin;
using IHI.Server.Networking;
using IHI.Server.Networking.Messages;
using Ion.Specialized.Encoding;

namespace IHI.Server.Plugins.Cecer1.PacketDebugger
{
    internal class Handlers
    {
        Dictionary<uint, PacketHandler> fHandlers;
        StringBuilder fPacketLogs;
        const string HeaderSelect = "<input name='header' type='textbox'>"; //"<select><option value='0'>0 - @@</option><option value='1'>1 - @A</option><option value='2'>2 - @B</option></select>";

        Plugin fOwner;
        internal Handlers(Plugin Owner)
        {
            this.fOwner = Owner;
            this.fHandlers = new Dictionary<uint, PacketHandler>();
            this.fPacketLogs = new StringBuilder();

            CoreManager.GetCore().GetConnectionManager().OnConnectionOpen += new ConnectionEventHandler(RegisterHandlersToNew);
        }

        internal void PAGE_List(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                StringBuilder Contents = new StringBuilder("<a href='packet/addform'>Add Handler</a><br><a href='packet/sendform'>Send Packet</a><br><a href='packet/logs'>View Logs</a><br><br>");

                lock (this.fHandlers)
                {
                    foreach (uint HeaderID in this.fHandlers.Keys)
                    {
                        Contents.Append("<a href='remove?header=").Append(HeaderID).Append('>').Append(HeaderID).Append(" (").Append(Encoding.UTF8.GetString(Base64Encoding.EncodeuUInt32(HeaderID, 2))).Append(")</a><br>");
                    }
                }
                CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), Contents.ToString());
            }
        }
        internal void PAGE_AddForm(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), "<form method='GET' action='add'><b>Header: </b>" + HeaderSelect + "<br><input type='submit' value='Go'></form>");
            }
        }
        internal void PAGE_Add(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                string Header = Context.Request.QueryString["header"];

                uint HeaderID = 0;

                if (!uint.TryParse(Header, out HeaderID))
                    HeaderID = Base64Encoding.DecodeUInt32(Encoding.UTF8.GetBytes(Header));

             
                PacketHandler Handler = new PacketHandler(Process_DYNAMIC);
                lock(this.fHandlers)
                    fHandlers.Add(HeaderID, Handler);
                RegisterHandlersToExisting(HeaderID, Handler);

                Context.Response.Redirect("addform");
                Context.Response.Close();
            }
        }
        internal void PAGE_Remove(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                string Header = Context.Request.QueryString["header"];

                uint HeaderID = 0;

                if (!uint.TryParse(Header, out HeaderID))
                    HeaderID = Base64Encoding.DecodeUInt32(Encoding.UTF8.GetBytes(Header));

                UnregisterHandlers(HeaderID, this.fHandlers[HeaderID]);

                lock (this.fHandlers)
                    this.fHandlers.Remove(HeaderID);
            }
        }
        internal void PAGE_SendForm(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                StringBuilder Contents = new StringBuilder();

                Contents.Append("<form method='GET' action='send'><b>Connection: </b><select name='connection'>");

                foreach(IonTcpConnection Connection in CoreManager.GetCore().GetConnectionManager().GetAllConnections())
                {
                    Contents.
                        Append("<option value='").
                        Append(Connection.GetID()).
                        Append("'>").
                        Append(Connection.GetID()).
                        Append(" [").
                        Append(Connection.GetIPAddressString()).
                        Append("] ");

                    Habbo ConnectionHabbo = Connection.GetHabbo();

                    if(ConnectionHabbo != null && ConnectionHabbo.IsLoggedIn())
                        Contents.Append(ConnectionHabbo.GetUsername());
                    else
                        Contents.Append("UNKNOWN");

                    Contents.Append("</option>");
                }

                Contents.Append("</select><br><b>Header: </b>").Append(HeaderSelect).Append("<br><b>Data: </b><textarea name='data'></textarea><br><input type='submit' value='Go'></form>");

                CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), Contents.ToString());
            }
        }
        internal void PAGE_Send(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                string Header = Context.Request.QueryString["header"];
                string Data = Context.Request.QueryString["data"];
                string SConnectionID = Context.Request.QueryString["connection"];

                uint HeaderID = 0;
                uint ConnectionID = 0;

                if (!uint.TryParse(Header, out HeaderID))
                    goto Done;
                if (!uint.TryParse(SConnectionID, out ConnectionID))
                    goto Done;

                Habbo Habbo = CoreManager.GetCore().GetConnectionManager().GetConnection(ConnectionID).GetHabbo();

                InternalOutgoingMessage Message = new InternalOutgoingMessage(HeaderID);
                Message.Append(Data);
                Habbo.SendMessage(Message);

            Done:
                Context.Response.Redirect("sendform");
                Context.Response.Close();
            }
        }
        internal void PAGE_Logs(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                StringBuilder Contents = new StringBuilder("<html><head><style>.packet{border: 1px solid #000; margin: 5px; padding: 5px; background-color: #DDD;}</style></head><body>");

                lock (this.fPacketLogs)
                {
                    Contents.Append(this.fPacketLogs.ToString());
                    this.fPacketLogs.Clear();
                }

                Contents.Append("</body></html>");
                CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), Contents.ToString());
            }
        }

        private void Process_DYNAMIC(Habbo Sender, IncomingMessage Message)
        {
            lock (this.fPacketLogs)
            {
                this.fPacketLogs.
                    Append("<div class='packet'>").
                    Append('[').
                    Append(Message.GetID()).
                    Append("] <u>").
                    Append(Message.GetHeader()).
                    Append("</u>").
                    Append(Message.GetContentString()).
                    Append("</div>");
            }
        }

        private void RegisterHandlersToNew(object source, ConnectionEventArgs args)
        {
            IonTcpConnection Connection = (source as IonTcpConnection);

            lock (this.fHandlers)
            {
                foreach (KeyValuePair<uint, PacketHandler> Handler in this.fHandlers)
                {
                    Connection.AddHandler(Handler.Key, PacketHandlerPriority.High, Handler.Value);
                }
            }
        }
        private void RegisterHandlersToExisting(uint HeaderID, PacketHandler PacketHandler)
        {
            foreach (IonTcpConnection Connection in CoreManager.GetCore().GetConnectionManager().GetAllConnections())
            {
                Connection.AddHandler(HeaderID, PacketHandlerPriority.High, PacketHandler);
            }
        }
        private void UnregisterHandlers(uint HeaderID, PacketHandler PacketHandler)
        {
            foreach (IonTcpConnection Connection in CoreManager.GetCore().GetConnectionManager().GetAllConnections())
            {
                Connection.RemoveHandler(HeaderID, PacketHandlerPriority.High, PacketHandler);
            }
        }
    }
}