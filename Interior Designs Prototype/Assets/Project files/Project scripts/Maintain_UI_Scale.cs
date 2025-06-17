using UnityEngine;

public class Maintain_UI_Scale : MonoBehaviour
{
    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
        transform.localScale = originalScale;
    }

    void LateUpdate()
    {
        transform.localScale = originalScale;
    }
}
