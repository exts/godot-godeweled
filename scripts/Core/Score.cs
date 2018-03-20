using Godot;
using System;

namespace scripts.Core
{
    public class Score
    {
        private HudScene Hud;
        private int points_per_block = 10;

        public Score(HudScene hud) 
        {
            Hud = hud;
        }

        public int calculateScore(int tile_count, int score)
        {
            var points = tile_count * points_per_block;
            score += points;

            return score;
        }

        public void UpdateScore(int tile_count, ref int score)
        {
            score = calculateScore(tile_count, score);
            Hud.UpdateScore(score);
        }

    }
}