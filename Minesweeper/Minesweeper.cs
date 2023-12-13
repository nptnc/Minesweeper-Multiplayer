using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Minesweeper.Multiplayer;
using Minesweeper.Multiplayer.Messages;
using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System.Windows.Forms;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.Brushes;

using Button = Myra.Graphics2D.UI.Button;
using Label = Myra.Graphics2D.UI.Label;
using Panel = Myra.Graphics2D.UI.Panel;
using HorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment;
using TextBox = Myra.Graphics2D.UI.TextBox;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;

namespace Minesweeper {
    public class MinesweeperDifficulty {
        public MinesweeperDifficulty(int x,int y,uint bombCount,string difficultyName) {
            this.x = x;
            this.y = y;
            this.bombCount = bombCount;
            this.difficultyName = difficultyName;
        }

        public int x;
        public int y;
        public uint bombCount;
        public string difficultyName;
    }

    public class Minesweeper : Game {
#nullable enable
        public static Minesweeper instance;

        public string steamClientFucked = "";
        public static GraphicsDeviceManager _graphics;
        public static SpriteBatch _spriteBatch;

        public static Texture2D gridFillTexture;
        public static Texture2D mouseCursor;
        public static Texture2D flag;
        public static Texture2D tile_idle;
        public static Texture2D tile_held;
        public static Texture2D mine;
        public static Texture2D tile_mine_clicked;
        public static Texture2D smile;
        public static Texture2D smileshock;
        public static Texture2D smilewon;
        public static Texture2D smiledead;
        public static Texture2D smileheld;
        public static Texture2D bomb_x;
        public static Texture2D minesweeperBackground;
        public static Texture2D defskin;
        public static Texture2D skin;
        public static Texture2D[] digits;
        public static Texture2D digitDisplay;
        public static Texture2D[] background;
        public static Texture2D[] tile_revealed = new Texture2D[8];

        public static MinesweeperDifficulty[] difficulties { get; private set; } = new MinesweeperDifficulty[] {
            new(8,8,10,"Beginner"),
            new(16,16,40,"Intermediate"),
            new(30,16,99,"Expert"),
        };

        public static MinesweeperDifficulty difficulty;

        public static Desktop _desktop;

        public static SpriteFont font;

        private static int size = 16;
        const int extraSize = 0;
        public static Vector2 gridSize = new Vector2(18, 18);
        const int grannyModeness = 1;
        const int navigationY = 20;
        const int informationSizeY = 40 + (12*3);
        const int padding = 24;
        const int paddingY = 20;
        static int remainingBombs = 0;

        public static int bombCount = 60;

        public Dictionary<(int x,int y),Node> nodes = new();
        public Node? clickedBombNode = null;

        public List<string> loggedData = new();
        private static List<Behavior> behaviors = new();

        public static bool gameOver = false;
        public static int inContextMenu = 0;
        public static bool won { get; private set; }

        public bool isFirstClick = true;
        public bool awaitingTileSync = false;

        public float elapsedTime = 0;

