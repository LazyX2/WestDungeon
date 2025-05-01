using System;
using System.Drawing;
using System.Transactions;

namespace WestDungeon {
    public class World {

        public class Tile {
            public string name;
            public Tile(string t_name) {
                name = t_name;
            }
            /// <summary>
            /// Should happen whenever you
            /// interact with the tile.
            /// </summary>
            /// <returns>false if it's passable, else true</returns>
            public virtual bool Interaction() {
                if (name == "tree" || name == "stone") {
                    return true;
                }
                return false;
            }
            public Image tex;
            public void Draw(Graphics g, Point p) {
                g.DrawImage(tex, p);
            }
        }



        public Dictionary<int, Tile> tileTypes = new Dictionary<int, Tile>();

        public int[][] tiles;
        public World(){
            tileTypes[0] = new Tile("grass") { tex = Game.resImg["grass"] }; //grass tile
            tileTypes[1] = new Tile("tree") { tex = Game.resImg["tree"] }; //tree tile
            tileTypes[2] = new Tile("hell_grass") { tex = Game.resImg["hell_grass"] }; //hell grass tile
            tileTypes[3] = new Tile("bricks") { tex = Game.resImg["bricks"] }; //bricks tile
            tileTypes[4] = new Tile("stone") { tex = Game.resImg["stone"] }; //stone tile
        }
        public World(int[][] t)
        {
            tileTypes[0] = new Tile("grass") { tex = Game.resImg["grass"] }; //grass tile
            tileTypes[1] = new Tile("tree") { tex = Game.resImg["tree"] }; //tree tile
            tileTypes[2] = new Tile("hell_grass") { tex = Game.resImg["hell_grass"] }; //hell grass tile
            tileTypes[3] = new Tile("bricks") { tex = Game.resImg["bricks"] }; //bricks tile
            tileTypes[4] = new Tile("stone") { tex = Game.resImg["stone"] }; //stone tile
            tiles = t;
        }

        public World(string filename)
        {
            IEnumerable<String> lines = File.ReadLines(filename);
            int h = lines.Count();
            tiles = new int[h][];
            for (int y = 0; y < h; y++)
            {
                int w = lines.ElementAt(y).Split(" ").Count();
                tiles[y] = new int[w];
                String[] stiles = lines.ElementAt(y).Split(" ");
                for (int x = 0; x < w; x++)
                {
                    tiles[y][x] = int.Parse(stiles[x]) - 1;
                }
            }
        }

        public int GetTile(int x, int y) {
            if (y < 0 || y >= tiles.Length || x < 0 || x >= tiles[0].Length)
            {
                return -1;
            }
            return tiles[y][x];
        }
        public int GetTile(float x, float y)
        {
            return GetTile((int)x, (int)y);
        }

        public int GetTile(PointF p) {
            return GetTile(p.X, p.Y);
        }

        public static World LoadFromFile(string filename) {
            IEnumerable<String> lines = File.ReadLines(filename);
            int h = lines.Count();
            int[][] tileset = new int[h][];
            for (int y = 0; y < h; y++) {
                int w = lines.ElementAt(y).Split(" ").Count();
                tileset[y] = new int[w];
                String[] stiles = lines.ElementAt(y).Split(" ");
                for (int x = 0; x < w; x++) {
                    tileset[y][x] = int.Parse(stiles[x]) - 1;
                }
            }
            return new World(tileset);
        }

        public void DrawWorld(Graphics g) {
            for (int y = 0; y < tiles.Length; y++)
            {
                for (int x = 0; x < tiles[y].Length; x++)
                {
                    tileTypes[tiles[y][x]].Draw(g, new Point(x * Game.size, y * Game.size));
                }
            }
        }

    }

    public class Dungeon : World
    {
        Random rand = new Random();
        public Dungeon() {}

        public Dungeon(int seed)
        {
            tileTypes[0] = new Tile("grass") { tex = Game.resImg["grass"] }; //grass tile
            tileTypes[1] = new Tile("tree") { tex = Game.resImg["tree"] }; //tree tile
            tileTypes[2] = new Tile("hell_grass") { tex = Game.resImg["hell_grass"] }; //hell grass tile
            tileTypes[3] = new Tile("bricks") { tex = Game.resImg["bricks"] }; //bricks tile
            tileTypes[4] = new Tile("stone") { tex = Game.resImg["stone"] }; //stone tile
            rand = new Random(seed);
        }
    }
}
