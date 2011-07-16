using IHI.Server.Plugins;
using IHI.Server.WebAdmin;

namespace IHI.Server.Plugins.Cecer1.PluginManager
{
    public class WEB_PluginManager : Plugin
    {
        Handlers fHandlers;

        public override void Start()
        {
            this.fHandlers = new Handlers(this);

            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/plugins", new HttpPathHandler(this.fHandlers.PAGE_Index));
            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/plugins/toggle", new HttpPathHandler(this.fHandlers.PAGE_Toggle));
        }

        public override void Stop()
        {
            CoreManager.GetCore().GetWebAdminManager().RemovePathHandler("/admin/plugins");
            CoreManager.GetCore().GetWebAdminManager().RemovePathHandler("/admin/plugins/toggle");

            this.fHandlers = null;
        }
    }
}