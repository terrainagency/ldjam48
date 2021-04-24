using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    public Transform Base;

    public GameObject StepFx;
    public GameObject StepLightFx;
    public GameObject StepAudio;

    public GameObject JumpFx;
    public GameObject JumpAudio;
    public GameObject JumpHeavyFx;
    public GameObject JumpHeavyAudio;

    public GameObject LandingFx;
    public GameObject LandingAudio;

    public GameObject SlidingFx;
    public GameObject SlidingAudio;

    public void Step()
    {
        if (StepAudio)
            Instantiate(StepAudio, Base.position, Quaternion.identity);

        if (StepFx)
        {
            GameObject STP = Instantiate(StepFx, Base.position, Quaternion.identity);
            STP.transform.parent = this.transform;
        }
    }

    public void LightStep()
    {
        if (StepAudio)
            Instantiate(StepAudio, Base.position, Quaternion.identity);

        if (StepLightFx)
        {
            GameObject STP = Instantiate(StepLightFx, Base.position, Quaternion.identity);
            STP.transform.parent = this.transform;
        }
    }

    public void Jump()
    {
        if(JumpAudio)
            Instantiate(JumpAudio, Base.position, Quaternion.identity);
        if (JumpFx)
        {
            GameObject STP = Instantiate(JumpFx, Base.position, Quaternion.identity);
            STP.transform.parent = this.transform;
        }
    }
    public void JumpHeavy()
    {
        if (JumpHeavyAudio)
            Instantiate(JumpHeavyAudio, Base.position, Quaternion.identity);
        if (JumpHeavyFx)
        {
            GameObject STP = Instantiate(JumpHeavyFx, Base.position, Quaternion.identity);
            STP.transform.parent = this.transform;
        }
    }
    public void Landing()
    {
        if (LandingFx)
            Instantiate(LandingFx, Base.position, Quaternion.identity);
        if (LandingAudio)
            Instantiate(LandingAudio, Base.position, Quaternion.identity);
    }

    public void Slide()
    {
        if (SlidingFx)
        {
            GameObject STP = Instantiate(SlidingFx, Base.position, Quaternion.identity);
            STP.transform.parent = this.transform;
        }
        if (SlidingAudio)
            Instantiate(SlidingAudio, Base.position, Quaternion.identity);
    }
}
