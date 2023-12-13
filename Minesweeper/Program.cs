
using System;

using var game = new Minesweeper.Minesweeper();
try {
    game.Run();
} catch (Exception exc) {
    Minesweeper.Minesweeper.Log(exc.ToString());
}