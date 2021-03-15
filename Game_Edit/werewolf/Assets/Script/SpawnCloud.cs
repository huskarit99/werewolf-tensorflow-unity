using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCloud : MonoBehaviour
{
    public GameObject[] Clouds;
    public GameObject StartPoint_Cloud;
    public GameObject EndPoint_Cloud;
    //---Spawn Time---//
    private float SpawnTime_Min = 6;
    private float SpawnTime_Max = 8;
    private float LastSpawnTime = 0;
    private float SpawnTime = 0;
    //---Spawn position---//
    private float DiffPosition_Y_Min = -40;
    private float DiffPosition_Y_Max = 30;
    private float DiffPosition_Z_Min = -10;
    private float DiffPosition_Z_Max =  5;
    private float LastDiffPosition_Y = 0;


    // Start is called before the first frame update
    void Start()
    {
        UpdateSpawnTime();
    }
    void UpdateSpawnTime()
    {
        LastSpawnTime = Time.time;
        SpawnTime = Random.Range(SpawnTime_Min, SpawnTime_Max);
    }
    void Spawn()
    {
        var cloud = Clouds[Random.Range(0, Clouds.Length)];
        LastDiffPosition_Y = Random.Range(DiffPosition_Y_Min, DiffPosition_Y_Max);
        var position = new Vector3(StartPoint_Cloud.transform.position.x,
                                    StartPoint_Cloud.transform.position.y + LastDiffPosition_Y,
                                    StartPoint_Cloud.transform.position.z + Random.Range(DiffPosition_Z_Min,DiffPosition_Z_Max));
        Instantiate(cloud, position, Quaternion.identity);
        UpdateSpawnTime();
    }
    // Update is called once per frame
    void Update()
    {
        if (Time.time >= LastSpawnTime + SpawnTime)
        {
            Spawn();
        }
        
    }
}
