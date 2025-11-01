using UnityEngine;

public class MenuRotacionarCarro : MonoBehaviour
{
    void FixedUpdate()
    {
        transform.Rotate(0, 0.5f, 0);
    }
}
