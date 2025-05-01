using System.Reflection;
using System;

namespace WestDungeon {
    
    public partial class Game : Form {
        public static int size = 20;
        public static int gameState = 1;
        public static Dictionary<string, Image> resImg;
        public static Mob player;
        public static List<Entity> entities;
        Font bold;
        public static World world;
        public static string[] chat = new string[3];
        Bitmap tileset;
        public static Brush bg_color, grayBr, redBr;
        Graphics g;
        public static string fs_location;

        public static void SendChatMessage(string str) {
            chat[2] = chat[1];
            chat[1] = chat[0];
            chat[0] = str;
        }

        public Game()
        {
            InitializeComponent();
            g = gameScreen.CreateGraphics();
            bg_color = new SolidBrush(Color.FromArgb(255,0,0,0));
            LoadResources();
        }

        public bool LoadResources() {
            fs_location = Assembly.GetExecutingAssembly().Location;
            fs_location = fs_location.Substring(0, fs_location.Length - 40);

            grayBr = new SolidBrush(Color.Gray);
            redBr = new SolidBrush(Color.Red);
            resImg = new Dictionary<String, Image>();
            entities = new List<Entity>();
            //loading images =>
            var img = new Bitmap(Image.FromFile(fs_location + "res\\entities.png"));
            img.MakeTransparent(Color.White);
            resImg["skeleton"] = img.Clone(new Rectangle(0, 0, 32, 32), img.PixelFormat);
            resImg["ghoul"] = img.Clone(new Rectangle(32, 0, 32, 32), img.PixelFormat);
            resImg["bone_knight"] = img.Clone(new Rectangle(32, 32, 32, 32), img.PixelFormat);
            resImg["wraith"] = img.Clone(new Rectangle(64, 0, 32, 32), img.PixelFormat);

            img = new Bitmap(Image.FromFile(fs_location + "res\\weapons.png"));
            img.MakeTransparent(Color.White);
            resImg["weapons"] = img;

            img = new Bitmap(Image.FromFile(fs_location + "res\\tileset.png"));
            resImg["grass"] = img.Clone(new Rectangle(0, 0, 20, 20), img.PixelFormat);
            resImg["tree"] = img.Clone(new Rectangle(20, 0, 20, 20), img.PixelFormat);
            resImg["hell_grass"] = img.Clone(new Rectangle(40, 0, 20, 20), img.PixelFormat);
            resImg["bricks"] = img.Clone(new Rectangle(60, 0, 20, 20), img.PixelFormat);
            resImg["stone"] = img.Clone(new Rectangle(80, 0, 20, 20), img.PixelFormat);

            world = World.LoadFromFile(fs_location + "res\\maps\\DungeonMap.txt");
            tileset = new Bitmap(Image.FromFile(fs_location + "res\\tileset.png"));
            //player
            player = new Skeleton(new PointF(75, 75));
            player.stats["spd"] = 7f;
            player.stats["xp"] = 0f;
            player.stats["level"] = 1;
            player.data["weapon"] = (Int16)1;

            bold = new Font("bold", 15.0f, FontStyle.Bold);
            entities.Add(new Skeleton(new Point(40, 40)));
            SendChatMessage("Console: finished loading the game...");
            return true;
        }

        private void OnClick(object sender, EventArgs e)
        {
            //clicked_pos = Cursor.Position;
            /*
            Point loc = player.loc + ((Size)Cursor.Position);
            Skeleton mob = new Skeleton(loc);
            entities.Add(mob);*/
        }

        private void OnRender(object sender, PaintEventArgs e) {
            Graphics g = e.Graphics;
            RectangleF dest, rect;
            Brush btc = new SolidBrush(Color.FromArgb(100, 255, 0, 0));
            Brush bc = new SolidBrush(Color.White);
            PointF chatLoc = player.loc + new SizeF(-player.loc.X, Height - 200);

            g.TranslateTransform(Width / 2 - player.loc.X, Height / 2 - player.loc.Y);

            for (int y = 0; y < world.tiles.Length; y++)
            {
                for (int x = 0; x < world.tiles[y].Length; x++)
                {
                    dest = new Rectangle(x * size, y * size, size, size);
                    g.DrawImage(tileset.Clone(RectangleF.FromLTRB(20 * world.tiles[y][x], 0, 20 * world.tiles[y][x] + 20, 20), tileset.PixelFormat), x * size, y * size);
                    //if (world.GetTile(x, y) == 1) g.FillRectangle(btc, dest);
                }
            }

            player.Render(g);
            foreach (Entity en in entities)
            {
                en.Render(g);
            }

            g.TranslateTransform(- (Width / 2 - player.loc.X), -(Height / 2 - player.loc.Y));
            
            dest = new RectangleF(32 * (Int16)player.data["weapon"], 0, 32, 32);
            g.DrawImage( ((Bitmap)resImg["weapons"]).Clone(dest, resImg["weapons"].PixelFormat), PointToClient(Cursor.Position));

            g.DrawString("Pos: " + player.loc,
                bold, bc, player.loc - new Size(Width/2, Height/2)
            );
            g.DrawString("Tile: (" + (player.loc.X / size) + ", " + (player.loc.Y / size) + ")",
                bold, bc, new Point(0, 25)
            );
            g.DrawString("Health: " + player.stats["hp"] + " / " + player.stats["maxhp"] + ")",
                bold, bc, new Point(0, 50)
            );
            g.DrawString("Mouse: " + chatLoc + "|" + Cursor.Position,
                bold, bc, new Point(0, 75)
            );
            for (int i = 0; i < chat.Length; i++) {
                g.DrawString(chat[i], bold, bc, new Point(0, ClientSize.Height - 50 - 25 * i));
            }
        }

