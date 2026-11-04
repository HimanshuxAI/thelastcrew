using System.Collections.Generic;
using UnityEngine;

namespace TheLastCrew.Maze
{
    /// <summary>
    /// A fixed maze, generated once from a seed.
    ///
    /// "Fixed" means the same seed always produces the same maze, so the level
    /// can be learned and landmarks can be memorised. The generator (rather
    /// than a hand-drawn map) is used because it guarantees the maze is
    /// solvable - a hand-drawn map with one sealed-off corridor wastes a day of
    /// playtesting on a bug that is not a bug.
    ///
    /// Odd coordinates are rooms, even coordinates are the walls between them.
    /// </summary>
    public class MazeGrid
    {
        public int Width { get; }
        public int Height { get; }

        private readonly bool[] solid;

        public MazeGrid(int width, int height)
        {
            Width = width;
            Height = height;
            solid = new bool[width * height];

            for (int i = 0; i < solid.Length; i++)
                solid[i] = true;
        }

        public bool IsSolid(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return true;

            return solid[y * Width + x];
        }

        public bool IsOpen(int x, int y) => !IsSolid(x, y);

        /// <summary>
        /// Builds a perfect maze with a few extra loops and one open chamber.
        ///
        /// The loops matter more than they look: a perfect maze is all dead
        /// ends, and in an audio game a dead end you cannot see is a dead end
        /// you walk into at speed. Loops give the player a second route and a
        /// way to confirm where they are.
        /// </summary>
        public static MazeGrid Generate(int width, int height, int seed)
        {
            width = Mathf.Max(7, width | 1);
            height = Mathf.Max(7, height | 1);

            MazeGrid grid = new MazeGrid(width, height);
            System.Random random = new System.Random(seed);

            Stack<Vector2Int> stack = new Stack<Vector2Int>();
            Vector2Int start = new Vector2Int(1, 1);

            grid.Set(start.x, start.y, false);
            stack.Push(start);

            Vector2Int[] steps =
            {
                new Vector2Int(0, 2),
                new Vector2Int(0, -2),
                new Vector2Int(2, 0),
                new Vector2Int(-2, 0)
            };

            while (stack.Count > 0)
            {
                Vector2Int current = stack.Peek();

                List<Vector2Int> candidates = new List<Vector2Int>(4);
                foreach (Vector2Int step in steps)
                {
                    int nx = current.x + step.x;
                    int ny = current.y + step.y;

                    if (nx <= 0 || ny <= 0 || nx >= width - 1 || ny >= height - 1)
                        continue;

                    if (!grid.IsSolid(nx, ny))
                        continue;

                    candidates.Add(new Vector2Int(nx, ny));
                }

                if (candidates.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                Vector2Int next = candidates[random.Next(candidates.Count)];

                // Carve the wall between the two rooms.
                grid.Set((current.x + next.x) / 2, (current.y + next.y) / 2, false);
                grid.Set(next.x, next.y, false);

                stack.Push(next);
            }

            grid.AddLoops(random, width * height / 18);
            grid.AddChamber(random);

            return grid;
        }

        private void Set(int x, int y, bool value)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                return;

            solid[y * Width + x] = value;
        }

        /// <summary>
        /// Opens a few extra doorways. Only walls that sit between two open
        /// cells are removed, so the maze never gets a hole in its outer edge.
        /// </summary>
        private void AddLoops(System.Random random, int count)
        {
            int attempts = count * 40;

            for (int a = 0; a < attempts && count > 0; a++)
            {
                int x = random.Next(1, Width - 1);
                int y = random.Next(1, Height - 1);

                if (IsOpen(x, y))
                    continue;

                bool horizontal = IsOpen(x - 1, y) && IsOpen(x + 1, y)
                                  && !IsOpen(x, y - 1) && !IsOpen(x, y + 1);

                bool vertical = IsOpen(x, y - 1) && IsOpen(x, y + 1)
                                && !IsOpen(x - 1, y) && !IsOpen(x + 1, y);

                if (!horizontal && !vertical)
                    continue;

                Set(x, y, false);
                count--;
            }
        }

        /// <summary>One open room, so the player has somewhere to orient by.</summary>
        private void AddChamber(System.Random random)
        {
            int cx = random.Next(2, Mathf.Max(3, Width - 4));
            int cy = random.Next(2, Mathf.Max(3, Height - 4));

            for (int x = cx; x < cx + 3; x++)
                for (int y = cy; y < cy + 3; y++)
                    Set(x, y, false);
        }
    }
}
