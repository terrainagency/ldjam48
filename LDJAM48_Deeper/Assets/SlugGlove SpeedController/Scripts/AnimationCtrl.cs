using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCtrl : MonoBehaviour
{
    private PlayerVisual Visual;


    private void Start()
    {
        Visual = GetComponentInParent<PlayerVisual>();
    }
    void Step()
    {
        Visual.Step();
    }

    void LightStep()
    {
        Visual.LightStep();
    }
}
