using Godot;
using System;

namespace scripts
{
    public class HudScene : Node
    {
        [Signal] 
        public delegate void LookingForHint();

        public override void _Init()
        {
        }

        public override void _Ready()
        {
        }

        public void UpdateScore(int score)
        {
            var score_value = GetNode("ScoreValue") as Label;
            score_value.Text = score.ToString();
        }

        /**
         * Quit Game!
         */
        public void OnQuitButtonPressed()
        {
            GetTree().Quit();
        }

        /**
         * We want to emite a signal so we can use it in our main scene
         * when the hint button is pressed
         */
        public void OnHintButtonPressed()
        {
            EmitSignal("LookingForHint");
        }
    }
}