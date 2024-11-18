using System;

namespace WestDungeon {
    public class World {

        public class Tile
        {
            public bool collidible = false;
            public Image img;
        }

        public int[][] tiles;
        public World(int[][] t) {
            tiles = t;
        }
        public int GetTile(int x, int y) {
            if (y < 0 || y >= tiles.Length || x < 0 || x >= tiles[0].Length)
            {
                return -1;
            }
            return tiles[y][x];
        }

        public int GetTile(Point p) {
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
                    tileset[y][x] = int.Parse(stiles[x]);
                }
            }
            return new World(tileset);
        }

    }
}
