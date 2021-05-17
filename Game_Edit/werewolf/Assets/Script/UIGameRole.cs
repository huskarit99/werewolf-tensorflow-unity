using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameRole : MonoBehaviour
{
    public Text RoleText;
    public void SetRoleText(string _role)
    {
        if (RoleText != null)
        {
            RoleText.text = "Role: " + _role;
        }
    }
}
