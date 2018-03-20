using Godot;
using System;
using System.Collections.Generic;

namespace scripts.Core
{
    public class Tiler
    {
        public Grid Grid;
        public Node Timers;
        public Node TilesContainer;
        public HudScene Hud;
        public PackedScene TileScene;

        public Random rand;
        protected string[] GemColors = {"yellow", "green", "pink", "orange", "blue"};

        private Score score;
        private int score_count = 0;

        public Timer ShowHintTimer;
        public Timer DestroyedTileTimer;

        public Boolean CheckingHints = false;
        public Boolean CheckingDestroyedNodes = false;

        public Boolean AnimatingTileSwitch = false;
        public Boolean ReverseAnimatedTiles = false;
        public Boolean CheckingMatchedTileSwitch = false;

        public List<Tuple<int, int>> TilesSelected = new List<Tuple<int, int>>();

        public Tiler(Grid grid, Node tilesContainer, PackedScene packedScene, Node timers, HudScene hud)
        {
            Hud = hud;
            Grid = grid;
            Timers = timers;
            TileScene = packedScene;
            TilesContainer = tilesContainer;

            rand = new Random();

            score = new Score(Hud);
            ShowHintTimer = Timers.GetNode("show_hint_timer") as Timer;
            DestroyedTileTimer = Timers.GetNode("destroy_tiles_timer") as Timer;
        }

        public bool AutomatedChecksRunning()
        {
            return CheckingHints || CheckingDestroyedNodes || AnimatingTileSwitch;
        }

        public void CreateAndSetupTiles(Vector2 window_size, Vector2 grid_offset)
        {
            for(int row = 0; row < Grid.ROW_COUNT; row++) {
                for(int col = 0; col < Grid.COL_COUNT; col++) {
                    var gem = GemColors[rand.Next(0, GemColors.Length)];

                    var grid_tile = Grid.Data[row][col];
                    grid_tile.Gem = gem;

                    var tile = TileScene.Instance() as TileScene;

                    tile.Id = grid_tile.Pos;
                    tile.Row = grid_tile.X;
                    tile.Col = grid_tile.Y;
                    tile.SetRandomGem(gem);
                    tile.SetOffsetPosition(grid_offset, col, row, out grid_tile.OriginalPosition);

                    grid_tile.Node = tile;
                    TilesContainer.AddChild(tile);
                }
            }
        }

        public void FindTileChainAndQueueRemoval()
        {
            //so we can let other functions know we're processing chain deletion
            CheckingDestroyedNodes = true;

            for(var row = 0; row < Grid.ROW_COUNT; row++) {
                for(var col = 0; col < Grid.COL_COUNT; col++) {
                    var dict = new Dictionary<int, Tuple<int, int>>();
                    var node = Grid.Data[row][col];
                    FindTileChains(node.X, node.Y, dict);
                    if(dict.Count > 0) {
                        //change matching tiles background to the selected animation
                        FindAndChangeTilesBgAndSetupForDeletion(dict);

                        //add to Score
                        score.UpdateScore(dict.Count, ref score_count);

                        //then start clock and exit the function
                        DestroyedTileTimer.Start();

                        return;
                    }
                }
            }

            //let other functions do their thing
            CheckingDestroyedNodes = false;

            //reassign nodes to the correct position
            ReassignFloatingTile();
        }

        public void FindAndChangeTilesBgAndSetupForDeletion(Dictionary<int, Tuple<int, int>> tiles)
        {
            foreach(var tile in tiles) {
                var node = Grid.Data[tile.Value.Item1][tile.Value.Item2];
                if(node != null) {
                    var tile_bg = node.Node.GetNode("TileBG") as AnimatedSprite;
                    tile_bg.Animation = "selected";
                    node.Destroyed = true;
                }
            }
        }

