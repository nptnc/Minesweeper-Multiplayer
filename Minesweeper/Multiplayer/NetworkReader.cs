using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer {
    public class NetworkReader {
        private byte[] bytes;
        private int offset;

        public NetworkReader(byte[] bytes) {
            this.bytes = bytes;
        }

        public float ReadFloat() {
            float converted = BitConverter.ToSingle(bytes, offset);
            offset += sizeof(float);
            return converted;
        }

        public ulong ReadUlong() {
            ulong converted = BitConverter.ToUInt64(bytes, offset);
            offset += sizeof(ulong);
            return converted;
        }

        public string ReadString() {
            byte[] newBytes = new byte[bytes.Length - offset];
            for (int i = offset; i < bytes.Length; i++) {
                newBytes[i - offset] = bytes[i];
            }
            string str = Encoding.UTF8.GetString(newBytes);
            offset = bytes.Length;
            return str;
        }

        public bool ReadBool() {
            bool yesOrNo = BitConverter.ToBoolean(bytes, offset);
            offset += sizeof(bool);
            return yesOrNo;
        }

        public int ReadInt() {
            int targetInt = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            return targetInt;
        }

        public short ReadShort() {
            short targetShort = BitConverter.ToInt16(bytes,offset);
            offset += sizeof(short);
            return targetShort;
        }

        public byte ReadByte() {
            byte b = bytes[offset];
            offset++;
            return b;
        }

        public Vector3 ReadVector3() {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }
    }

}
