using System.Net;
using IHI.Server.WebAdmin;

namespace IHI.Server.Plugins.Cecer1.PluginManager
{
    internal class Handlers
    {
        Plugin fOwner;
        internal Handlers(Plugin Owner)
        {
            this.fOwner = Owner;
        }

        internal void PAGE_Index(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), GetPluginList());
            }
        }
        internal void PAGE_Toggle(HttpListenerContext Context)
        {
            if (Context.Request.IsLocal)
            {
                string Name = Context.Request.QueryString["name"];

                Plugin PluginFromName = CoreManager.GetCore().GetPluginManager().GetPlugin(Name);

                GetPluginList();

                if (PluginFromName == null)
                {
                    CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), "No such plugin (" + Name + ")<br><br>" + GetPluginList());
                    return;
                }
                if (PluginFromName.IsRunning())
                {
                    if (Name == this.fOwner.GetName())
                    {
                        CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), Name + " refuses to suicide! <br><br>" + GetPluginList());
                        return;
                    }

                    CoreManager.GetCore().GetPluginManager().StopPlugin(PluginFromName);
                    CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), Name + " stopped<br><br>" + GetPluginList());
                }
                else
                {
                    CoreManager.GetCore().GetPluginManager().StartPlugin(PluginFromName);
                    CoreManager.GetCore().GetWebAdminManager().SendResponse(Context.Response, this.fOwner.GetName(), Name + " started<br><br>" + GetPluginList());
                }
            }
        }

        private string GetPluginList()
        {
            string PluginList = "<h1>Currently Loaded Plugins</h2><br>";

            foreach (Plugin P in CoreManager.GetCore().GetPluginManager().GetLoadedPlugins())
            {
                if (P.GetName() == this.fOwner.GetName())
                    PluginList += "<a style='color: gray;' href='/admin/plugins/toggle?name=" + P.GetName() + "'><b>" + P.GetName() + "</b><br></a>";
                else if (P.IsRunning())
                    PluginList += "<a style='color: green;' href='/admin/plugins/toggle?name=" + P.GetName() + "'><b>" + P.GetName() + "</b><br></a>";
                else
                    PluginList += "<a style='color: red;' href='/admin/plugins/toggle?name=" + P.GetName() + "'><i>" + P.GetName() + "</i><br></a>";
            }
            return PluginList;
        }
    }
}
