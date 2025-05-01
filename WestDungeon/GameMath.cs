using System;

namespace WestDungeon {
    public class GameMath {
        public static PointF PosToTile(PointF loc)
        {
            float tx = (float)Math.Round(loc.X / (double)Game.size);
            float ty = (float)Math.Round(loc.Y / (double)Game.size);
            return new PointF(tx, ty);
        }
        public static PointF PosToTileL(PointF loc)
        {
            float tx = (float)Math.Floor(loc.X / (double)Game.size);
            float ty = (float)Math.Ceiling(loc.Y / (double)Game.size);
            return new PointF(tx, ty);
        }
        public static PointF PosToTileH(PointF loc)
        {
            float tx = (float)Math.Ceiling(loc.X / (double)Game.size);
            float ty = (float)Math.Floor(loc.Y / (double)Game.size);
            return new PointF(tx, ty);
        }

        public static double Distance(PointF a, PointF b) {
            return Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
        }

        public static double Magnitude(PointF p) {
            return Math.Sqrt(p.X*p.X + p.Y*p.Y);
        }

        public static PointF Normalize(PointF p) {
            if (p.X + p.Y == 0) return new PointF(0,0);
            return new PointF(p.X/(float)Magnitude(p), p.Y/ (float)Magnitude(p));
        }
    }
}
