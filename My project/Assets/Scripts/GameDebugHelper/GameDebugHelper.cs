using UnityEngine;

public static class GameDebugHelper
{
    private const string DebugPath = "Stages/Debug";

    /// <summary>
    /// ローカルのResourcesからデバッグ用JSONを読み込む
    /// </summary>
    public static StageInfo GetLocalDebugStage()
    {
        Debug.LogWarning("デバッグモードまたは未ログインのため、ローカルJSONを読み込みます。");
        TextAsset asset = Resources.Load<TextAsset>(DebugPath);

        if (asset != null)
        {
            Debug.Log($"<color=orange>[Debug]</color> {DebugPath} からデータを読み込みました。");
            return JsonUtility.FromJson<StageInfo>(asset.text);
        }

        Debug.LogError($"[Debug] {DebugPath} が見つかりません。");
        return null;
    }
}