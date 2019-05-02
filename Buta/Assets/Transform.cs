using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transform : MonoBehaviour
{
    public float speed = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Animator Animtor = GetComponent<Animator>();
        bool Act = false;
        /*
        if (Input.GetKeyDown(KeyCode.Space)) {
            Animtor.SetBool("Act", (Act == true) ? false : true);
        }
        */
        if (Input.GetKey("up")) {
            transform.position += transform.forward * speed * Time.deltaTime;
            Act = true;
        }
        if (Input.GetKey("right")) {
            transform.position += transform.right * speed * Time.deltaTime;
            Act = true;
        }
        if (Input.GetKey("down")) {
            transform.position -= transform.forward * speed * Time.deltaTime;
            Act = true;
        }
        if (Input.GetKey("left")) {
            transform.position -= transform.right * speed * Time.deltaTime;
            Act = true;
        }
        Animtor.SetBool("Act", Act);
    }
}
