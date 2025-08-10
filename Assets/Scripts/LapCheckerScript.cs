using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LapCheckerScript : MonoBehaviour
{
    [SerializeField] GameObject text;
    [SerializeField] Rigidbody2D rb;
    private float currentTime = 0;
    private float lastTime = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        text.GetComponent<Text>().text = currentTime.ToString() + "\n" + lastTime.ToString() + "\n\n" + rb.linearVelocity.magnitude;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.name == "Player")
        {
            lastTime = currentTime;
            currentTime = 0;
        }
    }
}
