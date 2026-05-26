using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerposition : MonoBehaviour
{
    public float PlayerPosX = 0.0f, PlayerPosY = 0.0f, PlayerPosZ = 0.0f;

    void Start()
    {
        PlayerPosX = PlayerPrefs.GetFloat("playerPosX");
        PlayerPosY = PlayerPrefs.GetFloat("playerPosY");
        PlayerPosZ = PlayerPrefs.GetFloat("playerPosZ");

        if (PlayerPosX == 0.0f)
        {
            gameObject.transform.position = new Vector3(1668f, -122f, 74f);
            Debug.Log("³õÊ¼");
        }

        if (PlayerPosX != 0.0f)
        {
            gameObject.transform.position = new Vector3(PlayerPosX, PlayerPosY, PlayerPosZ);
            Debug.Log("¼ÇÂ¼");
        }
    }
}
