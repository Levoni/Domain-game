using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class HTTPRequestHelper : MonoBehaviour
{
   string baseURL = "http://localhost:3752";
   //string baseURL = "http://levonpersonalplayarea.com/iisnodetest";
   string gameName = "Domain";
   string submitHighScoreUrl = "/highscore/submit";
   string getHighScoreUrl = "/highscore";
   string loginUrl = "/login";
   string highScoreLimit = "10";
   public int highScoreLimitNum = 10;
   // Start is called before the first frame update


   public async Task<bool> SendSetHighScoreRequet(HighScoreObject highScore)
   {
      var username = PlayerPrefs.GetString("username", "");
      var password = PlayerPrefs.GetString("password", "");
      if (username != string.Empty && password != string.Empty) {
         var bodyString = JsonUtility.ToJson(highScore);
         var response = await SendHTTPPostRequest(submitHighScoreUrl, bodyString);
         if (response == null || !response.IsSuccessStatusCode)
         {
            return false;
         }
         return true;
      }
      return false;
   }

   public async Task<bool> SendSetHighScoreRequet(int score)
   {
      var username = PlayerPrefs.GetString("username", "");
      var password = PlayerPrefs.GetString("password", "");
      if(username != string.Empty && password != string.Empty)
      {
         var HighScoreObject = new HighScoreObject(gameName, score, username, password);
         var bodyString = JsonUtility.ToJson(HighScoreObject);
         var response = await SendHTTPPostRequest(submitHighScoreUrl, bodyString);
         if(response == null || !response.IsSuccessStatusCode)
         {
            return false;
         } 
         else { 
            return true; 
         }
      }
      return false;
   }

   public async Task<List<HighScore>> GetHighScores(bool daily, int pageIndex)
   {
      var date = DateTime.Now;
      var dateString = daily ? "&date=" + date.ToString() : "";
      var query = $"?game={gameName}&limit={highScoreLimit}&start={pageIndex * highScoreLimitNum}" + dateString;
      var response = await SendHTTPGetRequest(getHighScoreUrl, query);
      if(response == null || !response.IsSuccessStatusCode)
      {
         return null;
      } else
      {
         var json = await response.Content.ReadAsStringAsync();
         var scoreResponse = JsonUtility.FromJson<HighScoreResponsePayload>(json);
         return scoreResponse.data;
      }
   }

   public async Task<bool> SendLoginRequest(string username, string password)
   {
      var payload = new LoginRequest(username, password);
      var payloadString = JsonUtility.ToJson(payload);
      var response = await SendHTTPPostRequest(loginUrl, payloadString);
      if(response  == null || !response.IsSuccessStatusCode)
      {
         return false;
      } else
      {
         var json = await response.Content.ReadAsStringAsync();
         var loginResponse = JsonUtility.FromJson<LoginResponsePayload>(json);
         if (!string.IsNullOrEmpty(loginResponse.token))
         {
            return true;
         }
         return false;
      }
   }

   public async Task<HttpResponseMessage> SendHTTPGetRequest(string url, string queryParams)
   {
      try
      {
         var client = new HttpClient();
         var response = await client.GetAsync(baseURL + url + queryParams);
         return response;
      } catch (Exception e)
      {
         return null;
      }

   }
   public async Task<HttpResponseMessage> SendHTTPPostRequest (string url, string body) 
   {
      try
      {
         var client = new HttpClient();
         HttpContent content = new StringContent(body, Encoding.UTF8, "application/json");
         var response = await client.PostAsync(baseURL + url, content);
         return response;
      } catch (Exception e)
      {
         return null;
      }
   }
   void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class HighScoreObject
{
   public string game;
   public int score;
   public string userName;
   public string password;
   public HighScoreObject(string game, int score, string userName, string password)
   {
      this.game = game;
      this.score = score;
      this.userName = userName;
      this.password = password;
   }
}

public class LoginRequest
{
   public string name;
   public string password;

   public LoginRequest(string name, string password)
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
public class HighScoreRequestPayload
{
   public string game;
   public int limit;
   public HighScoreRequestPayload(string game, int limit)
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
