using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsSoundOnly : MonoBehaviour
{
    public int soundIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    private void OnTriggerEnter(Collider other)
    {
        if (soundIndex < 0)
            return;

        AudioSource audio = TheCellGameMgr.instance.Audio_Bank[soundIndex];
        if (audio != null)
        {
            if (audio.isPlaying == false)
            {
                audio.Play();
            }
        }
    }
}
