using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
   public int id;
   public int number;
   public int location;
   public bool isFlipped;
   public Building building = Building.None;
   public GameObject hint;
   public GameObject BuildingObject;
   public GameObject Shading;
   public Sprite frontSprite;
   public Sprite backSprite;


   // Start is called before the first frame update
   void Start()
   {
      var spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
      spriteRenderer.sprite = backSprite;
      hint.SetActive(false);
   }

   // Update is called once per frame
   void Update()
   {
      if(Input.GetMouseButtonDown(0))
      {
         Vector3 ray = Camera.main.ScreenToWorldPoint(Input.mousePosition);
         var hit = Physics2D.Raycast(ray, Vector2.zero);
         if (hit.collider == this.gameObject.GetComponent<BoxCollider2D>())
         {
            Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
               HandleCardClick();
         }
      }

   }

   public void setShading(bool isSelectable)
   {
      Shading.SetActive(isSelectable);
   }

   public void ShowHint()
   {
      var hintNumber = Mathf.FloorToInt(Random.Range(0, 3));
      var showNumber = number - hintNumber;
      showNumber = Mathf.Max(showNumber, 0);
      hint.GetComponentInChildren<TextMeshPro>().text = showNumber + "+";
      hint.SetActive(true);
   }

   public void HandleCardClick()
   {
      var manager = GameObject.Find("Manager").GetComponent<GameManager>();
      if (manager != null)
      {
         var result = manager.HandleCardClick(this.gameObject, id, !isFlipped, location);
         if (result)
         {
            FlipCard();
         }
      }
   }

   public void SetCard(int id, int number, int location, Sprite frontSprite)
   {
      this.id = id;
      this.number = number;
      this.location = location;
      this.frontSprite = frontSprite;
      var spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
      spriteRenderer.sprite = backSprite;
      BuildingObject.SetActive(false);
      building = Building.None;
      hint.SetActive(false);
      isFlipped = false;
   }
   
   public void SetBuilding(Building building)
   {
      this.building = building;
      BuildingObject.SetActive(true);
      var renderer = BuildingObject.GetComponent<SpriteRenderer>();
      renderer.sprite = GameObject.Find("SpriteManager").GetComponent<SpriteManager>().GetBuildingSprite(building);
   }

   public void FlipCard()
   {
      var spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
      spriteRenderer.sprite = frontSprite;
      isFlipped = true;
      var soundmanager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
      var audioSource = gameObject.GetComponent<AudioSource>();
      audioSource.PlayOneShot(soundmanager.GetAudioClip(Audio.CardFlip));
      if (number == 9)
      {
         audioSource.PlayOneShot(soundmanager.GetAudioClip(Audio.BadSound));
      }
   }
}
