using System.Collections.Generic;
using UnityEngine;

public class PositionOnPlaceSaver : MonoBehaviour
{
    [HideInInspector] [SerializeField]
    private List<GameObject> _gameObjects = new();

    [HideInInspector] [SerializeField]
    private List<Vector3> _boundsScale = new();

    [HideInInspector] [SerializeField]
    private List<Vector3> _position = new();


    public void Add(GameObject gameObject, Vector3 _bounds, Vector3 position)
    {
        _gameObjects.Add(gameObject);
        _boundsScale.Add(_bounds);
        _position.Add(position);
    }

    public bool CheckPosition(Vector3 position)
    {
        var realScale = Vector3.Scale(_boundsScale[0], _gameObjects[0].transform.localScale);
        var positionBuild = _position[0];
        return true;
    }
}