        public Minesweeper() {
            instance = this;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public static void Log(string text) {
            Debug.WriteLine(text);
            instance.loggedData.Add(text);
        }

        static Vector2[] directionsOrthogonal = new Vector2[] {
            new(1,0),
            new(-1,0),
            new(0,1),
            new(0,-1),
        };

        static Vector2[] directionsDiagonal = new Vector2[] {
            new(1,1),
            new(-1,-1),
            new(1,-1),
            new(-1,1),
        };

        static Vector2[] directionsAll = new Vector2[0];

        Color rgbColor(float r, float g, float b) {
            return new Color(r / 255, g / 255, b / 255);
        }

        public static Node? getNodeAtPosition(int x, int y) {
            if (instance.nodes.ContainsKey((x, y)))
                return instance.nodes[(x, y)];
            return null;
        }

        public static Vector2Int CalculateMaxTile() {
            return new (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / size,GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / size);
        }

        public static void CalculateNodes() {
            foreach (var pair in instance.nodes) {
                Node node = pair.Value;
                node.bombCount = 0;
                node.adjacentNodes = new(8);
                node.adjacentNodesDiagonal = new(4);
                node.adjacentNodesOrthogonal = new(4);

                foreach (Vector2 dir in directionsDiagonal) {
                    Node? lmaoXD = getNodeAtPosition(node.x + (int)dir.X, node.y + (int)dir.Y);
                    if (lmaoXD == null)
                        continue;
                    if (lmaoXD.thisType == NodeType.Bomb) {
                        node.bombCount++;
                    }
                    node.adjacentNodesDiagonal.Add(lmaoXD);
                    node.adjacentNodes.Add(lmaoXD);
                }

                foreach (Vector2 dir in directionsOrthogonal) {
                    int checkPosX = node.x + (int)dir.X;
                    int checkPosY = node.y + (int)dir.Y;
                    if (checkPosX < 0 || checkPosX > gridSize.X)
                        continue;
                    if (checkPosY < 0 || checkPosY > gridSize.Y)
                        continue;

                    Node? lmaoXD = getNodeAtPosition(checkPosX, checkPosY);
                    if (lmaoXD == null)
                        continue;
                    if (lmaoXD.thisType == NodeType.Bomb) {
                        node.bombCount++;
                    }
                    node.adjacentNodesOrthogonal.Add(lmaoXD);
                    node.adjacentNodes.Add(lmaoXD);
                }
            }
        }

        public static void ResetGameState() {
            instance.clickedBombNode = null;
            instance.isFirstClick = true;
            gameOver = false;
            CalculateWon();
        }

        public static void GenerateBomb(int amount) {
            Random random = new Random();
            for (int k = 0; k < amount; k++) {
                List<Node> emptyNodes = new();
                foreach (var pair in instance.nodes) {
                    Node node = pair.Value;
                    if (node.thisType == NodeType.Bomb)
                        continue;
                    emptyNodes.Add(node);
                }

                if (emptyNodes.Count == 0) {
                    break;
                }
                Node targnode = emptyNodes[random.Next(0, emptyNodes.Count - 1)];
                targnode.thisType = NodeType.Bomb;
            }
        }

        public void GenerateGame() {
            ApplyGraphics();

            nodes = new();

            int nodeIndex = 0;
            for (int x = 0; x < gridSize.X; x++) {
                for (int y = 0; y < gridSize.Y; y++) {
                    nodes.Add((x,y),new Node() { thisType = NodeType.Empty, x = x, y = y });
                    nodeIndex++;
                }
            }

            elapsedTime = 0;
            GenerateBomb(bombCount);
            CalculateNodes();
            ResetGameState();
        }

        public static void ApplyGraphics() {
            _graphics.IsFullScreen = false;
            _graphics.PreferredBackBufferHeight = (int)((gridSize.Y * size + (extraSize * gridSize.Y)) * grannyModeness + navigationY + informationSizeY);
            _graphics.PreferredBackBufferWidth = (int)((gridSize.X * size + (extraSize * gridSize.X)) * grannyModeness + padding);
            _graphics.ApplyChanges();
        }

        public static void ApplyDifficulty(MinesweeperDifficulty data) {
            bombCount = (int)data.bombCount;
            gridSize = new Vector2((int)data.x,(int)data.y);
            difficulty = data;

            ApplyGraphics();
        }

        protected override void Initialize() {
            difficulty = difficulties.FirstOrDefault(a => a.difficultyName == "Expert");
            ApplyDifficulty(difficulty);

            GenerateGame();

            base.Initialize();

            directionsAll = new Vector2[directionsDiagonal.Length + directionsOrthogonal.Length];
            int index = 0;
            foreach (Vector2 direction in directionsDiagonal) {
                directionsAll[index] = direction;
                index++;
            }
            foreach (Vector2 direction in directionsOrthogonal) {
                directionsAll[index] = direction;
                index++;
            }

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
                if (type.IsSubclassOf(typeof(Behavior))) {
                    Behavior behavior = Activator.CreateInstance(type) as Behavior;
                    behaviors.Add(behavior);
                }
            }
            NetworkHandler.Init();
        }

