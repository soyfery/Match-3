using TMPro;
using UnityEngine;

public class HodService : MonoBehaviour
{
    [SerializeField] private TMP_Text _hodText;
    private int _hod;

    public void Start()
    {
        _hod = 30;
        _hodText.text = "30";
    }
    public void DelHod(int hod)
    {
        _hod -= hod;
        _hodText.text = _hod.ToString();
    }

}
