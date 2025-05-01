using System;
using System.Drawing;
using System.Net.Http.Json;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace WestDungeon
{
    public class Entity
    {
        public PointF loc;
        public Vector2 velocity;
        public Dictionary<string, float> stats;
        public Dictionary<string, object> data;
        public static double calcLevel(double xp)
        {
            return Math.Pow(xp/2, 0.5) + 1;
        }
        public Entity(PointF location) {
            loc = location;
        }
        public virtual void Death(Entity killer) { ((Func<object>)data["deathEvent"]).DynamicInvoke(killer); }
        public virtual void Update() { }
        public virtual void Render(Graphics g) { g.DrawImage((Image)data["img"], loc); }

        public static Entity ReadFromFile(string file_path, string entity_name, PointF loc)
        {
            Entity enFF = new Entity(loc);
            string json = File.ReadAllText(file_path);
            Entity enLoaded = JsonSerializer.Deserialize<Dictionary<string, Entity>>(json)[entity_name];
            if (enLoaded == null) {
                throw new Exception("Couldn't read entity");
            }
            enFF.stats = enLoaded.stats;
            enFF.data["img"] = enLoaded.data["img"];
            return enFF;
        }
    }

    public class Mob : Entity
    {
        public Entity? target = null;
        public Mob(PointF loc) : base(loc)
        {
            stats = new Dictionary<string, float>();
            data = new Dictionary<string, object>();
        }
        public virtual void Attack(Mob target) {
            target.stats["hp"] -= stats["atk"];
        }

        public override void Death(Entity killer) { }
        public override void Update() {
            if (!stats.ContainsKey("xp"))
            {
                if (stats["hp"] < 1)
                {
                    Death(Game.player);
                    return;
                }

                if (target == null)
                {
                    target = Game.player;
                    return;
                }

                double dist = GameMath.Distance(GameMath.PosToTile(loc), GameMath.PosToTile(target.loc));
                if (dist <= 3)
                {
                    PointF slope = target.loc;
                    slope.X -= loc.X;
                    slope.Y -= loc.Y;
                    loc += (SizeF)GameMath.Normalize(slope).ToVector2();
                    if (dist <= 1.5 && DateTime.UtcNow.Ticks - (long)data["lastAttack"] >= 5L)
                    {
                        data["lastAttack"] = DateTime.UtcNow.Ticks;
                        Game.player.stats["hp"] -= 1;
                        Game.player.loc += (SizeF)GameMath.Normalize(slope).ToVector2();
                    }
                }
            }
        }
        public PointF[] PathFind(PointF target) {
            PointF[] path = Array.Empty<PointF>();
            PointF start = GameMath.PosToTile(loc);
            PointF end = GameMath.PosToTile(target);

            float yDir = Math.Abs(end.Y - start.Y);
            float xDir = Math.Abs(end.X - start.X);
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
                for (float x = start.X; x != end.X; x+=xDir) {
                    path = path.Append(new PointF(x * Game.size, start.Y * Game.size)).ToArray();
                }
                return path;
            }
            else if (xDir == 0) {
                for (float y = start.Y; y != end.Y; y+=yDir) {
                    path = path.Append(new PointF(start.X * Game.size, y * Game.size)).ToArray();
                }
                return path;
            }

            for (float y = start.Y; y != end.Y; y+=yDir) {
                if (Game.world.GetTile(start.X, y) != 1)
                {
                    PointF p = new PointF(start.X * Game.size, y * Game.size);
                    path = path.Append(p).ToArray();
                }
            }
            for (float x = start.X; x != end.X; x += xDir)
            {
                if (Game.world.GetTile(x, end.Y) != 1)
                {
                    PointF p = new PointF(x * Game.size, start.Y * Game.size);
                    path = path.Append(p).ToArray();
                }
            }
            return path.Where(p => p != start).ToArray();
        }
        public override void Render(Graphics g) {
            //MathF.Floor(loc.X / Game.size) * Game.size
            g.DrawImage((Image)data["img"], loc.X, loc.Y, (float)Game.size, (float)Game.size);
            RectangleF rect = new RectangleF(loc, new SizeF(Game.size, 5));
            if (stats["hp"] < stats["maxhp"]) g.FillRectangle(Game.grayBr, rect);
            rect.Width = (int)Math.Floor(stats["hp"] / stats["maxhp"] * Game.size);
            g.FillRectangle(Game.redBr, rect);
        }
    }

    public class Skeleton : Mob
    {
        public Skeleton(PointF loc) : base(loc)
        {
            stats["hp"] = 20f;
            stats["atk"] = 1f;
            stats["spd"] = 1f;
            stats["maxhp"] = 20f;
            stats["level"] = 1;
            data["lastAttack"] = 0L;
            data["img"] = Game.resImg["skeleton"];
            target = Game.player;
        }

        public override void Death(Entity killer)
        {
            if (killer.stats.ContainsKey("xp"))
            {
                killer.stats["xp"] += 5f * stats["level"];
            }
            Game.SendChatMessage("The player has gained " + 5 * stats["level"] + " xp!");
            Game.entities = Game.entities.Where(en => en != this).ToList();
            Skeleton w = new Skeleton(loc);
            w.stats["level"] = killer.stats["level"]+1;
            Game.entities.Add(w);
        }
        public override void Update() {
            if (stats.ContainsKey("xp"))
            {
                if (stats["level"] < (int)calcLevel(stats["xp"]))
                {
                    int lvl = (int)calcLevel(stats["xp"]);
                    Game.SendChatMessage("The player has leveled up to level " + lvl + " from level " + stats["level"]);
                    stats["maxhp"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["hp"] = stats["maxhp"];
                    stats["atk"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["level"] = lvl;

                    if (stats["level"] >= 5)
                    {
                        string[] options = { "Ghoul", "Bone Knight" };
                        string choice = WestGUI.Show("Evolution", "Do you want to evolve into a Ghoul or a Bone Knight?", options);
                        if (choice == "Ghoul") {
                            Ghoul evol = new Ghoul(loc);
                            evol.stats = stats;
                            evol.stats["atk"] = 5 * stats["atk"];
                            evol.stats["maxhp"] = 5 * stats["maxhp"];
                            evol.stats["hp"] = stats["hp"];
                            evol.stats["spd"] = 10f;
                            evol.data["weapon"] = data["weapon"];
                            Game.player = evol;
                        } else if (choice == "Bone Knight")
                        {
                            BoneKnight evol = new BoneKnight(loc);
                            evol.stats = stats;
                            evol.stats["atk"] = 5 * stats["atk"];
                            evol.stats["maxhp"] = 5 * stats["maxhp"];
                            evol.stats["hp"] = stats["hp"];
                            evol.stats["spd"] = 10f;
                            evol.data["weapon"] = data["weapon"];
                            Game.player = evol;
                        }
                    }
                }
            }
            base.Update();
        }
        public override void Render(Graphics g) {
            base.Render(g);
        }
    }
    //level 5 evolutions
    public class Ghoul : Mob
    {
        public Ghoul(PointF loc) : base(loc)
        {
            stats["hp"] = 80f;
            stats["atk"] = 4f;
            stats["spd"] = 1f;
            stats["maxhp"] = 80f;
            stats["level"] = 5;
            data["lastAttack"] = 0L;
            data["img"] = Game.resImg["ghoul"];
            target = Game.player;
        }

        public override void Death(Entity killer) {
            if (killer.stats.ContainsKey("xp")) {
                killer.stats["xp"] += 25f * stats["level"];
            }
            Game.SendChatMessage("The player has gained " + 25 * stats["level"] + " xp!");
            Game.entities = Game.entities.Where(en => en != this).ToList();
            Ghoul w = new Ghoul(loc);
            w.stats["level"] = killer.stats["level"] + 1;
            Game.entities.Add(w);
        }
        public override void Update() {
            if (stats.ContainsKey("xp"))
            {
                if (stats["level"] < (int)calcLevel(stats["xp"]))
                {
                    int lvl = (int)calcLevel(stats["xp"]);
                    Game.SendChatMessage("The player has leveled up to level " + lvl + " from level " + stats["level"]);
                    stats["maxhp"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["hp"] = stats["maxhp"];
                    stats["atk"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["level"] = lvl;

                    if (stats["level"] >= 15)
                    {
                        if (MessageBox.Show("Do you want to evolve into a wraith?", "Evolution", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Wraith evol = new Wraith(loc);
                            evol.stats = stats;
                            evol.stats["atk"] = 3 * stats["atk"];
                            evol.stats["maxhp"] = 3 * stats["maxhp"];
                            evol.stats["hp"] = stats["hp"];
                            evol.data["weapon"] = data["weapon"];
                            Game.player = evol;
                        }
                    }
                }
            }
            base.Update();
        }
        public override void Render(Graphics g)
        {
            base.Render(g);
        }
    }

    public class BoneKnight : Mob
    {
        public BoneKnight(PointF loc) : base(loc)
        {
            stats["hp"] = 80f;
            stats["atk"] = 4f;
            stats["spd"] = 1f;
            stats["maxhp"] = 80f;
            stats["level"] = 5;
            data["lastAttack"] = 0L;
            data["img"] = Game.resImg["bone_knight"];
            target = Game.player;
        }

        public override void Death(Entity killer)
        {
            if (killer.stats.ContainsKey("xp"))
            {
                killer.stats["xp"] += 25f * stats["level"];
            }
            Game.SendChatMessage("The player has gained " + 25 * stats["level"] + " xp!");
            Game.entities = Game.entities.Where(en => en != this).ToList();
            Ghoul w = new Ghoul(loc);
            w.stats["level"] = killer.stats["level"] + 1;
            Game.entities.Add(w);
        }
        public override void Update()
        {
            if (stats.ContainsKey("xp"))
            {
                if (stats["level"] < (int)calcLevel(stats["xp"]))
                {
                    int lvl = (int)calcLevel(stats["xp"]);
                    Game.SendChatMessage("The player has leveled up to level " + lvl + " from level " + stats["level"]);
                    stats["maxhp"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["hp"] = stats["maxhp"];
                    stats["atk"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["level"] = lvl;

                    if (stats["level"] >= 15)
                    {
                        if (MessageBox.Show("Do you want to evolve into a wraith?", "Evolution", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            Wraith evol = new Wraith(loc);
                            evol.stats = stats;
                            evol.stats["atk"] = 3 * stats["atk"];
                            evol.stats["maxhp"] = 3 * stats["maxhp"];
                            evol.stats["hp"] = stats["hp"];
                            evol.data["weapon"] = data["weapon"];
                            Game.player = evol;
                        }
                    }
                }
            }
            base.Update();
        }
        public override void Render(Graphics g)
        {
            base.Render(g);
        }
    }

    public class Wraith : Mob
    {
        public Wraith(PointF loc) : base(loc)
        {
            stats["hp"] = 80f;
            stats["atk"] = 4f;
            stats["spd"] = 1f;
            stats["maxhp"] = 80f;
            stats["level"] = 15;
            data["lastAttack"] = 0L;
            data["img"] = Game.resImg["wraith"];
            target = Game.player;
        }

        public override void Death(Entity killer)
        {
            if (killer.stats.ContainsKey("xp"))
            {
                killer.stats["xp"] += 75f * stats["level"];
            }
            Game.SendChatMessage("The player has gained " + 75 * stats["level"] + " xp!");
            Game.entities = Game.entities.Where(en => en != this).ToList();
            Wraith w = new Wraith(loc);
            w.stats["level"] = killer.stats["level"] + 1;
            Game.entities.Add(w);
        }
        public override void Update()
        {
            if (stats.ContainsKey("xp"))
            {
                if (stats["level"] < (int)calcLevel(stats["xp"]))
                {
                    int lvl = (int)calcLevel(stats["xp"]);
                    Game.SendChatMessage("The player has leveled up to level " + lvl + " from level " + stats["level"]);
                    stats["maxhp"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["hp"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["atk"] *= (float)Math.Pow(1.2f, lvl - stats["level"]);
                    stats["level"] = lvl;

                    if (stats["level"] >= 30)
                    {
                        if (MessageBox.Show("Do you want to evolve into a ghoul?", "Evolution", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            stats["hp"] *= 3;
                            stats["atk"] *= 3;
                            //stats["spd"] *= 3;

                        }
                    }
                }
            }
            base.Update();
        }
        public override void Render(Graphics g)
        {
            base.Render(g);
        }
    }
}