using System;
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

        private Grid _grid;
        private float _timer;
        private Texture2D _texture;

        private void Awake()
        {
            _timer = UpdateTime;
            _grid = new Grid(GridResolution.x, GridResolution.y);

            for (int i = -3; i < 20; i++)
            {
                _grid.Cells[_grid.Width / 2, _grid.Height / 2 + i].Filled = true;
                
                _grid.Cells[_grid.Width / 2 + 6, _grid.Height / 2 + i].Filled = true;
            }
        }

        private void Start()
        {
            _texture = new Texture2D((int)RawImage.rectTransform.rect.width, (int)RawImage.rectTransform.rect.height, TextureFormat.RGBA32, false);
            RawImage.texture = _texture;
            
            GridToTexture.UpdateTexture(_texture, _grid);
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
                    _grid.UpdateGrid();
                    GridToTexture.UpdateTexture(_texture, _grid);
                }
            }
        }
    }
}