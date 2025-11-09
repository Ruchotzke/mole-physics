using System;
using mole_physics.cpu_threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace mole_physics
{
    /// <summary>
    /// A manager for updating the sim and texture
    /// </summary>
    public class TextureManager : MonoBehaviour
    {
        public RawImage RawImage;

        public bool Run;
        public float UpdateTime;
        
        public Vector2Int GridResolution;

        private NativeGrid _grid;
        private float _timer;
        private Texture2D _texture;

        private Vector2Int ChunkSize = new Vector2Int(8, 8);
        private Vector2Int NumChunks;

        private void Awake()
        {
            _timer = UpdateTime;
            _grid = new NativeGrid(GridResolution.x, GridResolution.y);

            for (int y = 20; y < 95; y += 2)
            {
                _grid[55, y] = new cpu_threading.Cell()
                {
                    Color = Color.white,
                    Filled = true
                };
            }
            

            NumChunks = new Vector2Int(
                x: Mathf.CeilToInt(GridResolution.x / (float)ChunkSize.x),
                y: Mathf.CeilToInt(GridResolution.y / (float)ChunkSize.y)
            );
        }

        private void Start()
        {
            _texture = new Texture2D((int)RawImage.rectTransform.rect.width, (int)RawImage.rectTransform.rect.height, TextureFormat.RGBA32, false);
            RawImage.texture = _texture;

            _grid.GenerateTexture(_texture);
        }

        private void Update()
        {
            /* Check for update */
            if (Run)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _timer = UpdateTime;
                    
                    /* Run the job */
                    var resolveQueue = new NativeQueue<Vector2Int>(Allocator.TempJob);
                    var job = new UpdateChunkJob()
                    {
                        Grid = _grid,
                        SecondaryResolveQueue = resolveQueue.AsParallelWriter(),
                        ChunkSize = ChunkSize,
                        NumChunks = NumChunks
                    };
                    var handle = job.Schedule(NumChunks.x * NumChunks.y, 10);
                    handle.Complete();
                    
                    /* Manual intervention on chunk movement */
                    while (resolveQueue.TryDequeue(out var coord))
                    {
                        /* Straight down */
                        if (!_grid[coord.x, coord.y - 1].Filled)
                        {
                            _grid[coord.x, coord.y - 1] = _grid[coord.x, coord.y];
                            _grid[coord.x, coord.y] = new cpu_threading.Cell()
                            {
                                Color = Color.white,
                                Filled = false
                            };
                            continue;
                        }
                        
                        /* Left */
                        if (coord.x > 0 && !_grid[coord.x - 1, coord.y - 1].Filled)
                        {
                            _grid[coord.x - 1, coord.y - 1] = _grid[coord.x, coord.y];
                            _grid[coord.x, coord.y] = new cpu_threading.Cell()
                            {
                                Color = Color.white,
                                Filled = false
                            };

                            continue;
                        }
                        
                        /* Right */
                        if (coord.x < GridResolution.x-1 && !_grid[coord.x + 1, coord.y - 1].Filled)
                        {
                            _grid[coord.x + 1, coord.y - 1] = _grid[coord.x, coord.y];
                            _grid[coord.x, coord.y] = new cpu_threading.Cell()
                            {
                                Color =  Color.white,
                                Filled = false
                            };

                            continue;
                        }    
                    }   
                    
                    /* Render the output */
                    _grid.GenerateTexture(_texture);
                    
                    /* Dispose of temp items */
                    resolveQueue.Dispose();
                }
            }
        }
    }
}