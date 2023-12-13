using Microsoft.Xna.Framework;
using Steamworks;
using System;

namespace Minesweeper.Multiplayer.Messages {
    public class MouseDownMessage : NetworkMessage {
        public override void OnServer(SteamId id, byte[] data) {
            PlayerId playerId = PlayerIds.GetPlayerFromSteamId(id);
            if (PlayerIds.GetPlayerFromSteamId(id) == null)
                return;

            NetworkReader reader = new(data);
            byte mouseIndex = reader.ReadByte();
            if (mouseIndex > 2)
                return;
            bool isDown = reader.ReadBool();

            NetworkWriter writer = new();
            writer.Write(playerId.smallId);
            writer.Write(mouseIndex);
            writer.Write(isDown);

            NetworkHandler.SendToClients(new SteamId[] { id }, typeof(MouseDownMessage), writer.Create(), Steamworks.Data.SendType.Reliable);
        }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);
            byte id = reader.ReadByte();
            byte mouseIndex = reader.ReadByte();
            bool isDown = reader.ReadBool();
            if (mouseIndex > 2)
                return;

            PlayerId playerid = PlayerIds.GetPlayerFromByteId(id);
            if (playerid == null) {
                return;
            }

            playerid.mouseDown[mouseIndex] = isDown;
        }
    }
}
