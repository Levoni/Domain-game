using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerCard : MonoBehaviour
{
   public bool isFlipped;
   public Sprite frontSprite;
   public Sprite backSprite;
   public Power power;
   public GameObject shading;


   public void HandleClick()
   {
      var manager = GameObject.Find("Manager").GetComponent<GameManager>();
      if (manager != null && isFlipped == false)
      {
         SetShading(true);
         manager.HandlePowerClick(power);
      }
   }

   public void SetShading(bool isSelected)
   {
      shading.SetActive(isSelected);
   }

   public void FlipCard()
   {
      isFlipped = true;
      shading.SetActive(false);
      gameObject.GetComponent<SpriteRenderer>().sprite = backSprite;
      var AudioManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
      var AudioSource = GetComponent<AudioSource>();
      AudioSource.PlayOneShot(AudioManager.GetAudioClip(Audio.CardFlip));
   }

   public void ResetCard()
   {
      isFlipped = false;
      shading.SetActive(false);
      gameObject.GetComponent<SpriteRenderer>().sprite = frontSprite;
   }

   // Start is called before the first frame update
   void Start()
    {
      ResetCard();
    }

    // Update is called once per frame
    void Update()
    {
      if (Input.GetMouseButtonDown(0))
      {
         Vector3 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
         var hit = Physics2D.Raycast(ray, Vector2.zero);
         if (hit.collider == this.gameObject.GetComponent<BoxCollider2D>())
         {
            Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
            HandleClick();
         }
      }
   }
}
