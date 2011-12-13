using IHI.Server.Plugins;
using IHI.Server.WebAdmin;

namespace IHI.Server.Plugins.Cecer1.PacketDebugger
{
    public class WEB_PacketDebugger : Plugin
    {
        Handlers fHandlers;

        public override void Start()
        {
            this.fHandlers = new Handlers(this);

            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/packet", new HttpPathHandler(this.fHandlers.PAGE_List));
            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/packet/add", new HttpPathHandler(this.fHandlers.PAGE_Add));
            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/packet/remove", new HttpPathHandler(this.fHandlers.PAGE_Remove));
            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/packet/send", new HttpPathHandler(this.fHandlers.PAGE_Send));
            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/packet/addform", new HttpPathHandler(this.fHandlers.PAGE_AddForm));
            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/packet/sendform", new HttpPathHandler(this.fHandlers.PAGE_SendForm));
            CoreManager.GetCore().GetWebAdminManager().AddPathHandler("/admin/packet/logs", new HttpPathHandler(this.fHandlers.PAGE_Logs));
        }
    }
}