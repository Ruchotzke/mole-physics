using mole_physics.cpu_threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using Vector2Int = UnityEngine.Vector2Int;

namespace mole_physics.gpu_automata
{
    
    /// <summary>
    /// The manager used to write textures via a compute shader.
    /// </summary>
    public class GpuTextureManager : MonoBehaviour
    {
        public RawImage RawImage;

        public bool Run;
        public float UpdateTime;

        /// <summary>
        /// The shader used to clear the render texture's color buffer to 0.
        /// </summary>
        public ComputeShader ClearShader;
        
        /// <summary>
        /// The shader used to update the grid.
        /// </summary>
        public ComputeShader UpdateShader;
        
        private static readonly int gridID = Shader.PropertyToID("_Grid");
        private static readonly int offsetID = Shader.PropertyToID("_UseOffset");
        private static readonly int initialPosId = Shader.PropertyToID("_InitialPositions");
        private static readonly int initialPosLen = Shader.PropertyToID("_BufferLen");
        
        private float _timer;
        private RenderTexture _texture;

        private bool useOffset = false;
        
        private void Awake()
        {
            _timer = UpdateTime;
        }

        private void Start()
        {
            /* Create texture */
            _texture = new RenderTexture((int)RawImage.rectTransform.rect.width,
                (int)RawImage.rectTransform.rect.height, 0);
            _texture.enableRandomWrite = true;
            _texture.Create();
            RawImage.texture = _texture;
            
            /* Create initial positions buffer */
            Vector2Int[] initialPos = new[]
            {
                new Vector2Int(110, 110),
                new Vector2Int(115, 115),
                new Vector2Int(112, 112),
                new Vector2Int(133, 133),
            };
            ComputeBuffer buffer = new ComputeBuffer(initialPos.Length, sizeof(int) * 2);
            buffer.SetData(initialPos);
            
            /* Bind the shader */
            ClearShader.SetTexture(0, gridID, RawImage.texture);
            ClearShader.SetBuffer(0, initialPosId, buffer);
            ClearShader.SetInt(initialPosLen, initialPos.Length);
            UpdateShader.SetTexture(0, gridID, RawImage.texture);
            UpdateShader.SetBool(offsetID, useOffset);
            
            /* Clear the screen */
            ClearShader.Dispatch(0, Mathf.CeilToInt(_texture.width / 32.0f), Mathf.CeilToInt(_texture.height / 32.0f), 1);
            
            /* Run the actual sim */
            UpdateShader.Dispatch(0, Mathf.CeilToInt(_texture.width / 8.0f), Mathf.CeilToInt(_texture.height / 8.0f), 1);
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
                    
                    /* Actually update the texture */
                    useOffset = !useOffset;
                    UpdateShader.SetBool(offsetID, useOffset);
                    UpdateShader.Dispatch(0, Mathf.CeilToInt(_texture.width / 8.0f), Mathf.CeilToInt(_texture.height / 8.0f), 1);       
                }
            }
        }
    }
}