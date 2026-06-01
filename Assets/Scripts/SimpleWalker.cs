using UnityEngine;

public class SimpleWalker : MonoBehaviour
{
    public float speed = 3f;

    void Update()
    {
        Vector3 input = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) input += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) input += Vector3.back;
        if (Input.GetKey(KeyCode.A)) input += Vector3.left;
        if (Input.GetKey(KeyCode.D)) input += Vector3.right;

        if (input.sqrMagnitude < 0.0001f) return;

        Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = transform.right; right.y = 0f; right.Normalize();

        Vector3 move = (fwd * input.z + right * input.x).normalized;
        Vector3 next = transform.position + move * speed * Time.deltaTime;
        next.y = transform.position.y;
        transform.position = next;
    }
}
