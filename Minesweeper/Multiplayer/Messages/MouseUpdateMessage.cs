using Microsoft.Xna.Framework;
using Steamworks;
using System;

namespace Minesweeper.Multiplayer.Messages {
    public class MouseUpdateMessage : NetworkMessage {
        public override void OnServer(SteamId id, byte[] data) {
            PlayerId playerId = PlayerIds.GetPlayerFromSteamId(id);
            if (PlayerIds.GetPlayerFromSteamId(id) == null)
                return;

            NetworkReader reader = new(data);
            float x = reader.ReadFloat();
            float y = reader.ReadFloat();
            x = Math.Clamp(x, 0, 1);
            y = Math.Clamp(y, 0, 1);

            NetworkWriter writer = new();
            writer.Write(playerId.smallId);
            writer.Write(x);
            writer.Write(y);

            NetworkHandler.SendToClients(new SteamId[] { id }, typeof(MouseUpdateMessage), writer.Create(), Steamworks.Data.SendType.Unreliable);
        }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);
            byte id = reader.ReadByte();
            float x = reader.ReadFloat();
            float y = reader.ReadFloat();

            PlayerId playerid = PlayerIds.GetPlayerFromByteId(id);
            if (playerid == null) {
                Minesweeper.instance.steamClientFucked = $"player with the small id of {id} is null";
                return;
            }

            playerid.mousePosition = new Vector2(x,y);
        }
    }
}
