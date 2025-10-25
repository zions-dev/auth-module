using GameServer.Extensions;
using GTANetworkAPI;

namespace GameServer.Modules.Notify
{
    public static class NotifyModule
    {
        public static void SendNotify(this Player player, string message, NotifyType type = NotifyType.Info, int duration = 4000)
        {
            player.SafeTriggerEvent("Notify:Show", message, type.ToString().ToLower(), duration);
        }
    }
}