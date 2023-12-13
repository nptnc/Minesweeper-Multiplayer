using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Minesweeper.Multiplayer;
using Minesweeper.Multiplayer.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper {
    public enum NodeType {
        Empty,
        Bomb,
    }

    public class Node {
        public bool revealed = false;
        public bool marked = false;
        public bool held = false;

        public Vector2Int? renderPosition;
        public Vector2Int? renderSize;

        public PlayerId? clickedBy = null;
        public PlayerId? markedBy = null;

        public List<Node> adjacentNodesDiagonal = new();
        public List<Node> adjacentNodesOrthogonal = new();
        public List<Node> adjacentNodes = new();

        public NodeType thisType = NodeType.Empty;
        public int x;
        public int y;
        public int bombCount;

        TextureData[] GenerateTextures(params (Texture2D,Color)[] textures) {
            TextureData[] textureDatas = new TextureData[textures.Length];
            int ind = 0;
            foreach (var pair in textures) {
                textureDatas[ind] = new TextureData() { color = pair.Item2, texture = pair.Item1 };
                ind++;
            }
            return textureDatas;
        }

        public TextureData[] texture {
            get {
                TextureData[] newTexture = new TextureData[0];
                bool isBomb = thisType == NodeType.Bomb;
                if (marked && Minesweeper.gameOver && thisType == NodeType.Empty)
                    return GenerateTextures((Minesweeper.tile_idle, Color.White), (Minesweeper.mine, Color.White), (Minesweeper.bomb_x, Cursors.GetPlayerColor(markedBy)));
                if (marked)
                    return GenerateTextures((Minesweeper.tile_idle, Color.White),(Minesweeper.flag,Cursors.GetPlayerColor(markedBy)));
                if (!revealed && isBomb && Minesweeper.gameOver)
                    return GenerateTextures((Minesweeper.tile_held, Color.White), (Minesweeper.mine, Color.White));
                if (held && !revealed)
                    return GenerateTextures((Minesweeper.tile_held, Color.White));
                if (!revealed)
                    return GenerateTextures((Minesweeper.tile_idle, Color.White));
                if (bombCount > 0 && revealed && !isBomb)
                    return GenerateTextures((Minesweeper.tile_held, Color.White), (Minesweeper.tile_revealed[bombCount-1],Color.White));
                if (bombCount <= 0 && revealed && !isBomb)
                    return GenerateTextures((Minesweeper.tile_held, Color.White));
                if (revealed && isBomb) {
                    return GenerateTextures((Minesweeper.tile_mine_clicked, Cursors.GetPlayerColor(clickedBy)),(Minesweeper.mine, Color.White));
                }
                return newTexture;
            }
            private set { }
        }
    }
}
