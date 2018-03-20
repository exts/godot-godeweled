using System;
using System.Collections.Generic;

namespace scripts.Core 
{
    public class Grid
    {
        public const int COL_COUNT = 6;
        public const int ROW_COUNT = 6;

        public List<List<Tile>> Data = new List<List<Tile>>();

        public static Grid CopyGrid(Grid grid)
        {
            var new_grid = new Grid();
            new_grid.SetupGrid();

            for(var row = 0; row < ROW_COUNT; row++) {
                for(var col = 0; col < COL_COUNT; col++) {
                    new_grid.Data[row][col].Y = grid.Data[row][col].Y;
                    new_grid.Data[row][col].X = grid.Data[row][col].X;
                    new_grid.Data[row][col].Gem = grid.Data[row][col].Gem;
                    new_grid.Data[row][col].Pos = grid.Data[row][col].Pos;
                    new_grid.Data[row][col].HasNode = grid.Data[row][col].HasNode;
                    new_grid.Data[row][col].Destroyed = grid.Data[row][col].Destroyed;
                }
            }

            return new_grid;
        }

        public void SwitchTiles(Tile tile, Tile tile2)
        {
            var tile_node = tile.Node;
            var tile2_node = tile2.Node;
            var tile_gem = tile.Gem;
            var tile2_gem = tile2.Gem;

            Data[tile.Y][tile.X].Gem = tile2_gem;
            Data[tile.Y][tile.X].Node = tile2_node;

            Data[tile2.Y][tile2.X].Gem = tile_gem;
            Data[tile2.Y][tile2.X].Node = tile_node;
        }

        /**
         * Setup grid with default tile positions & visibilitX
         */
        public void SetupGrid()
        {
            var count = 1;
            for(var row = 0; row < ROW_COUNT; row++) {
                var grid_row = new List<Tile>();
                for(var col = 0; col < COL_COUNT; col++) {
                    grid_row.Add(new Tile(count, false, row, col));
                    ++count;
                }
                Data.Add(grid_row);
            }
        }

        public Tile GetSelectedTile(Tuple<int, int> cords)
        {
            return Data[cords.Item1][cords.Item2];
        }
    }
}