using UnityEngine;

namespace Infra2DAction
{
    public class ScaleReferenceGrid : MonoBehaviour
    {
        [Header("Tamaño de celda (en unidades de mundo)")]
        [SerializeField] private float _cellWidth = 2f;
        [SerializeField] private float _cellHeight = 2f;

        [Header("Extensión del grid dibujado (en celdas)")]
        [SerializeField] private int _cellsX = 20;
        [SerializeField] private int _cellsY = 10;

        [Header("Color de las líneas")]
        [SerializeField] private Color _lineColor = new Color(0f, 1f, 1f, 0.5f);

        private void OnDrawGizmos()
        {
            Gizmos.color = _lineColor;
            Vector3 origin = transform.position;

            float width = _cellsX * _cellWidth;
            float height = _cellsY * _cellHeight;

            for (int x = 0; x <= _cellsX; x++)
            {
                Vector3 from = origin + new Vector3(x * _cellWidth, 0f, 0f);
                Gizmos.DrawLine(from, from + new Vector3(0f, height, 0f));
            }

            for (int y = 0; y <= _cellsY; y++)
            {
                Vector3 from = origin + new Vector3(0f, y * _cellHeight, 0f);
                Gizmos.DrawLine(from, from + new Vector3(width, 0f, 0f));
            }
        }
    }
}
