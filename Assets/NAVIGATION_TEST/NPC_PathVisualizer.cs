using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class NPC_PathVisualizer : MonoBehaviour {
    [Header("References")]
    public NPC_PathHelper pathHelper;

    [Header("Mode")]
    [Tooltip("True = Curved Spline. False = Straight lines between Knots.")]
    public bool showSmoothPath = true;

    [Header("Visual Settings")]
    public float lineWidth = 0.2f;
    public Color lineColor = Color.cyan;
    public float groundOffset = 0.1f;

    [Header("Smoothness (Only if Smooth Path is on)")]
    [Range(1f, 20f)]
    public float resolution = 5f;

    private LineRenderer _line;
    private SplineContainer _splineContainer;
    private bool _needsUpdate = false;

    private void OnEnable() {
        if (pathHelper == null) pathHelper = GetComponent<NPC_PathHelper>();
        _line = GetComponent<LineRenderer>();

        Spline.Changed += OnSplineChanged;

        _needsUpdate = true;
    }

    private void OnDisable() {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnValidate() {
        _needsUpdate = true;
    }

    private void OnSplineChanged(Spline spline, int index, SplineModification modification) {
        _needsUpdate = true;
    }

    private void Update() {
        if (_needsUpdate || transform.hasChanged) {
            UpdateVisuals();
            _needsUpdate = false;
            transform.hasChanged = false;
        }
    }

    public void UpdateVisuals() {
        if (pathHelper == null) return;
        _splineContainer = pathHelper._Spline;
        if (_splineContainer == null || _splineContainer.Splines.Count == 0) return;

        if (_line == null) _line = GetComponent<LineRenderer>();

        if (_line.sharedMaterial == null) {
            _line.material = new Material(Shader.Find("Sprites/Default"));
        }

        _line.startWidth = lineWidth;
        _line.endWidth = lineWidth;
        _line.startColor = lineColor;
        _line.endColor = lineColor;
        _line.loop = _splineContainer.Splines[0].Closed;

        DrawSpline();
    }

    private void DrawSpline() {
        Spline spline = _splineContainer.Splines[0];

        if (showSmoothPath) {
            float length = spline.GetLength();
            int steps = Mathf.CeilToInt(length * resolution);
            if (steps < 2) steps = 2;

            _line.positionCount = steps;

            for (int i = 0; i < steps; i++) {
                float t = (float)i / (float)(steps - 1);
                if (spline.Closed) t = (float)i / (float)steps;

                Vector3 localPos = spline.EvaluatePosition(t);
                Vector3 worldPos = _splineContainer.transform.TransformPoint(localPos);
                worldPos.y += groundOffset;

                _line.SetPosition(i, worldPos);
            }
        }
        else {
            int knotCount = spline.Count;
            _line.positionCount = knotCount;

            for (int i = 0; i < knotCount; i++) {
                BezierKnot knot = spline[i];
                Vector3 worldPos = _splineContainer.transform.TransformPoint(knot.Position);
                worldPos.y += groundOffset;
                _line.SetPosition(i, worldPos);
            }
        }
    }
}