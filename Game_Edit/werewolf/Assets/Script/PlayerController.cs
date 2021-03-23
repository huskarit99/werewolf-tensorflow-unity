using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector3 target;
    GameObject Player;
    Animator anim;
    Vector3 main;
    //--- Đầu
    GameObject head;
    Vector3 Head_Default;
    //--- Tay trái
    GameObject UpperArm_Left;
    Vector3 UpperArm_Left_Default;
    //--- Tay phải
    GameObject UpperArm_Right;
    Vector3 UpperArm_Right_Default;
    //--- Camera
    Transform main_camera;
    //--- Plane
    public GameObject plane;

    public int position;
    // Start is called before the first frame update
    void Start()
    {
        //--- Lấy tọa độ góc ban đầu của nhân vật
        Player = GameObject.FindGameObjectWithTag(Tags_4_Object.Player);
        anim = Player.gameObject.GetComponent<Animator>();
        main = transform.rotation.eulerAngles;
        //--- Lấy tọa độ góc ban đầu của đầu
        head = GameObject.FindGameObjectWithTag(Tags_4_Object.Head);
        Head_Default = head.transform.rotation.eulerAngles;
        //---Lấy tọa độ góc ban đầu của tay trái
        UpperArm_Left = GameObject.FindGameObjectWithTag(Tags_4_Object.UpperArm_Left);
        UpperArm_Left_Default = UpperArm_Left.transform.rotation.eulerAngles;
        //--- Lấy tọa độ góc ban đầu của tay phải
        UpperArm_Right = GameObject.FindGameObjectWithTag(Tags_4_Object.UpperArm_Right);
        UpperArm_Right_Default = UpperArm_Right.transform.rotation.eulerAngles;
        //--- Lấy tọa độ camera
        main_camera = Camera.main.transform;
        plane.transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        //LookAtMainCamera();
        if (Input.GetMouseButtonDown(0) && !anim.GetBool("isVoteYourSelf"))
        {
            //Vote();
            Vote_02();
        }
        if (Input.GetKeyDown(KeyCode.Q)) // && anim.GetBool("isVote"))
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
        if(Input.GetKeyDown(KeyCode.Z) && !anim.GetBool("isVote") && !anim.GetBool("isVoteYourSelf"))
        {
            Dead();
        }
    }
    void Vote()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            target = hit.point;
        }
        //gameObject.
        transform.LookAt(target);
        anim.SetBool("isVote", true);
    }
    void CancelVote(string param)
    {
        anim.SetBool(param, false);
        Player.transform.rotation = Quaternion.Euler(main);

        head.transform.rotation = Quaternion.Euler(Head_Default);
        UpperArm_Left.transform.rotation = Quaternion.Euler(UpperArm_Left_Default);
        UpperArm_Right.transform.rotation = Quaternion.Euler(UpperArm_Right_Default);
    }
    void VoteYourSelf()
    {
        anim.SetBool("isVoteYourSelf", true);
    }
    void Vote_02()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            target = hit.point;
        }
        /*
         * Tìm đầu của nhân vật và xoay theo hướng chọn
         */
        head.transform.rotation = Quaternion.Euler(Head_Default);
        float angle = (target.x - head.transform.position.x) % 90;
        head.transform.Rotate(angle,0,0);
        /*
         * Xoay tay theo hướng chỉ của nhân vật 
         *  - Nếu xoay theo góc dương  => chỉ tay trái
         *  - Nếu xoay theo góc âm  => chỉ tay phải
         */
        UpperArm_Left.transform.rotation = Quaternion.Euler(UpperArm_Left_Default);
        UpperArm_Right.transform.rotation = Quaternion.Euler(UpperArm_Right_Default);
        if (angle >= 0)
        {
            UpperArm_Left.transform.Rotate(-1*angle, 0,90 );
        }
        else
        {
            UpperArm_Right.transform.Rotate(-1*angle, 0, 90);
        }
        anim.SetBool("isVote", true);
    }
    void Dead()
    {
        transform.rotation = Quaternion.Euler(80, 0, 0);
        Destroy(gameObject, 1f);
    }
    //void LookAtMainCamera()
    //{
    //    plane.transform.LookAt(main_camera.transform.position);
    //}
}
