using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minesweeper.Multiplayer.Features {
    public class Cursors : Behavior {
        static Color[] playerColors = new Color[] {
            Color.Red,
            Color.Blue,
            Color.Green,
            Color.Yellow,
            Color.HotPink,
        };

        public static Color GetPlayerColor(PlayerId playerid) {
            Color col = Color.White;
            if (playerid.smallId < playerColors.Length-1)
                col = playerColors[playerid.smallId];
            return col;
        }

        public override void PostRender() {
            foreach (PlayerId playerid in PlayerIds.ids) {
                if (playerid.id == SteamClient.SteamId)
                    continue;
                int x = (int)Minesweeper.ScaleToOffsetX(playerid.mousePosition.X);
                int y = (int)Minesweeper.ScaleToOffsetY(playerid.mousePosition.Y);
                int sizeX = Minesweeper.mouseCursor.Width / 110;

                Color col = GetPlayerColor(playerid);

                float range = 60;
                float distance = Vector2.Distance(InputHelper.MousePosition2(), new Vector2(x, y));
                if (distance < range) {
                    col.A = (byte)Math.Round(0.3f * 255);
                }
                Minesweeper._spriteBatch.Draw(Minesweeper.mouseCursor, new Rectangle(x, y, sizeX, Minesweeper.mouseCursor.Height / 110), col);

                Color textColor = new((float)col.R/255, (float)col.G/255, (float)col.B/255, (float)1);
                if (distance < range) {
                    continue;
                }
                float xOffset = (x + sizeX / 2) - (Minesweeper.font.MeasureString(playerid.name).X / 2);
                Minesweeper._spriteBatch.DrawString(Minesweeper.font, playerid.name, new Vector2(xOffset, y + 24), textColor);
            }
        }
    }
}
