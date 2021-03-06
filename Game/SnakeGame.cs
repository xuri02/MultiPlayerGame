﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameFramework;
using OpenTKFramework.Framework;

namespace Game {
    class SnakeGame {
        public const  int MultiScale = 30;
        private const int Start      = 0;

        private static readonly Rectangle[] Head = { new Rectangle( Start * MultiScale, Start * MultiScale, MultiScale, MultiScale ) };

        private Point     _food              = new Point( 5 * MultiScale, 5 * MultiScale );
        private Direction _forwardDirection  = Direction.S;
        private bool      _isRunning         = true;
        private Direction _quarriedDirection = Direction.S;
        private int       _round;

        private List<Rectangle> _snakeList = new List<Rectangle>( Head );
        private double          _speed     = 10;

        private int _tick;

        private Size size;
        //Rectangle    test = new Rectangle();

        public void SetSize(Size s) {
            this.size = new Size( (int)( s.Width +MultiScale), (int) ( s.Height * ( 1 + ( 1 - this.h ) ) ) );
            Console.WriteLine( this.size );
        }

        float h = (float) Math.Sqrt( (double) 3 / 4 ) * 1;

        public SnakeGame(Size s) {
            this.SetSize( s );

            for ( var j = 0; j < 4; j++ ) this._snakeList.Add( Head[0] );

            ResultSwitch( DialogResult.Ignore );
        }

        public bool GameUpdate_Tick() {
            if ( !this._isRunning ) return false;
            this._tick++;

            try {
                if ( this._tick % (int) this._speed != 0 ) return true;
            } catch { }

            this._forwardDirection = this._quarriedDirection;
            return SnakeMove();
        }

        public bool SnakeMove() {
            this._snakeList.RemoveAt( 0 );
            if ( AddInFront() ) return true;

            this._isRunning = false;
            return false;
        }

        public void ResultSwitch(DialogResult result) { // ReSharper disable once SwitchStatementMissingSomeCases
            switch (result) {
                case DialogResult.Retry: break;
                case DialogResult.Abort:
                    Environment.Exit( 0 );
                    break;
                case DialogResult.Ignore: {
                    var count = this._snakeList.Count;
                    this._snakeList = new List<Rectangle>( Head );

                    int maxSquare = 30; //(int) ( this.size.Height/MultiScale * h ) ;

                    for ( var i = 0; i < 999; i++ ) {
                        this._forwardDirection = Direction.S;

                        for ( var j = 0; j < maxSquare; j++ ) {
                            AddInFront();
                            count--;
                            if ( count <= 0 ) goto LastStep;
                        }

                        this._forwardDirection = Direction.O;

                        AddInFront();
                        count--;
                        if ( count <= 0 ) goto LastStep;

                        this._forwardDirection = Direction.N;

                        for ( var j = 0; j < maxSquare; j++ ) {
                            AddInFront();
                            count--;
                            if ( count <= 0 ) goto LastStep;
                        }

                        this._forwardDirection = Direction.O;

                        AddInFront();
                        count--;
                        if ( count <= 0 ) goto LastStep;
                    }

                    LastStep:

                    this._isRunning         = true;
                    this._forwardDirection  = Direction.O;
                    this._quarriedDirection = this._forwardDirection;

                    break;
                }
            }
        }

