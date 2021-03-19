using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector3 target;
    Animator anim;
    Vector3 main;
    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        main = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !anim.GetBool("isVoteYourSelf"))
        {
            Vote();
        }
        if (Input.GetKeyDown(KeyCode.Q) && anim.GetBool("isVote"))
        {
            CancelVote("isVote");
        }
        if(Input.GetKeyDown(KeyCode.A) && !anim.GetBool("isVote"))
        {
            VoteYourSelf();
        }
        if (Input.GetKeyDown(KeyCode.Q) && anim.GetBool("isVoteYourSelf"))
        {
            CancelVote("isVoteYourSelf");
        }
    }
    void Vote()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            target = new Vector3(hit.point.x, 0, 0);
        }
        transform.LookAt(target);
        anim.SetBool("isVote", true);
    }
    void CancelVote(string param)
    {
        anim.SetBool(param, false);
        transform.rotation = Quaternion.Euler(main);
    }
    void VoteYourSelf()
    {
        anim.SetBool("isVoteYourSelf", true);
    }
   
}
