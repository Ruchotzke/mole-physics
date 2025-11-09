using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace mole_physics.cpu_threading
{
    [BurstCompile]
    public struct UpdateChunkJob : IJobParallelFor
    {

        /// <summary>
        /// The grid containing cell data.
        /// </summary>
        public NativeGrid Grid;

        /// <summary>
        /// The queue for any particles which need to be resolved later (falling through chunks).
        /// </summary>
        [WriteOnly] public NativeQueue<Vector2Int>.ParallelWriter SecondaryResolveQueue;
        
        /// <summary>
        /// The chunk size.
        /// </summary>
        [ReadOnly] public Vector2Int ChunkSize;

        /// <summary>
        /// The number of chunks in the X/Y direction.
        /// </summary>
        [ReadOnly] public Vector2Int NumChunks;
        
        public void Execute(int index)
        {
            /* Generate an RNG for this job */
            var rng = new Unity.Mathematics.Random((uint)(876234 * index + 77));
            
            /* Convert the index into a chunk offset (lower-left coord) and extents (since we may border texture) */
            var chunkId = new Vector2Int(index % NumChunks.x, index / NumChunks.x);
            var minCoord = new Vector2Int(ChunkSize.x * chunkId.x, ChunkSize.y * chunkId.y);
            var maxCoord = new Vector2Int(minCoord.x + ChunkSize.x, minCoord.y + ChunkSize.y);
            maxCoord.x = Mathf.Min(maxCoord.x, Grid.XSize);
            maxCoord.y = Mathf.Min(maxCoord.y, Grid.YSize);
            
            /* Iterate over all cells in our region */
            Color c = Color.HSVToRGB(rng.NextFloat(), 1.0f, 1.0f);
            for (int y = minCoord.y; y < maxCoord.y; y++)
            {
                for (int x = minCoord.x; x < maxCoord.x; x++)
                {
                    var curr = Grid[x, y];
                    
                    /* No need to process if there is nothing here */
                    if (!curr.Filled) continue;
                    
                    /* Can we move straight down? */
                    if (y > 0)  // Constraint: no moving past bottom of screen
                    {
                        
                        /* Straight down */
                        if (!Grid[x, y - 1].Filled)
                        {
                            /* Move down (or resolve later if on a boundary */
                            if (y != minCoord.y)
                            {
                                Grid[x, y - 1] = curr;
                                Grid[x, y] = new Cell()
                                {
                                    Color = c,
                                    Filled = false
                                };
                            }
                            else
                            {
                                SecondaryResolveQueue.Enqueue(new Vector2Int(x, y));
                            }

                            continue;
                        }
                        
                        /* Left */
                        if (x > 0 && !Grid[x - 1, y - 1].Filled)
                        {
                            /* Move or resolve later if on a boundary */
                            if (y != minCoord.y && x != minCoord.x)
                            {
                                Grid[x - 1, y - 1] = curr;
                                Grid[x, y] = new Cell()
                                {
                                    Color = c,
                                    Filled = false
                                };
                            }
                            else
                            {
                                SecondaryResolveQueue.Enqueue(new Vector2Int(x, y));
                            }

                            continue;
                        }
                        
                        /* Right */
                        if (x < maxCoord.x-1 && !Grid[x + 1, y - 1].Filled)
                        {
                            /* Move or resolve later if on a boundary */
                            if (y != minCoord.y && x != maxCoord.x-1)
                            {
                                Grid[x + 1, y - 1] = curr;
                                Grid[x, y] = new Cell()
                                {
                                    Color = c,
                                    Filled = false
                                };
                            }
                            else
                            {
                                SecondaryResolveQueue.Enqueue(new Vector2Int(x, y));
                            }

                            continue;
                        }
                    }
                }
            }
        }
    }
}