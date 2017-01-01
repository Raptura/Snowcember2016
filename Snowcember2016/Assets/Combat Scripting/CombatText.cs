using UnityEngine;
using System.Collections;

public class CombatText : MonoBehaviour
{

    public string text { get; set; }
    public float lingerTime { get; set; }
    public Vector2 direction { get; set; }
    public float startTime { get; set; }
    public Color color { get; set; }
    public int fontSize { get; set; }
    public TextMesh mesh;
    public static Font font;

    private float movSpeed = 1f;


    // Use this for initialization
    void Start()
    {
        startTime = Time.time;
        mesh = GetComponent<TextMesh>();
        font = Resources.Load<Font>("WinterlandFont");
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction * movSpeed * Time.deltaTime);

        if (Time.time - startTime > lingerTime)
        {
            Destroy(this.gameObject);
        }
    }

    public static void createCombatText(string text, Color color, float lingerTime, Vector2 position, Vector2 direction, int fontSize = 40)
    {
        GameObject newText = new GameObject("Combat Text");
        newText.transform.localScale = new Vector2(0.1f, 0.1f);
        newText.AddComponent<TextMesh>();
        newText.GetComponent<MeshRenderer>().materials = new Material[1];

        newText.GetComponent<TextMesh>().font = font;
        newText.GetComponent<TextMesh>().color = color;
        newText.GetComponent<TextMesh>().text = text;
        newText.GetComponent<TextMesh>().fontSize = fontSize;

        newText.AddComponent<CombatText>();
        newText.GetComponent<CombatText>().text = text;
        newText.GetComponent<CombatText>().color = color;
        newText.GetComponent<CombatText>().lingerTime = lingerTime;
        newText.GetComponent<CombatText>().direction = direction;
        newText.GetComponent<CombatText>().fontSize = fontSize;

        newText.transform.position = position;
    }
}
