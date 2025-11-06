using Unity.Collections;
using UnityEngine;

namespace mole_physics.cpu_threading
{
    /// <summary>
    /// A blittable grid type.
    /// </summary>
    public struct NativeGrid
    {
        /// <summary>
        /// The cells in the simulation.
        /// </summary>
        [NativeDisableParallelForRestriction] public NativeArray<Cell> Cells;

        /// <summary>
        /// The width of the grid.
        /// </summary>
        public int XSize;
        
        /// <summary>
        /// The height of the grid.
        /// </summary>
        public int YSize;

        /// <summary>
        /// Construct a new NativeGrid
        /// </summary>
        /// <param name="xSize"></param>
        /// <param name="ySize"></param>
        public NativeGrid(int xSize, int ySize)
        {
            Cells = new NativeArray<Cell>(xSize * ySize, Allocator.Persistent);
            XSize = xSize;
            YSize = ySize;
        }
        
        /// <summary>
        /// One-dimensionally index this array.
        /// </summary>
        /// <param name="index"></param>
        public Cell this[int index]
        {
            get => Cells[index];
            set => Cells[index] = value;
        }

        /// <summary>
        /// Two-dimensionally index this array.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Cell this[int x, int y]
        {
            get => Cells[x + y * XSize];
            set => Cells[x + y * XSize] = value;
        }
    
        public void Dispose() => Cells.Dispose();
        
        /// <summary>
        /// Convert this grid into a render texture.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="grid"></param>
        public void GenerateTexture(Texture2D texture)
        {
            /* Compute the number of pixels for a given cell */
            int xSize = texture.width / XSize;
            int ySize = texture.height / YSize;
            
            /* Work through each cell */
            for (int x = 0; x < XSize; x++)
            {
                for (int y = 0; y < YSize; y++)
                {
                    for (int xx = 0; xx < xSize; xx++)
                    {
                        for (int yy = 0; yy < ySize; yy++)
                        {
                            texture.SetPixel(xSize * x + xx, ySize * y + yy, this[x,y].Filled ? Color.yellow : Color.black);
                        }
                    }
                }
            }
            texture.Apply();
        }
    }
}