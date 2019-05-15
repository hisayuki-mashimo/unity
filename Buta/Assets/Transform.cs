using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transform : MonoBehaviour
{
    GameObject MainCamera;

    public float speed = 3.0f;

    // Start is called before the first frame update
    void Start()
    {
        this.MainCamera = GameObject.Find("Main Camera");
    }

    // Update is called once per frame
    void Update()
    {
        Animator Animtor = GetComponent<Animator>();

        bool Act = false;

        if (Input.GetKey("left")) {
            this.transform.position += this.transform.forward * speed * Time.deltaTime;
            this.MainCamera.transform.position += this.transform.forward * speed * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            Act = true;
        }
        if (Input.GetKey("up")) {
            this.transform.position += this.transform.forward * speed * Time.deltaTime;
            this.MainCamera.transform.position += this.transform.forward * speed * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(0.0f, 90, 0.0f);
            Act = true;
        }
        if (Input.GetKey("right")) {
            this.transform.position += this.transform.forward * speed * Time.deltaTime;
            this.MainCamera.transform.position += this.transform.forward * speed * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(0.0f, 180, 0.0f);
            Act = true;
        }
        if (Input.GetKey("down")) {
            this.transform.position += this.transform.forward * speed * Time.deltaTime;
            this.MainCamera.transform.position += this.transform.forward * speed * Time.deltaTime;
            this.transform.rotation = Quaternion.Euler(0.0f, 270, 0.0f);
            Act = true;
        }

        Animtor.SetBool("Act", Act);
    }
}
