using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerNetworkBehavior : NetworkBehaviour
{
    Vector3 target;
    GameObject MainPlayer;
    GameObject Human;
    GameObject CameraPlayer;
    Animator anim;
    Vector3 main;
    // Start is called before the first frame update
    void Start()
    {
        MainPlayer = GameObject.FindGameObjectWithTag(Tags_4_Object.Player);
        Human = GameObject.FindGameObjectWithTag(Tags_4_Object.Human);
        CameraPlayer = GameObject.FindGameObjectWithTag(Tags_4_Object.Camera);
        anim = Human.gameObject.GetComponent<Animator>();
        main = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vote();
            }
        }  
    }
    void Vote()
    {
        Ray ray = CameraPlayer.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            target = hit.point;
        }
        //gameObject.
        transform.LookAt(target);
        anim.SetBool(Param_4_Anim.VoteLeft, true);
    }
}
