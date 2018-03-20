using Godot;
using System;

namespace scripts.Core 
{
    /**
     * Using this struct to handle if a tile id destroyed
     * all while keeping the grid position. Easier for reassigning
     * tile nodes.
     */
    public class Tile
    {
        public int X;
        public int Y;
        public int Pos;
        public string Gem = "";
        public bool Destroyed = false;
        public bool HasNode = true;
        public bool Selected = false;
        public Vector2 OriginalPosition = new Vector2();
        public TileScene Node = new TileScene();

        public Tile(int pos, bool destroyed, int row, int col)
        {
            this.X = col;
            this.Y = row;
            this.Pos = pos;
            this.Destroyed = destroyed;
        }

        public override string ToString()
        {
            return $"{Gem} - {Pos} - {X}:{Y}";
        }

        public void CorrectPosition(float delta)
        {
            if(InPosition()) {
                return;
            }

            var position = new Vector2(0, delta * Node.Velocity);
            if(Node.Position.y > OriginalPosition.y) {
                // GD.Print("Made it!");
                Node.Position = OriginalPosition;
                return;
            }

            GD.Print($"Delta: {delta}, Velocity: {Node.Velocity}, DV: {delta * Node.Velocity}");

            Node.Position += position;
        }

        public void MoveTileBack(Tile tile, float delta)
        {
            var velocity = delta * Node.Velocity;
            var direction = SiblingStepDirection(tile, -velocity); //reverse velocity
            var sib_direction = SiblingDirection(tile);

            var correct_position = false;
            if(sib_direction == "up" && Node.Position.y > OriginalPosition.y) {
                correct_position = true;
            } else if(sib_direction == "down" && Node.Position.y < OriginalPosition.y) {
                correct_position = true;
            } else if(sib_direction == "left" && Node.Position.x > OriginalPosition.x) {
                correct_position = true;
            } else if(sib_direction == "right" && Node.Position.x < OriginalPosition.x) {
                correct_position = true;
            }

            if(correct_position) {
                Node.Position = OriginalPosition;
                return;
            }

            Node.Position += direction;
        }

        public void SwitchTilePosition(Tile tile, float delta)
        {
            var velocity = delta * Node.Velocity;
            var direction = SiblingStepDirection(tile, velocity);
            var sib_direction = SiblingDirection(tile);

            var correct_position = false;
            if(sib_direction == "up" && Node.Position.y < tile.OriginalPosition.y) {
                correct_position = true;
            } else if(sib_direction == "down" && Node.Position.y > tile.OriginalPosition.y) {
                correct_position = true;
            } else if(sib_direction == "left" && Node.Position.x < tile.OriginalPosition.x) {
                correct_position = true;
            } else if(sib_direction == "right" && Node.Position.x > tile.OriginalPosition.x) {
                correct_position = true;
            }

            if(correct_position) {
                Node.Position = tile.OriginalPosition;
                return;
            }

            Node.Position += direction;
        }

        public bool InPosition()
        {
            return OriginalPosition == Node.Position;
        }

        public bool TilesSwitchedPositions(Tile tile)
        {
            return OriginalPosition == tile.Node.Position && Node.Position == tile.OriginalPosition;
        }

        public bool IsSibling(Tile tile)
        {
            //same row to the right
            if(tile.X == X+1 && tile.Y == Y) {
                return true;
            //same row to the left
            } else if(tile.X == X-1 && tile.Y == Y) {
                return true;
            //same col below
            } else if(tile.Y == Y+1 && tile.X == X) {
                return true;
            //same col above
            } else if(tile.Y == Y-1 && tile.X == X) {
                return true;
            }

            return false;
        }

        public string SiblingDirection(Tile tile)
        {
            //same row to the right
            if(tile.X == X+1 && tile.Y == Y) {
                return "right";
            //same row to the left
            } else if(tile.X == X-1 && tile.Y == Y) {
                return "left";
            //same col below
            } else if(tile.Y == Y+1 && tile.X == X) {
                return "down";
            //same col above
            } else if(tile.Y == Y-1 && tile.X == X) {
                return "up";
            }

            return "invalid";
        }

        public Vector2 SiblingStepDirection(Tile tile, float velocity)
        {
            string dir = SiblingDirection(tile);
            var direction = new Vector2();
            switch(dir) {
                case "up":
                    direction.y = -velocity;
                break;
                case "down":
                    direction.y = velocity;
                break;
                case "left":
                    direction.x = -velocity;
                break;
                case "right":
                    direction.x = velocity;
                break;
            }

            return direction;
        }

        public void Reset()
        {
            Gem = "";
            HasNode = false;
            Destroyed = true;
        }

        public void ReparentNode(Tile tile)
        {
            Gem = tile.Gem;
            Node = tile.Node;
            HasNode = true;
            Destroyed = false;

            //reset original node
            tile.Node = new TileScene();
            tile.Reset();
        }

        public bool SelectTile(Vector2 mouse_pos)
        {
            //for fucks sake, tiles aren't positioned by the top left, but the center lol
            //makes it annoying when you need to deal w/ outside position checking
            var scaled_dims = Node.GetScaledDimensions();
            var x = Node.Position.x - (scaled_dims.x / 2);
            var xw = x + scaled_dims.x;
            var y = Node.Position.y - (scaled_dims.y / 2);
            var yw = y + scaled_dims.y;

            var mx = mouse_pos.x;
            var my = mouse_pos.y;

            // GD.Print($"Node: {Gem}, {Pos}, {X}, {Y}");
            // GD.Print($"X: {x}, XW: {xw}, Y: {y}, YW: {yw}, MX: {mx}, MY: {my}");
            // GD.Print($"X < MX : {x < mx}, XW > MX: {xw > mx}, Y < MY: {y < my}, YW > MY: {yw > my}");
            // GD.Print("--------------");
            if(x < mx && xw > mx && y < my && yw > my) {
                Selected = true;
                Node.SetBackground(TileBG.SELECTED);
                return true;
            }
            return false;
        }

        public void DeselectTile()
        {
            Selected = false;
            Node.SetBackground(TileBG.DEFAULT);
        }

        public void SetHintBackground()
        {
            Node.SetBackground(TileBG.HINT);
        }

        public void SetDefaultBackground()
        {
            Node.SetBackground(TileBG.DEFAULT);
        }

        public bool Empty()
        {
            if(Destroyed && !HasNode) {
                return true;
            }

            return false;
        }

    }
}