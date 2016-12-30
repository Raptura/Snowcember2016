using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GamecheeOpening : MonoBehaviour
{

    public float timeToMenu = 0.5f;
    private MovieTexture clip;

    // Use this for initialization
    void Start()
    {
        clip = (MovieTexture)GetComponent<RawImage>().mainTexture;
        StartCoroutine(pass(timeToMenu));
    }

    IEnumerator pass(float time)
    {
        yield return new WaitForSeconds(0.2f);

        clip.loop = false;
        clip.Play();

        while (clip.isPlaying)
        {
            yield return null;
        }

        yield return new WaitForSeconds(time);
        SceneManager.LoadScene((int)GameManager.SceneBuild.MainMenu);
    }

}
