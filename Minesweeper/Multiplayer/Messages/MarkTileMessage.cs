using Microsoft.Xna.Framework;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Multiplayer.Messages {
    public class MarkTileMessage : NetworkMessage {
#nullable enable
        public override void OnServer(SteamId id, byte[] data) {
            if (Minesweeper.gameOver)
                return;
            if (Minesweeper.won)
                return;
            NetworkReader reader = new(data);
            byte x = reader.ReadByte();
            byte y = reader.ReadByte();

            PlayerId playerId = PlayerIds.GetPlayerFromSteamId(id);

            NetworkWriter writer = new();
            writer.Write(playerId.smallId);
            writer.Write(x);
            writer.Write(y);

            Node? node = Minesweeper.getNodeAtPosition(x, y);
            if (node == null)
                return;
            if (node.revealed)
                return;
            node.marked = !node.marked;
            writer.Write(node.marked);

            NetworkHandler.SendToClients(new SteamId[] { },typeof(MarkTileMessage),writer.Create(),SendType.Reliable);
        }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);
            byte id = reader.ReadByte();
            byte x = reader.ReadByte();
            byte y = reader.ReadByte();
            bool marked = reader.ReadBool();

            PlayerId? playerId = PlayerIds.GetPlayerFromByteId(id);
            if (playerId == null)
                return;

            Node? node = Minesweeper.getNodeAtPosition(x, y);
            if (node == null)
                return;
            node.marked = marked;
            node.markedBy = playerId;
            Minesweeper.CalculateWon();
        }
    }
}
