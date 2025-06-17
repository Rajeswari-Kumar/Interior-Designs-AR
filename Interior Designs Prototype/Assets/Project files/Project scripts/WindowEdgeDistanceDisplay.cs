using UnityEngine;
using TMPro;

public class WindowEdgeDistanceDisplay : MonoBehaviour
{
    public GameObject canvasPrefab;
    public Vector3 canvasOffset = new Vector3(0, 2f, 0f);

    private GameObject canvasInstance;
    private TMP_InputField distanceInputHorizontal;
    private TMP_InputField distanceInputVertical;

    private Transform wall;
    private LineRenderer lineRendererHorizontal;
    private LineRenderer lineRendererVertical;
    public Material glass;

    private GameObject leftEdgeSphere;
    private GameObject rightEdgeSphere;
    public float edgeSphereRadius = 0.025f; // small radius
    public Material edgeSphereMaterial;
    Vector3 wallRight;
    float halfWidthWall;
    Vector3 wallCenter;
    Vector3 leftEdgeWall;
    Vector3 rightEdgeWall;
    Vector3 winHorizEdge;
    Vector3 wallHorizEdge;
    Vector3 leftEdge;
    void Start()
    {
        wall = transform.parent;
        if (wall == null)
        {
            Debug.LogWarning("Window is not parented to a wall.");
            //transform.gameObject.GetComponent<Renderer>().material.color = Color.blue;
            return;
        }

        // Instantiate canvas
        canvasInstance = Instantiate(canvasPrefab);
        canvasInstance.GetComponent<Canvas>().sortingOrder = 10; // higher than main canvas

        canvasInstance.name = $"EdgeDistanceCanvas_{name}";
        canvasInstance.transform.SetParent(null); // World-space
        WindowReferenceHolder holder = canvasInstance.AddComponent<WindowReferenceHolder>();
        holder.windowDisplay = this;

        TMP_InputField[] inputs = canvasInstance.GetComponentsInChildren<TMP_InputField>();
        if (inputs.Length >= 2)
        {
            distanceInputHorizontal = inputs[0];
            distanceInputVertical = inputs[1];
        }

        // Create child GameObjects for LineRenderers
        GameObject horizLineObj = new GameObject("HorizontalLineRenderer");
        horizLineObj.transform.SetParent(transform);
        lineRendererHorizontal = horizLineObj.AddComponent<LineRenderer>();

        GameObject vertLineObj = new GameObject("VerticalLineRenderer");
        vertLineObj.transform.SetParent(transform);
        lineRendererVertical = vertLineObj.AddComponent<LineRenderer>();

        SetupLineRenderer(lineRendererHorizontal, Color.red);
        SetupLineRenderer(lineRendererVertical, Color.red);
    }

    void SetupLineRenderer(LineRenderer lr, Color color)
    {
        lr.positionCount = 2;
        lr.startWidth = 0.01f;
        lr.endWidth = 0.01f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;
    }

    void Update()
    {
        //UpdateCanvasAndLines();
    }

