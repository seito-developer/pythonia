using UnityEngine;

public class StageSelectManager : MonoBehaviour
{
    public GameObject stageButtonPrefab;
    public Transform container;
    public GameObject backButton;
    private StageDataWrapper dataWrapper;

    void Awake()
    {
        LoadJson();
        ShowCategories(); // 最初は難易度一覧を表示
    }

    void LoadJson()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("StageData");
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
            script.stageText.text = info.stageName;

            // ステージボタンとしてのクリックイベント（シーン遷移など）を設定
            UnityEngine.UI.Button btn = btnObj.GetComponent<UnityEngine.UI.Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => script.OnClickStage());
        }
    }
}