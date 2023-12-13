using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer.Messages {
    public class PlayerRegisterMessage : NetworkMessage {
        public override void OnServer(SteamId id, byte[] data) {
            NetworkReader reader = new(data);
            string name = reader.ReadString();

            byte smallId = PlayerIds.globalSmallId;
            PlayerIds.globalSmallId++;

            NetworkWriter writer = new();
            writer.Write(id);
            writer.Write(smallId);
            writer.Write(name);

            PlayerIds.RegisterPlayer(id, smallId, name);

            Minesweeper.Log($"Registered player\nsmallId: {smallId}\nid: {id}\nname: {name}");

            int nameLimit = 15;
            if (name.Length > nameLimit) {
                name = name.Substring(0, nameLimit);
            }

            /// <summary>
            /// send this player to every player other than the host and them
            /// </summary>
            foreach (PlayerId playerid in PlayerIds.ids) {
                if (playerid.id == SteamClient.SteamId || playerid.id == id) // we skip the host and themselves so they dont duplicate!
                    continue;
                NetworkHandler.SendToClient(playerid.id, typeof(PlayerRegisterMessage), writer.Create(), Steamworks.Data.SendType.Reliable);
            }

            /// <summary>
            /// send every player to this player
            /// </summary>
            if (id != SteamClient.SteamId) {
                foreach (PlayerId playerid in PlayerIds.ids) {
                    NetworkWriter writer1 = new();
                    writer1.Write(playerid.id);
                    writer1.Write(playerid.smallId);
                    writer1.Write(playerid.name);

                    NetworkHandler.SendToClient(id, typeof(PlayerRegisterMessage), writer1.Create(), Steamworks.Data.SendType.Reliable);
                }
            }

            if (id != SteamClient.SteamId) {
                NetworkWriter difficultyWriter = new();
                difficultyWriter.Write((short)Minesweeper.difficulty.bombCount);
                difficultyWriter.Write((short)Minesweeper.difficulty.x);
                difficultyWriter.Write((short)Minesweeper.difficulty.y);

                NetworkHandler.SendToClient(id, typeof(ChangeDifficultyMessage), difficultyWriter.Create(), Steamworks.Data.SendType.Reliable);

                NetworkWriter worldWriter = Minesweeper.instance.NodeSyncData();
                NetworkHandler.SendToClient(id, typeof(TileMessage), worldWriter.Create(), Steamworks.Data.SendType.Reliable);
            }
        }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);
            SteamId id = reader.ReadUlong();
            byte smallid = reader.ReadByte();
            string name = reader.ReadString();

            Minesweeper.Log($"Registered player\nsmallId: {smallid}\nid: {id}\nname: {name}");

            PlayerIds.RegisterPlayer(id,smallid,name);
        }
    }
}
