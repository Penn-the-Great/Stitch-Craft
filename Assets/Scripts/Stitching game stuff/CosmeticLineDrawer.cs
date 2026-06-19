using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CosmeticLineDrawer : MonoBehaviour
{
    [SerializeField] private Camera drawingCamera;
    [SerializeField] private float zDistance = 10f;
    [SerializeField] private float minPointDistance = 0.1f;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private bool clearOnEnable = true;
    [SerializeField] private bool drawOnlyInBounds = false;
    [SerializeField] private Collider2D bounds2D;
    [SerializeField] private Collider bounds3D; 

    private LineRenderer lr;
    private bool isDrawing;
    private Vector3 lastPoint;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
    }

    private void OnEnable()
    {
        isDrawing = false;
        if (clearOnEnable) ClearLine();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDrawing = true;
            TryAddPoint(forceAdd: true);
        }

        if (Input.GetMouseButton(0) && isDrawing)
        TryAddPoint(forceAdd: false);

        if (Input.GetMouseButtonUp(0))
            isDrawing = false;
    }

    private void TryAddPoint(bool forceAdd)
    {
        Camera cam = drawingCamera != null ? drawingCamera: Camera.main;

        if (cam == null) return;

        Vector3 mp = Input.mousePosition;
        mp.z = zDistance;
        Vector3 world = cam.ScreenToWorldPoint(mp);

        if (drawOnlyInBounds && !IsInsideBounds(world))
            return;

        if (lr.positionCount == 0)
        {
            lr.positionCount = 1;
            lr.SetPosition(0, world);
            lastPoint = world;
            return;
        }

        if (!forceAdd && Vector3.Distance(lastPoint, world) < minPointDistance)
            return;

        lr.positionCount += 1;
        lr.SetPosition(lr.positionCount - 1, world);
        lastPoint = world;
    }

    private bool IsInsideBounds(Vector3 world)
    {
        if (bounds2D != null) return bounds2D.OverlapPoint(world);
        if (bounds3D != null) return bounds3D.bounds.Contains(world);
        return true;
    }

    public void ClearLine()
    {
        if (lr!= null) lr.positionCount = 0;
    }

    public void SetDrawingEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled) isDrawing = false;
    }
}
