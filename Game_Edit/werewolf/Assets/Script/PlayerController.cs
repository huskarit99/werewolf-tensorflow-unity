using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector3 target;
    GameObject Player;
    Animator anim;
    Vector3 main;
    //--- Đầu
    Transform Head;
    Vector3 Head_Default;
    //--- Tay trái
    Transform UpperArm_Left;
    Vector3 UpperArm_Left_Default;
    //--- Camera
    Transform main_camera;
    //--- Plane
    public GameObject plane;
    //--- TimerCountdown
    TimerCountdown TimerCountdown;

    public int position;
    // Start is called before the first frame update
    void Start()
    {
        //--- Lấy tọa độ góc ban đầu của nhân vật
        Player = GameObject.FindGameObjectWithTag(Tags_4_Object.Player);
        anim = Player.gameObject.GetComponent<Animator>();
        main = transform.rotation.eulerAngles;
        //--- Lấy tọa độ góc ban đầu của đầu
        Head = anim.GetBoneTransform(HumanBodyBones.Head);
        Head_Default = Head.transform.rotation.eulerAngles;
        //--- Lấy tọa độ tay trái 
        UpperArm_Left = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        UpperArm_Left_Default = UpperArm_Left.transform.rotation.eulerAngles;
        //--- Lấy tọa độ camera
        main_camera = Camera.main.transform;
        plane.transform.rotation = Quaternion.Euler(Vector3.zero);
        //--- Tìm đối tượng TimerCountdown
        TimerCountdown = FindObjectOfType<TimerCountdown>();
    }

    // Update is called once per frame
    void Update()
    {
        //LookAtMainCamera();
        if (TimerCountdown.getSecondsLeft() > 0)
        {
            if (Input.GetMouseButtonDown(0) && !anim.GetBool("isVoteYourSelf"))
            {
                //Vote();
                Vote_02();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TimerCountdown.setSecondsLeft(10);
            }
        }
        
        
        if (Input.GetKeyDown(KeyCode.Q)) // && anim.GetBool("isVote"))
        {
            CancelVote("isVote");
        }
        if (Input.GetKeyDown(KeyCode.A) && !anim.GetBool("isVote"))
        {
            VoteYourSelf();
        }
        if (Input.GetKeyDown(KeyCode.Q) && anim.GetBool("isVoteYourSelf"))
        {
            CancelVote("isVoteYourSelf");
        }
        if (Input.GetKeyDown(KeyCode.Z) && !anim.GetBool("isVote") && !anim.GetBool("isVoteYourSelf"))
        {
            Dead();
        }
    }
    private void LateUpdate()
    {
        float angle = (target.x - Head.transform.position.x) % 90;
        Head.transform.Rotate(angle, 0, 0);
        UpperArm_Left.transform.Rotate(-1 * angle, 0, 90);
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
            if (hit.transform.tag.Equals(Tags_4_Object.Player))
            {
                /*
                 * Tìm đầu của nhân vật và xoay theo hướng chọn
                 */
                anim.SetBool("isVote", true);
                //Animator animHit = hit.transform.gameObject.GetComponent<Animator>();
                //animHit.SetBool("isDead", true);
                //Destroy(hit.transform.gameObject, 3f);
            }
        }

    }
    void Dead()
    {

    }
    //void LookAtMainCamera()
    //{
    //    plane.transform.LookAt(main_camera.transform.position);
    //}
}
