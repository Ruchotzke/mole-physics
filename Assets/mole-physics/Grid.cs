using UnityEngine;

namespace mole_physics
{
    /// <summary>
    /// A grid containing a set of particles.
    /// </summary>
    public class Grid
    {
        public Cell[,] Cells;

        public int Width;
        public int Height;

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new Cell[Width, Height];
        }

        /// <summary>
        /// Update the entire grid.
        /// </summary>
        public void UpdateGrid()
        {
            /* Make sure to update bottom to top */
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    UpdateCell(x, y);
                }
            }
        }

        
        /// <summary>
        /// Update the cell at the provided position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void UpdateCell(int x, int y)
        {
            /* Don't update an empty cell */
            if (!Cells[x, y].Filled) return;
            
            /* The botton row doesn't need to be updated */
            if (y == 0) return;
            
            /* If we can move down, do so */
            if (!Cells[x, y - 1].Filled)
            {
                Cells[x, y-1].Filled = true;
                Cells[x, y].Filled = false;
                return;
            }
            
            /* If not, try moving left/right */
            if (!Cells[x - 1, y - 1].Filled)
            {
                Cells[x - 1, y - 1].Filled = true;
                Cells[x,y].Filled = false;
                return;
            }
            if (!Cells[x + 1, y - 1].Filled)
            {
                Cells[x + 1, y - 1].Filled = true;
                Cells[x,y].Filled = false;
                return;
            }
            
            /* Otherwise this particle CANNOT move */
            
        }
        
    }
}