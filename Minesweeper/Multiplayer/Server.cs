using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer {
    public class Server {
        public virtual void OnMessage(byte[] data) {

        }

        public virtual void Start() { }

        public virtual void Update() { }

        public virtual void Stop() { }

        public virtual void SendToClient(SteamId id, byte[] data, SendType sendType) { }
        public virtual void SendToClients(SteamId[] except, Type messageType, byte[] data, SendType sendType) { }
    }
}
