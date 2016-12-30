using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public CanvasGroup infoPanel;

    private bool showingCredits, showingHowToPlay;

    public void StartGameSmallMap()
    {
        GameManager.instance.changeMapType(GameManager.MapType.Small);
        SceneManager.LoadScene((int)GameManager.SceneBuild.PlayMode);
    }

    public void StartGameMediumMap()
    {
        GameManager.instance.changeMapType(GameManager.MapType.Medium);
        SceneManager.LoadScene((int)GameManager.SceneBuild.PlayMode);
    }

    public void StartGameLargeMap()
    {
        GameManager.instance.changeMapType(GameManager.MapType.Large);
        SceneManager.LoadScene((int)GameManager.SceneBuild.PlayMode);
    }

    public void ShowHowToPlay()
    {

        if (showingHowToPlay)
        {
            infoPanel.alpha = 0;
            showingHowToPlay = false;
        }
        else
        {
            infoPanel.alpha = 1;
            infoPanel.GetComponentInChildren<Text>().text = GameManager.instance.howToPlay.text;
            showingCredits = false;
            showingHowToPlay = true;
        }
    }

    public void ShowCredits()
    {
        if (showingCredits)
        {
            infoPanel.alpha = 0;
            showingCredits = false;
        }
        else
        {
            infoPanel.alpha = 1;
            infoPanel.GetComponentInChildren<Text>().text = GameManager.instance.credits.text;
            showingCredits = true;
            showingHowToPlay = false;
        }
    }

    public void Quit()
    {
        Application.Quit();
    }

    void Start()
    {
        infoPanel.alpha = 0;
        showingCredits = showingHowToPlay = false;
    }
}
