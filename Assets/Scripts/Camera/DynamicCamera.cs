using System.Collections.Generic;
using UnityEngine;

public class MultiTargetCamera : MonoBehaviour
{
    public List<Transform> targets;
    public float minSize = 5f;
    public float padding = 2f;
    public float moveSmoothTime = 0.3f; // Adjust to control movement smoothness
    public float zoomSmoothTime = 0.5f; // Adjust to control zoom smoothness

    private Camera _camera;
    private Vector3 _moveVelocity;
    private float _zoomVelocity;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (targets.Count == 0)
            return;

        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        //centerPoint.x = transform.position.x; // Fix x-coordinate
        centerPoint.x = centerPoint.x; // Fix x-coordinate

        centerPoint.y = centerPoint.y + (padding*0.75f);

        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(centerPoint.x, centerPoint.y, transform.position.z), ref _moveVelocity, moveSmoothTime);
    }

    void Zoom()
    {
        float requiredSize = CalculateRequiredSize();
        _camera.orthographicSize = Mathf.SmoothDamp(_camera.orthographicSize, requiredSize, ref _zoomVelocity, zoomSmoothTime);
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        Bounds bounds = GetBounds();
        return bounds.center;
    }

    Bounds GetBounds()
    {
        List<Transform> activeTransforms = new List<Transform>();
        
        foreach (Transform target in targets)
        {
            if (target.gameObject.activeSelf)
            {
                activeTransforms.Add(target);
            }
        }

        if (activeTransforms.Count == 0)
        {
            return new Bounds(targets[0].position, Vector3.zero);
        }
        
        Bounds bounds = new Bounds(activeTransforms[0].position, Vector3.zero);

        foreach (Transform target in activeTransforms)
        {
                bounds.Encapsulate(target.position);
        }

        return bounds;
    }

    float CalculateRequiredSize()
    {
        Bounds bounds = GetBounds();

        float targetSizeX = bounds.size.x + padding * 2;
        float targetSizeY = bounds.size.y + padding * 2;

        float sizeX = Mathf.Max(targetSizeX, minSize * 2);
        float sizeY = Mathf.Max(targetSizeY, minSize * 2);

        float aspectRatio = _camera.aspect;
        float newSize = Mathf.Max(sizeX / 2 / aspectRatio, sizeY / 2);

        return newSize;
    }
}
