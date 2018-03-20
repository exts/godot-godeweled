using Godot;
using System;

namespace scripts
{

    public enum TileBG {
        DEFAULT,
        HINT,
        SELECTED,
        SUCCESS
    }

    public class TileScene : Area2D
    {
        /**
         * Used to keep track of where this tile is located
         */
        public int Id = -1;
        public int Row = -1;
        public int Col = -1;

        public const int Width = 276;
        public const int Height = 276;
        public const float Padding = 2f;
        public const float TileScale = .25f;

        [Export] public float Velocity = 200f;

        // protected Random rand = new Random();

        public override string ToString()
        {
            //change position
            var offset = new Vector2(80, 100);
            var xpos = Row * ((Width * TileScale) / 2);
            var ypos = Col * ((Height * TileScale) / 2);

            var debug = $"Id: {Id}, Row: {Row}, Col: {Col}, Height: {Height}, Width: {Width}, TileScale: {TileScale}, ";
            debug += $"xpos: {xpos}, ypos: {ypos}, offset: {offset.x},{offset.y}, xposo: {xpos+offset.x}, yposo: {ypos+offset.y}";

            return debug;
        }

        public override void _Ready()
        {
            
        }

        public Vector2 GetScaledDimensions()
        {
            var wscale = Width * TileScale;
            var hscale = Height * TileScale;

            return new Vector2(wscale, hscale);
        }

        public void SetOffsetPosition(Vector2 offset, int col, int row, out Vector2 original_pos)
        {
            //change mode
            // this.Mode = RigidBody2D.ModeEnum.Kinematic;

            //change position
            var tile_size = GetScaledDimensions();
            var xpos = tile_size.x * col + (tile_size.x / 2);
            var ypos = tile_size.y * row + (tile_size.y / 2);
            var cur_offset = new Vector2(offset.x, offset.y);

            //padding?
            if(col > 0) {
                xpos += Padding * col;
            }

            cur_offset.x += xpos;
            cur_offset.y += ypos;
            
            this.Position = cur_offset;
            original_pos = cur_offset;
        }

        public void SetRandomGem(string Gem)
        {
            var gem = GetNode("GemSprite") as AnimatedSprite;
            gem.Animation = Gem;
        }

        public void SetBackground(TileBG background)
        {
            var bg = "";
            switch(background)
            {
                case TileBG.HINT:
                    bg = "hint";
                break;
                case TileBG.SELECTED:
                    bg = "selected";
                    break;
                case TileBG.SUCCESS:
                    bg = "success";
                    break;
                case TileBG.DEFAULT:
                default:
                    bg = "default";
                break;
            }

            var tilebg = GetNode("TileBG") as AnimatedSprite;
            tilebg.Animation = bg;
        }

        public void ResetBackground()
        {
            var tilebg = GetNode("TileBG") as AnimatedSprite;
            if(tilebg.Animation != "default") {
                SetBackground(TileBG.DEFAULT); // i could write it here
            }
        }

        public void OnSleepingChange()
        {
            GD.Print($"{Id} Sleeping?");
        }
    } 
}
