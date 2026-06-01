using UnityEngine;

public class SimpleWalker : MonoBehaviour
{
    public float speed = 3f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 dir = transform.forward * v + transform.right * h;
        dir.y = 0;
        transform.position += dir * speed * Time.deltaTime;
    }
}
