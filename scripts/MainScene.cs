using Godot;
using System;
using System.Collections.Generic;

namespace scripts
{
    public class MainScene : Node
    {
        public Core.Tiler Tiler;
        public Core.Grid Grid = new Core.Grid();

        public Node Timers;
        public Node TilesContainer;
        public HudScene Hud;
        public PackedScene TileNode;

        public int score = 0;

        public override void _Ready()
        {
            SetProcessInput(true);

            var window_size = GetViewport().Size;
            var grid_offset = new Vector2(80, 73);

            Hud = GetNode("hud") as HudScene;
            Timers = GetNode("timers") as Node;
            TileNode = ResourceLoader.Load("res://scenes/tile.tscn") as PackedScene;
            TilesContainer = GetNode("tiles_container") as Node;

            //setup grid
            Grid.SetupGrid();

            Tiler = new Core.Tiler(Grid, TilesContainer, TileNode, Timers, Hud);
            Tiler.CreateAndSetupTiles(window_size, grid_offset);
            Tiler.FindTileChainAndQueueRemoval();

            //handle hint signal
            Hud.Connect("LookingForHint", this, "HandleHintSignal");
        }

        public override void _Process(float delta)
        {
            //spawn tiles
            if(Tiler.TilesAreMissing() && !Tiler.AutomatedChecksRunning()) {
                Tiler.SpawnTiles();
            }

            //move tiles & check for new matches
            if(!Tiler.TilesAreInCorrectPosition() && !Tiler.AutomatedChecksRunning()) {
                Tiler.MoveTilesToPosition(delta);

                //lets check if the last move we did positioned everything in place
                //and lets go and check if any new tile chains exist and remove them
                if(Tiler.TilesAreInCorrectPosition()) {
                    Tiler.FindTileChainAndQueueRemoval();
                }
            }

            //animate tiles
            Tiler.AnimateAndCheckMatchingTiles(delta);
        }

        public override void _Input(InputEvent input)
        {
            //handle user input
            if(Tiler.TilesAreInCorrectPosition() && !Tiler.AutomatedChecksRunning()) {
                if(input is InputEventMouseButton && Input.IsMouseButtonPressed((int) ButtonList.Left)) {
                    var mouse = input as InputEventMouseButton;
                    Tiler.SelectAndSwitchTiles(mouse.Position);
                }
            }
        }

        public void HandleHintSignal()
        {
            if(!Tiler.AutomatedChecksRunning() && Tiler.TilesSelected.Count == 0 && Tiler.TilesAreInCorrectPosition()) {
                Tiler.FindFirstHint();
            }
        }

        public void OnShowHintTimer()
        {
            Tiler.ResetTileHints();
        }

        public void OnDestroyTilesTimer()
        {
            Tiler.RemoveDestroyedTilesWithNodes();
        }
    }
}