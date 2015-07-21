using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.Constants
{
    public static class Constant
    {
        // MapGeneration.cs, NavigationController.cs, Maps.cs
        public const int GRID_WIDTH = 20;
        public const int GRID_HEIGHT = 20;
        public const int NUM_OF_TOWERS = 20;
        public const int NUM_OF_DECORS = 0;
        public const int NUM_OF_DIRECTIONS = 4;

        // IGuiItemInterface.cs, GuiStatics.cs, TileInterface.cs, GuiAPI
        public const float X_MIN_BOUND = -182.5f;
        public const float Y_MIN_BOUND = -182.5f;
        public const float POSITION_SCALE = 18.25f;
        public const float SIZE_SCALE = 28.6f;

        // WaveController.cs
        private static double MAX_TIME_BETWEEN_SPAWNS = 3.0;
        private static double MIN_TIME_BETWEEN_SPAWNS = 0.0;
        private static int START_LOWER_WAVE_BOUND = 1;
        private static int START_UPPER_WAVE_BOUND = 4;
        private static double TIME_TIL_NEXT_WAVE = 5.0;
    }
}
