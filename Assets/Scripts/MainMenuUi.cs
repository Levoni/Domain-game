using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUi : MonoBehaviour
{
   public MainMenuManager manager { get
      {
         return GameObject.Find("Main Camera").GetComponent<MainMenuManager>();
      } 
   }
   
   public void StartGame()
   {
      SceneManager.LoadScene(1);
   }

   public async void Login()
   {
      manager.Login();
   }

   public void ViewMainMenu()
   {
      manager.ViewUI("main");
   }
   public void ViewLogin()
   {
      manager.ViewUI("login");
   }

   public void ViewHighScore()
   {
      manager.ViewUI("highscore");
   }

   public void ViewCredits()
   {
      manager.ViewUI("credit");
   }

   public void ViewHowToPlay()
   {

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