        public void RemoveDestroyedTilesWithNodes()
        {
            foreach(var row in Grid.Data) {
                foreach(var tile in row) {
                    if(tile.Destroyed && tile.HasNode) {
                        tile.Node.QueueFree();
                        tile.Reset();
                    }
                }
            }
            
            DestroyedTileTimer.Stop();
            FindTileChainAndQueueRemoval();
        }

        public void FindTileChains(int row, int col, Dictionary<int, Tuple<int, int>> chain)
        {
            FindTileChainWithGridData(Grid, row, col, chain);
        }

        public void FindTileChainWithGridData(Grid grid, int row, int col, Dictionary<int, Tuple<int, int>> chain) 
        {
            var tile = grid.Data[row][col];
            var atleast_one_match = false;

            Dictionary<int, Tuple<int, int>> current_chain = new Dictionary<int, Tuple<int, int>>();
            Dictionary<int, Tuple<int, int>> vertical_chain = new Dictionary<int, Tuple<int, int>>();
            Dictionary<int, Tuple<int, int>> horizontal_chain = new Dictionary<int, Tuple<int, int>>();
            
            //look ahead (need to match 2 or more excluding itself)
            if(col != grid.Data[row].Count-1) {
                for(int fwd = col+1; fwd < grid.Data[row].Count; fwd++) {
                    if(chain.ContainsKey(grid.Data[row][fwd].Pos) || grid.Data[row][fwd].Gem != tile.Gem || grid.Data[row][fwd].Destroyed != false) {
                        break;
                    }

                    //will only add when we match else break the loop
                    horizontal_chain.Add(grid.Data[row][fwd].Pos, new Tuple<int, int>(row, fwd));
                }
            }

            //look behind (need to match 2 or more excluding itself)
            if(col != 0) {
                for(int bkw = col-1; bkw > -1; bkw--) {
                    if(chain.ContainsKey(grid.Data[row][bkw].Pos) || grid.Data[row][bkw].Gem != tile.Gem || grid.Data[row][bkw].Destroyed != false) {
                        break;
                    }

                    horizontal_chain.Add(grid.Data[row][bkw].Pos, new Tuple<int, int>(row, bkw));
                }
            }
            
            //look up (need to match 2 or more excluding itself)
            if(row != grid.Data.Count-1) {
                for(int up = row+1; up < grid.Data.Count; up++) {
                    if(chain.ContainsKey(grid.Data[up][col].Pos) || grid.Data[up][col].Gem != tile.Gem || grid.Data[up][col].Destroyed != false) {
                        break;
                    }

                    //will only add when we match else break the loop
                    vertical_chain.Add(grid.Data[up][col].Pos, new Tuple<int, int>(up, col));
                }
            }

            //look below (need to match 2 or more excluding itself)
            if(row != 0) {
                for(int dw = row-1; dw > -1; dw--) {
                    if(chain.ContainsKey(grid.Data[dw][col].Pos) || grid.Data[dw][col].Gem != tile.Gem || grid.Data[dw][col].Destroyed != false) {
                        break;
                    }

                    vertical_chain.Add(grid.Data[dw][col].Pos, new Tuple<int, int>(dw, col));
                }
            }

            //we need to get out when we only find a pair 
            if(horizontal_chain.Count < 2 && vertical_chain.Count < 2) {
                return;
            }

            //now we need to check if we have at least 3 horizontal matching tiles including the current tiles
            if(horizontal_chain.Count >= 2) {
                if(!atleast_one_match) {
                    atleast_one_match = true;
                }

                foreach(KeyValuePair<int, Tuple<int, int>> pair in horizontal_chain) {
                    if(!current_chain.ContainsKey(pair.Key)) {
                        current_chain.Add(pair.Key, pair.Value);
                    }
                }
            }

            //now we need to check if we have at least 3 vertical matching tiles including the current tiles
            if(vertical_chain.Count >= 2) {
                if(!atleast_one_match) {
                    atleast_one_match = true;
                }

                foreach(KeyValuePair<int, Tuple<int, int>> pair in vertical_chain) {
                    if(!current_chain.ContainsKey(pair.Key)) {
                        current_chain.Add(pair.Key, pair.Value);
                    }
                }
            }

            // add the current tile that we're looking for chains on to the current chain array
            if(atleast_one_match && !current_chain.ContainsKey(tile.Pos)) {
                current_chain.Add(tile.Pos, new Tuple<int, int>(row, col));
            }

            //merge current chain & main chain dictionaries
            foreach(KeyValuePair<int, Tuple<int, int>> pair in current_chain) {
                if(!chain.ContainsKey(pair.Key)) {
                    chain.Add(pair.Key, pair.Value);
                }
            }

            //only loop through our current chain tiles to find branching chains,
            //but pass the MAIN chain to append matches to the main dictionary to keep
            //track of all matching tiles.
            foreach(KeyValuePair<int, Tuple<int, int>> pair in current_chain) {
                var tuple = pair.Value;
                FindTileChainWithGridData(grid, tuple.Item1, tuple.Item2, chain);
            }
        }

