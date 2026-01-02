using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BackTitleButton : MonoBehaviour
{
    public int stageNumber;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI rankText;

    public void OnClickBackTitle()
    {
        SceneManager.LoadScene("TitleScene");
        AudioManager.instance.PlayMenu(AudioManager.instance.seMenuSource.clip);
    }
}
