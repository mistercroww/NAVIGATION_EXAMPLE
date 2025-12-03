using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using Unity.Mathematics;

[CustomEditor(typeof(NPC_PathHelper))]
public class NPC_PathHelperEditor : Editor {
    private int pointsToGenerate = 10;

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        NPC_PathHelper script = (NPC_PathHelper)target;

        GUILayout.Space(20);
        EditorGUILayout.LabelField("Path Generation Tools", EditorStyles.boldLabel);

        if (script._PathPoints == null || script._Spline == null) {
            EditorGUILayout.HelpBox("Please assign both the Path Points Parent and the Spline Container.", MessageType.Warning);
            return;
        }

        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Generator", EditorStyles.miniLabel);

        pointsToGenerate = EditorGUILayout.IntField("Point Count", pointsToGenerate);
        if (GUILayout.Button($"Generate {pointsToGenerate} Points Along Spline")) {
            GeneratePoints(script);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Generate Points on Spline Knots")) {
            GenerateFromKnots(script);
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Align Existing Children", EditorStyles.miniLabel);

        if (GUILayout.Button("Distribute Evenly Along Spline")) {
            DistributeEvenly(script);
        }
        if (GUILayout.Button("Align Rotation to Path Forward")) {
            AlignRotation(script);
        }

        GUILayout.EndVertical();
    }

    private void GeneratePoints(NPC_PathHelper script) {
        Spline spline = script._Spline.Splines[0];
        Transform parent = script._PathPoints;

        var children = new GameObject[parent.childCount];
        for (int i = 0; i < parent.childCount; i++) children[i] = parent.GetChild(i).gameObject;
        foreach (var child in children) Undo.DestroyObjectImmediate(child);

        for (int i = 0; i < pointsToGenerate; i++) {
            float t;
            if (spline.Closed)
                t = (float)i / (float)pointsToGenerate;
            else
                t = (float)i / (float)(pointsToGenerate - 1);

            float3 localPos = spline.EvaluatePosition(t);
            Vector3 worldPos = script._Spline.transform.TransformPoint(localPos);

            GameObject go = new GameObject($"Point_{i}");
            go.transform.position = worldPos;
            go.transform.SetParent(parent);

            Undo.RegisterCreatedObjectUndo(go, "Generate Points");
        }
        AlignRotation(script);
    }

    private void GenerateFromKnots(NPC_PathHelper script) {
        Spline spline = script._Spline.Splines[0];
        Transform parent = script._PathPoints;

        var children = new GameObject[parent.childCount];
        for (int i = 0; i < parent.childCount; i++) children[i] = parent.GetChild(i).gameObject;
        foreach (var child in children) Undo.DestroyObjectImmediate(child);

        for (int i = 0; i < spline.Count; i++) {
            BezierKnot knot = spline[i];
            Vector3 worldPos = script._Spline.transform.TransformPoint(knot.Position);

            GameObject go = new($"Point_Knot_{i}");
            go.transform.position = worldPos;
            go.transform.SetParent(parent);

            Undo.RegisterCreatedObjectUndo(go, "Generate Points From Knots");
        }

        AlignRotation(script);
    }
    private void DistributeEvenly(NPC_PathHelper script) {
        Spline spline = script._Spline.Splines[0];
        Transform parent = script._PathPoints;
        int count = parent.childCount;
        if (count < 2) return;

        for (int i = 0; i < count; i++) {
            Transform child = parent.GetChild(i);
            Undo.RecordObject(child.transform, "Distribute Evenly");

            float t;
            if (spline.Closed)
                t = (float)i / count;
            else
                t = (float)i / (count - 1);

            Vector3 worldPos = script._Spline.transform.TransformPoint(spline.EvaluatePosition(t));
            child.position = worldPos;
        }
    }
    private void AlignRotation(NPC_PathHelper script) {
        Spline spline = script._Spline.Splines[0];
        Transform parent = script._PathPoints;

        for (int i = 0; i < parent.childCount; i++) {
            Transform child = parent.GetChild(i);
            Undo.RecordObject(child.transform, "Align Rotation");

            // Efficient way to find nearest point info
            using (var native = new NativeSpline(spline, script._Spline.transform.localToWorldMatrix)) {
                float t;
                SplineUtility.GetNearestPoint(native, child.position, out _, out t);
                Vector3 forward = native.EvaluateTangent(t);

                if (forward != Vector3.zero) {
                    child.rotation = Quaternion.LookRotation(forward, Vector3.up);
                }
            }
        }
    }
}