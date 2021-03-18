using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPlayer : MonoBehaviour
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
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                target = new Vector3(hit.point.x, 0, 0);
            }
            transform.LookAt(target);
            anim.SetBool("isPoint", true);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetBool("isPoint", false);
            transform.rotation = Quaternion.Euler(main);
        }
    }
}
