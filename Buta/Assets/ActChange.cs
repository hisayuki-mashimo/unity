using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActChange : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Animator Animtor = GetComponent<Animator>();
        bool Act = Animtor.GetBool("Act");
        if (Input.GetKeyDown(KeyCode.Space)) {
            Animtor.SetBool("Act", (Act == true) ? false : true);
        }
    }
}
