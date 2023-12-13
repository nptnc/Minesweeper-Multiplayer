using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Steamworks.Data;
using Minesweeper.Multiplayer.Messages;

namespace Minesweeper.Multiplayer {
    public static class NetworkHandler {
        public static Server currentServer = null;

#nullable enable
        public static SteamId? connectingTo = null;
        public static float untilConnect = 0;

        public class Client : ConnectionManager {
            public override void OnConnecting(ConnectionInfo info) {
                base.OnConnecting(info);
            }

            public override void OnConnected(ConnectionInfo info) {
                base.OnConnected(info);

                PlayerIds.ids = new();
                PlayerIds.globalSmallId = 0;

                if (currentServer == null) {
                    //Minesweeper.instance.awaitingTileSync = true;
                }

                Minesweeper.Log("client connected to server");

                NetworkWriter writer = new();
                writer.Write(SteamClient.Name);

                SendToServer(typeof(PlayerRegisterMessage), writer.Create(), SendType.Reliable);
            }

            public override void OnDisconnected(ConnectionInfo info) {
                PlayerIds.ids.Clear();
                if (cm != null && cm.Connected)
                    cm?.Close();
                cm = null;
                Minesweeper.instance.GenerateGame();
                base.OnDisconnected(info);
            }

            public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel) {
                byte[] byteData = new byte[size];
                Marshal.Copy(data, byteData, 0, size);

                byte messageId = byteData[0];
                byte[] actualData = new byte[byteData.Length - 1];

                for (int i = 1; i < byteData.Length; i++) {
                    actualData[i - 1] = byteData[i];
                }

                HandleMessage(messageId, actualData, false);
                base.OnMessage(data, size, messageNum, recvTime, channel);
            }
        }
        public static Client? cm = null;
#nullable disable

        public static bool connected {
            get { return cm != null && cm.Connected; }
            private set { }
        }

        public static void Shutdown() {
            SteamClient.Shutdown();
        }

        private static byte messageId = 0;
        private static List<NetworkMessage> messages = new();

        public static void Init() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            foreach (Type type in assembly.GetTypes()) {
                if (type.IsSubclassOf(typeof(NetworkMessage))) {
                    NetworkMessage message = Activator.CreateInstance(type) as NetworkMessage;
                    message.messageId = messageId;
                    messages.Add(message);
                    messageId++;
                }
            }

            bool errored = false;
            try {
                SteamClient.Init(480);
            }
            catch (Exception ex) {
                Minesweeper.instance.steamClientFucked = ex.ToString();
                errored = true;
            }
            if (errored)
                return;

            SteamFriends.OnGameLobbyJoinRequested += (Lobby lobby, SteamId steamid) => {
                currentServer?.Stop();
                currentServer = null;

                cm?.Close();
                cm = null;

                connectingTo = steamid;
                untilConnect = 1;
            };

            currentServer = new SteamworksSocket();
            currentServer.Start();

            cm = SteamNetworkingSockets.ConnectRelay<Client>(SteamClient.SteamId);
        }

        public static void Update(float deltaTime) {
            currentServer?.Update();
            cm?.Receive(256);
            untilConnect -= deltaTime;
            if (untilConnect <= 0 && connectingTo != null) {
                cm = SteamNetworkingSockets.ConnectRelay<Client>((SteamId)connectingTo);
                connectingTo = null;
            }
            SteamClient.RunCallbacks();
        }

        public static void SendToServer(Type messageType, byte[] data, SendType sendType) {
            if (connected == false) {
                return;
            }
            NetworkMessage message = messages.FirstOrDefault(message => message.GetType().FullName == messageType.FullName);
            if (currentServer != null) {
                HandleMessage(message.messageId,data,true,SteamClient.SteamId);
                return;
            }
            byte[] newData = new byte[data.Length+1];
            newData[0] = message.messageId;
            for (int i = 0; i < data.Length; i++) {
                newData[i+1] = data[i];
            }
            cm.Connection.SendMessage(newData, sendType);
        }

        public static void SendToClient(SteamId id, Type messageType, byte[] data, SendType sendType) {
            NetworkMessage message = messages.FirstOrDefault(message => message.GetType().FullName == messageType.FullName);
            if (id == SteamClient.SteamId) {
                HandleMessage(message.messageId, data, false);
                return;
            }
            byte[] newData = new byte[data.Length + 1];
            newData[0] = message.messageId;
            for (int i = 0; i < data.Length; i++) {
                newData[i + 1] = data[i];
            }
            currentServer.SendToClient(id, newData, sendType);
        }

        public static void SendToClients(SteamId[] except, Type messageType, byte[] data, SendType sendType) {
            currentServer.SendToClients(except, messageType, data, sendType);
        }

#nullable enable
        public static NetworkMessage? GetMessageFromId(byte id) {
            foreach (NetworkMessage message in messages) {
                if (message.messageId == id)
                    return message;
            }
            return null;
        }

        public static void HandleMessage(byte messageId, byte[] data, bool isServer, SteamId? by = null) {
            NetworkMessage? message = GetMessageFromId(messageId);
            if (message == null)
                return;
            if (isServer) {
                message.OnServer((SteamId)by!,data);
                return;
            }
            message.OnClient(data);
        }
    }
}
