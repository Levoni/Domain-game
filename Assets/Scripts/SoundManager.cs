using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
   public AudioClip[] AudioClips;

   public AudioClip GetAudioClip(Audio audio)
   {
      return AudioClips[(int)audio];
   }
    // Start is called before the first frame update
    void Start()
   {

   }

   // Update is called once per frame
   void Update()
   {

   }
}

public enum Audio
{
   TavernMusic,
   CardFlip,
   BadSound,
   hammer
}
