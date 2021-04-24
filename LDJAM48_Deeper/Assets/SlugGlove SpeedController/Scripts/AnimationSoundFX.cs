using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSoundFX : MonoBehaviour
{
    public AudioSource SoundBlock;
    public float PitchMax;
    public float PitchMin;

    // Start is called before the first frame update
    void SoundFX()
    {
        SoundBlock.pitch = Random.Range(PitchMin, PitchMax);
        SoundBlock.Play();
    }
}
