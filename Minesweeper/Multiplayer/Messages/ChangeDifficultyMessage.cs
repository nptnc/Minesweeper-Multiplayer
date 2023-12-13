using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer.Messages {
    public class ChangeDifficultyMessage : NetworkMessage {
        public override void OnServer(SteamId id, byte[] data) {
            NetworkReader reader = new(data);
            short bombs = reader.ReadShort();
            short x = reader.ReadShort();
            short y = reader.ReadShort();

            x = (short)Math.Clamp((int)x, 1, Minesweeper.CalculateMaxTile().x);
            y = (short)Math.Clamp((int)y, 1, Minesweeper.CalculateMaxTile().y);

            Minesweeper.ApplyDifficulty(new MinesweeperDifficulty(x, y, (uint)bombs, "networkedDifficulty"));

            Minesweeper.instance.GenerateGame();

            NetworkWriter writer = new();
            writer.Write(bombs);
            writer.Write(x);
            writer.Write(y);

            NetworkHandler.SendToClients(new SteamId[] { SteamClient.SteamId }, typeof(ChangeDifficultyMessage), writer.Create(), Steamworks.Data.SendType.Reliable);

            NetworkWriter worldWriter = Minesweeper.instance.NodeSyncData();
            NetworkHandler.SendToClients(new SteamId[] { SteamClient.SteamId }, typeof(TileMessage), worldWriter.Create(), Steamworks.Data.SendType.Reliable);
        }

        public override void OnClient(byte[] data) {
            NetworkReader reader = new(data);
            short bombs = reader.ReadShort();
            short x = reader.ReadShort();
            short y = reader.ReadShort();

            Minesweeper.Log($"changed difficulty {bombs} {x} {y}");
            Minesweeper.ApplyDifficulty(new MinesweeperDifficulty(x,y,(uint)bombs,"networkedDifficulty"));
            Minesweeper.ResetGameState();
        }
    }
}
