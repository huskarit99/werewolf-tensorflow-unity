using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameRole : MonoBehaviour
{
    public Text RoleText;
    public void SetRoleText(string _role, bool _isKing)
    {
        if (RoleText != null)
        {
            if(_isKing)
            {
                RoleText.text = "Role: " + _role + " (King)" ;
            }
            else
            {
                RoleText.text = "Role: " + _role;
            }
        }
    }
}
