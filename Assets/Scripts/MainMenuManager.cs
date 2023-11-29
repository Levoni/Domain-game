using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
   public GameObject userNameMainText;
   public GameObject LoginUI;
   public GameObject MainMenuUI;
   public GameObject CreditUI;
   public GameObject HighScoreUI;
   public GameObject HowToPlayUI;

   public GameObject usernameInput;
   public GameObject passwordInput;
   public GameObject loginResult;
   public GameObject highScoreListStart;
   public GameObject highScorePrefab;

   public int highScorePage = 0;
   public GameObject highScoreNextButton;
   public GameObject highScorePrevButton;

   public void ViewUI(string UIType)
   {
      if (UIType == "login")
      {
         DisableAllUI();
         LoginUI.SetActive(true);
      }
      else if (UIType == "main")
      {
         DisableAllUI();
         MainMenuUI.SetActive(true);
      }
      else if (UIType == "credit")
      {
         DisableAllUI();
         CreditUI.SetActive(true);
      }
      else if (UIType == "highscore")
      {
         DisableAllUI();
         HighScoreUI.SetActive(true);
         GetHighScores();
      }
      else if (UIType == "howtoplay")
      {
         DisableAllUI();
         HowToPlayUI.SetActive(true);
      }
   }

   public async void Login()
   {
      var userNameText = usernameInput.GetComponent<TMP_InputField>().text;
      var passwordText = passwordInput.GetComponent<TMP_InputField>().text;
      loginResult.GetComponent<TextMeshProUGUI>().SetText("Logging in...");
      var helper = new HTTPRequestHelper();
      var success = await helper.SendLoginRequest(userNameText, passwordText);
      if (success)
      {
         PlayerPrefs.SetString("username", userNameText);
         PlayerPrefs.SetString("password", passwordText);
         userNameMainText.GetComponent<TextMeshProUGUI>().SetText("User: " + userNameText);
         loginResult.GetComponent<TextMeshProUGUI>().SetText("Logged in as user: " + userNameText);
      }
      else
      {
         loginResult.GetComponent<TextMeshProUGUI>().SetText("Failed to log in as user: " + userNameText);
      }
   }

   public void Logout()
   {
      PlayerPrefs.DeleteKey("username");
      PlayerPrefs.DeleteKey("password");
      userNameMainText.GetComponent<TextMeshProUGUI>().SetText("User: ");
      loginResult.GetComponent<TextMeshProUGUI>().SetText("Logged out");
   }
   public async void GetHighScores()
   {
      highScoreListStart.GetComponent<TextMeshProUGUI>().text = "Loading...";
      var local = GameObject.Find("Local").GetComponent<Checkbox>().GetValue();
      var daily = GameObject.Find("Daily").GetComponent<Checkbox>().GetValue();
      highScoreNextButton.SetActive(false);
      highScorePrevButton.SetActive(false);
      if (local)
      {
         string highScoreString = string.Empty;
         if (daily)
         {
            highScoreString = PlayerPrefs.GetString("localDailyHighScore");
         }
         else
         {
            highScoreString = PlayerPrefs.GetString("localHighScore");
         }
         var leaderboard = JsonUtility.FromJson<Leaderboard>(highScoreString);
         var scores = leaderboard.highScores;
         if (scores.Count > 0)
         {
            highScoreListStart.GetComponent<TextMeshProUGUI>().text = "";
            var num = highScoreListStart.transform.childCount;
            for (int i = num - 1; i >= 0; i--)
            {
               GameObject.Destroy(highScoreListStart.transform.GetChild(i).gameObject);
            }
            var count = 0;
            scores = scores.OrderByDescending(x => x.score).ToList();
            foreach (HighScore highScore in scores)
            {
               if (count < 5)
               {
                  var newObject = GameObject.Instantiate(highScorePrefab, highScoreListStart.transform);
                  newObject.transform.localPosition = new Vector3(-400, -100 * count, 0);
                  newObject.GetComponent<TextMeshProUGUI>().text = (count + 1).ToString() + ". " + highScore.display_name + ": " + highScore.score;
                  count++;
               }
               if (count >= 5)
               {
                  var newObject = GameObject.Instantiate(highScorePrefab, highScoreListStart.transform);
                  newObject.transform.localPosition = new Vector3(400, -100 * (count - 5), 0);
                  newObject.GetComponent<TextMeshProUGUI>().text = (count + 1).ToString() + ". " + highScore.display_name + ": " + highScore.score;
                  count++;
               }
            }
         }
         else
         {
            var num = highScoreListStart.transform.childCount;
            for (int i = num - 1; i >= 0; i--)
            {
               GameObject.Destroy(highScoreListStart.transform.GetChild(i).gameObject);
            }
            highScoreListStart.GetComponent<TextMeshProUGUI>().text = "No High Scores";
         }

      }
      else
      {
         var helper = new HTTPRequestHelper();
         var scores = await helper.GetHighScores(daily, highScorePage);
         if (scores != null)
         {
            if (scores.Count > 0)
            {
               highScoreListStart.GetComponent<TextMeshProUGUI>().text = "";
               var num = highScoreListStart.transform.childCount;
               for (int i = num - 1; i >= 0; i--)
               {
                  GameObject.Destroy(highScoreListStart.transform.GetChild(i).gameObject);
               }
               var count = 0;
               scores = scores.OrderByDescending(x => x.score).ToList();
               foreach (HighScore highScore in scores)
               {
                  if(count < 5)
                  {
                     var newObject = GameObject.Instantiate(highScorePrefab, highScoreListStart.transform);
                     newObject.transform.localPosition = new Vector3(-400, -100 * count, 0);
                     newObject.GetComponent<TextMeshProUGUI>().text = ((count + (highScorePage * helper.highScoreLimitNum)) + 1).ToString() + ". " + highScore.display_name + ": " + highScore.score;
                     count++;
                  } else if(count >= 5)
                  {
                     var newObject = GameObject.Instantiate(highScorePrefab, highScoreListStart.transform);
                     newObject.transform.localPosition = new Vector3(400, -100 * (count - 5), 0);
                     newObject.GetComponent<TextMeshProUGUI>().text = ((count + (highScorePage * helper.highScoreLimitNum)) + 1).ToString() + ". " + highScore.display_name + ": " + highScore.score;
                     count++;
                  }
               }
               if(scores.Count == helper.highScoreLimitNum)
               {
                  highScoreNextButton.SetActive(true);
               }
               if(highScorePage > 0)
               {
                  highScorePrevButton.SetActive(true);
               }
            }
            else
            {
               var num = highScoreListStart.transform.childCount;
               for (int i = num - 1; i >= 0; i--)
               {
                  GameObject.Destroy(highScoreListStart.transform.GetChild(i).gameObject);
               }
               highScoreListStart.GetComponent<TextMeshProUGUI>().text = "No High Scores";
            }
         }
         else
         {
            highScoreListStart.GetComponent<TextMeshProUGUI>().text = "Failed to get high scores";
         }

      }

   }

   public void UpdateHSPage(int incriment)
   {
      highScorePage += incriment;
      GetHighScores();
   }

   public void Quit()
   {
      Application.Quit();
   }
   // Start is called before the first frame update
   void Start()
   {
      var username = PlayerPrefs.GetString("username", "");
      userNameMainText.GetComponent<TextMeshProUGUI>().SetText("User: " + username);
      var localHighScore = PlayerPrefs.GetString("localHighScore", string.Empty);
      if (localHighScore == string.Empty)
      {
         var newLHS = new Leaderboard();
         newLHS.maxCount = 10;
         newLHS.highScores = new List<HighScore>();
         var objectString = JsonUtility.ToJson(newLHS);
         PlayerPrefs.SetString("localHighScore", objectString);
      }
      var localDailyHighScore = PlayerPrefs.GetString("localDailyHighScore", string.Empty);
      if (localDailyHighScore == string.Empty)
      {
         var newLDHS = new Leaderboard();
         newLDHS.maxCount = 10;
         newLDHS.highScores = new List<HighScore>();
         var objectString = JsonUtility.ToJson(newLDHS);
         PlayerPrefs.SetString("localDailyHighScore", objectString);
      }
      var lastLocalUpdate = PlayerPrefs.GetFloat("lastLocalUpdate", -1);
      if (lastLocalUpdate == -1)
      {
         var baseDate = DateTime.UnixEpoch;
         PlayerPrefs.SetFloat("lastLocalUpdate", (float)baseDate.ToBinary());
      }

   }

   // Update is called once per frame
   void Update()
   {

   }

   public void DisableAllUI()
   {
      LoginUI.SetActive(false);
      MainMenuUI.SetActive(false);
      CreditUI.SetActive(false);
      HighScoreUI.SetActive(false);
      HowToPlayUI.SetActive(false);
   }
}

[System.Serializable]
public class Leaderboard
{
   public List<HighScore> highScores;
   public int maxCount;
}