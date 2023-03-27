using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowScript : MonoBehaviour
{
    [SerializeField] GameObject target;
    [SerializeField] Vector3 offset;
    [SerializeField] float damping;
    [SerializeField] float rotationSlerp;

    private Vector3 velocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Camera>().orthographicSize = 15f;
    }

    private void Update()
    {
        //gameObject.GetComponent<Camera>().orthographicSize -= Input.GetAxis("Mouse ScrollWheel");
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //transform.position = new Vector3(target.transform.position.x, target.transform.position.y, -10);
        //transform.rotation = target.transform.rotation;
        //if(Mathf.Abs(target.transform.rotation.eulerAngles.z - transform.rotation.eulerAngles.z) > 1)
        //{
            //transform.Rotate(new Vector3(0, 0, (Mathf.Sign(target.transform.rotation.eulerAngles.z - transform.rotation.eulerAngles.z) * 0.9f)));
        //}
        
    }

    private void FixedUpdate()
    {
        //offset should be -3 for y for third person
        Vector3 movePosition = target.transform.TransformPoint(offset);
        Vector3 damped = Vector3.SmoothDamp(transform.position, movePosition, ref velocity, damping);
        damped = new Vector3(damped.x, damped.y, -3);
        //damped = new Vector3(damped.x, damped.y, -0.35f);
        transform.position = damped;
        Quaternion rotation = Quaternion.Slerp(transform.localRotation, target.transform.localRotation, rotationSlerp);
        rotation = Quaternion.Euler(new Vector3(-60f, 0, 0)); //-60f
        //rotation = Quaternion.Euler(new Vector3(-80f, 0, 0));
        transform.localRotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
        
        //transform.rotation = Quaternion.Euler(-60f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
}
