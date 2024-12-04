using TMPro;
using UnityEngine;

public class ScoreCounter : MonoBehaviour
{
    private int _score = 0;
    private TextMeshProUGUI _txt;

    private void Awake()
    {
        _txt = GetComponent<TextMeshProUGUI>();
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