    public void UpdateCanvasAndLines()
    {
       
        if (canvasInstance == null || wall == null) return;

        WindowDistanceEditor editor = canvasInstance.GetComponent<WindowDistanceEditor>();
        if (editor != null && editor.isEditing)
            return; // Skip update while user is editing input

        MakeWallTransparent(0.4f, wall.gameObject);
        // MakeWallTransparent(0.85f , this.gameObject);

        //Vector3 windowCenter = transform.position;
        Vector3 windowCenter = transform.position;

        Vector3 windowRight = transform.right;
        Vector3 windowUp = transform.up;

        float halfWidth = transform.lossyScale.x / 2f;
        float halfHeight = transform.lossyScale.y / 2f;

        leftEdge = windowCenter - windowRight * halfWidth;
        Vector3 rightEdge = windowCenter + windowRight * halfWidth;
        Vector3 topEdge = windowCenter + windowUp * halfHeight;
        Vector3 bottomEdge = windowCenter - windowUp * halfHeight;


        Vector3 forward = wall.transform.forward;
        Vector3 right = wall.transform.right;

        // Check which axis the wall is aligned with more
        if (Mathf.Abs(Vector3.Dot(right, Vector3.right)) > Mathf.Abs(Vector3.Dot(forward, Vector3.right)))
        {
            // Wall is aligned more with world X
            wallRight = wall.transform.right;
            halfWidthWall = wall.transform.localScale.x / 2f;
            //Debug.Log("Wall aligned along X-axis");
        }
        else
        {
            // Wall is aligned more with world Z
            //wallRight = wall.transform.InverseTransformDirection(wall.transform.forward);
            wallRight = transform.right;
            halfWidthWall = wall.transform.localScale.x / 2f;
           // Debug.Log("Wall aligned along Z-axis");
        }


           wallCenter = wall.transform.position;
        
        Vector3 wallUp = wall.transform.up;

        float halfHeightWall = wall.transform.lossyScale.y / 2f;

        leftEdgeWall = wallCenter - wallRight * halfWidthWall;
        rightEdgeWall = wallCenter + wallRight * halfWidthWall;
        Vector3 topEdgeWall = wallCenter + wallUp * halfHeightWall;
        Vector3 bottomEdgeWall = wallCenter - wallUp * halfHeightWall ;


        //Bounds wallBounds = wall.GetComponent<Renderer>().bounds;

        // Horizontal
        float distLeft = Vector3.Distance(leftEdge, leftEdgeWall);
        float distRight = Vector3.Distance(rightEdge, rightEdgeWall);
        bool closerToLeft = distLeft < distRight;

        winHorizEdge = closerToLeft ? leftEdge : rightEdge;


        //wallHorizEdge = closerToLeft
        //    ? new Vector3((wall.lossyScale.x > wall.lossyScale.z ? leftEdgeWall.x : leftEdgeWall.z), winHorizEdge.y, winHorizEdge.z)  // Only X from wall, Y & Z from window
        //    : new Vector3((wall.lossyScale.x < wall.lossyScale.z ? rightEdgeWall.x : rightEdgeWall.z), winHorizEdge.y, winHorizEdge.z);

        bool wallLocalXAlignedWithGlobalX = Mathf.Abs(Vector3.Dot(wall.transform.right.normalized, Vector3.right)) >
                                    Mathf.Abs(Vector3.Dot(wall.transform.right.normalized, Vector3.forward));

        Vector3 wallHorizEdge = closerToLeft ? (wallLocalXAlignedWithGlobalX
        ? new Vector3(leftEdgeWall.x, winHorizEdge.y, winHorizEdge.z)  // Vary X only
        : new Vector3(winHorizEdge.x, winHorizEdge.y, leftEdgeWall.z)) // Vary Z only
    : (wallLocalXAlignedWithGlobalX
        ? new Vector3(rightEdgeWall.x, winHorizEdge.y, winHorizEdge.z)
        : new Vector3(winHorizEdge.x, winHorizEdge.y, rightEdgeWall.z));


        float horizontalDist = Vector3.Distance(winHorizEdge, wallHorizEdge);


        // Vertical
        float distTop = Vector3.Distance(topEdge,topEdgeWall);
        float distBottom = Vector3.Distance(bottomEdge, bottomEdgeWall);
        bool closerToBottom = distBottom < distTop;

        Vector3 winVertEdge = closerToBottom ? bottomEdge : topEdge;
        Vector3 wallVertEdge = closerToBottom
              ? new Vector3(winVertEdge.x, bottomEdgeWall.y, winVertEdge.z)
              : new Vector3(winVertEdge.x, topEdgeWall.y, winVertEdge.z);
        float verticalDist = Vector3.Distance(winVertEdge, wallVertEdge);

        // Update canvas
        Vector3 horizMid = (wallHorizEdge + winHorizEdge) / 2f;
        Vector3 vertMid = (wallVertEdge + winVertEdge) / 2f;
        Vector3 finalCanvasPos = (horizMid + vertMid) / 2f;

        //canvasInstance.transform.position = finalCanvasPos + canvasOffset;
        // Determine wall's outward normal in world space
        //Vector3 outward = wall.transform.forward;

        // Offset canvas to be in front of wall, not inside
        //float forwardOffset = 0.05f; // 5 cm in front
        //Vector3 adjustedOffset = outward.normalized * forwardOffset;

        //canvasInstance.transform.position = finalCanvasPos + adjustedOffset;

        // Determine wall's outward normal in world space
        Vector3 outward = wall.transform.forward;

        // Offset canvas to be in front of wall, not inside
        float forwardOffset = 0.05f; // 5 cm in front
        Vector3 adjustedOffset = outward.normalized * forwardOffset;

        canvasInstance.transform.position = finalCanvasPos + transform.forward * -0.5f;

        canvasInstance.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);


        if (distanceInputHorizontal != null)
            distanceInputHorizontal.text = $"--: {horizontalDist:F2} m";

        if (distanceInputVertical != null)
            distanceInputVertical.text = $"|: {verticalDist:F2} m";

        // Draw lines
        if (lineRendererHorizontal != null)
        {
            lineRendererHorizontal.SetPosition(0, wallHorizEdge);
            //this.gameObject.GetComponent<MeshRenderer>().material.color = Color.magenta;
            lineRendererHorizontal.SetPosition(1, winHorizEdge);
        }

        if (lineRendererVertical != null)
        {
            lineRendererVertical.SetPosition(0, wallVertEdge);
            lineRendererVertical.SetPosition(1, winVertEdge);
        }

        // Create or update left and right edge spheres
        if (leftEdgeSphere == null)
        {
            leftEdgeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftEdgeSphere.transform.localScale = Vector3.one * edgeSphereRadius;
            leftEdgeSphere.GetComponent<Collider>().enabled = false;
            if (edgeSphereMaterial) leftEdgeSphere.GetComponent<Renderer>().material = edgeSphereMaterial;
        }

        if (rightEdgeSphere == null)
        {
            rightEdgeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightEdgeSphere.transform.localScale = Vector3.one * edgeSphereRadius;
            rightEdgeSphere.GetComponent<Collider>().enabled = false;
            if (edgeSphereMaterial) rightEdgeSphere.GetComponent<Renderer>().material = edgeSphereMaterial;
        }

        // Update positions of edge spheres
        leftEdgeSphere.transform.position = leftEdge;
        rightEdgeSphere.transform.position = topEdge;

    }

    void MakeWallTransparent(float transparency, GameObject wall)
    {
        Renderer wallRenderer = wall.GetComponent<Renderer>();
        if (wallRenderer == null) return;

        Material mat = wallRenderer.sharedMaterial;
        if (mat == null) return;

        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        mat.renderQueue = 3000;

        Color color = mat.color;
        color.a = transparency;
        mat.color = color;
        wallRenderer.material = glass;
    }

    void OnDestroy()
    {
        if (canvasInstance != null)
            Destroy(canvasInstance);
    }

}
