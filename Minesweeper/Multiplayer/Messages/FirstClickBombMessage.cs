using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer.Messages {
#nullable enable
    public class FirstClickBombMessage : NetworkMessage {
        public override void OnServer(SteamId id, byte[] data) { }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);
            byte x = reader.ReadByte();
            byte y = reader.ReadByte();

            byte bombx = reader.ReadByte();
            byte bomby = reader.ReadByte();

            Node? node = Minesweeper.getNodeAtPosition(x, y);
            if (node == null)
                return;
            node.thisType = NodeType.Empty;

            Node? bombnode = Minesweeper.getNodeAtPosition(bombx, bomby);
            if (bombnode == null)
                return;
            bombnode.thisType = NodeType.Bomb;

            Minesweeper.CalculateNodes();
        }
    }
}