        private void OnTick(object sender, EventArgs e)
        {
            gameScreen.Refresh();
            player.Update();
            entities.ForEach(e => e.Update());
        }

        private void OnMClick(object sender, MouseEventArgs e)
        {
            PointF p = player.loc - new SizeF(ClientSize.Width / 2 - e.Location.X, ClientSize.Height / 2 - e.Location.Y);
            Entity[] ens = entities.Where(en => GameMath.Distance(GameMath.PosToTileH(en.loc), GameMath.PosToTileH(p)) < 2.5).ToArray();
            if (e.Button == MouseButtons.Left) {
                if (ens.Count() > 0) {
                    if (ens[0] is Mob && GameMath.Distance(GameMath.PosToTile(ens[0].loc), GameMath.PosToTile(player.loc)) < 5) {
                        player.Attack((Mob)ens[0]);
                        /*Mob clickedEntity = (Mob)ens[0];
                        clickedEntity.stats["hp"] -= player.stats["atk"];

                        SendChatMessage(p + "|" + ens[0].stats["hp"]);*/
                    }
                }
            } else if (e.Button == MouseButtons.Right) {

            }
        }

        private void OnKey(object sender, PreviewKeyDownEventArgs e)
        {
            int tx = (int) Math.Floor(player.loc.X / (double)size);
            int ty = (int) Math.Floor(player.loc.Y / (double)size);
            int spd = (int)player.stats["spd"];
            int lvl = (int)player.stats["level"];
            PointF vel = player.loc;
            switch (e.KeyCode)
            {
                //General Controls
                case Keys.T:
                    player.loc = GameMath.PosToTile(Cursor.Position);
                    break;
                case Keys.B:
                    MessageBox.Show(string.Join("\n",File.ReadAllLines(fs_location + "res\\backstory.txt").Take(lvl).Where(str => str != "You try to remember more about your past, but you can't remember anything.")), "Memories");
                    break;
                case Keys.M:
                    MessageBox.Show(
                        "hp: " + player.stats["hp"] + "/" + player.stats["maxhp"]
                        + "\natk: " + player.stats["atk"]
                        + "\nLevel: " + lvl
                        + "\nXP: " + player.stats["xp"] + "/" + 2*(int)Math.Pow(lvl, 2),
                        "Player:", MessageBoxButtons.OK);
                    break;
                //WASD movement
                case Keys.W:
                    vel -= new Size(0, spd);
                    //if (world.GetTile(PosToTile(player.loc - new Size(0, spd))) == 0)
                    //    player.loc.Offset(0, -spd);
                    break;
                case Keys.S:
                    vel += new Size(0, spd);
                    //if (world.GetTile(PosToTile(player.loc + new Size(0, spd))) == 0)
                    //    player.loc.Offset(0, spd);
                    break;
                case Keys.A:
                    vel -= new Size(spd, 0);
                    //if (world.GetTile(PosToTile(player.loc - new Size(spd, 0))) == 0)
                    //    player.loc.Offset(-spd, 0);
                    break;
                case Keys.D:
                    vel += new Size(spd, 0);
                    //if (world.GetTile(PosToTile(player.loc + new Size(spd, 0))) == 0)
                    //    player.loc.Offset(spd, 0);
                    break;
            }
            int tile = world.GetTile(GameMath.PosToTile(vel));
            if (tile != -1 && !world.tileTypes[tile].Interaction()) {
                player.loc.X += vel.X - player.loc.X;
                player.loc.Y += vel.Y - player.loc.Y;
            }
        }
    }
}
