/*
The MIT License

Copyright (c) 2010 Christoph Husse

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Assets.Code.Navigation
{
    /// <summary>
    /// An interface that ensures whatever class inherits from IPathNode will have a function
    /// to check the walkability of a node for a pather with a given mover type. This only exists
    /// because this was all originally abstracted out before it became more concrete for optimization
    /// purposes.
    /// </summary>
    /// <typeparam name="TUserContext">The object to check against for checking traversability (ie UnitMoverType).</typeparam>
    public interface IPathNode<TUserContext>
    {
        bool IsWalkable(TUserContext inContext);
    }

    public interface IIndexedObject
    {
        int Index { get; set; }
    }

    public class SpatialAStar<TPathNode, TUserContext> where TPathNode : IPathNode<TUserContext>
    {
        private OpenNodesMap m_OpenSet;
        private PriorityQueue<PathNode> m_OrderedOpenSet;
        private PathNode[,] m_CameFrom;
        private OpenNodesMap m_RuntimeGrid;
        private PathNode[,] m_SearchSpace;

        /// <summary>
        /// The two-dimensional array of TPathNodes that represents your "map".
        /// </summary>
        public TPathNode[,] SearchSpace { get; private set; }
        /// <summary>
        /// The width of the grid you are searchin.g
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// The height of the grid you are searching.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Used to quickly assign distances of sqrt(2) without repeated calls to the sqrt function.
        /// </summary>
        private static readonly double SQRT_2 = Math.Sqrt(2);

        /// <summary>
        /// Used to store the neighbors of each given node. Filled upon instantiation of the pathfinding
        /// class so these values are not called dynamically. Because we have a square grid, we are able
        /// to determine neighbors and neighboring distances at the time of instantiation.
        /// </summary>
        private Dictionary<PathNode, PathNode[]> neighbors;

        protected class PathNode : IPathNode<TUserContext>, IComparer<PathNode>, IIndexedObject
        {
            public static readonly PathNode Comparer = new PathNode(0, 0, default(TPathNode));

            public TPathNode UserContext { get; internal set; }
            public Double G { get; internal set; }
            public Double H { get; internal set; }
            public Double F { get; internal set; }
            public int Index { get; set; }
            public bool Checked { get; set; }

            public Boolean IsWalkable(TUserContext inContext)
            {
                return UserContext.IsWalkable(inContext);
            }

            public int X { get; internal set; }
            public int Y { get; internal set; }

            public int Compare(PathNode x, PathNode y)
            {
                if (x.F < y.F)
                    return -1;
                else if (x.F > y.F)
                    return 1;

                return 0;
            }

            public PathNode(int inX, int inY, TPathNode inUserContext)
            {
                X = inX;
                Y = inY;
                UserContext = inUserContext;
                Checked = false;
            }
        }

        public SpatialAStar(TPathNode[,] inGrid)
        {
            SearchSpace = inGrid;
            Width = inGrid.GetLength(0);
            Height = inGrid.GetLength(1);
            m_SearchSpace = new PathNode[Width, Height];
            m_OpenSet = new OpenNodesMap(Width, Height);
            m_CameFrom = new PathNode[Width, Height];
            m_RuntimeGrid = new OpenNodesMap(Width, Height);
            m_OrderedOpenSet = new PriorityQueue<PathNode>(PathNode.Comparer, Width * Height);
            neighbors = new Dictionary<PathNode, PathNode[]>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (inGrid[x, y] == null)
                        throw new ArgumentNullException();

                    m_SearchSpace[x, y] = new PathNode(x, y, inGrid[x, y]);
                }
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    neighbors.Add(m_SearchSpace[x, y], new PathNode[4]);
                    StoreNeighborNodes(m_SearchSpace[x, y], neighbors[m_SearchSpace[x, y]]);
                }
            }
        }

        /// <summary>
        /// Estimates a cost from point 'a' to point 'b' based on quick-Euclidean distance.
        /// </summary>
        /// <param name="a">The GridPoint representing the original point.</param>
        /// <param name="b">The GridPoint representing the point to which 'a' will be compared.</param>
        protected virtual Double Heuristic(PathNode a, PathNode b)
        {
            int min, max;
            int dx = Math.Abs(a.X - b.X);
            int dy = Math.Abs(a.Y - b.Y);

            if (dx < dy)
            {
                min = dx;
                max = dy;
            }
            else
            {
                min = dy;
                max = dx;
            }

            return (((max << 8) + (max << 3) - (max << 4) - (max << 1) +
                     (min << 7) - (min << 5) + (min << 3) - (min << 1)) >> 8);
        }

        /// <summary>
        /// Finds the distance between one PathNode and its neighbor.
        /// </summary>
        /// <param name="a">The GridPoint representing the original point.</param>
        /// <param name="b">The GridPoint representing the neighbor of 'a'.</param>
        protected virtual Double NeighborDistance(PathNode a, PathNode b)
        {
            int diffX = Math.Abs(a.X - b.X);
            int diffY = Math.Abs(a.Y - b.Y);

            switch (diffX + diffY)
            {
                case 1: return 1;
                case 2: return SQRT_2;
                case 0: return 0;
                default:
                    throw new ApplicationException();
            }
        }

        /// <summary>
        /// Finds a path between two GridPoints taking mover types into account.
        /// </summary>
        /// <param name="inStartNode">The GridPoint representing the place on the map that the path will begin.</param>
        /// <param name="inEndNode">The GridPoint representing the point on the map that the path is trying to reach.
        /// The path will treat the destination GridPoint as walkable to enable itself to generate a path to the destination,
        /// then return a LinkedList of GridPoints with the final point (destinatin) GridPoint removed.</param>
        /// <param name="inUserContext">An object representing whatever determines whether terrain is walkable for a given unit (ie UnitMoverType).</param>
        public LinkedList<TPathNode> Search(GridPoint inStartNode, GridPoint inEndNode, TUserContext inUserContext)
        {
            //int searched_nodes = 0;

            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            // get start and end points from the A* grid
            PathNode startNode = m_SearchSpace[inStartNode.x, inStartNode.y];
            PathNode endNode = m_SearchSpace[inEndNode.x, inEndNode.y];

            // if you are pathing to your current postition, you're done
            if (startNode == endNode)
                return new LinkedList<TPathNode>(new TPathNode[] { startNode.UserContext });

            // store the neighboring nodes around the current node
            //PathNode[] neighborNodes = new PathNode[8];

            m_OpenSet.Clear();
            m_RuntimeGrid.Clear();
            m_OrderedOpenSet.Clear();

            Array.Clear(m_CameFrom, 0, m_CameFrom.Length - 1);

            startNode.G = 0;
            startNode.H = Heuristic(startNode, endNode);
            startNode.F = startNode.H;

            m_OpenSet.Add(startNode);
            m_OrderedOpenSet.Push(startNode);

            m_RuntimeGrid.Add(startNode);

            int nodes = 0;

            //watch.Stop();
            //DebugEx.LogF("beginning part took {0} ticks", watch.ElapsedTicks);

            //TimeController.Instance.StartTimer("SECTION THREE");

            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            while (!m_OpenSet.IsEmpty)
            {
                //searched_nodes++;
                PathNode x = m_OrderedOpenSet.Pop();

                //DebugEx.LogF("investigating ({0}, {1})", x.X, x.Y);

                if (x == endNode)
                {
                    LinkedList<TPathNode> result = ReconstructPath(m_CameFrom, m_CameFrom[endNode.X, endNode.Y]);

                    result.AddLast(endNode.UserContext);

                    //DebugEx.LogF("Checked out {0} nodes (in a grid of {1})", searched_nodes, m_SearchSpace.Length);
                    //watch.Stop();
                    //DebugEx.LogF("iterating through nodes took {0} ticks", watch.ElapsedTicks);
                    // TimeController.Instance.EndTimer(showTicks: false);

                    //Debug.Log("found a path! path from " + inStartNode + " to " + inEndNode + " goes:");

                    // foreach (TPathNode node in result) Debug.Log(node);

                    return result;
                }

                m_OpenSet.Remove(x);
                x.Checked = true;

                PathNode[] neighborNodes = neighbors[x];

                int numNeighbors = neighborNodes.Length;
                for (int i = 0; i < numNeighbors; i++)
                {
                    PathNode y = neighborNodes[i];
                    Boolean tentative_is_better;

                    if (y == null
                        || !y.UserContext.IsWalkable(inUserContext)
                        || y.Checked)
                        continue;

                    nodes++;

                    Double tentative_g_score = m_RuntimeGrid[x].G + NeighborDistance(x, y);
                    Boolean wasAdded = false;

                    if (!m_OpenSet.Contains(y))
                    {
                        m_OpenSet.Add(y);
                        tentative_is_better = true;
                        wasAdded = true;
                    }
                    else if (tentative_g_score < m_RuntimeGrid[y].G)
                    {
                        tentative_is_better = true;
                    }
                    else
                    {
                        tentative_is_better = false;
                    }

                    if (tentative_is_better)
                    {
                        m_CameFrom[y.X, y.Y] = x;

                        if (!m_RuntimeGrid.Contains(y))
                            m_RuntimeGrid.Add(y);

                        m_RuntimeGrid[y].G = tentative_g_score;
                        m_RuntimeGrid[y].H = Heuristic(y, endNode);
                        m_RuntimeGrid[y].F = m_RuntimeGrid[y].G + m_RuntimeGrid[y].H;

                        if (wasAdded)
                            m_OrderedOpenSet.Push(y);
                        else
                            m_OrderedOpenSet.Update(y);
                    }
                }
            }

            Debug.LogWarning(String.Format("could not path from ({0}, {1}) --> ({2}, {3})",
                startNode.X, startNode.Y, endNode.X, endNode.Y));
            return null;
        }

        public TPathNode FindFirstBlockerInShortestPath(GridPoint inStartNode, GridPoint inEndNode, TUserContext inUserContext)
        {
            // get start and end points from the A* grid
            PathNode startNode = m_SearchSpace[inStartNode.x, inStartNode.y];
            PathNode endNode = m_SearchSpace[inEndNode.x, inEndNode.y];

            // if you are pathing to your current postition, you're done
            if (startNode == endNode)
                return default(TPathNode);

            // store the neighboring nodes around the current node
            //PathNode[] neighborNodes = new PathNode[8];

            m_OpenSet.Clear();
            m_RuntimeGrid.Clear();
            m_OrderedOpenSet.Clear();

            Array.Clear(m_CameFrom, 0, m_CameFrom.Length);

            startNode.G = 0;
            startNode.H = Heuristic(startNode, endNode);
            startNode.F = startNode.H;

            m_OpenSet.Add(startNode);
            m_OrderedOpenSet.Push(startNode);

            m_RuntimeGrid.Add(startNode);

            int nodes = 0;

            //TimeController.Instance.StartTimer("SECTION THREE");

            while (!m_OpenSet.IsEmpty)
            {
                PathNode x = m_OrderedOpenSet.Pop();

                if (x == endNode)
                {
                    LinkedList<TPathNode> result = ReconstructPath(m_CameFrom, m_CameFrom[endNode.X, endNode.Y]);
                    LinkedListNode<TPathNode> curNode = result.First;

                    while (curNode != null && curNode.Value.IsWalkable(inUserContext))
                        curNode = curNode.Next;

                    // TimeController.Instance.EndTimer(showTicks: false);
                    if (curNode == null)
                        return default(TPathNode);
                    else
                        return curNode.Value;
                }

                m_OpenSet.Remove(x);
                x.Checked = true;

                PathNode[] neighborNodes = neighbors[x];

                int numNeighbors = neighborNodes.Length;
                for (int i = 0; i < numNeighbors; i++)
                {
                    PathNode y = neighborNodes[i];
                    Boolean tentative_is_better;

                    if (y == null
                        || y.Checked)
                        continue;

                    nodes++;

                    Double tentative_g_score = m_RuntimeGrid[x].G + NeighborDistance(x, y);
                    Boolean wasAdded = false;

                    if (!m_OpenSet.Contains(y))
                    {
                        m_OpenSet.Add(y);
                        tentative_is_better = true;
                        wasAdded = true;
                    }
                    else if (tentative_g_score < m_RuntimeGrid[y].G)
                    {
                        tentative_is_better = true;
                    }
                    else
                    {
                        tentative_is_better = false;
                    }

                    if (tentative_is_better)
                    {
                        m_CameFrom[y.X, y.Y] = x;

                        if (!m_RuntimeGrid.Contains(y))
                            m_RuntimeGrid.Add(y);

                        m_RuntimeGrid[y].G = tentative_g_score;
                        m_RuntimeGrid[y].H = Heuristic(y, endNode);
                        m_RuntimeGrid[y].F = m_RuntimeGrid[y].G + m_RuntimeGrid[y].H;

                        if (wasAdded)
                            m_OrderedOpenSet.Push(y);
                        else
                            m_OrderedOpenSet.Update(y);
                    }
                }
            }

            return default(TPathNode);
        }

        private LinkedList<TPathNode> ReconstructPath(PathNode[,] came_from, PathNode current_node)
        {
            LinkedList<TPathNode> result = new LinkedList<TPathNode>();

            PathNode current = came_from[current_node.X, current_node.Y];
            result.AddLast(current_node.UserContext);

            while (current != null)
            {
                result.AddFirst(current.UserContext);
                current = came_from[current.X, current.Y];
            }
            return result;
        }

        /// <summary>
        /// Stores the neighboring nodes of centerNode in the provided PathNode array.
        /// </summary>
        /// <param name="centerNode">The node whose neighbors are being identified.</param>
        /// <param name="inNeighbors">An eight-node-long array to be filled with the neighbors of centerNode.</param>
        private void StoreNeighborNodes(PathNode centerNode, PathNode[] inNeighbors)
        {
            int x = centerNode.X;
            int y = centerNode.Y;

            // directly up
            if (y > 0)
                inNeighbors[0] = m_SearchSpace[x, y - 1];
            else
                inNeighbors[0] = null;

            // directly left
            if (x > 0)
                inNeighbors[1] = m_SearchSpace[x - 1, y];
            else
                inNeighbors[1] = null;

            // directly right
            if (x < Width - 1)
                inNeighbors[2] = m_SearchSpace[x + 1, y];
            else
                inNeighbors[2] = null;

            // directly below
            if (y < Height - 1)
                inNeighbors[3] = m_SearchSpace[x, y + 1];
            else
                inNeighbors[3] = null;

            /*

             * WE DON'T WANT DIAGONAL MOVEMENT ANYMORE, BUT THAT CODE IS BELOW
             * 
            // bottom right
            if ((x < Width - 1) && (y < Height - 1))
                inNeighbors[7] = m_SearchSpace[x + 1, y + 1];
            else
                inNeighbors[7] = null;

            // lower left
            if ((x > 0) && (y < Height - 1))
                inNeighbors[3] = m_SearchSpace[x - 1, y + 1];
            else
                inNeighbors[3] = null;

            // upper left
            if ((x > 0) && (y > 0))
                inNeighbors[0] = m_SearchSpace[x - 1, y - 1];
            else
                inNeighbors[0] = null;

            // upper right
            if ((x < Width - 1) && (y > 0))
                inNeighbors[2] = m_SearchSpace[x + 1, y - 1];
            else
                inNeighbors[2] = null;
            */
        }

        private class OpenNodesMap
        {
            private PathNode[,] m_Map;

            private int _width;
            public int Width
            {
                get { return _width; }
                private set { _width = value; }
            }

            private int _height;
            public int Height
            {
                get { return _height; }
                private set { _height = value; }
            }

            private int _count;
            public int Count
            {
                get { return _count; }
                private set { _count = value; }
            }

            public PathNode this[Int32 x, Int32 y]
            {
                get
                {
                    return m_Map[x, y];
                }
            }

            public PathNode this[PathNode Node]
            {
                get
                {
                    return m_Map[Node.X, Node.Y];
                }

            }

            public bool IsEmpty
            {
                get
                {
                    return Count == 0;
                }
            }

            public OpenNodesMap(int inWidth, int inHeight)
            {
                m_Map = new PathNode[inWidth, inHeight];
                Width = inWidth;
                Height = inHeight;
            }

            public void Add(PathNode inValue)
            {
                Count++;
                m_Map[inValue.X, inValue.Y] = inValue;
            }

            public bool Contains(PathNode inValue)
            {
                PathNode item = m_Map[inValue.X, inValue.Y];

                return item != null;
            }

            public void Remove(PathNode inValue)
            {
                Count--;
                m_Map[inValue.X, inValue.Y] = null;
            }

            public void Clear()
            {
                Count = 0;
                for (int x = 0; x < Width; x++ )
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (m_Map[x, y] != null)
                        {
                            m_Map[x, y].Checked = false;
                            m_Map[x, y] = null;
                        }
                    }
                }
            }
        }
    }
}
