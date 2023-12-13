using Microsoft.Xna.Framework;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Multiplayer.Messages {
    public class RevealTileMessage : NetworkMessage {
#nullable enable
        public override void OnServer(SteamId id, byte[] data) {
            if (Minesweeper.gameOver) {
                Minesweeper.Log("game over cant reveal node");
                return;
            }
            if (Minesweeper.won)
                return;
            NetworkReader reader = new(data);
            byte x = reader.ReadByte();
            byte y = reader.ReadByte();

            Node? node = Minesweeper.getNodeAtPosition(x, y);
            if (node == null)
                return;

            if (node.marked) {
                Minesweeper.Log("node is marked");
                return;
            }
            if (node.revealed)
                return;
            if (node.thisType == NodeType.Bomb && Minesweeper.instance.isFirstClick) {
                // this handles first click protection against bombs, without this desync occurs when a player first clicks a bomb
                node.thisType = NodeType.Empty;

                List<Node> emptyNodes = new();
                foreach (var pair in Minesweeper.instance.nodes) {
                    Node targetNode = pair.Value;
                    if (targetNode.thisType == NodeType.Bomb) {
                        continue;
                    }
                    emptyNodes.Add(targetNode);
                }

                if (emptyNodes.Count == 0)
                    return;

                Random random = new();
                Node replacementNode = emptyNodes[random.Next(0, emptyNodes.Count - 1)];
                replacementNode.thisType = NodeType.Bomb;
                Minesweeper.CalculateNodes();

                NetworkWriter protectionWriter = new();
                protectionWriter.Write(x);
                protectionWriter.Write(y);
                protectionWriter.Write((byte)replacementNode.x);
                protectionWriter.Write((byte)replacementNode.y);

                NetworkHandler.SendToClients(new SteamId[] { }, typeof(FirstClickBombMessage), protectionWriter.Create(), SendType.Reliable);
            }

            PlayerId playerId = PlayerIds.GetPlayerFromSteamId(id);

            Minesweeper.Log("sending node reveal to clients");

            NetworkWriter writer = new();
            writer.Write(playerId.smallId);
            writer.Write(x);
            writer.Write(y);

            NetworkHandler.SendToClients(new SteamId[] { },typeof(RevealTileMessage),writer.Create(),SendType.Reliable);
        }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);
            byte id = reader.ReadByte();
            byte x = reader.ReadByte();
            byte y = reader.ReadByte();

            PlayerId? playerId = PlayerIds.GetPlayerFromByteId(id);
            if (playerId == null) {
                return;
            }

            Minesweeper.Log("received reveal node");
            Node? node = Minesweeper.getNodeAtPosition(x, y);
            if (node == null)
                return;
            Minesweeper.RevealNode(node, playerId);
        }
    }
}
