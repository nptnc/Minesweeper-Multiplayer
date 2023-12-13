using Steamworks;
using Steamworks.Data;

namespace Minesweeper.Multiplayer.Messages {
    public class PlayerDisconnectMessage : NetworkMessage {
        public override void OnServer(SteamId id, byte[] data) { }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);
            byte smallid = reader.ReadByte();

            PlayerId playerid = PlayerIds.GetPlayerFromByteId(smallid);
            if (playerid == null) {
                return;
            }
            PlayerIds.DestroyPlayer(playerid.id);
        }
    }
}
