using System;
using System.Numerics;

namespace WestDungeon
{
    public class Entity
    {
        public Point loc;
        public Vector2 velocity;
        public Dictionary<string, float> stats;
        public Dictionary<string, object> data;
        public Entity(Point location) {
            loc = location;
        }
        public virtual void Death(Entity killer) { }
        public virtual void Update() { }
        public virtual void Render(Graphics g) { }
    }

    public class Player : Entity
    {
        public string race;
        public Player(Point loc) : base(loc)
        {
            stats = new Dictionary<string, float>();
            data = new Dictionary<string, object>();
            race = "undead";
            stats["hp"] = 20f;
            stats["maxhp"] = 20f;
            stats["atk"] = 2f;
            stats["spd"] = 5f;
            stats["xp"] = 0f;
            data["weapon"] = null;
        }
        Color eyeC = Color.White;

        public override void Death(Entity killer) { }
        public override void Update() { }
        public override void Render(Graphics g)
        {
            var bmp = new Bitmap(Game.resImg[race]);
            bmp.MakeTransparent(Color.White);
            g.DrawImage(bmp, loc.X, loc.Y, Game.size, Game.size);
        }
    }

    public class Mob : Entity
    {
        public Entity? target = null;
        public Mob(Point loc) : base(loc)
        {
            stats = new Dictionary<string, float>();
            data = new Dictionary<string, object>();
        }
        Color eyeC = Color.White;

        public override void Death(Entity killer) { }
        public override void Update() {}
        public Point[] PathFind(Point target) {
            //bool finished = false;
            Point[] path = Array.Empty<Point>();
            Point start = Game.PosToTile(loc);
            Point end = Game.PosToTile(target);

            int yDir = Math.Abs(end.Y - start.Y);
            int xDir = Math.Abs(end.X - start.X);
            if (yDir != 0) {
                yDir = (end.Y - start.Y) / yDir;
            }
            if (xDir != 0) {
                xDir = (end.X - start.X) / xDir;
            }

            if (yDir == 0 && xDir == 0)
            {
                return path;
            }
            else if (yDir == 0) {
                for (int x = start.X; x != end.X; x+=xDir) {
                    path = path.Append(new Point(x * Game.size, start.Y * Game.size)).ToArray();
                }
                return path;
            }
            else if (xDir == 0) {
                for (int y = start.Y; y != end.Y; y+=yDir) {
                    path = path.Append(new Point(start.X * Game.size, y * Game.size)).ToArray();
                }
                return path;
            }
            //Game.SendChatMessage("yDir="+yDir+",xDir="+xDir);

            for (int y = start.Y; y != end.Y; y+=yDir) {
                if (Game.world.GetTile(start.X, y) != 1)
                {
                    Point p = new Point(start.X * Game.size, y * Game.size);
                    path = path.Append(p).ToArray();
                }
            }
            for (int x = start.X; x != end.X; x += xDir)
            {
                if (Game.world.GetTile(x, end.Y) != 1)
                {
                    Point p = new Point(x * Game.size, start.Y * Game.size);
                    path = path.Append(p).ToArray();
                }
            }
            return path.Where(p => p != start).ToArray();
        }
        public override void Render(Graphics g) { }
    }

    public class Skeleton : Mob
    {
        public Skeleton(Point loc) : base(loc) {
            stats["hp"] = 20f;
            stats["atk"] = 1f;
            stats["spd"] = 1f;
            stats["maxhp"] = 20f;
            data["lastAttack"] = 0L;
            target = Game.player;
        }
        Color eyeC = Color.White;

        public override void Death(Entity killer) {
            if (killer is Player) {
                killer.stats["xp"] += 25f;
            }
            Game.entities.Remove(this);
        }
        public override void Update()
        {
            if (target == null)
            {
                return;
            }
            target = Game.player;
            Point[] path = PathFind(target.loc);
            Point p0;
            int ox = 0, oy = 0;

            Point tm = Game.PosToTile(loc);
            Point tilePlayer = Game.PosToTile(Game.player.loc);
            double dist = Math.Sqrt(Math.Pow(tm.Y - tilePlayer.Y, 2) + Math.Pow(tm.X - tilePlayer.X, 2));
            if (dist < 2 && (long)data["lastAttack"] + 2000L / stats["spd"] < DateTime.Now.Ticks / 10000 % 100000) {
                target.stats["hp"] -= stats["atk"];
                ox = target.loc.X - loc.X;
                if (ox != 0) ox /= -Math.Abs(ox);
                p0 = loc - (Size)target.loc;
                target.loc.Offset(p0.X * ox, p0.Y * ox);
                data["lastAttack"] = DateTime.Now.Ticks / 10000 % 100000;
            } /*else if (dist > 3) {
                ox = target.loc.X - loc.X;
                if (ox != 0) ox /= -Math.Abs(ox);
                p0 = loc - (Size)target.loc;
                loc.Offset(p0.X * ox, p0.Y * ox);
            }*/ else if (path.Length > 1) {
                p0 = Game.PosToTile(path[1] - (Size)path[0]);
                if (p0.X != 0 && path[0].X != Game.PosToTile(target.loc).X) { ox = p0.X / Math.Abs(p0.X); }
                if (p0.Y != 0 && path[0].Y != Game.PosToTile(target.loc).Y) { oy = p0.Y / Math.Abs(p0.Y); }
                loc.Offset(ox, oy);
            }
        }
        public override void Render(Graphics g) {
            var bmp = new Bitmap(Game.resImg["undead"]);
            bmp.MakeTransparent(Color.White);
            g.DrawImage(bmp, loc.X, loc.Y, Game.size, Game.size);
        }
    }


}