        public static void LoadSkin(string path) {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            skin = Texture2D.FromStream(_graphics.GraphicsDevice, fileStream);
            fileStream.Dispose();

            int gapBetweenDigits = 12;
            for (int i = 0; i < 10; i++) {
                digits[i] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(gapBetweenDigits * i, 33, 11, 21));
            }
            digitDisplay = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(28, 82, 41, 25));
            tile_idle = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(0, 16, 16, 16));
            tile_held = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(0, 0, 16, 16));
            mine = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(16 * 2, 16, 16, 16));
            flag = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(16 * 3, 16, 16, 16));
            bomb_x = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(16 * 4, 16, 16, 16));
            tile_mine_clicked = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(16 * 5, 16, 16, 16));
            for (int i = 1; i < 8; i++) {
                tile_revealed[i - 1] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(16 * i, 0, 16, 16));
            }

            smile = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(0, 55, 26, 26));
            smileshock = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(27, 55, 26, 26));
            smilewon = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(27, 55, 26, 26));
            smiledead = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(54, 55, 26, 26));
            smileheld = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(108, 55, 26, 26));

            background[0] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(0, 82, 12, 11));
            background[1] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(13, 82, 1, 11));
            background[2] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(15, 82, 12, 11));

            background[3] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(0, 96, 12, 11));
            background[4] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(13, 96, 1, 11));
            background[5] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(15, 96, 12, 11));

            background[6] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(0, 110, 12, 12));
            background[7] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(13, 110, 1, 12));
            background[8] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(15, 110, 12, 12));

            background[9] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(0, 94, 12, 1));
            background[10] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(15, 94, 12, 1));

            background[11] = TextureHelper.SubTexture(skin, _graphics.GraphicsDevice, new Rectangle(70, 82, 1, 1));
        }

        public static string skinsPath = "";
        protected override void LoadContent() {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("text");

            gridFillTexture = new Texture2D(GraphicsDevice, 1, 1);
            gridFillTexture.SetData(new Color[] { Color.White });

            mouseCursor = Content.Load<Texture2D>("mouse");
            minesweeperBackground = Content.Load<Texture2D>("minesweeperbackground");
            defskin = Content.Load<Texture2D>("winxpskin");

            string parent = Environment.CurrentDirectory.ToString();
            skinsPath = Path.Combine(parent, "skins");
            if (!Directory.Exists(skinsPath)) {
                Directory.CreateDirectory(skinsPath);
            }

            string defaultSkinPath = Path.Combine(skinsPath, "default.png");
            if (!File.Exists(defaultSkinPath)) {
                Stream stream = File.Create(defaultSkinPath);
                defskin.SaveAsPng(stream, defskin.Width, defskin.Height);
                stream.Dispose();
                defskin.Dispose();
            }

            digits = new Texture2D[10];
            background = new Texture2D[18];

            string skinPath = Path.Combine(skinsPath, "default.png");
            LoadSkin(skinPath);

            UI.Initialize();
        }

        public static void CalculateWon() {
            bool allRevealed = true;
            bool allMarked = true;

            int bombs = 0;
            int flagged = 0;
            foreach (var pair in instance.nodes) {
                Node node = pair.Value;
                if (node.thisType == NodeType.Bomb)
                    bombs++;
                if (node.marked)
                    flagged++;
                if (node.thisType == NodeType.Empty && node.revealed == false)
                    allRevealed = false;
                if (node.thisType == NodeType.Bomb && node.marked == false)
                    allMarked = false;
                if (node.thisType == NodeType.Empty && node.marked == true)
                    allMarked = false;
            }
            won = allRevealed && allMarked;
            remainingBombs = bombs - flagged;
        }

        protected override void OnExiting(object sender, EventArgs args) {
            NetworkHandler.Shutdown();

            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "latest.txt"))) {
                foreach (string line in loggedData)
                    outputFile.WriteLine(line);
            }

            base.OnExiting(sender, args);
        }

        public static float ScaleToOffsetX(float size) => size* _graphics.PreferredBackBufferWidth;
        public static float ScaleToOffsetY(float size) => size * _graphics.PreferredBackBufferHeight;
        public static float OffsetToScaleX(float size) => size / _graphics.PreferredBackBufferWidth;
        public static float OffsetToScaleY(float size) => size / _graphics.PreferredBackBufferHeight;

        public static bool InBounds(Vector2Int pos, Vector2Int objectPos, Vector2Int objectSize) {
            bool inX = pos.x >= objectPos.x && pos.x <= objectPos.x + objectSize.x;
            bool inY = pos.y <= objectPos.y + objectSize.y && pos.y >= objectPos.y;
            if (inX && inY) {
                return true;
            }
            return false;
        }

        public static bool Reveal(Node node,PlayerId playerId) {
            if (node.thisType == NodeType.Bomb) {
                node.clickedBy = playerId;
                instance.clickedBombNode = node;
                node.revealed = true;
                node.marked = false;
                gameOver = true;
                return true;
            }
            if (node.marked) {
                RecursiveReveal(node, playerId);
                return false;
            }
            node.revealed = true;
            node.marked = false;
            node.clickedBy = playerId;
            if (node.bombCount <= 0)
                RecursiveReveal(node, playerId);
            return false;
        }

        public static void RecursiveReveal(Node currentNode, PlayerId playerId) {
            var nodesToSearch = currentNode.bombCount == 0 ? currentNode.adjacentNodes : new List<Node>();
            foreach (Node adjacentNode in nodesToSearch) {
                if (adjacentNode.revealed)
                    continue;
                if (adjacentNode.marked)
                    continue;
                if (adjacentNode.thisType == NodeType.Bomb)
                    continue;
                Reveal(adjacentNode,playerId);
            }
        }

        public static void RevealNode(Node node,PlayerId playerId) {
            if (instance.awaitingTileSync) {
                Log("Awaiting tile sync you literally cannot lmao");
                return;
            }

            bool clickedbomb = Reveal(node,playerId);
            if (clickedbomb)
                return;

            instance.isFirstClick = false;
            RecursiveReveal(node, playerId);
            CalculateWon();
        }

        public NetworkWriter NodeSyncData() {
            NetworkWriter worldWriter = new();
            foreach (var pair in nodes) {
                Node node = pair.Value;
                worldWriter.Write((byte)node.x);
                worldWriter.Write((byte)node.y);
                byte markedBy = 255;
                if (node.markedBy != null) {
                    markedBy = node.markedBy.smallId;
                }
                byte clickedBy = 255;
                if (node.clickedBy != null) {
                    clickedBy = node.clickedBy.smallId;
                }
                worldWriter.Write(markedBy);
                worldWriter.Write(clickedBy);
                worldWriter.Write((byte)node.thisType);
            }
            return worldWriter;
        }

        private static bool heldNode = false;
        private static bool heldSmile = false;
        float mouseUpdateDuration = 0;

        static int smileSize = 26;
        Vector2Int smileSize2 = new(0,0);
        Vector2Int smilePos = new(0,0);
        protected override void Update(GameTime gameTime) {
            NetworkHandler.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            if (PlayerIds.ids.Count <= 0)
                return;

            heldSmile = false;
            foreach (Vector2Int? pos in InputHelper.GetMousePositionsDown(0)) {
                if (pos == null)
                    continue;
                if (InBounds(pos, smilePos, smileSize2))
                    heldSmile = true;
            }

            Vector2Int mousePos = InputHelper.MousePosition();
            Vector2Int?[] mousePositions = InputHelper.GetMousePositionsDown(0);
            Vector2Int?[] mouseMiddlePositions = InputHelper.GetMousePositionsDown(2);

            if (gameOver == false && won == false)
                elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            bool?[] mouseChanged = InputHelper.MouseButtonChanged();
            for (byte i = 0; i < 3; i++) {
                if (mouseChanged[i] == null)
                    continue;
                NetworkWriter mouseDownWriter = new();
                mouseDownWriter.Write(i);
                mouseDownWriter.Write((bool)mouseChanged[i]!);

                NetworkHandler.SendToServer(typeof(MouseDownMessage), mouseDownWriter.Create(), SendType.Reliable);
            }
            if (mouseUpdateDuration <= 0) {
                NetworkWriter writer = new();
                writer.Write(OffsetToScaleX(Mouse.GetState().X));
                writer.Write(OffsetToScaleY(Mouse.GetState().Y));

                NetworkHandler.SendToServer(typeof(MouseUpdateMessage), writer.Create(), SendType.Unreliable);
            }

            if (InputHelper.MouseButtonUp(0) && InBounds(InputHelper.MousePosition(), smilePos, smileSize2) && inContextMenu <= 0) {
                NetworkHandler.SendToServer(typeof(RestartMessage), new byte[0], SendType.Reliable);
            }

            if (PlayerIds.myId != null) {
                PlayerIds.myId.mouseDown[0] = Mouse.GetState().LeftButton == ButtonState.Pressed;
                PlayerIds.myId.mouseDown[1] = Mouse.GetState().RightButton == ButtonState.Pressed;
                PlayerIds.myId.mouseDown[2] = Mouse.GetState().MiddleButton == ButtonState.Pressed;
            }

            heldNode = false;

            List<Node> heldNodes = new();

            bool hasClickedNode = false;
            foreach (var pair in nodes) {
                Node node = pair.Value;
                if (node.renderPosition == null || node.renderSize == null)
                    continue;
                bool inBounds = InBounds(mousePos, node.renderPosition, node.renderSize);

                /// <summary>
                /// holding display
                /// </summary>
                int ind = -1;
                foreach (Vector2Int? mousePosition in mousePositions) {
                    ind++;
                    if (mousePosition == null)
                        continue;
                    bool thisInBounds = InBounds(mousePosition, node.renderPosition, node.renderSize);
                    if (thisInBounds) {
                        heldNodes.Add(node);
                        heldNode = true;
                        mousePositions[ind] = null;
                    }
                }

                /// <summary>
                /// chording display
                /// </summary>
                ind = -1;
                foreach (Vector2Int? mousePosition in mouseMiddlePositions) {
                    ind++;
                    if (mousePosition == null)
                        continue;
                    bool thisInBounds = InBounds(mousePosition, node.renderPosition, node.renderSize);
                    if (thisInBounds) {
                        if (heldNodes.Contains(node) == false)
                            heldNodes.Add(node);
                        foreach (Node adjacentNode in node.adjacentNodes) {
                            if (adjacentNode.marked)
                                continue;
                            if (adjacentNode.revealed)
                                continue;
                            adjacentNode.held = true;
                            if (!heldNodes.Contains(adjacentNode))
                                heldNodes.Add(adjacentNode);
                        }
                        mouseMiddlePositions[ind] = null;
                        heldNode = true;
                    }
                }

                if (heldNodes.Contains(node)) {
                    node.held = true;
                } else {
                    node.held = false;
                }

                /// <summary>
                /// chording
                /// </summary>
                if (InputHelper.MouseButtonUp(2) && inBounds && hasClickedNode == false && inContextMenu == 0) {
                    Log("trying to chord");
                    hasClickedNode = true;
                    heldNode = false;

                    NetworkWriter writer = new();
                    writer.Write((byte)node.x);
                    writer.Write((byte)node.y);

                    NetworkHandler.SendToServer(typeof(ChordTileMessage),writer.Create(),SendType.Reliable);
                }

                /// <summary>
                /// revealing
                /// </summary>
                if (InputHelper.MouseButtonUp(0) && inBounds && hasClickedNode == false && inContextMenu == 0) {
                    hasClickedNode = true;
                    heldNode = false;

                    NetworkWriter writer = new();
                    writer.Write((byte)node.x);
                    writer.Write((byte)node.y);

                    NetworkHandler.SendToServer(typeof(RevealTileMessage), writer.Create(), SendType.Reliable);
                }

                /// <summary>
                /// flagging
                /// </summary>
                if (InputHelper.MouseButtonDown(1) && inBounds && hasClickedNode == false && inContextMenu == 0) {
                    hasClickedNode = true;

                    NetworkWriter writer = new();
                    writer.Write((byte)node.x);
                    writer.Write((byte)node.y);

                    NetworkHandler.SendToServer(typeof(MarkTileMessage), writer.Create(), SendType.Reliable);
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(rgbColor(255, 255, 255));

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, null);

            /// <summary>
            /// i am so sorry
            /// </summary>

            int screenx = _graphics.PreferredBackBufferWidth;
            int screeny = _graphics.PreferredBackBufferHeight;

            // this draws the background color
            _spriteBatch.Draw(background[11], new Rectangle(0, 0, screenx, screeny), Color.White);

            int dy = navigationY;
            _spriteBatch.Draw(background[0], new Rectangle(0,dy,12,11), Color.White);
            _spriteBatch.Draw(background[1], new Rectangle(12,dy,screenx-12, 11), Color.White);
            _spriteBatch.Draw(background[2], new Rectangle(screenx - 12, dy, 12, 11), Color.White);
            dy += 11;
            int firstAreaSize = 40;
            int firstAreaY = dy;
            _spriteBatch.Draw(background[9], new Rectangle(0, dy, 11, firstAreaSize), Color.White);
            _spriteBatch.Draw(background[10], new Rectangle(screenx-11, dy, 11, firstAreaSize), Color.White);
            dy += firstAreaSize;
            _spriteBatch.Draw(background[3], new Rectangle(0, dy, 12, 11), Color.White);
            _spriteBatch.Draw(background[4], new Rectangle(12, dy, screenx - 12, 11), Color.White);
            _spriteBatch.Draw(background[5], new Rectangle(screenx - 12, dy, 12, 11), Color.White);
            dy += 11;
            _spriteBatch.Draw(background[9], new Rectangle(0, dy, 11, screeny-dy-12), Color.White);
            _spriteBatch.Draw(background[10], new Rectangle(screenx - 11, dy, 11, screeny-dy-12), Color.White);
            dy += (screeny - dy - 12);
            _spriteBatch.Draw(background[6], new Rectangle(0, dy, 12, 12), Color.White);
            _spriteBatch.Draw(background[7], new Rectangle(12, dy, screenx - 12, 12), Color.White);
            _spriteBatch.Draw(background[8], new Rectangle(screenx - 12, dy, 12, 12), Color.White);

            /// <summary>
            /// draw nodes
            /// </summary>

            int marks = 0;
            int bombCount = 0;
            foreach (var pair in nodes) {
                Node node = pair.Value;
                int x = node.x;
                int y = node.y;
                if (node.marked)
                    marks++;
                if (node.thisType == NodeType.Bomb)
                    bombCount++;

                int _size = size * grannyModeness;

                int posx = x * _size + ((extraSize * grannyModeness) * x) + padding/2 - 1;
                int posy = y * _size + ((extraSize * grannyModeness) * y) + navigationY + informationSizeY - 13;
                node.renderPosition = new Vector2Int(posx, posy);
                node.renderSize = new Vector2Int(_size,_size);

                TextureData[] textures = node.texture;
                foreach (TextureData tex in textures) {
                    _spriteBatch.Draw(tex.texture, new Rectangle(posx, posy, _size, _size), tex.color);
                }
            }

            smileSize2 = new(smileSize,smileSize);
            smilePos = new(_graphics.PreferredBackBufferWidth / 2 - smileSize / 2, firstAreaY + (firstAreaSize/2) - smileSize / 2);

            if (heldSmile) {
                _spriteBatch.Draw(smileheld, new Rectangle(smilePos.x, smilePos.y, smileSize, smileSize), Color.White);
            }
            else {
                if (won)
                    _spriteBatch.Draw(smilewon, new Rectangle(smilePos.x, smilePos.y, smileSize, smileSize), Color.White);
                else if (gameOver)
                    _spriteBatch.Draw(smiledead, new Rectangle(smilePos.x, smilePos.y, smileSize, smileSize), Color.White);
                else if (heldNode)
                    _spriteBatch.Draw(smileshock, new Rectangle(smilePos.x, smilePos.y, smileSize, smileSize), Color.White);
                else
                    _spriteBatch.Draw(smile, new Rectangle(smilePos.x, smilePos.y, smileSize, smileSize), Color.White);
            }

            foreach (Behavior behavior in behaviors) {
                behavior.PostRender();
            }

            void DrawDigitDisplay(uint num,Rectangle rect,int padding) {
                if (num > 999)
                    num = 999;
                _spriteBatch.Draw(digitDisplay, rect, Color.White);
                string convertedString = num.ToString();

                List<int> digitsToDraw = new(3);
                for (int i = 0; i < 3; i++) {
                    if (i < convertedString.Length)
                        digitsToDraw.Add(int.Parse(convertedString[i].ToString()));
                    if (i >= convertedString.Length) {
                        digitsToDraw.Insert(0, 0);
                    }
                }
                int index = 0;
                foreach (int digitToDraw in digitsToDraw) {
                    Texture2D targetTexture = digits[digitToDraw];
                    int paddingY = 4;
                    _spriteBatch.Draw(targetTexture, new Rectangle(rect.X + 1 + ((padding / 2) * index) + (rect.Width / 3 * index), rect.Y + paddingY, (rect.Width - 2) / 3 - 1 - padding, rect.Height - paddingY - 2), Color.White);
                    index++;
                }
            }

            int sum = bombCount - marks;
            if (sum < 0)
                sum = 0;
            uint sum1 = (uint)sum;

            int digitDisplaySizeX = 41;
            int digitDisplaySizeY = 25;
            DrawDigitDisplay(sum1, new Rectangle(12, smilePos.y-(digitDisplaySizeY/ 2) + smileSize/2, digitDisplaySizeX, digitDisplaySizeY), 2);
            DrawDigitDisplay((uint)elapsedTime, new Rectangle(screenx - 12 - digitDisplaySizeX, smilePos.y - (digitDisplaySizeY / 2) + smileSize / 2, digitDisplaySizeX, digitDisplaySizeY), 2);

            _spriteBatch.Draw(gridFillTexture, new Rectangle(0, 0, screenx, navigationY), Color.White);

            _spriteBatch.End();
            _desktop.Render();

            InputHelper.UpdateMouseState();

            base.Draw(gameTime);
        }
    }
}