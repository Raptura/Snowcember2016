using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{

    public Transform target;
    public float damp = 50;
    public float limit;

    private Vector3 velocity = Vector3.zero;
    public Camera cam { get; set; }

    void Awake()
    {
        cam = this.GetComponent<Camera>();

    }

    // Update is called once per frame
    void Update()
    {
        //Check before, and then check in the late update as well
        Vector3 pos = transform.position;

        if (limit != 0)
        {
            pos.x = Mathf.Clamp(pos.x, -limit, limit);
            pos.y = Mathf.Clamp(pos.y, -limit, limit);
        }

        transform.position = pos;

        if (target != null)
        {

            Vector3 point = cam.WorldToViewportPoint(target.position);
            Vector3 delta = target.position - cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = transform.position + delta;
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, damp * Time.deltaTime);
        }

    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (limit != 0)
        {
            pos.x = Mathf.Clamp(pos.x, -limit, limit);
            pos.y = Mathf.Clamp(pos.y, -limit, limit);
        }
        transform.position = pos;
    }
}
