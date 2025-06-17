using UnityEngine;

public class Furniture_mover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.005f;
    private bool isSelected = false;
    private Camera mainCamera;
    private Vector2 lastTouchPosition;

    private void Start()
    {
        mainCamera = Camera.main;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
       // HandleMouseSelection();
        if (isSelected)
            //MoveWithMouse();

        HandleTouchSelection();
        if (isSelected)
            MoveWithTouch();

    }

    // 🖱️ PC: Mouse-based selection
    void HandleMouseSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    isSelected = !isSelected;
                    GetComponent<Renderer>().material.color = Color.yellow;
                }
                else
                {
                    isSelected = false;
                }
            }
            else
            {
                isSelected = false;
            }
        }
    }

    void MoveWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        Vector3 moveDirection = new Vector3(mouseX, 0, mouseY);
        transform.localPosition += moveDirection * moveSpeed;
    }

    // 📱 Mobile: Touch-based selection
    void HandleTouchSelection()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Touch touch = Input.GetTouch(0);
            Ray ray = mainCamera.ScreenPointToRay(touch.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    isSelected = !isSelected;
                    GetComponent<Renderer>().material.color = Color.yellow;
                }
                else
                {
                    isSelected = false;
                }
            }
            else
            {
                isSelected = false;
            }
        }
    }

    void MoveWithTouch()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;

                Vector3 moveDirection = new Vector3(delta.x, 0, delta.y);
                transform.localPosition += moveDirection * moveSpeed * Time.deltaTime;
            }
        }
    }
}
