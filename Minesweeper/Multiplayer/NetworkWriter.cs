using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer {
    public class NetworkWriter {
        public List<byte> bytes = new();

        public void Write(float f) {
            bytes.AddRange(BitConverter.GetBytes(f));
        }

        public void Write(int i) {
            bytes.AddRange(BitConverter.GetBytes(i));
        }

        public void Write(short s) {
            bytes.AddRange(BitConverter.GetBytes(s));
        }

        public void Write(Vector3 v3) {
            Write(v3.X);
            Write(v3.Y);
            Write(v3.Z);
        }

        public void Write(ulong u) {
            bytes.AddRange(BitConverter.GetBytes(u));
        }

        public void Write(string s) {
            bytes.AddRange(Encoding.UTF8.GetBytes(s));
        }

        public void Write(bool b) {
            bytes.AddRange(BitConverter.GetBytes(b));
        }

        public void Write(byte b) {
            bytes.Add(b);
        }

        public byte[] Create() {
            return bytes.ToArray();
        }
    }
}
