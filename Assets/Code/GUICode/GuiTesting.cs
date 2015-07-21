using Assets.Code.Navigation;
using System;
using Assets.Code.Controllers;
using System.Collections.Generic;
using System.Linq;
using Assets.Code.Enums;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;
using Assets.Code.MapGeneration;
using Assets.Code.Scores;

namespace Assets.Code.GUICode
{
    class GuiTesting : MonoBehaviour
    {
        void Awake()
        {
            TileType[,] map = MapGeneration.MapGeneration.GenerateMap();

            GridPoint[,] gridPoints = NavigationController.Instance.NavigationGrid;
            
            int width = NavigationController.GRID_WIDTH;
            int height = NavigationController.GRID_HEIGHT;

            NavigationController.Instance.FirstTile = NavigationController.Instance.GetGridPoint(MapGeneration.MapGeneration._start.x, MapGeneration.MapGeneration._start.y);
            NavigationController.Instance.LastTile = NavigationController.Instance.GetGridPoint(MapGeneration.MapGeneration._end.x, MapGeneration.MapGeneration._end.y);

            //Debug.Log("Starting at (" + NavigationController.Instance.FirstTile.x + ", " + NavigationController.Instance.FirstTile.y + ")");
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // tiles that walker units can't go on
                    if (map[y, x] == TileType.Tower || map[y, x] == TileType.EmptySpace || map[y, x] == TileType.Decor || map[y, x] == TileType.Decor2)
                    {
                        gridPoints[x, y].WalkerBlockers++;
                    }

                    // tiles that flyers can't go through
                    if (map[y, x] == TileType.Tower || map[y, x] == TileType.Decor || map[y, x] == TileType.Decor2)
                        gridPoints[x, y].FlyerBlockers++;

                    gridPoints[x, y].TileType = map[y, x];
                }
            }

            GuiAPI.DrawGrid(map);
        }
    }
}
