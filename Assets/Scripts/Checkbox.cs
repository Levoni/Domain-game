using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Checkbox : MonoBehaviour
{
   public Sprite checkedSprite;
   public Sprite uncheckedSprite;
   public bool isChecked = false;
   public Image spriteRenderer;

   public bool GetValue()
   {
      return isChecked;
   }

   public void Toggle()
   {
      isChecked = !isChecked;
      if (isChecked) {
         spriteRenderer.sprite = checkedSprite;
      } else
      {
         spriteRenderer.sprite = uncheckedSprite;
      }
      GameObject.Find("Main Camera").GetComponent<MainMenuManager>().GetHighScores();
   }
   // Start is called before the first frame update
   void Start()
   {
      spriteRenderer.sprite = uncheckedSprite;
   }

   // Update is called once per frame
   void Update()
   {

   }
}
