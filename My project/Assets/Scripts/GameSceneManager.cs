using UnityEngine;
using TMPro; // テキスト表示用

public class GameSceneManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI questionText;
    public Transform handZone;

    [Header("Prefabs")]
    public GameObject piecePrefab;
    private StageInfo currentStage;

    void Start()
    {
        LoadStageData();
        SetupGameUI();
    }

    // 個別JSONファイルを読み込む処理
    void LoadStageData()
    {
        int targetId = GameData.SelectedStageId;

        // Resources/Stages/Stage_X を読み込む
        string filePath = $"Stages/Stage_{targetId}";
        TextAsset jsonFile = Resources.Load<TextAsset>(filePath);

        if (jsonFile != null)
        {
            // 個別ファイルは StageInfo 構造そのものなので直接デコード
            currentStage = JsonUtility.FromJson<StageInfo>(jsonFile.text);
            Debug.Log($"ステージ {targetId} のデータを読み込みました。");
        }
        else
        {
            Debug.LogError($"エラー: {filePath} が見つかりません。Resources/Stages フォルダを確認してください。");
        }
    }

    // 読み込んだデータを画面に反映する処理
    void SetupGameUI()
    {
        if (currentStage == null)
        {
            Debug.LogError("currentStageがNullです！JSON読み込みに失敗しています。");
            return;
        }

        // 問題文とタイトルのセット
        titleText.text = currentStage.stageName;
        questionText.text = currentStage.question;

        // 既存のピースがあれば削除（念のため）
        foreach (Transform child in handZone)
        {
            Destroy(child.gameObject);
        }

        // 手札ピースの生成
        foreach (var pData in currentStage.handPieces)
        {
            GameObject pObj = Instantiate(piecePrefab, handZone);
            GamePiece script = pObj.GetComponent<GamePiece>();

            if (script != null)
            {
                script.pieceId = pData.id;
                script.codeText.text = pData.code;
            }
        }
    }
}