using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecognizedObjectBehavior : MonoBehaviour
{

    public Vector3 _newPosition = Vector3.zero;
    public Quaternion _newRotation = Quaternion.identity;
    public TextMeshPro tm;
    [HideInInspector]
    public DateTime refreshTime;
    public string Name = string.Empty;
    [SerializeField]
    private GameObject _visual;
    private string _category = string.Empty;
    private Vector3[] _maskPoints = new Vector3[0];
    private LineRenderer _lineRenderer;
    private const int _visibilityLag = 1;


    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public Vector3[] MaskPoints
    {
        set { _maskPoints = value; }
        private get { return _maskPoints; }
    }

    public void ApplyMaterial(Material mat)
    {
        //var rend = _visual.GetComponent<MeshRenderer>();
        //rend.material = mat;
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.material = mat;
    }

    public string Category
    {
        set { if(_category != value) _category = value; }
        get { return _category; }
    }

    // Update is called once per frame
    void Update()
    {
        if (Category != Name || Category == string.Empty) return;


        tm.text = $"{Category}";
        if(DateTime.Now - refreshTime > TimeSpan.FromSeconds(_visibilityLag))
        {
            _visual.SetActive(false);
            _lineRenderer.enabled = false;
        }
        else
        {
            if (!_visual.activeSelf)
            {
                _visual.SetActive(true);
                _lineRenderer.enabled = true;
            }
            var _correctedPosition = new Vector3(_newPosition.x, _newPosition.y, _newPosition.z);
            _visual.transform.localPosition = _correctedPosition;

            _lineRenderer.positionCount = _maskPoints.Length;
            _lineRenderer.startWidth = 0.004f;
            _lineRenderer.endWidth = 0.004f;
            _lineRenderer.loop = true;
            _lineRenderer.SetPositions(_maskPoints);
        }
    }
}
