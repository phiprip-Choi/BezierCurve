using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BezierCurve : MonoBehaviour
{
    public GameObject prefab;
    [Tooltip("체크하면 씬 뷰에 커서를 올리고 클릭하여 점 하나가 생기는 기능이 활성화됩니다.")]
    public bool AddPoint = false;
    [Tooltip("활성화할 경우 재귀적인 방식으로 베지어 곡선이 그려집니다.")]
    public bool onRecursive = false;

    [HideInInspector]
    public GameObject[] Points;
    public List<GameObject> points;

    [Range(0f, 1f)]
    public float temp;

    // 재귀 호출 방식
    public Vector3 Recursive(Vector3[] point, float t)
    {
        if (point.Length > 1)
        {
            Vector3[] mid = new Vector3[point.Length - 1];
            for (int i = 0; i < mid.Length; i++) mid[i] = Vector3.Lerp(point[i], point[i + 1], t);
            return Recursive(mid, t);
        }
        return point[0];
    }
    // 공식 도출 방식
    public Vector3 Formula(Vector3[] point, float t)
    {
        Vector3 num = Vector3.zero;
        for (int i = 0; i < point.Length; i++)
            num += combi(point.Length - 1, i) * Mathf.Pow(1 - t, point.Length - 1 - i) * Mathf.Pow(t, i) * point[i];

        return num;
    }
    private int combi(int n, int r) // 이항계수
    {
        if (n == r || r == 0) return 1;

        int max = (n - r >= r) ? n - r : r, num = 1;
        for (int i = n; i > max; i--) num *= i;
        for (int i = n - max; i > 1; i--) num /= i;
        return num;
    }
}

[CanEditMultipleObjects]
[CustomEditor(typeof(BezierCurve))]

public class BezierCurveEdit : Editor
{
    BezierCurve curve;
    private void OnSceneGUI()
    {
        curve = (BezierCurve)target;
        Handles.color = Color.red;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        Vector3 mousePosition = ray.origin; 

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && curve.AddPoint)
        {
            mousePosition.y = 0;
            curve.points.Add(Instantiate(curve.prefab, mousePosition,
            Quaternion.identity, curve.transform));
        }
        else if ((Event.current.type == EventType.MouseDown && Event.current.button == 1)) curve.points.Clear();
        foreach (var nonPoint in curve.Points) if (!curve.points.Contains(nonPoint)) DestroyImmediate(nonPoint);        

        curve.Points = new GameObject[curve.points.Count];
        for (int i = 0; i < curve.Points.Length; i++) curve.Points[i] = curve.points[i];

        Vector3[] Vec = new Vector3[curve.Points.Length];
        for (int i = 0; i < curve.Points.Length; i++)
        {
            curve.Points[i].transform.position = Handles.PositionHandle(curve.Points[i].transform.position, Quaternion.identity);
            if (i < curve.Points.Length - 1) Handles.DrawLine(curve.Points[i].transform.position, curve.Points[i + 1].transform.position);
            Vec[i] = curve.Points[i].transform.position;

            var guiLoc = HandleUtility.WorldToGUIPoint(curve.Points[i].transform.position);  // 오브젝트의 월드좌표를 2D 좌표로 변환
            var rect = new Rect(guiLoc.x - 50.0f, guiLoc.y - 50, 100, 25);    // 라벨 위치 지정

            Handles.BeginGUI();
            var oldbgcolor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;     // 배경 색 지정

            GUI.Box(rect, $"점 {i + 1}");      // Box UI 표시

            GUI.backgroundColor = oldbgcolor;
            Handles.EndGUI();
        }

        Handles.color = Color.blue;
        for (float temp = 0; temp < 1f; temp += 0.01f)
        {
            Handles.DrawLine(curve.onRecursive ? curve.Recursive(Vec, temp) : curve.Formula(Vec, temp), curve.Formula(Vec, temp + 0.01f));
        }
        Recursive(Vec, curve.temp);
    }

    public void Recursive(Vector3[] point, float t)
    {
        Handles.color = Color.green;
        if (point.Length > 1)
        {
            Vector3[] mid = new Vector3[point.Length - 1];
            for (int i = 0; i < mid.Length; i++)
            {
                if(mid.Length < curve.Points.Length - 1) Handles.DrawLine(point[i], point[i+1]);
                mid[i] = Vector3.Lerp(point[i], point[i + 1], t);
            }
            Recursive(mid, t);
        }
    }
}
