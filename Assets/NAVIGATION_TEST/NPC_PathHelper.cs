using UnityEngine;
using UnityEngine.Splines;

public class NPC_PathHelper : MonoBehaviour {
    public Transform _PathPoints;
    public SplineContainer _Spline;

    [Header("Debug Visuals")]
    public Color gizmoColor = Color.yellow;
    [Range(0.1f, 2f)]
    public float gizmoSize = 0.3f;
    public bool showLines = true;

    private void OnDrawGizmos() {
        if (_PathPoints == null) return;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < _PathPoints.childCount; i++) {
            Transform current = _PathPoints.GetChild(i);
            Gizmos.DrawSphere(current.position, gizmoSize);

            if (showLines && i < _PathPoints.childCount - 1) {
                Transform next = _PathPoints.GetChild(i + 1);
                Gizmos.DrawLine(current.position, next.position);
            }
        }
    }
    public int GetPointCount() {
        if (_PathPoints == null) return 0;

        return _PathPoints.childCount;
    }
    public Vector3 GetPointPosition(int index) {
        if (_PathPoints == null || index < 0 || index >= _PathPoints.childCount)
            return Vector3.zero;

        return _PathPoints.GetChild(index).position;
    }
}