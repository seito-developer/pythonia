using UnityEngine;

public class StageSelectManager : MonoBehaviour
{
    public GameObject stageButtonPrefab;
    public Transform container;

    void Start()
    {
        LoadStagesFromJson();
    }

    void LoadStagesFromJson()
    {
        // 1. ResourcesフォルダからJSONを読み込む
        TextAsset jsonFile = Resources.Load<TextAsset>("StageData");

        if (jsonFile == null)
        {
            Debug.LogError("JSONファイルが見つかりません！");
            return;
        }

        // 2. JSONをクラスの形に変換
        StageDataWrapper dataWrapper = JsonUtility.FromJson<StageDataWrapper>(jsonFile.text);

        // 3. 読み込んだリストを元にボタンを生成
        foreach (StageInfo info in dataWrapper.stages)
        {
            GameObject btnObj = Instantiate(stageButtonPrefab, container);
            StageButton script = btnObj.GetComponent<StageButton>();

            if (script != null)
            {
                script.stageNumber = info.id;
                if (script.stageText != null)
                {
                    script.stageText.text = info.stageName;
                    Debug.Log("テキストをセットしました: " + info.stageName); // ログを出してみる
                }
                else
                {
                    Debug.LogError("stageTextがアサインされていません！");
                }
            }
        }
    }
}