using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameListPlayer : MonoBehaviour
{
    public GameObject PlayerItem;

    public void ShowListPlayer(GameObject[] players)
    {
        foreach (var player in players)
        {
            Transform child = this.gameObject.transform.Find(player.GetComponent<PlayerNetworkBehavior>().index.ToString()); // Tìm người chơi trên danh sách
            if (child == null) // Nếu không tìm thấy sẽ tạo ra, còn ngược lại giữ nguyên
            {
                GameObject obj = Instantiate(PlayerItem); // Tạo ra game object
                obj.name = player.GetComponent<PlayerNetworkBehavior>().index.ToString();
                obj.transform.SetParent(this.gameObject.transform); // đưa vào trong content
                obj.transform.GetChild(0).GetComponent<Text>().text = player.GetComponent<PlayerNetworkBehavior>().index.ToString(); // gán vị trí
                obj.transform.GetChild(1).GetComponent<Text>().text = player.GetComponent<PlayerNetworkBehavior>().playerName; // gán tên
            }
        }
    }
}
