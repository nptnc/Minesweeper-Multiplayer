using Microsoft.Xna.Framework;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Multiplayer.Messages {
    public class ChordTileMessage : NetworkMessage {
#nullable enable
        public override void OnServer(SteamId id, byte[] data) {
            if (Minesweeper.gameOver) {
                Minesweeper.Log("game over cant chord node");
                return;
            }
            if (Minesweeper.won) {
                Minesweeper.Log("cant chord, won");
                return;
            }
            NetworkReader reader = new(data);
            byte x = reader.ReadByte();
            byte y = reader.ReadByte();

            Node? node = Minesweeper.getNodeAtPosition(x, y);
            if (node == null) {
                Minesweeper.Log("cant chord, node null");
                return;
            }

            if (node.marked) {
                Minesweeper.Log("node is marked");
                return;
            }
            if (node.revealed == false) {
                Minesweeper.Log("node revealed, cannot chord");
                return;
            }

            int howManyMarked = 0;
            foreach (Node adjacentNode in node.adjacentNodes) {
                if (adjacentNode.marked)
                    howManyMarked++;
            }
            if (node.bombCount != howManyMarked) {
                Minesweeper.Log($"cant chord, bomb count is {node.bombCount} and marked count is {howManyMarked}");
                return;
            }

            PlayerId playerId = PlayerIds.GetPlayerFromSteamId(id);
            Minesweeper.Log("sending node chord to clients");

            NetworkWriter writer = new();
            writer.Write(playerId.smallId);
            writer.Write(x);
            writer.Write(y);

            NetworkHandler.SendToClients(new SteamId[] { }, typeof(ChordTileMessage), writer.Create(), SendType.Reliable);
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

            Minesweeper.Log("received chord node");
            Node? node = Minesweeper.getNodeAtPosition(x, y);
            if (node == null)
                return;
            foreach (Node adjacentNode in node.adjacentNodes) {
                if (adjacentNode.marked)
                    continue;
                if (adjacentNode.revealed)
                    continue;
                if (Minesweeper.gameOver)
                    return;
                Minesweeper.RevealNode(adjacentNode, playerId);
            }
        }
    }
}
