using System.Reflection;
using System;

namespace WestDungeon {

    public partial class Game : Form {
        public static int size = 20;
        public static Dictionary<string, Image> resImg;
        Point clicked_pos = Point.Empty;
        public static Player player;
        public static List<Entity> entities;
        Font bold;
        public static World world;
        public static string[] chat = new string[3];
        Image tileset;
        Brush bg_color, grayBr, redBr;
        Graphics g;
        string fs_location;

        public static void SendChatMessage(string str) {
            chat[2] = chat[1];
            chat[1] = chat[0];
            chat[0] = str;
        }

        public static double Distance(Point a, Point b)
        {
            return Math.Sqrt((a.X-b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public Game()
        {
            InitializeComponent();
            g = gameScreen.CreateGraphics();
            bg_color = new SolidBrush(Color.FromArgb(255,0,0,0));
            LoadResources();
        }

        public bool LoadResources() {
            grayBr = new SolidBrush(Color.Gray);
            redBr = new SolidBrush(Color.Red);
            resImg = new Dictionary<String, Image>();
            entities = new List<Entity>();
            fs_location = Assembly.GetExecutingAssembly().Location;
            fs_location = fs_location.Substring(0, fs_location.Length - 40);
            resImg["undead"] = Image.FromFile(fs_location + "res\\skeleton.png");
            world = World.LoadFromFile(fs_location + "res\\maps\\testmap.txt");
            tileset = Image.FromFile(fs_location + "res\\tileset.png");
            player = new Player(new Point(75, 75));
            bold = new Font("bold", 15.0f, FontStyle.Bold);
            entities.Add(new Skeleton(new Point(40, 40)));
            SendChatMessage("Console: finished loading the game...");
            return true;
        }

        public static Point PosToTile(Point loc)
        {
            int tx = (int)Math.Round(loc.X / (double)size);
            int ty = (int)Math.Round(loc.Y / (double)size);
            return new Point(tx, ty);
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
            Rectangle dest_rect;
            Brush btc = new SolidBrush(Color.FromArgb(100, 255, 0, 0));
            Brush bc = new SolidBrush(Color.White);
            Point chatLoc = player.loc + new Size(-player.loc.X, Height - 200);


            g.TranslateTransform(Width / 2 - player.loc.X, Height / 2 - player.loc.Y);

            for (int y = 0; y < world.tiles.Length; y++)
            {
                for (int x = 0; x < world.tiles[y].Length; x++)
                {
                    dest_rect = new Rectangle(x * size, y * size, size, size);
                    g.DrawImage(tileset, dest_rect, 20 * world.tiles[y][x], 0, 20, 20, GraphicsUnit.Pixel);
                    //if (world.GetTile(x, y) == 1) g.FillRectangle(btc, dest_rect);
                }
            }

            player.Render(g);
            if (entities.First() is Mob)
            {
                Entity en = entities.First();
                Mob mob = (Mob)en;
                foreach (Point p in mob.PathFind(player.loc))
                {
                    g.FillRectangle(btc, p.X * size, p.Y * size, size, size);
                };
            }
            foreach (Entity en in entities)
            {
                en.Render(g);
                Rectangle rect = new Rectangle(en.loc, new Size(size,5));
                if (en.stats["hp"] < en.stats["maxhp"]) g.FillRectangle(grayBr, rect);
                rect.Width = (int)Math.Floor(en.stats["hp"] / en.stats["maxhp"] * 10);
                g.FillRectangle(redBr, rect);
            }

            g.TranslateTransform(- (Width / 2 - player.loc.X), -(Height / 2 - player.loc.Y));

            
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
            //Debuging
            g.DrawString("Data: " + entities.First().data["lastAttack"] + ", Current: " + DateTime.Now.Ticks / 10000,
                bold, bc, new Point(0, 100)
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
            Point p = player.loc - new Size(ClientSize.Width / 2 - e.Location.X, ClientSize.Height / 2 - e.Location.Y);
            Entity[] ens = entities.Where(en => Distance(PosToTile(en.loc), PosToTile(p)) < 2).ToArray();
            if (e.Button == MouseButtons.Left) {
                if (ens.Count() > 0) {
                    ens[0].stats["hp"] -= player.stats["atk"];
                    SendChatMessage(p + "|" + ens[0].stats["hp"]);
                }
            } else if (e.Button == MouseButtons.Right) {

            }
        }

        private void OnKey(object sender, PreviewKeyDownEventArgs e)
        {
            int tx = (int) Math.Floor(player.loc.X / (double)size);
            int ty = (int) Math.Floor(player.loc.Y / (double)size);
            int spd = (int)player.stats["spd"];
            switch (e.KeyCode)
            {
                case Keys.T:
                    player.loc = PosToTile(Cursor.Position);
                    break;
                case Keys.W:
                    if (world.GetTile(PosToTile(player.loc - new Size(0, spd))) == 0)
                        player.loc.Offset(0, -spd);
                    break;
                case Keys.S:
                    if (world.GetTile(PosToTile(player.loc + new Size(0, spd))) == 0)
                        player.loc.Offset(0, spd);
                    break;
                case Keys.A:
                    if (world.GetTile(PosToTile(player.loc - new Size(spd, 0))) == 0)
                        player.loc.Offset(-spd, 0);
                    break;
                case Keys.D:
                    if (world.GetTile(PosToTile(player.loc + new Size(spd, 0))) == 0)
                        player.loc.Offset(spd, 0);
                    break;
            }
        }
    }
}
