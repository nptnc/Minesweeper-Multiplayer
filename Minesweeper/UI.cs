using Microsoft.Xna.Framework;
using Minesweeper.Multiplayer;
using Minesweeper.Multiplayer.Messages;
using Myra;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;

namespace Minesweeper {
    public static class UI {
        public static void Initialize() {
            MyraEnvironment.Game = Minesweeper.instance;

            var grid = new Grid {
                RowSpacing = 1,
                ColumnSpacing = 8
            };

            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            // Button
            var button = new Button {
                Content = new Label {
                    Text = "Game",
                    TextColor = Color.Black,
                    Background = new SolidBrush(Color.White),
                },
            };

            Grid.SetColumn(button, 0);
            Grid.SetRow(button, 0);

            var panel = new Panel() {
                Background = new SolidBrush(Color.White),
                Top = 20,
                Width = 100,
                Height = (Minesweeper.difficulties.Length * 20) + 20 + 20 + 20 + (5 * 2),
                Visible = false,
            };

            bool enabled = false;

            void togglePanel() {
                enabled = !enabled;
                panel.Visible = enabled;
                Minesweeper.inContextMenu += enabled ? 1 : -1;
            }

            grid.Widgets.Add(button);
            grid.Widgets.Add(panel);

            Panel? panel1 = null;
            button.Click += (s, a) => {
                panel.Widgets.Clear();
                if (enabled == true) {
                    togglePanel();
                    return;
                }

                togglePanel();

                int currentY = 0;

                TextButton CreateButton(string txt, Action act) {
                    var paddedCenteredButton = new TextButton();
                    paddedCenteredButton.Text = txt;
                    paddedCenteredButton.VerticalAlignment = VerticalAlignment.Top;
                    paddedCenteredButton.Top = currentY;
                    paddedCenteredButton.Width = 100;
                    paddedCenteredButton.Background = new SolidBrush(new Color(0, 0, 0, 0));
                    paddedCenteredButton.HorizontalAlignment = HorizontalAlignment.Left;
                    paddedCenteredButton.ContentHorizontalAlignment = HorizontalAlignment.Left;
                    paddedCenteredButton.TextColor = Color.Black;
                    paddedCenteredButton.Click += (s, a) => act.Invoke();
                    currentY += 20;
                    panel.Widgets.Add(paddedCenteredButton);
                    return paddedCenteredButton;
                }

                void CreateSeperation() {
                    int size = 5;
                    var zaza = new Panel() {
                        Background = new SolidBrush(Color.DarkGray),
                        Top = currentY,
                        Left = 0,
                        Width = 90,
                        Height = size,
                        Visible = true,
                    };
                    currentY += size;
                    panel.Widgets.Add(zaza);
                }

                CreateButton("Reset", delegate () {
                    NetworkHandler.SendToServer(typeof(RestartMessage), new byte[0], SendType.Reliable);
                    togglePanel();
                });

                CreateSeperation();

                foreach (MinesweeperDifficulty difficultyData in Minesweeper.difficulties) {
                    CreateButton(difficultyData.difficultyName, delegate () {
                        NetworkWriter writer = new();
                        writer.Write((short)difficultyData.bombCount);
                        writer.Write((short)difficultyData.x);
                        writer.Write((short)difficultyData.y);

                        NetworkHandler.SendToServer(typeof(ChangeDifficultyMessage), writer.Create(), SendType.Reliable);
                        togglePanel();
                    });
                }

                CreateButton("Custom...", delegate () {
                    if (panel1 != null) {
                        grid.Widgets.Remove(panel1);
                        Minesweeper.inContextMenu--;
                        panel1 = null;
                        return;
                    }

                    int width = 140;
                    int height = 110;
                    panel1 = new Panel() {
                        Background = new SolidBrush(Color.White),
                        Top = Minesweeper._graphics.PreferredBackBufferHeight / 2 - height / 2,
                        Left = Minesweeper._graphics.PreferredBackBufferWidth / 2 - width / 2,
                        Width = width,
                        Height = height,
                        Visible = true,
                    };

                    Minesweeper.inContextMenu++;
                    int labelsize = width / 4;

                    var text = new Label();
                    text.Text = "Width";
                    text.VerticalAlignment = VerticalAlignment.Top;
                    text.Top = 0;
                    text.Width = width;
                    text.HorizontalAlignment = HorizontalAlignment.Left;
                    text.TextColor = Color.Black;
                    panel1.Widgets.Add(text);

                    var button = new TextBox();
                    button.Text = "1";
                    button.VerticalAlignment = VerticalAlignment.Top;
                    button.Top = 0;
                    button.Left = 0;
                    button.Width = labelsize;
                    button.Background = new SolidBrush(new Color(0, 0, 0, 1));
                    button.HorizontalAlignment = HorizontalAlignment.Center;
                    button.TextColor = Color.Black;
                    panel1.Widgets.Add(button);

                    var text2 = new Label();
                    text2.Text = "Height";
                    text2.VerticalAlignment = VerticalAlignment.Top;
                    text2.Top = 20;
                    text2.Width = width;
                    text2.HorizontalAlignment = HorizontalAlignment.Left;
                    text2.TextColor = Color.Black;
                    panel1.Widgets.Add(text2);

                    var button2 = new TextBox();
                    button2.Text = "1";
                    button2.VerticalAlignment = VerticalAlignment.Top;
                    button2.Top = 20;
                    button2.Left = 0;
                    button2.Width = labelsize;
                    button2.Background = new SolidBrush(new Color(0, 0, 0, 1));
                    button2.HorizontalAlignment = HorizontalAlignment.Center;
                    button2.TextColor = Color.Black;
                    panel1.Widgets.Add(button2);

                    var text1 = new Label();
                    text1.Text = "Mines";
                    text1.VerticalAlignment = VerticalAlignment.Top;
                    text1.Top = 40;
                    text1.Width = width;
                    text1.HorizontalAlignment = HorizontalAlignment.Left;
                    text1.TextColor = Color.Black;
                    panel1.Widgets.Add(text1);

                    var button1 = new TextBox();
                    button1.Text = "1";
                    button1.VerticalAlignment = VerticalAlignment.Top;
                    button1.Top = 40;
                    button1.Left = 0;
                    button1.Width = labelsize;
                    button1.Background = new SolidBrush(new Color(0, 0, 0, 1));
                    button1.HorizontalAlignment = HorizontalAlignment.Center;
                    button1.TextColor = Color.Black;
                    panel1.Widgets.Add(button1);

                    var confirm = new TextButton();
                    confirm.Text = "Confirm";
                    confirm.VerticalAlignment = VerticalAlignment.Top;
                    confirm.Top = 60;
                    confirm.Width = width;
                    confirm.HorizontalAlignment = HorizontalAlignment.Left;
                    confirm.TextColor = Color.Black;
                    confirm.Background = new SolidBrush(new Color(0, 0, 0, 0));
                    confirm.Click += (s, a) => {
                        Minesweeper.inContextMenu--;

                        int convertedWidth = 1;
                        int.TryParse(button.Text, out convertedWidth);

                        int convertedHeight = 1;
                        int.TryParse(button2.Text, out convertedHeight);

                        int convertedMines = 1;
                        int.TryParse(button1.Text, out convertedMines);

                        MinesweeperDifficulty diff = new MinesweeperDifficulty(convertedWidth, convertedHeight, (uint)convertedMines, "Custom");

                        NetworkWriter writer = new();
                        writer.Write((short)diff.bombCount);
                        writer.Write((short)diff.x);
                        writer.Write((short)diff.y);

                        NetworkHandler.SendToServer(typeof(ChangeDifficultyMessage), writer.Create(), SendType.Reliable);

                        grid.Widgets.Remove(panel1);
                    };
                    panel1.Widgets.Add(confirm);

                    grid.Widgets.Add(panel1);
                });

                CreateSeperation();

                Panel? skinsPanel = null;
                CreateButton("Skins", delegate () {
                    if (skinsPanel != null) {
                        grid.Widgets.Remove(skinsPanel);
                        Minesweeper.inContextMenu--;
                        skinsPanel = null;
                        return;
                    }

                    Minesweeper.inContextMenu++;

                    string[] files = Directory.GetFiles(Minesweeper.skinsPath);

                    int width = 140;
                    int height = files.Length * 20;
                    skinsPanel = new Panel() {
                        Background = new SolidBrush(Color.White),
                        Top = Minesweeper._graphics.PreferredBackBufferHeight / 2 - height / 2,
                        Left = Minesweeper._graphics.PreferredBackBufferWidth / 2 - width / 2,
                        Width = width,
                        Height = height,
                        Visible = true,
                    };

                    int labelsize = width / 4;

                    int downness = 0;
                    TextButton SkinCreateButton(string txt, Action act) {
                        var paddedCenteredButton = new TextButton();
                        paddedCenteredButton.Text = txt;
                        paddedCenteredButton.VerticalAlignment = VerticalAlignment.Top;
                        paddedCenteredButton.Top = downness;
                        paddedCenteredButton.Width = width;
                        paddedCenteredButton.Background = new SolidBrush(new Color(0, 0, 0, 0));
                        paddedCenteredButton.HorizontalAlignment = HorizontalAlignment.Left;
                        paddedCenteredButton.ContentHorizontalAlignment = HorizontalAlignment.Left;
                        paddedCenteredButton.TextColor = Color.Black;
                        paddedCenteredButton.Click += (s, a) => act.Invoke();
                        skinsPanel.Widgets.Add(paddedCenteredButton);
                        return paddedCenteredButton;
                    }

                    foreach (string filePath in files) {
                        SkinCreateButton(Path.GetFileName(filePath), () => {
                            Minesweeper.LoadSkin(filePath);
                            togglePanel();
                            Minesweeper.inContextMenu--;
                            grid.Widgets.Remove(skinsPanel);
                        });
                        downness += 20;
                    }

                    grid.Widgets.Add(skinsPanel);
                });
            };

            Minesweeper._desktop = new Desktop();
            Minesweeper._desktop.Root = grid;
        }
    }
}
