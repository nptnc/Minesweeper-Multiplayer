using Microsoft.Xna.Framework;
using Steamworks;
using System.Collections.Generic;

namespace Minesweeper.Multiplayer.Messages {
    public class TileMessage : NetworkMessage {
#nullable enable
        public override void OnServer(SteamId id, byte[] data) {

        }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);

            Dictionary<(int x, int y), Node> nodes = new();

            int nodeIndex = 0;
            for (int i = 0; i < Minesweeper.gridSize.X * Minesweeper.gridSize.Y; i++) {
                byte x = reader.ReadByte();
                byte y = reader.ReadByte();
                byte markedBy = reader.ReadByte();
                bool marked = false;
                PlayerId? id = null;
                if (markedBy != 255) {
                    marked = true;
                    id = PlayerIds.GetPlayerFromByteId(markedBy);
                }
                byte revealedBy = reader.ReadByte();
                bool revealed = false;
                PlayerId? id2 = null;
                if (revealedBy != 255) {
                    revealed = true;
                    id2 = PlayerIds.GetPlayerFromByteId(revealedBy);
                }
                byte tileType = reader.ReadByte();
                nodes.Add((x,y),new Node() { 
                    x = x,
                    y = y,
                    thisType = (NodeType)tileType,
                    revealed = revealed,
                    marked = marked,
                    markedBy = id,
                    clickedBy = id2,
                });
                nodeIndex++;
            }
            Minesweeper.instance.nodes = nodes;
            Minesweeper.instance.awaitingTileSync = false;

            Minesweeper.CalculateNodes();

            Minesweeper.Log("Synced tiles with server");
        }
    }
}
