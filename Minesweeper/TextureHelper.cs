using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper {
    public static class TextureHelper {
        public static Texture2D SubTexture(this Texture2D src, GraphicsDevice graphics, Rectangle rect) {
            Texture2D tex = new Texture2D(graphics, rect.Width, rect.Height);
            int count = rect.Width * rect.Height;
            Color[] data = new Color[count];
            src.GetData(0, rect, data, 0, count);
            tex.SetData(data);
            return tex;
        }
    }
}
