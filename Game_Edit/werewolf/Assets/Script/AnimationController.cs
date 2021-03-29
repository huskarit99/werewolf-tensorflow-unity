using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!this.transform.parent.GetComponent<PlayerNetworkBehavior>().isLocalPlayer)
        {
            gameObject.GetComponent<Animator>().enabled = false;
        }
    }
}
