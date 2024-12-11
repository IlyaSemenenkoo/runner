using UnityEngine;
using UnityEngine.UI;

public class ScoreCounter : MonoBehaviour
{
    private int _score = 0;
    private Text _txt;

    private void Awake()
    {
        _txt = GetComponent<Text>();
    }

    private void Update()
    {
       _txt.text = " " + _score.ToString(); 
    }


    public int Score
    {
        get { return _score; }
        set { _score = value; }
    }
}
