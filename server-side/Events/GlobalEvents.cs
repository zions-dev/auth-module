using System.Collections.Generic;
using GameServer.Constants;
using GameServer.Services;
using GameServer.Utils;
using GTANetworkAPI;

namespace GameServer.Events
{
    public class GlobalEvents : Script
    {
        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            NAPI.Server.SetAutoSpawnOnConnect(false);
            NAPI.Server.SetAutoRespawnAfterDeath(false);

            Logger.LogInfo("Сервер запущен (ResourceStart)");
        }

        [ServerEvent(Event.PlayerConnected)]
        public void OnPlayerConnected(Player player)
        {
            NAPI.Entity.SetEntityTransparency(player, 0);

            player.Dimension = (uint)Dimensions.Auth + player.Id;

            player.TriggerEvent("Auth:ShowLogin");

            Logger.LogInfo($"Игрок подключился: {player.SocialClubName}, {player.SocialClubId}");
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnected(Player player, DisconnectionType type, string reason)
        {
            Logger.LogInfo($"Игрок отключился: {player.Name}, причина: {reason}");
        }
    }
}