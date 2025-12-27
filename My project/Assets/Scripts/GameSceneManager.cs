using UnityEngine;
using TMPro; // テキスト表示用
using System.Collections.Generic; // リスト操作に必要
using System.Linq;               // シャッフルに便利

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

        // 1. 手札のデータを一時的なリストにコピーする
        List<PieceData> shuffledPieces = new List<PieceData>(currentStage.handPieces);

        // 2. リストをランダムにシャッフルする（System.LinqとSystem.Randomを使用）
        System.Random rng = new System.Random();
        shuffledPieces = shuffledPieces.OrderBy(p => rng.Next()).ToList();

        // 3. 既存のピースを削除（念のため）
        foreach (Transform child in handZone)
        {
            Destroy(child.gameObject);
        }

        // 4. シャッフルされた順番でピースを生成し、HandZoneを親にする
        foreach (var pData in shuffledPieces)
        {
            // 第二引数に handZone を指定することで、生成時に直接 HandZone の中に入ります
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