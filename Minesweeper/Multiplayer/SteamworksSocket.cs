using Minesweeper.Multiplayer.Messages;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.InteropServices;

namespace Minesweeper.Multiplayer {
    public class SteamworksSocket : Server {
        public ActualSocket socket;
        public static Dictionary<SteamId, Connection> connections = new Dictionary<SteamId, Connection>();

        public static int connectionsCount;

        public class ActualSocket : SocketManager {
            public override void OnConnecting(Connection connection, ConnectionInfo info) {
                base.OnConnecting(connection, info);
            }

            public override void OnConnected(Connection connection, ConnectionInfo info) {
                base.OnConnected(connection, info);
                if (connectionsCount == 0) {
                    connections.Add(SteamClient.SteamId, connection);
                }
                connectionsCount += 1;
            }

            public override void OnDisconnected(Connection connection, ConnectionInfo info) {
                var pair = connections.FirstOrDefault(a => a.Value == connection);
                PlayerId playerid = PlayerIds.GetPlayerFromSteamId(pair.Key);
                if (playerid == null) {
                    return;
                }

                NetworkWriter writer = new();
                writer.Write(playerid.smallId);

                NetworkHandler.SendToClients(new SteamId[] { playerid.id }, typeof(PlayerDisconnectMessage), writer.Create(), SendType.Reliable);
                connections.Remove(pair.Key);

                base.OnDisconnected(connection, info);
            }

            public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel) {
                if (!connections.ContainsValue(connection)) {
                    connections.Add(identity.SteamId, connection);
                    Minesweeper.Log("a client steam id was caught with their connection");
                }

                byte[] byteData = new byte[size];
                Marshal.Copy(data, byteData, 0, size);

                byte messageId = byteData[0];
                byte[] actualData = new byte[byteData.Length - 1];

                for (int i = 1; i < byteData.Length; i++) {
                    actualData[i-1] = byteData[i];
                }

                NetworkHandler.HandleMessage(messageId, actualData, true, identity.SteamId);
                base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
            }
        }

        public override void Stop() {
            foreach (var pair in connections) {
                pair.Value.Close();
            }
            socket.Close();
            socket = null;
            connections = new();
        }

        public override async void Start() {
            socket = SteamNetworkingSockets.CreateRelaySocket<ActualSocket>();

            Lobby? lobby = await SteamMatchmaking.CreateLobbyAsync(8);
            lobby.Value.SetPublic();
        }

        public override void SendToClient(SteamId id, byte[] data, SendType sendType) {
            connections.FirstOrDefault(a => a.Key == id).Value.SendMessage(data, sendType);
        }

        public override void SendToClients(SteamId[] except, Type messageType, byte[] data, SendType sendType) {
            foreach (var pair in connections) {
                if (except.Contains(pair.Key)) {
                    continue;
                }
                NetworkHandler.SendToClient(pair.Key, messageType, data, sendType);
            }
        }

        public override void Update() {
            socket.Receive(256);
        }
    }
}
