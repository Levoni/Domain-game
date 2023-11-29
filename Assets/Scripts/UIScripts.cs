using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIScripts : MonoBehaviour
{
   public GameManager Manager { get
      {
         return GameObject.Find("Manager").GetComponent<GameManager>();
      }
   }
   public void MoveOn()
   {
      Manager.BeginLevel();
   }

   public void Finish()
   {
      SceneManager.LoadScene(0);
   }

   public void Resume()
   {
      Manager.display(false, "Menu");
   }

   public void pause()
   {
      Manager.display(true, "Menu");
   }

   public void showQuckUI()
   {
      Manager.display(true, "QuickInfo");
   }

   public void hideQuickUI ()
   {
      Manager.display(false, "QuickInfo");
   }

   public void Build()
   {
      Manager.HandleBuildClick();
   }

   public void UseInfluence()
   {
      Manager.HandleUniquePowerClick(UniquePower.rally);
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
