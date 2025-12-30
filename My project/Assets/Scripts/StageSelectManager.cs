using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;

public class StageSelectManager : MonoBehaviour
{
    public GameObject stageButtonPrefab;
    public Transform container;
    public GameObject backButton;
    private StageDataWrapper dataWrapper;

    // PlayFabから取得したランクデータを保持する
    private Dictionary<string, string> playerRanks = new Dictionary<string, string>();

    void Awake()
    {
        LoadJson();
        // 1. まずは難易度一覧を表示（この時点ではランク不要）
        ShowCategories();

        // 2. 裏でPlayFabから全ランクデータをロードしておく
        LoadAllRanks();
    }

    // PlayFabからデータ取得
    public void LoadAllRanks()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request,
            result =>
            {
                playerRanks.Clear();
                if (result.Data != null)
                {
                    foreach (var item in result.Data)
                    {
                        playerRanks.Add(item.Key, item.Value.Value);
                    }
                }
                Debug.Log("PlayFabデータの同期完了");
            },
            error => Debug.LogError("ロード失敗: " + error.GenerateErrorReport())
        );
    }

    void LoadJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("StageIndex");

        if (jsonFile == null)
        {
            Debug.LogError("Resources/StageIndex.json が見つかりません！");
            return;
        }

        dataWrapper = JsonUtility.FromJson<StageDataWrapper>(jsonFile.text);
    }

    // コンテナの中身を空にする共通処理
    void ClearContainer()
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }

    // 第一段階：難易度一覧を表示
    public void ShowCategories()
    {
        ClearContainer();

        // 戻るボタンを隠す
        if (backButton != null) backButton.SetActive(false);

        foreach (CategoryInfo cat in dataWrapper.categories)
        {
            GameObject btnObj = Instantiate(stageButtonPrefab, container);
            StageButton script = btnObj.GetComponent<StageButton>();

            // ボタンに難易度名を表示し、クリック時に「ShowStages」を呼ぶようにする
            script.stageText.text = cat.categoryName;
            // 難易度ボタンにはランク表示は不要
            if (script.rankText != null)
            {
                script.rankText.gameObject.SetActive(false);
            }

            // 重要：ボタンのクリックイベントをスクリプトから上書き設定する
            UnityEngine.UI.Button btn = btnObj.GetComponent<UnityEngine.UI.Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => ShowStages(cat));
        }
    }

    // 第二段階：特定の難易度内のステージ一覧を表示
    public void ShowStages(CategoryInfo category)
    {
        ClearContainer();

        // 戻るボタンを表示する
        if (backButton != null) backButton.SetActive(true);

        foreach (StageInfo info in category.stages)
        {
            GameObject btnObj = Instantiate(stageButtonPrefab, container);
            StageButton script = btnObj.GetComponent<StageButton>();

            script.stageNumber = info.id;

            string rankKey = $"Stage_{info.id}_Rank";
            string rankValue = "---"; // デフォルト（未クリア）

            if (playerRanks.ContainsKey(rankKey))
            {
                rankValue = playerRanks[rankKey];
            }

            // script.stageText.text = $"{info.stageName} [{rankValue}]";
            script.stageText.text = info.stageName;
            script.rankText.text = $"Clear Rank: {rankValue}";

            // ステージボタンとしてのクリックイベント（シーン遷移など）を設定
            UnityEngine.UI.Button btn = btnObj.GetComponent<UnityEngine.UI.Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => script.OnClickStage());
        }
    }
}