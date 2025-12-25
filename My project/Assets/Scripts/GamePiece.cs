using UnityEngine;
using TMPro;

public class GamePiece : MonoBehaviour
{
    public int pieceId;        // JSONのIDと一致させる
    public int currentIndent = 0; // 現在のインデント数
    public TextMeshProUGUI codeText;

    private float indentWidth = 40f; // 1インデントあたりのズレ幅

    // インデントを増やす
    public void IncreaseIndent()
    {
        if (currentIndent < 5) // 最大インデント制限（任意）
        {
            currentIndent++;
            UpdateVisual();
        }
    }

    // インデントを減らす
    public void DecreaseIndent()
    {
        if (currentIndent > 0)
        {
            currentIndent--;
            UpdateVisual();
        }
    }

    // 見た目を更新（テキストを右にズラす）
    void UpdateVisual()
    {
        // テキストのRectTransformを操作して右にスライドさせる
        Vector3 pos = codeText.rectTransform.localPosition;
        pos.x = currentIndent * indentWidth;
        codeText.rectTransform.localPosition = pos;
    }
}