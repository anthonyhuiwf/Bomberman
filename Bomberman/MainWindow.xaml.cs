using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
 
namespace Group_Project
{

    public delegate void objectdispose(string type,int num);
    public interface Mobs
    {
        public void direction() {}
    }
    public class BMobs : Mapping, Mobs
    {

        public int x;
        public int[] dir;
        public bool hori = false;
        public bool vert = false;
        public int facing;           // 0 1 2 3 left right Up,down,
        public BMobs(int[] dir,int num) : base("blue_mobs") 
        {   this.num = num;
            this.dir = dir;
            direction();

        }
        public void direction()
        {
            Random random = new Random();
            if (dir[0] != 0 || dir[1] != 0 || dir[2] != 0 || dir[3] != 0)
            {
                while (true)
                {
                    x = random.Next(0, 3);
                    if (dir[x] == 1) break;
                }
                if (x == 2 || x == 3)
                {
                    vert = true;
                    facing = random.Next(2, 3);   //2-3
                }
                else
                {
                    hori = true;
                    facing = random.Next(0, 1);   //0-1
                }
            }
            else
            {
                x = random.Next(1, 2);
                if (x == 2)
                {
                    vert = true;
                    facing = random.Next(2, 3);   //2-3
                }
                if (x == 1)
                {
                    hori = true;
                    facing = random.Next(0, 1);   //0-1
                }
            }
        }
    }
    public class PMobs : Mapping, Mobs

    {
        public int x { get; private set; }
        public int[] dir;
        public bool hori = false;
        public bool vert = false;
        public int facing;                                                          // 0 1 2 3 left right Up,down,
        public PMobs(int[] dir, int num) : base("purple_mobs")
        {
            this.num = num;
            this.dir = dir;
            direction();
        }
        public void direction()
        {
            Random random = new Random();
            if (dir[0] != 0 || dir[1] != 0 || dir[2] != 0 || dir[3] != 0)
            {
              
                while (true)
                {
                    x = random.Next(0, 4);
                    if (dir[x] == 1) break;
                }
                if (x == 2 || x == 3)
                {
                    vert = true;
                    facing = random.Next(2, 4);   //2-3                             //UP /down
                }
                else
                {
                    hori = true;
                    facing = random.Next(0, 2);   //0-1                            //L/R
                }
            }
            else
            {
                x = random.Next(1, 2);
                if (x == 2)
                {
                    vert = true;
                    facing = random.Next(2, 4);   //2-3                             //Up/down
                }
                if (x == 1)
                {
                    hori = true;
                    facing = random.Next(0, 2);   //0-1                            //L/R
                }
            }
        }
    }
    public class Bomb : Mapping
    {
        DispatcherTimer bomb_timer = new DispatcherTimer();
        private objectdispose bombdispo;
        public Bomb(int num, objectdispose bombdispo) : base("bomb")
        {
            bomb_timer.Tick += bomb_expo;
            bomb_timer.Interval = new TimeSpan(0, 0, 2);
            bomb_timer.Start();
            this.num = num;
            this.bombdispo = bombdispo;
        }

        private void bomb_expo(object? sender, EventArgs e)
        {
            bomb_timer.Stop();
            bombdispo(this.Type,num);
        }
        public void immed_expo()
        {

            bomb_timer.Stop();
            bombdispo(this.Type, num);

        }
    }
    public class Flame : Mapping
    {
        DispatcherTimer flame_timer = new DispatcherTimer();
        private objectdispose flamedispose;
        public Flame(int num, objectdispose flamedispose) : base("flame")
        {
            flame_timer.Tick += flame_expo;
            flame_timer.Interval = new TimeSpan(0, 0, 1);
            flame_timer.Start();
            this.num = num;
            this.flamedispose = flamedispose;
        }

        private void flame_expo(object? sender, EventArgs e)
        {
            flame_timer.Stop();
            flamedispose(this.Type,num);

        }

    }
    public class Player:Mapping
    {

