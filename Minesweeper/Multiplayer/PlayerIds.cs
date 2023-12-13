using Microsoft.Xna.Framework;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer {
    public class PlayerId {
        public SteamId id;
        public Vector2 mousePosition;
        public string name;
        public byte smallId;
        public bool[] mouseDown = new bool[3];

        public PlayerId(SteamId id, byte smallId, string name) {
            this.id = id;
            this.smallId = smallId;
            this.name = name;
        }
    }

    public static class PlayerIds {
        public static List<PlayerId> ids = new();
        public static byte globalSmallId = 0;
        public static PlayerId myId;

        public static PlayerId RegisterPlayer(SteamId id, byte smallid, string name) {
            PlayerId id2 = new(id, smallid, name);
            ids.Add(id2);
            if (id == SteamClient.SteamId) {
                myId = id2;
            }
            return id2;
        }

        public static void DestroyPlayer(SteamId id) {
            foreach (PlayerId playerId in ids) {
                if (playerId.id == id) {
                    ids.Remove(playerId);
                    break;
                }
            }
        }

        public static PlayerId GetPlayerFromSteamId(SteamId id) {
            foreach (PlayerId playerId in ids) {
                if (playerId.id == id) {
                    return playerId;
                }
            }
            return null;
        }

        public static PlayerId? GetPlayerFromByteId(byte id) {
            foreach (PlayerId playerId in ids) {
                if (playerId.smallId == id) {
                    return playerId;
                }
            }
            return null;
        }
    }
}
