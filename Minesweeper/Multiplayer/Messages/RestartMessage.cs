using Microsoft.Xna.Framework;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.Multiplayer.Messages {
    public class RestartMessage : NetworkMessage {
        public override void OnServer(SteamId id, byte[] data) {
            Minesweeper.instance.GenerateGame();

            NetworkHandler.SendToClients(new SteamId[] { SteamClient.SteamId },typeof(RestartMessage),new byte[0],SendType.Reliable);

            NetworkWriter worldWriter = Minesweeper.instance.NodeSyncData();
            NetworkHandler.SendToClients(new SteamId[] { SteamClient.SteamId }, typeof(TileMessage), worldWriter.Create(), SendType.Reliable);
        }

        public override void OnClient(byte[] data) {
            Minesweeper.ResetGameState();
        }
    }
}