        public void FindFirstHint()
        {
            CheckingHints = true;
            for(var row = 0; row < Grid.ROW_COUNT; row++) {
                for(var col = 0; col < Grid.COL_COUNT; col++) {
                    //switch tile with the tile to the right
                    if(SwitchedTileHasHint(row, col, 0, 1)) {
                        ShowTileHint(row, col, 0, 1);
                        return;
                    }

                    //switch tile with the tile to the left
                    if(SwitchedTileHasHint(row, col, 0, -1)) {
                        ShowTileHint(row, col, 0, -1);
                        return;
                    }

                    //switch tile with the tile to the up
                    if(SwitchedTileHasHint(row, col, 1, 0)) {
                        ShowTileHint(row, col, 1, 0);
                        return;
                    }

                    //switch tile with the tile to the down
                    if(SwitchedTileHasHint(row, col, -1, 0)) {
                        ShowTileHint(row, col, -1, 0);
                        return;
                    }
                }
                GD.Print("------End Row------");
            }

            CheckingHints = false;
            GD.Print("No Hints Found");
        }

        public Boolean SwitchedTileHasHint(int row, int col, int r_dir, int c_dir) 
        {
            //avoid out of range checks
            if(row + r_dir < 0 || row + r_dir >= Grid.ROW_COUNT || col + c_dir < 0 || col + c_dir >= Grid.COL_COUNT) {
                return false;
            }
            
            // GD.Print($"Orign - R: {row}, C: {col}, POS: {Grid.Data[row][col].Pos} | Switch Tile - R: {row+r_dir}, C: {col+c_dir}, POS: {Grid.Data[row+r_dir][col+c_dir].Pos}");

            var dict = new Dictionary<int, Tuple<int, int>>();
            var grid_tmp = SwitchTilePosition(row, col, r_dir, c_dir);
            FindTileChainWithGridData(grid_tmp, row, col, dict);


            return dict.Count > 0;
        }

        /**
         * r_dir and c_dir is to look ahead/behind in a certain direction
         * by adding +INT or -INT to the current row/col position
         */
        public Grid SwitchTilePosition(int row, int col, int r_dir, int c_dir)
        {
            return SwitchTwoTilesPosition(row, col, row+r_dir, col+c_dir);
        }

        //todo: update this to handle which grid to copy so we cna do a REAL switch later
        public Grid SwitchTwoTilesPosition(int row, int col, int row2, int col2)
        {
            var grid_clone = Grid.CopyGrid(Grid);
            var temp = grid_clone.Data[row2][col2];
            grid_clone.Data[row2][col2] = grid_clone.Data[row][col];
            grid_clone.Data[row][col] = temp;
            
            return grid_clone;
        }

        public void ShowTileHint(int row, int col, int r_dir, int c_dir)
        {
            //set background
            Grid.Data[row][col].SetHintBackground();
            Grid.Data[row+r_dir][col+c_dir].SetHintBackground();

            ShowHintTimer.Start();
        }

