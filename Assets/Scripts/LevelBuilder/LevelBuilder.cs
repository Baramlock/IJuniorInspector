using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class LevelBuilder : EditorWindow
{
    private const string _path = "Assets/Editor Resources";

    private Vector2 _scrollPosition;
    private int _selectedElement;
    private int _currentCatalog;
    private int _countContentElement = 5;

    private List<CatalogItem> _catalogs = new();
    private List<GameObject> _catalog = new();

    private bool _building;

    private Matrix4x4 _current = Matrix4x4.identity;
    private TransformPlug _transform = new();

    private GameObject _parent;
    private GameObject _createdObject;

    [MenuItem("Level/Builder")]
    private static void ShowWindow()
    {
        GetWindow(typeof(LevelBuilder));
    }

    private void OnFocus()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        RefreshCatalog();
        if (_parent == null)
            _parent = new GameObject("Environments");
    }

    private void OnGUI()
    {
        DrawDefaultSettings();
        DrawStartBuildingButton();
        DrawCatalogs();
    }

    private void DrawCatalogs()
    {
        if (_catalogs.Count == 0)
            return;

        EditorGUILayout.BeginVertical(GUI.skin.window);
        _currentCatalog = GUILayout.Toolbar(_currentCatalog, _catalogs.Select(x => x.Name).ToArray());
        _catalog = _catalogs[_currentCatalog].Item;
        DrawCatalog(GetCatalogIcons());
        EditorGUILayout.EndVertical();
    }

    private void DrawDefaultSettings()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("Created Object Settings");

        _transform.Position = EditorGUILayout.Vector3Field("Position", _transform.Position);
        _transform.Rotation = EditorGUILayout.Vector3Field("Position", _transform.Rotation);
        _transform.Scale = EditorGUILayout.Vector3Field("Position", _transform.Scale);
    }

    private void DrawStartBuildingButton()
    {
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        _building = GUILayout.Toggle(_building, "Start building", "Button", GUILayout.Height(60));
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Q:
                    _transform.Rotation.y++;
                    break;
                case KeyCode.E:
                    _transform.Rotation.y--;
                    break;
            }
        }

        if (_building)
        {
            if (Raycast(out Vector3 hit))
            {
                DrawPointer(hit, Color.red);

                if (CheckInput())
                    CreateObject(hit);

                sceneView.Repaint();
            }
        }
    }

    private bool Raycast(out Vector3 hit)
    {
        var guiRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        hit = default;
        if (Physics.Raycast(guiRay, out var raycastHit))
        {
            hit = raycastHit.point;
            return true;
        }

        return false;
    }

    private void DrawPointer(Vector3 position, Color color)
    {
        var last = Handles.matrix;

        var meshFilter = _catalog[_selectedElement].GetComponentInChildren<MeshFilter>();
        var mesh = meshFilter.sharedMesh;

        _current = Matrix4x4.TRS(
            position,
            Quaternion.Euler(_transform.Rotation),
            _transform.Scale);

        Handles.matrix = _current;
        Handles.color = color;
        Handles.DrawWireCube(Vector3.zero, mesh.bounds.size);
        Handles.matrix = last;

        Graphics.DrawMeshNow(mesh, position - Quaternion.Euler(_transform.Rotation) * mesh.bounds.center,
            Quaternion.Euler(_transform.Rotation));
    }

    private bool CheckInput()
    {
        HandleUtility.AddDefaultControl(0);
        return Event.current.type == EventType.MouseDown && Event.current.button == 0;
    }

    private void CreateObject(Vector3 positions)
    {
        if (_selectedElement < _catalog.Count)
        {
            GameObject prefab = _catalog[_selectedElement];
            var meshFilter = _catalog[_selectedElement].GetComponentInChildren<MeshFilter>();
            var mesh = meshFilter.sharedMesh;
            _createdObject = Instantiate(prefab,
                positions + Vector3.up * mesh.bounds.size.y / 2 -
                Quaternion.Euler(_transform.Rotation) * mesh.bounds.center,
                Quaternion.Euler(_transform.Rotation), _parent.transform);
            _createdObject.transform.localScale = _transform.Scale;
            Undo.RegisterCreatedObjectUndo(_createdObject, "Create Building");
        }
    }

    private void DrawCatalog(List<GUIContent> catalogIcons)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Buildings");
        _countContentElement = (int) GUILayout.HorizontalSlider(_countContentElement, 1f, 15f, GUILayout.Width(100));
        const float horizontalSliderSize = 30;
        EditorGUILayout.EndHorizontal();
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        
        var countHeightElement = Mathf.RoundToInt((float) catalogIcons.Count / _countContentElement);
        var widthElementSize = ((position.width - horizontalSliderSize) / _countContentElement);
        var heightSize = countHeightElement * widthElementSize;

        _selectedElement = GUILayout.SelectionGrid(_selectedElement, catalogIcons.ToArray(),
            _countContentElement,
            GUILayout.Width(position.width - horizontalSliderSize),
            GUILayout.Height(heightSize));

        EditorGUILayout.EndScrollView();
    }

    private List<GUIContent> GetCatalogIcons()
    {
        List<GUIContent> catalogIcons = new List<GUIContent>();

        foreach (var element in _catalog)
        {
            Texture2D texture = AssetPreview.GetAssetPreview(element);
            catalogIcons.Add(new GUIContent(texture));
        }

        return catalogIcons;
    }

    private void RefreshCatalog()
    {
        _catalog.Clear();
        _catalogs.Clear();

        Directory.CreateDirectory(_path);
        var folders = Directory.GetDirectories(_path);
        foreach (var folder in folders)
        {
            var prefabFiles = Directory.GetFiles(folder, "*.prefab");
            var catalog = prefabFiles.Select(prefabFile =>
                AssetDatabase.LoadAssetAtPath(prefabFile, typeof(GameObject)) as GameObject).ToList();
            _catalogs.Add(new CatalogItem(Path.GetFileName(folder), catalog));
        }
    }
}