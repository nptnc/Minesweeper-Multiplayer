using Microsoft.Xna.Framework.Input;
using Minesweeper.Multiplayer;
using Steamworks;
using System.Numerics;

namespace Minesweeper {
    public class InputHelper {
        static bool[] wasButtonDown = new bool[3];

        public static void UpdateMouseState() {
            wasButtonDown[0] = Mouse.GetState().LeftButton == ButtonState.Pressed;
            wasButtonDown[1] = Mouse.GetState().RightButton == ButtonState.Pressed;
            wasButtonDown[2] = Mouse.GetState().MiddleButton == ButtonState.Pressed;
        }

        public static ButtonState WhichMouseButton(int whichOne) {
            if (whichOne == 0)
                return Mouse.GetState().LeftButton;
            else if (whichOne == 1)
                return Mouse.GetState().RightButton;
            return Mouse.GetState().MiddleButton;
        }

        public static Vector2Int MousePosition() {
            return new Vector2Int(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public static Vector2 MousePosition2() {
            return new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public static bool?[] MouseButtonChanged() {
            int ind = 0;
            bool?[] thatAreDown = new bool?[3] { null, null, null };
            foreach (bool button in wasButtonDown) {
                bool isCurrentButton = WhichMouseButton(ind) == ButtonState.Pressed;
                if (button != isCurrentButton) {
                    thatAreDown[ind] = isCurrentButton;
                }
                ind++;
            }
            return thatAreDown;
        }

        public static bool MouseButtonDown(int whichOne) {
            if (wasButtonDown[whichOne] == false && WhichMouseButton(whichOne) == ButtonState.Pressed) {
                return true;
            }
            return false;
        }

        public static bool MouseButtonUp(int whichOne) {
            if (wasButtonDown[whichOne] == true && WhichMouseButton(whichOne) == ButtonState.Released) {
                return true;
            }
            return false;
        }

#nullable enable
        public static Vector2Int?[] GetMousePositionsDown(int whichOne) {
            if (PlayerIds.ids.Count == 0)
                return new Vector2Int[0];

            Vector2Int mousePos = MousePosition();
            Vector2Int?[] mousePositions = new Vector2Int[PlayerIds.ids.Count];
            int currentIndex = 0;

            if (WhichMouseButton(whichOne) == ButtonState.Pressed) {
                mousePositions[currentIndex] = mousePos;
                currentIndex++;
            }

            foreach (PlayerId playerMouseId in PlayerIds.ids) {
                if (playerMouseId.id == SteamClient.SteamId || playerMouseId.mouseDown[whichOne] == false)
                    continue;
                mousePositions[currentIndex] = new Vector2Int((int)Minesweeper.ScaleToOffsetX(playerMouseId.mousePosition.X), (int)Minesweeper.ScaleToOffsetY(playerMouseId.mousePosition.Y));
                currentIndex++;
            }
            return mousePositions;
        }
    }
}