        public int range = 1;
        public int maxbomb = 1;                     //max.
        public int bomb_placed = 0;                 //no. placed
        public int bomb_count = 1;                  //enable
        public bool contains_key = false;
        public Player() : base("bomberman") { }
    }
    public class Mapping
    {
        public Image img { get; private set; }
        public string Type { get; private set; }
        public Rectangle rect { get; private set; }

        public int num { get; set; }
        public Mapping(string type)
        {
            Type = type;
            img = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/"+type+".jpg", UriKind.RelativeOrAbsolute)),
            };

            ImageBrush BodyBrush = new ImageBrush(img.Source);
            rect = new Rectangle
            {
                Tag = type,
                Height = 20,
                Width = 20,
                Fill = BodyBrush
            };
        }
    }

    public partial class MainWindow : Window
    {
        private static int row = 13;
        private static int col = 17;
        DispatcherTimer GameTimer = new DispatcherTimer();
        int GridSize = 20; // size of the each object
        uint XGridNum, YGridNum; 
        Random rand = new Random();
        Mapping?[,] map = new Mapping[row, col];
        Mapping? key;
        Mapping? door;
        int mobs_count=0;
        static int mobs_total=5;
        BMobs?[] bmobs = new BMobs[mobs_total];
        PMobs?[] pmobs = new PMobs[mobs_total];
        Mapping?[] items = new Mapping[item_total];
        Dictionary<int, Flame?> flames = new Dictionary<int, Flame?>();
        Player player = new Player();
        Bomb?[] bomb;
        Dictionary<int, Point> bomb_loc = new Dictionary<int, Point>();
        int wall_total = 17 * 11 / 3;
        int wall_count;
        static int item_total = 5;
        int item_count;
        int key_count;
        int door_count;
        int MoveCountdown;
        int BaseMoveCountdown;
        int Mobmovecountdown;
        int BaseMobmovecountdown;
        bool space;
        enum Direction
        {
            Up, Left, Down, Right, Null
        }
        Direction HeadDirection;

        public MainWindow()
        {
            InitializeComponent();
            GameTimer.Tick += GameLoop;
            GameTimer.Interval = TimeSpan.FromMilliseconds(10);
            Game_setup();
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            int Nx = 0, Ny = 0;
            Point playerL = player.rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));

            if (MoveCountdown <= 1)
            {

                if (HeadDirection == Direction.Left)
                {
                    if (playerL.X > 0)
                    {
                        Nx = (int)playerL.X - GridSize;
                        Ny = (int)playerL.Y;
                        if (Checkblock(Nx, Ny))
                            Move(player, Nx, Ny);
                    }
                }
                if (HeadDirection == Direction.Up)
                {
                    if (playerL.Y > 0)
                    {
                        Nx = (int)playerL.X;
                        Ny = (int)playerL.Y - GridSize;
                        if (Checkblock(Nx, Ny))
                            Move(player, Nx, Ny);
                    }
                }
                if (HeadDirection == Direction.Right)
                {
                    if (playerL.X < 320)
                    {
                        Nx = (int)playerL.X + GridSize;
                        Ny = (int)playerL.Y;
                        if (Checkblock(Nx, Ny))
                            Move(player, Nx, Ny);
                    }
                }
                if (HeadDirection == Direction.Down)
                {
                    if (playerL.Y < 260)
                    {
                        Nx = (int)playerL.X;
                        Ny = (int)playerL.Y + GridSize;
                        if (Checkblock(Nx, Ny))
                            Move(player, Nx, Ny);
                    }
                }
                if (space == true)
                {
                    Place_Bomb();
                    space = false;
                }
                Try_pickupitem((int)playerL.X, (int)playerL.Y);
                if (Mobmovecountdown <= 1)
                {
                    for (int i = 0; i < mobs_count; i++)
                    {
                        if (pmobs[i] != null)
                        {
                            Point pmobL = pmobs[i].rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                            Move(pmobs[i], (int)pmobL.X, (int)pmobL.Y);
                        }
                        if (bmobs[i] != null)
                        {
                            Point bmobL = bmobs[i].rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                            Move(bmobs[i], (int)bmobL.X, (int)bmobL.Y);
                        }
                    }
                    Mobmovecountdown = BaseMobmovecountdown;
                }
                HeadDirection = Direction.Null;
                MoveCountdown = BaseMoveCountdown;
                Mobmovecountdown--;    
            }
            MoveCountdown--;
        }
        private void Try_pickupitem(int row,int col)
        {
            int num=0;
            if (checkitem(row/GridSize,col / GridSize) >=0)
            {
                num = checkitem(row / GridSize, col / GridSize);

                GameCanvas?.Children.Remove(items[num].rect);
                if(items[num].Type=="More_bomb")
                {
                    player.bomb_count++;
                }
                else if(items[num].Type == "More_Range")
                {
                    player.range++;
                }

                items[num] = null;
                item_count--;
            }
            else if (checkitem(row / GridSize, col / GridSize) ==-1)
            {
                GameCanvas?.Children.Remove(key.rect);
                key = null;
                player.contains_key = true;
            }
            else if (checkitem(row / GridSize, col / GridSize) == -2)
            {

                if(player.contains_key) Gamewin();
            }

        }
        private void Gamewin()
        {
            GameTimer.Stop();
            MessageBox.Show("You Win!");
        }
        private void GameLose()
        {
            GameTimer.Stop();
            MessageBox.Show("You Lose!");
        }
        private void Place_Bomb()
        {
            if (player.bomb_placed < player.bomb_count)
            {
                int x = player.bomb_placed;
                for (int i=0; i < player.bomb_count; i++)
                {
                    if (bomb[x]==null)
                    {
                        Point playerL = player.rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                        bomb[x] = new Bomb(x, object_dispo);
                        AddToCanvas(bomb[x], (int)playerL.Y / GridSize, (int)playerL.X / GridSize);
                        player.bomb_placed++;
                        bomb_loc[x] = playerL;
                        return;
                    }

                }
            }
        }
        private void Game_setup()
        {
            HeadDirection = Direction.Null;
            XGridNum = (uint)(GameCanvas.Width / GridSize);
            YGridNum = (uint)(GameCanvas.Height / GridSize);
            GameCanvas.Focus();
            BaseMoveCountdown = 10;
            BaseMobmovecountdown = 4;
            Mobmovecountdown = BaseMobmovecountdown;
            MoveCountdown = BaseMoveCountdown;
            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 17; j++)
                { if (i == 0 || i == 12)      //Top bottom
                    {
                        map[i, j] = new Mapping("Explosion-proof");
                        AddToCanvas(map[i, j], i, j);
                    }
                    else if (i % 2 == 0)         // ODD
                    {
                        if (j % 2 != 0)
                        {
                            map[i, j] = new Mapping("Explosion-proof");
                            AddToCanvas(map[i, j], i, j);
                        }
                    }

                }

            }
            int _row = 0;
            int _col = 0;
            while (wall_count < wall_total || item_count < item_total || mobs_count < mobs_total)
            { 
                _row = rand.Next(1,row-1);
                _col = rand.Next(1,col-1);
                    if (map[_row, _col] == null)
                    {
                        if (wall_count < wall_total)
                        {
                            if (key_count < 1)
                            {
                                key = new Mapping("key");
                                AddToCanvas(key, _row, _col);
                                key_count++;
                            }
                            else if (door_count < 1)
                            {
                                door = new Mapping("door");
                                AddToCanvas(door, _row, _col);
                                door_count++;
                            }
                            else if (item_count < item_total)
                            {
                                Items_factory(_row, _col);
                            }
                            map[_row, _col] = new Mapping("Brick");
                            AddToCanvas(map[_row, _col], _row, _col);
                            wall_count++;
                        }
                        else
                        {
                            if(mobs_count < mobs_total)
                            {
                                int[] dir;
                                dir=checkdirection(_row, _col);
                                Mobs_factory(dir, _row, _col);
                            }

                        }
                    }
            }
            Panel.SetZIndex(player.rect, 1);
            AddToCanvas(player, 1, 0);
            bomb = new Bomb[player.maxbomb];
            GameTimer.Start();
        }
        private void CanvasKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Left))
                HeadDirection = Direction.Left;
            if ((e.Key == Key.Up))
                HeadDirection = Direction.Up;
            if ((e.Key == Key.Right))
                HeadDirection = Direction.Right;
            if ((e.Key == Key.Down))
                HeadDirection = Direction.Down;
            if (e.Key == Key.Space) space = true;

        }
        private void AddToCanvas(Mapping? obj, int row, int col)
        {
                GameCanvas.Children.Add(obj.rect);
                Canvas.SetTop(obj.rect, GridSize * row);
                Canvas.SetLeft(obj.rect, GridSize * col);
        }

        private bool check_bomb(int row,int col)
        {
            for (int i = 0; i < player.bomb_placed; i++)
            {
                if (bomb[i] != null)
                {
                    if (bomb_loc[i].X == row && bomb_loc[i].Y == col)
                        return false;
                }

            }
            return true;
        }
        private bool check_flames(int row ,int col)
        {
            foreach (KeyValuePair<int, Flame?> flame in flames)
            {
                if (flame.Value != null)
                {
                    Point flameL = flame.Value.rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                    if (flameL.X == row && flameL.Y == col)
                        return true;
                }
            }
            return false;
        }
        private bool Checkblock(int row, int col)
        {
            int x = player.bomb_placed;
            if (map[col / GridSize, row / GridSize] == null)
            {
                if(!check_bomb(row,col)) return false;
                if(check_flames(row,col)) GameLose();
                for (int i = 0; i < mobs_count; i++)
                {
                    if (pmobs[i] != null)
                    {
                        Point pmobL = pmobs[i].rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                        if (pmobL.X == row && pmobL.Y == col)
                            GameLose();
                    }
                    if (bmobs[i] != null)
                    {
                        Point bmobL = bmobs[i].rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                        if (bmobL.X == row && bmobL.Y == col)
                            GameLose();
                    }

                }
                return true;
            }
            return false;
        }
        private bool Checkblock(Mapping? obj,int row, int col)
        {
            int x = player.bomb_placed;
            if (map[col / GridSize, row / GridSize] == null)
            {
                if (!check_bomb(row, col)) return false;
                if(check_flames(row, col))
                {
                    GameCanvas?.Children.Remove(obj.rect);
                    if (obj.Type == "blue_mobs") bmobs[obj.num] = null;
                    if (obj.Type == "purple_mobs") pmobs[obj.num] = null;
                    return false;
                };
                Point playerL = player.rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                if (playerL.X == row && playerL.Y == col) GameLose();
                return true;
            }
            return false;
        }
        private void Move(Mapping obj, int row, int col)
        {
            Canvas.SetLeft(obj.rect, row );
            Canvas.SetTop(obj.rect, col );
        }
        private void Move(BMobs obj, int row, int col)
        {

            if (obj.vert)
            {
                if(obj.facing == 2)  //Up
                {
                    if(col > 0)
                    {
                        if(Checkblock(obj, row , col - GridSize))
                        {
                            Canvas.SetLeft(obj.rect, row);
                            Canvas.SetTop(obj.rect, col - GridSize);
                        }
                        else
                        {
                            obj.facing = 3;
                        }
                    }
                    else 
                    {
                        obj.facing = 3;
                    }
                }
                if (obj.facing == 3)  //down
                {
                    if(col < 260)
                    {
                        if (Checkblock(obj, row , col + GridSize))
                        {
                            Canvas.SetLeft(obj.rect, row );
                            Canvas.SetTop(obj.rect, col + GridSize);
                        }
                        else
                        {
                            obj.facing = 2;
                        }
                    }

                    else
                    {
                        obj.facing = 2;
                    }
                }
            }
            else
            {
                if (obj.facing == 0)  //left
                {
                    if(row > 0)
                    {
                        if (Checkblock(obj, row - GridSize, col ))
                        {
                            Canvas.SetLeft(obj.rect, row - GridSize);
                            Canvas.SetTop(obj.rect, col);
                        }
                        else
                        {
                            obj.facing = 1;
                        }
                    }

                    else
                    {
                        obj.facing = 1;
                    }
                }
                if (obj.facing == 1)  //right
                {
                    if(row < 320)
                    {
                        if (Checkblock(obj, row + GridSize, col ))
                        {
                            Canvas.SetLeft(obj.rect, row + GridSize);
                            Canvas.SetTop(obj.rect, col );
                        }
                        else
                        {
                            obj.facing = 0;
                        }
                    }

                    else
                    {
                        obj.facing = 0;
                    }
                }
            }
        }
        private void Move(PMobs obj,int row, int col)
        {   Random rand = new Random();
            if (obj.facing == 0)  //left
            {
                if (row > 0)
                {
                    if (Checkblock(obj, row - GridSize, col))
                    {
                        Canvas.SetLeft(obj.rect, row - GridSize);
                        Canvas.SetTop(obj.rect, col);
                    }
                    else
                    {
                        obj.facing = rand.Next(0,4);
                    }
                }

                else
                {
                    obj.facing = rand.Next(0, 4); 
                }
            }
            if (obj.facing == 1)  //right
            {
                if (row < 320)
                {
                    if (Checkblock(obj, row + GridSize, col))
                    {
                        Canvas.SetLeft(obj.rect, row + GridSize);
                        Canvas.SetTop(obj.rect, col);
                    }
                    else
                    {
                        obj.facing = rand.Next(0, 4);
                    }
                }

                else
                {
                    obj.facing = rand.Next(0, 4);
                }
            }
            if (obj.facing == 2)  //Up
            {
                if (col > 0)
                {
                    if (Checkblock(obj, row, col - GridSize))
                    {
                        Canvas.SetLeft(obj.rect, row);
                        Canvas.SetTop(obj.rect, col - GridSize);
                    }
                    else
                    {
                        obj.facing = rand.Next(0, 4);
                    }
                }
                else
                {
                    obj.facing = rand.Next(0, 4);
                }
            }
            if (obj.facing == 3)  //down
            {
                if (col < 260)
                {
                    if (Checkblock(obj, row, col + GridSize))
                    {
                        Canvas.SetLeft(obj.rect, row);
                        Canvas.SetTop(obj.rect, col + GridSize);
                    }
                    else
                    {
                        obj.facing = rand.Next(0, 4);
                    }
                }

                else
                {
                    obj.facing = rand.Next(0, 4);
                }
            }
        }
        public void object_dispo(string type,int num)
        {

            if (type == "bomb")
            {
                GameCanvas?.Children.Remove(bomb[num].rect);
                bomb[num] = null;
                player.bomb_placed--;
                explosion(bomb_loc[num], num);
            }
            if (type == "flame")
            {   
                GameCanvas?.Children.Remove(flames[num].rect);
                flames[num] = null;
            }
        }

        private int checkitem(int row , int col)
        {
            if (key != null)
            {
                Point keyL = key.rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                if (keyL.X == row * GridSize && keyL.Y == col * GridSize) return -1;
            }
            Point doorL = door.rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
            if (doorL.X == row * GridSize && doorL.Y == col * GridSize) return -2;
            for (int i = 0; i < item_total ; i++)
            {
                if (items[i] != null)
                {
                    Point itemL = items[i].rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
                    if (itemL.X == row * GridSize && itemL.Y == col * GridSize)
                    {
                        return i;
                    }
                }

            }

            return -3;
        }
        private int[] checkdirection(int _row,int _col)
        {
            int[] dir = { 0, 0, 0, 0 };       //left,right,Up,down,
            if (map[_row + 1, _col] == null)
            {
                dir[3] = 1;
            }
            if (map[_row - 1, _col] == null)
            {
                dir[2] = 1;
            }
            if (map[_row, _col + 1] == null)
            {
                dir[1] = 1;
            }
            if (map[_row, _col - 1] == null)
            {
                dir[0] = 1;
            }
            return dir;
        }
        private void bomb_collin(int num,int row,int col)
        {
            for (int i = 0; i < player.bomb_count; i++)
            {
                if (i != num && bomb[i] != null)
                {
                    if (bomb_loc[i].X == row * GridSize && bomb_loc[i].Y == col * GridSize)
                    bomb[i]?.immed_expo();
                }
            }
        }
        private bool check_player(int row, int col)
        {
            Point playerL = player.rect.TransformToAncestor(GameCanvas).Transform(new Point(0, 0));
            if (playerL.X == row * GridSize && playerL.Y == col * GridSize)
                return true;
            return false;
        }
        private void Mobs_factory(int[] dir,int _row,int _col)
        {   Random rand = new Random();
            if(rand.Next(0,10)>=7)
            {
                pmobs[mobs_count] = new PMobs(dir, mobs_count);
                AddToCanvas(pmobs[mobs_count], _row, _col);
            }
            else 
            {
                bmobs[mobs_count] = new BMobs(dir, mobs_count);
                AddToCanvas(bmobs[mobs_count], _row, _col);
            }
            mobs_count++;
        }

        private void Items_factory(int _row, int _col)
        {
            if ((_row + _col) % 2 == 0)
            {
                items[item_count] = new Mapping("More_bomb");
                player.maxbomb++;
            }
            else
            {
                items[item_count] = new Mapping("More_Range");
            }

            AddToCanvas(items[item_count], _row, _col);
            item_count++;
        }

        private void explosion(Point bombL,int num)
        {
            int[] X = { 0, 1, 0, -1 };
            int[] Y = { 1, 0, -1, 0 };
            bool[] check_dir = { true, true, true, true };
            flames[num] = new Flame(num, object_dispo);
            Mapping? check;
            if(checkitem((int)bombL.X / GridSize, (int)bombL.Y / GridSize) >= 0 && item_count>=0)
            {
                int x = checkitem((int)bombL.X / GridSize, (int)bombL.Y / GridSize);
                GameCanvas?.Children.Remove(items[x].rect);
                items[x] = null;
                item_count--;
            }
            AddToCanvas(flames[num], (int)bombL.Y / GridSize, (int)bombL.X / GridSize);
            if(check_player((int)bombL.X / GridSize, (int)bombL.Y / GridSize)) GameLose();//center
            for (int i = 1; i <= player.range; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    if (check_dir[j])
                    {
                        int temp_row = (int)bombL.X / GridSize + i * X[j];
                        int temp_col = (int)bombL.Y / GridSize + i * Y[j];
                        if (temp_row < 0 || temp_col < 0 || temp_row > 17 || temp_col > 13) break;                                                // skip margin
                        if (map[temp_col, temp_row] != null)
                        {
                            check = map[temp_col, temp_row];
                            if (check.Type == "Brick")
                            {
                                check_dir[j] = false;
                                GameCanvas?.Children.Remove(check.rect);
                                map[temp_col, temp_row] = null;
                                int mapIndex = flames.Count * 1000 +1;
                                flames[mapIndex] = new Flame(mapIndex, object_dispo);
                                AddToCanvas(flames[mapIndex], temp_col, temp_row);

                            }
                            else if (check.Type == "Explosion-proof")
                            {
                                check_dir[j] = false;
                            }

                        }
                        else                                                                                                            // empty space
                        {
                            if (checkitem(temp_row, temp_col) >= 0 && item_count >= 0)
                            {
                                int x = checkitem(temp_row, temp_col);
                                GameCanvas?.Children.Remove(items[x].rect);
                                items[x] = null;
                                item_count--;
                            }
                            bomb_collin(num, temp_row, temp_col);                                                                       //immed expo
                            if(check_player(temp_row, temp_col)) GameLose();
                            int mapIndex = flames.Count*1000+1;
                            flames[mapIndex] = new Flame(mapIndex, object_dispo);
                            AddToCanvas(flames[mapIndex], temp_col, temp_row);

                        }
                    }
                }

            }
        }
    }
}
