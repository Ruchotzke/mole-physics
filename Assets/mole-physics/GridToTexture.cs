using UnityEngine;

namespace mole_physics
{
    /// <summary>
    /// Update
    /// </summary>
    public static class GridToTexture
    {
        /// <summary>
        /// Update the provided texture2D with thee given grid object.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="grid"></param>
        public static void UpdateTexture(Texture2D texture, Grid grid)
        {
            /* Compute the number of pixels for a given cell */
            int xSize = texture.width / grid.Width;
            int ySize = texture.height / grid.Height;
            Debug.Log($"x: {xSize} y: {ySize}");
            
            /* Work through each cell */
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    Color cellCol = grid.Cells[x, y].Filled ? Color.yellow : Color.black;

                    for (int xx = 0; xx < xSize; xx++)
                    {
                        for (int yy = 0; yy < ySize; yy++)
                        {
                            texture.SetPixel(xSize * x + xx, ySize * y + yy, cellCol);
                        }
                    }
                }
            }
            texture.Apply();
        }
    }
}