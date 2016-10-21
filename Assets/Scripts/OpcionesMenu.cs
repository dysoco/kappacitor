﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class OpcionesMenu : MonoBehaviour
{

    int ind_index = 0;
    GameObject[] left_indicators;
    GameObject[] right_indicators;

    // Use this for initialization
    void Start()
    {
        left_indicators = new GameObject[3];
        right_indicators = new GameObject[3];

        for (int i = 0; i <= 2; i++)
        {
            left_indicators[i] = GameObject.Find("ind_left_" + i.ToString());
            right_indicators[i] = GameObject.Find("ind_right_" + i.ToString());
        }

        ind_index = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Submit"))
        {
            switch (ind_index)
            {
                case 0:
                    break;
                case 1:
                    SceneManager.LoadScene("AudioMenu", LoadSceneMode.Single);
                    break;
                case 2:
                    SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                    break;
            }
        }

        if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            if (Input.GetKeyUp(KeyCode.DownArrow))
            {
                ind_index++;
                if (ind_index > 2)
                    ind_index = 2;
            }
            else if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                ind_index--;
                if (ind_index < 0)
                    ind_index = 0;
            }

            for (int i = 0; i <= 2; i++)
            {
                if (i == ind_index)
                {
                    left_indicators[i].GetComponent<SpriteRenderer>().enabled = true;
                    right_indicators[i].GetComponent<SpriteRenderer>().enabled = true;
                }
                else
                {
                    left_indicators[i].GetComponent<SpriteRenderer>().enabled = false;
                    right_indicators[i].GetComponent<SpriteRenderer>().enabled = false;
                }
            }

        }

    }
}