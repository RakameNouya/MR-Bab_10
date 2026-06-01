using UnityEngine;

public class SimpleWalker : MonoBehaviour
{
    public float speed = 3f;

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = transform.forward * v + transform.right * h;
        move.y = 0;
        transform.position += move * speed * Time.deltaTime;
    }
}