        public bool AddInFront() {
            lock (this._snakeList) {
                if ( this._snakeList.Count == 0 ) return false;
                var cur = this._snakeList[this._snakeList.Count - 1];

                var rec = new Rectangle( Point.Empty, new Size( MultiScale, MultiScale ) );

                switch (this._forwardDirection) {
                    case Direction.W:
                        rec.X = cur.X - MultiScale;
                        rec.Y = cur.Y;
                        break;
                    case Direction.O:
                        rec.X = cur.X + MultiScale;
                        rec.Y = cur.Y;
                        break;
                    case Direction.N:
                        rec.Y = cur.Y - MultiScale;
                        rec.X = cur.X;
                        break;
                    case Direction.S:
                        rec.Y = cur.Y + MultiScale;
                        rec.X = cur.X;
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

                var t = IsSave( rec );
                if ( !t ) return false;

                this._snakeList.Add( rec );
                return true;
            }
        }

        public bool IsSave(Rectangle rec, bool isFood = false) {
            if ( rec.X >= this.size.Width  || rec.X < 0 ) return false;
            if ( rec.Y >= this.size.Height || rec.Y < 0 ) return false;

            if ( this._snakeList.TakeWhile( (t, index) => index < this._snakeList.Count ).Any( r => r.X == rec.X && r.Y == rec.Y ) ) return false;

            if ( isFood ) return true;
            if ( this._food.X != rec.X || this._food.Y != rec.Y ) return true;
            GenerateNewFood();
            AddInFront();
            this._round++;

            return true;
        }

        public void GenerateNewFood() {
            var rnd = new Random();

            var pnd = Point.Empty;

            for ( int i = 0; i < 100; i++ ) {
                pnd.X = rnd.Next( 0, this.size.Width  / MultiScale ) * MultiScale;
                pnd.Y = rnd.Next( 0, this.size.Height / MultiScale ) * MultiScale;

                if ( IsSave( new Rectangle( pnd, new Size( MultiScale, MultiScale ) ) ) ) break;
            }

            this._speed *= 0.9;
            this._food  =  pnd;
        }

        public PointF MapXPointF(Point p) {
            var x = this.h * p.Y;
            var y = p.X + ( ( (int) ( p.Y / MultiScale ) % 2 ) == 0 ? 0 : .5f ) * MultiScale;
            return new PointF( y, x );
        }

        public void PaintEventHandler(object sender, GraphicsManager e) {
            //for ( int i = 0; i < this.size.Width; i += MultiScale ) {
            //    for ( int j = 0; j < this.size.Height; j += MultiScale ) {
            //        e.DrawTriangle( new PointF( i, j ), MultiScale, Color.Blue );
            //    }
            //}

            for ( int i = 0; i < this.size.Width; i += MultiScale ) {
                for ( int j = 0; j < this.size.Height; j += MultiScale ) {
                    e.DrawTriangle( MapXPointF( new Point( i, j ) ), MultiScale, Color.FromArgb( 0, 0, 150 ) );
                    e.DrawTriangle( MapXPointF( new Point( i, j ) ), MultiScale, Color.FromArgb( 0, 0, 180 ), true );
                }
            }

            var rd = new Rectangle();
            var p  = new PointF();

            for ( var i = 0; i < this._snakeList.Count; i++ ) {
                rd = this._snakeList[i];
                p  = MapXPointF( new Point( rd.X, rd.Y ) );
                e.DrawTriangle( p, MultiScale, Color.Red );
                //e.DrawCycle( p,MultiScale/4,Color.Black, 10 );

                //e.DrawTriangle( p, MultiScale, Color.Red,true );

                //e.DrawRect( r, Color.Red );
                //e.DrawRect( r, Color.Black, OpenTK.Graphics.OpenGL.PrimitiveType.LineLoop );
            }

            rd = this._snakeList[this._snakeList.Count - 1];
            p  = MapXPointF( new Point( rd.X, rd.Y ) );
            e.DrawTriangle( p, MultiScale, Color.DarkRed );

            //e.DrawRect( this._snakeList[this._snakeList.Count - 1], Color.DarkRed );

            //e.DrawRect( this.test, Color.OliveDrab );

            e.DrawTriangle( MapXPointF( this._food ), MultiScale, Color.Green );
        }

        public void ClientKeyPress(object sender, OpenTK.KeyPressEventArgs e) {
            var code = e.KeyChar.ToString().ToUpper();
            //left or right
            if ( this._forwardDirection == Direction.N || this._forwardDirection == Direction.S ) {
                if ( code == Keys.Left.ToString() || code == Keys.D.ToString() ) this._quarriedDirection = Direction.O;

                if ( code == Keys.Right.ToString() || code == Keys.A.ToString() ) this._quarriedDirection = Direction.W;
            }
            //up or down
            else {
                if ( code == Keys.Up.ToString() || code == Keys.W.ToString() ) this._quarriedDirection = Direction.N;

                if ( code == Keys.Down.ToString() || code == Keys.S.ToString() ) this._quarriedDirection = Direction.S;
            }
        }

        public enum Direction {
            N,
            S,
            O,
            W
        }
    }
}