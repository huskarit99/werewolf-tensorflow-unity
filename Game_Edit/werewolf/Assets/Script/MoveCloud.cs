using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class MoveCloud : MonoBehaviour
{
    public float MoveSpeed_Max = 20;
    public float MoveSpead_Min = 15;

    GameObject EndPoint_Cloud;
    private GameObject Cloud;
    // Start is called before the first frame update
    void Start()
    {
        EndPoint_Cloud = GameObject.FindGameObjectWithTag("EndPoint_Cloud");
        Cloud = GameObject.FindGameObjectWithTag("Cloud");
    }
    // Update is called once per frame
    void Update()
    {
        Move();
    }
    void Move()
    {
        if (EndPoint_Cloud == null)
        {
            return;
        }
        var endPoint_Cloud = new GameObject();
        endPoint_Cloud.transform.position = new Vector3(EndPoint_Cloud.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);
        transform.LookAt(endPoint_Cloud.transform.position);
        var moveSpeed = Random.Range(MoveSpead_Min, MoveSpeed_Max);
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        if (EndPoint_Cloud.transform.position.x <= transform.position.x)
        {
            Destroy(gameObject);
        }
    }
}
