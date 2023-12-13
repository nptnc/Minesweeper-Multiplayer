using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer.Messages {
    public class NetworkMessage {
        public byte messageId;

        public virtual void OnServer(SteamId sent, byte[] data) { }
        public virtual void OnClient(byte[] data) { }
    }
}
