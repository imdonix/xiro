using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI : MonoBehaviour
{

    public TMPro.TMP_Text hp;
    public TMPro.TMP_Text mag;
    public RectTransform trans;

    public TMPro.TMP_Text obj;

    private static UI ui;

    private void Awake()
    {
        ui = this;
    }

    public static void Show(int hpp, int magg, float cooldown)
    {
        ui.hp.text = hpp.ToString();
        ui.mag.text = magg.ToString();
        ui.trans.sizeDelta = new Vector2(500 * Math.Max((cooldown / 2), 0), 10); 
    }

    public static void Objective(int stage, int zombie, int box)
    {
        if(stage == -1)
        {
            ui.obj.text = "Waiting for players...";
        }
        else if(stage == 0)
        {
            ui.obj.text = $"Kill Zombies: 50/{zombie}";
        }
        else if(stage == 1)
        {
            ui.obj.text = $"Deliver box to the Garage: 3/{box}";
        }
        else if(stage == 2)
        {
            ui.obj.text = $"Chicken Eater Dinner Winter";
        }
    }
}
