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

   public GameObject usernameInput;
   public GameObject passwordInput;
   public GameObject loginResult;
   public GameObject highScoreListStart;
   public GameObject highScorePrefab;

   //string baseURL = "http://localhost:3752";
   string baseURL = "http://levonpersonalplayarea.com/iisnodetest";


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
   }

   public async void Login()
   {
      var userNameText = usernameInput.GetComponent<TMP_InputField>().text;
      var passwordText = passwordInput.GetComponent<TMP_InputField>().text;
      loginResult.GetComponent<TextMeshProUGUI>().SetText("Logging in...");
      var client = new HttpClient();
      HttpContent content = new StringContent(JsonUtility.ToJson(new LoginPaylad(userNameText, passwordText)), Encoding.UTF8, "application/json");
      try
      {
         var response = await client.PostAsync(baseURL + "/login", content);
         if (response.IsSuccessStatusCode)
         {
            var json = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonUtility.FromJson<LoginResponsePayload>(json);
            if (!string.IsNullOrEmpty(loginResponse.token))
            {
               PlayerPrefs.SetString("username", userNameText);
               PlayerPrefs.SetString("password", passwordText);
               userNameMainText.GetComponent<TextMeshProUGUI>().SetText("User: " + userNameText);
               loginResult.GetComponent<TextMeshProUGUI>().SetText("Logged in as user: " + userNameText);
            }
         }
         else
         {
            loginResult.GetComponent<TextMeshProUGUI>().SetText("Failed to log in as user: " + userNameText);
         }
      }
      catch (Exception ex)
      {
         loginResult.GetComponent<TextMeshProUGUI>().SetText("Failed to log in as user: " + userNameText);
      }

   }

   public async void GetHighScores()
   {
      highScoreListStart.GetComponent<TextMeshProUGUI>().text = "Loading...";
      var local = GameObject.Find("Local").GetComponent<Checkbox>().GetValue();
      var daily = GameObject.Find("Daily").GetComponent<Checkbox>().GetValue();
      if (local)
      {
         string highScoreString = string.Empty;
         if(daily)
         {
            highScoreString = PlayerPrefs.GetString("localDailyHighScore");
         } else
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
               var newObject = GameObject.Instantiate(highScorePrefab, highScoreListStart.transform);
               newObject.transform.localPosition = new Vector3(-200, -100 * count, 0);
               newObject.GetComponent<TextMeshProUGUI>().text = (count + 1).ToString() + ". " + highScore.display_name + ": " + highScore.score;
               count++;
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
         try
         {
            var client = new HttpClient();
            var date = DateTime.Now;
            var dateString = daily ? "&date=" + date.ToString() : "";
            var url = baseURL + "/highscore?game=Domain&limit=10" + dateString;
            var response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
               var json = await response.Content.ReadAsStringAsync();

               var scoreResponse = JsonUtility.FromJson<HighScoreResponsePayload>(json);
               if (scoreResponse.data.Count > 0)
               {
                  highScoreListStart.GetComponent<TextMeshProUGUI>().text = "";
                  var num = highScoreListStart.transform.childCount;
                  for (int i = num - 1; i >= 0; i--)
                  {
                     GameObject.Destroy(highScoreListStart.transform.GetChild(i).gameObject);
                  }
                  var count = 0;
                  var scores = scoreResponse.data;
                  scores = scores.OrderByDescending(x => x.score).ToList();
                  foreach (HighScore highScore in scores)
                  {
                     var newObject = GameObject.Instantiate(highScorePrefab, highScoreListStart.transform);
                     newObject.transform.localPosition = new Vector3(-200, -100 * count, 0);
                     newObject.GetComponent<TextMeshProUGUI>().text = (count + 1).ToString() + ". " + highScore.display_name + ": " + highScore.score;
                     count++;
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
         catch (Exception ex)
         {
            highScoreListStart.GetComponent<TextMeshProUGUI>().text = "Failed to get high scores";
         }
      }

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
   }
}
public class LoginPaylad
{
   public string name;
   public string password;

   public LoginPaylad(string name, string password)
   {
      this.name = name;
      this.password = password;
   }
}
public class LoginResponsePayload
{
   public string token;
   public LoginResponsePayload(string token)
   {
      this.token = token;
   }
}


public class HighScorePayload
{
   public string game;
   public int limit;
   public HighScorePayload(string game, int limit)
   {
      this.game = game;
      this.limit = limit;
   }
}

[System.Serializable]
public class HighScoreResponsePayload
{
   public string success;
   public List<HighScore> data;
}

[System.Serializable]
public class HighScore
{
   public string game;
   public string user_name;
   public int score;
   public string display_name;
}
[System.Serializable]
public class Leaderboard
{
   public List<HighScore> highScores;
   public int maxCount;
}