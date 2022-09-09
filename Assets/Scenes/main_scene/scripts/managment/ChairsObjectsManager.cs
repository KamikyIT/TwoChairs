using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChairsObjectsManager : MonoBehaviour
{
    [SerializeField]
    Transform _chairsParent;

    Dictionary<string, Chair> _chairs;
    bool _initialized;

    public Transform ChairsParent { get { return _chairsParent; } }

    public void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;

        _chairs = GameObject.FindObjectsOfType<Chair>().ToDictionary(x => x.ChairId);
    }

    public Chair FindChair(string chairId)
    {
        if (!_chairs.TryGetValue(chairId, out var chair))
        {
            Debug.LogError($"Not found chair for chairId : {chairId}");
            return null;
        }

        return chair;
    }
}