        public void ResetTileHints()
        {
            for(var row = Grid.ROW_COUNT-1; row > -1; row--) {
                for(var col = 0; col < Grid.COL_COUNT; col++) {
                    Grid.Data[row][col].Node.ResetBackground();
                }
            }

            ShowHintTimer.Stop();
            CheckingHints = false;
        }

        public void ReassignFloatingTile()
        {
            //loop through rows in reverse (from the bottom), but columns normally
            for(var row = Grid.ROW_COUNT-1; row > -1; row--) {
                for(var col = 0; col < Grid.COL_COUNT; col++) {
                    var tile = Grid.Data[row][col];
                    if(tile == null || tile.Empty()) {
                        continue;
                    }

                    //move the first match
                    FindFirstEmptySpaceBelowCurrentTileAndMoveTile(row, col, tile);
                }
            }

            // GD.Print("Moved All!");
        }

        public bool TilesAreInCorrectPosition()
        {
            foreach(var row in Grid.Data) {
                foreach(var tile in row) {
                    if(!tile.Empty() && !tile.InPosition() || !tile.Empty() && !tile.Node.Visible) {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool TilesAreMissing()
        {
            foreach(var row in Grid.Data) {
                foreach(var tile in row) {
                    if(tile.Empty()) {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SpawnTiles()
        {
            for(int row = 0; row < Grid.ROW_COUNT; row++) {
                for(int col = 0; col < Grid.COL_COUNT; col++) {
                    var grid_tile = Grid.Data[row][col];
                    if(grid_tile.Empty()) {
                        
                        var gem = GemColors[rand.Next(0, GemColors.Length)];
                        grid_tile.Gem = gem;

                        var first_tile = Grid.Data[0][col];

                        var tile = TileScene.Instance() as TileScene;
                        tile.Id = grid_tile.Pos;
                        tile.Row = grid_tile.X;
                        tile.Col = grid_tile.Y;
                        tile.Visible = false; //hide by default
                        tile.Position = new Vector2(first_tile.OriginalPosition.x, first_tile.OriginalPosition.y);
                        tile.SetRandomGem(gem);

                        grid_tile.Node = tile;
                        grid_tile.HasNode = true;
                        grid_tile.Destroyed = false;
                        TilesContainer.AddChild(tile);

                        // GD.Print($"Spawned: {row}:{col} - {tile.Position.x}:{tile.Position.y} - {gem} - {grid_tile.Empty()}");
                    }
                }
            }
        }

        public void MoveTilesToPosition(float delta)
        {
            //loop through rows in reverse (from the bottom), but columns normally
            for(var row = Grid.ROW_COUNT-1; row > -1; row--) {
            // for(var row = 0; row < Grid.ROW_COUNT; row++) {
                for(var col = 0; col < Grid.COL_COUNT; col++) {
                    var tile = Grid.Data[row][col];

                    if(!tile.InPosition() && TileSiblingNotOverlapping(tile, row, col)) {
                        if(!tile.Node.Visible) {
                            tile.Node.Visible = true;
                        }
                        
                        tile.CorrectPosition(delta);
                    }

                    //enable tile to be visible for first row tiles
                    if(tile.InPosition() && TileSiblingNotOverlapping(tile, row, col) && !tile.Node.Visible) {
                        tile.Node.Visible = true;
                    }
                }
            }
        }

        private bool TileSiblingNotOverlapping(Tile tile, int row, int col)
        {
            if(row + 1 >= Grid.ROW_COUNT) {
                return true;
            }

            var sibling = Grid.Data[row+1][col];
            if(sibling.InPosition() || sibling.Node.Position.y > tile.Node.Position.y + tile.Node.GetScaledDimensions().y) {
                return true;
            }

            return false;
        }

        private void FindFirstEmptySpaceBelowCurrentTileAndMoveTile(int row, int col, Tile tile)
        {
            for(var t_row = Grid.ROW_COUNT - 1; t_row > row; t_row--) {
                var current_tile = Grid.Data[t_row][col];
                if(current_tile != null && current_tile.Destroyed && !current_tile.HasNode) {
                    current_tile.ReparentNode(tile);
                    break;
                }
            }
        }

        public void SelectAndSwitchTiles(Vector2 mouse_pos)
        {
            if(TilesSelected.Count < 2) {
                SelectTile(mouse_pos);
            }
        }

        public void SelectTile(Vector2 mouse_pos)
        {
            for(var row = 0; row < Grid.ROW_COUNT; row++) {
                for(var col = 0; col < Grid.COL_COUNT; col++) {
                    if(Grid.Data[row][col].SelectTile(mouse_pos)) {
                        TilesSelected.Add(new Tuple<int, int>(row, col));
                        UnselectNonChildSiblingTiles();
                        return;
                    }
                }
            }
        }

        public void UnselectNonChildSiblingTiles()
        {
            //check if tiles are siblings
            if(TilesSelected.Count == 2) {
                var first_tile = Grid.GetSelectedTile(TilesSelected[0]);
                var second_tile = Grid.GetSelectedTile(TilesSelected[1]);

                if(!first_tile.IsSibling(second_tile)) {
                    first_tile.Node.ResetBackground();
                    second_tile.Node.ResetBackground();
                    TilesSelected.RemoveRange(0, 2);
                }
            }
        }

        public void AnimateAndCheckMatchingTiles(float delta)
        {
            if(TilesSelected.Count < 2) return;

            AnimatingTileSwitch = true;

            var first_tile = Grid.GetSelectedTile(TilesSelected[0]);
            var second_tile = Grid.GetSelectedTile(TilesSelected[1]);
            
            //reverse tile animation then exit, only executes on failure
            if(ReverseAnimatedTiles) {

                //correct position
                first_tile.MoveTileBack(second_tile, delta);
                second_tile.MoveTileBack(first_tile, delta);

                if(TilesAreInCorrectPosition()) {
                    AnimatingTileSwitch = false;
                    ReverseAnimatedTiles = false;
                    first_tile.Node.ResetBackground();
                    second_tile.Node.ResetBackground();
                    TilesSelected.RemoveRange(0, 2);
                }

                return;
            }

            //check if tiles are in each others original position
            if(!first_tile.TilesSwitchedPositions(second_tile)) {
                first_tile.SwitchTilePosition(second_tile, delta);
                second_tile.SwitchTilePosition(first_tile, delta);
                return;
            }
            
            //when tiles ARE in animated position check if there's a match from either tile
            var found_match = true;
            if(first_tile.TilesSwitchedPositions(second_tile) && !SwitchedTilesHasMatch(first_tile, second_tile)) {
                found_match = false; //jk
                ReverseAnimatedTiles = true;
                return;
            }
            
            if(found_match) {
                AnimatingTileSwitch = false;
                ReverseAnimatedTiles = false;

                first_tile.Node.ResetBackground();
                second_tile.Node.ResetBackground();
                TilesSelected.RemoveRange(0, 2);

                //switch tiles officially
                Grid.SwitchTiles(first_tile, second_tile);

                //lets queue up tiles for removal
                FindTileChainAndQueueRemoval();
            }
        }

        public bool SwitchedTilesHasMatch(Tile tile, Tile tile2)
        {
            if(CheckSwitchedTileHasMatch(tile, tile2) || CheckSwitchedTileHasMatch(tile2, tile)) {
                return true;
            }

            return false;
        }

        private bool CheckSwitchedTileHasMatch(Tile tile, Tile tile2)
        {
            var dict = new Dictionary<int, Tuple<int, int>>();
            var grid = SwitchTwoTilesPosition(tile.Y, tile.X, tile2.Y, tile2.X);
            FindTileChainWithGridData(grid, tile.Y, tile.X, dict);

            return dict.Count > 0;
        }
    }
}