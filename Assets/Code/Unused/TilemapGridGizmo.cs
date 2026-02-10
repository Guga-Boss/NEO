using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//[ExecuteAlways]
public class MyTilemapEditor: MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize = new Vector2Int(29, 29);
    public Color gridLineColor = Color.green;      // cor do grid interno
    [Range(1f, 10f)] public float gridLineWidth = 2f;
    public bool drawGrid = true;                   // toggle para desenhar o grid

    [Header("Border Settings")]
    public bool drawBorder = true;                 // toggle para desenhar a borda
    public Color borderLineColor = Color.red;      // cor da borda
    [Range(1f, 50f)] public float borderLineWidth = 3f;  

    public Vector2 cellSize = new Vector2(1, 1);  // tamanho de cada célula do Tilemap

#if UNITY_EDITOR

    private void OnValidate()
    {
#if UNITY_EDITOR
        SceneView.RepaintAll();
#endif
    }
    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position;

        // --- Desenhar grid interno ---
        if( drawGrid )
        {
            Handles.color = gridLineColor;

            // Linhas horizontais
            for( int y = 0; y <= gridSize.y; y++ )
            {
                Vector3 start = origin + new Vector3(0, y * cellSize.y, 0);
                Vector3 end   = origin + new Vector3(gridSize.x * cellSize.x, y * cellSize.y, 0);
                Handles.DrawAAPolyLine( gridLineWidth, start, end );
            }

            // Linhas verticais
            for( int x = 0; x <= gridSize.x; x++ )
            {
                Vector3 start = origin + new Vector3(x * cellSize.x, 0, 0);
                Vector3 end   = origin + new Vector3(x * cellSize.x, gridSize.y * cellSize.y, 0);
                Handles.DrawAAPolyLine( gridLineWidth, start, end );
            }
        }

        // --- Desenhar apenas a borda ---
        if( drawBorder )
        {
            Handles.color = borderLineColor;

            Vector3 bottomLeft  = origin;
            Vector3 bottomRight = origin + new Vector3(gridSize.x * cellSize.x, 0, 0);
            Vector3 topLeft     = origin + new Vector3(0, gridSize.y * cellSize.y, 0);
            Vector3 topRight    = origin + new Vector3(gridSize.x * cellSize.x, gridSize.y * cellSize.y, 0);

            // Linha inferior
            Handles.DrawAAPolyLine( borderLineWidth, bottomLeft, bottomRight );
            // Linha superior
            Handles.DrawAAPolyLine( borderLineWidth, topLeft, topRight );
            // Linha esquerda
            Handles.DrawAAPolyLine( borderLineWidth, bottomLeft, topLeft );
            // Linha direita
            Handles.DrawAAPolyLine( borderLineWidth, bottomRight, topRight );
        }
    }
#endif
}
