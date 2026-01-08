
using CorePublic.Helpers;
using UnityEngine;

namespace CorePublic.LevelDesignComponents
{
    public class GridLayout3D : LayoutController
    {
        public enum PivotMode
        {
            GridCenter,
            StartCell
        }

        public enum AxisDirection
        {
            Positive = 1,
            Negative = -1
        }

        public Vector3 cellSize = new Vector3(1, 1, 1);
        public Vector3 spacing = new Vector3(0.5f, 0.5f, 0.5f);
        public bool fixedColumn = false;
        public bool fixedRow = false;
        public bool fixedLayer = false;
        public int columnCount = 1;
        public int rowCount = 1;
        public int layerCount = 1;
        [SerializeField] public PivotMode pivotMode = PivotMode.GridCenter;
        [SerializeField] public AxisDirection xDirection = AxisDirection.Positive;
        [SerializeField] public AxisDirection yDirection = AxisDirection.Positive;
        [SerializeField] public AxisDirection zDirection = AxisDirection.Positive;
        [SerializeField] public Color gizmoColor = Color.white;
        [SerializeField] public bool showGizmos = true;
        [SerializeField] public int gizmoCount = 10;

        private Vector3 _gridSize;

        public override void UpdateChildrenPosition()
        {
            int childCount = transform.childCount;
            _gridSize = CalculateGridSize(childCount);
            Vector3 pivotOffset = CalculatePivotOffset(_gridSize);
            Vector3 step = cellSize + spacing;
            Vector3 dir = GetDirectionVector();

            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
                Vector3 basePos = Vector3.Scale(GetGridIndex(i), step) - pivotOffset;
                Vector3 newPosition = Vector3.Scale(basePos, dir);
                child.localPosition = newPosition;
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(child);
#endif
            }
        }

        public Vector3 GetElementPositionWithCustomChildCount(int index, int count)
        {
            _gridSize = CalculateGridSize(count);
            Vector3 pivotOffset = CalculatePivotOffset(_gridSize);
            Vector3 step = cellSize + spacing;
            Vector3 dir = GetDirectionVector();
            Vector3 basePos = Vector3.Scale(GetGridIndex(index), step) - pivotOffset;
            return Vector3.Scale(basePos, dir);
        }

        public Vector3 GetElementPosition(int index)
        {
            int childCount = transform.childCount;
            _gridSize = CalculateGridSize(childCount);
            Vector3 pivotOffset = CalculatePivotOffset(_gridSize);
            Vector3 step = cellSize + spacing;
            Vector3 dir = GetDirectionVector();
            Vector3 basePos = Vector3.Scale(GetGridIndex(index), step) - pivotOffset;
            return Vector3.Scale(basePos, dir);
        }

        private Vector3 CalculateGridSize(int count)
        {
            int x, y, z;

            if (fixedColumn && fixedRow)
            {
                x = columnCount;
                y = rowCount;
                z = Mathf.CeilToInt(count / (float)(x * y));
            }
            else if (fixedColumn && fixedLayer)
            {
                x = columnCount;
                z = layerCount;
                y = Mathf.CeilToInt(count / (float)(x * z));
            }
            else if (fixedRow && fixedLayer)
            {
                y = rowCount;
                z = layerCount;
                x = Mathf.CeilToInt(count / (float)(y * z));
            }
            else if (fixedColumn)
            {
                x = columnCount;
                y = Mathf.CeilToInt(Mathf.Pow(count / (float)x, 1 / 2f));
                z = Mathf.CeilToInt(count / (float)(x * y));
            }
            else if (fixedRow)
            {
                y = rowCount;
                x = Mathf.CeilToInt(Mathf.Pow(count / (float)y, 1 / 2f));
                z = Mathf.CeilToInt(count / (float)(x * y));
            }
            else if (fixedLayer)
            {
                z = layerCount;
                x = Mathf.CeilToInt(Mathf.Pow(count / (float)z, 1 / 2f));
                y = Mathf.CeilToInt(count / (float)(x * z));
            }
            else
            {
                x = Mathf.CeilToInt(Mathf.Pow(count, 1 / 3f));
                y = x;
                z = x;
            }

            return new Vector3(x, y, z);
        }

        private Vector3 CalculatePivotOffset(Vector3 gridSize)
        {
            if (pivotMode == PivotMode.StartCell)
            {
                return Vector3.zero;
            }

            return new Vector3(
                (gridSize.x - 1) * (cellSize.x + spacing.x) / 2,
                (gridSize.y - 1) * (cellSize.y + spacing.y) / 2,
                (gridSize.z - 1) * (cellSize.z + spacing.z) / 2
            );
        }

        private Vector3 GetDirectionVector()
        {
            float dx = xDirection == AxisDirection.Positive ? 1f : -1f;
            float dy = yDirection == AxisDirection.Positive ? 1f : -1f;
            float dz = zDirection == AxisDirection.Positive ? 1f : -1f;
            return new Vector3(dx, dy, dz);
        }

        private Vector3 GetGridIndex(int index)
        {
            int x = index % (int)_gridSize.x;
            int y = (index / (int)_gridSize.x) % (int)_gridSize.y;
            int z = (index / (int)_gridSize.x) / (int)_gridSize.y;

            return new Vector3(x, y, z);
        }

        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            Gizmos.color = gizmoColor;

            var gizmoGridSize = CalculateGridSize(gizmoCount);
            Vector3 pivotOffset = CalculatePivotOffset(gizmoGridSize);
            Vector3 step = cellSize + spacing;
            Vector3 dir = GetDirectionVector();

            for (int x = 0; x < gizmoGridSize.x; x++)
            {
                for (int y = 0; y < gizmoGridSize.y; y++)
                {
                    for (int z = 0; z < gizmoGridSize.z; z++)
                    {
                        Vector3 basePos = Vector3.Scale(new Vector3(x, y, z), step) - pivotOffset;
                        Vector3 cellCenter = Vector3.Scale(basePos, dir);
                        Gizmos.DrawWireCube(transform.position + cellCenter, cellSize);
                    }
                }
            }
        }
    }
}
