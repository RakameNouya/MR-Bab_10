using UnityEngine;

public class SimpleWalker : MonoBehaviour
{
    [SerializeField] float speed = 4f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = transform.forward * v + transform.right * h;
        transform.position += dir * speed * Time.deltaTime;
    }
}
