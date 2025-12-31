using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // どこからでもアクセスできるようにするための変数
    public static AudioManager instance;

    public AudioSource bgmSource; // BGM用スピーカー
    public AudioSource seMenuSource;  // SE用スピーカー
    public AudioSource seTitleSource;  // タイトル用スピーカー
    public AudioSource seResultSuccessSource;  // 結果成功用スピーカー
    public AudioSource seResultFailureSource;  // 結果失敗用スピー
    public AudioSource sePieceSource;  // ピース用スピーカー
    public AudioSource seIndentSource;  // インデント用スピーカー
    public AudioSource seMissSource;  // ミス用スピーカー
    public AudioSource seWindowSource;  // ウィンドウ用スピーカー

    void Awake()
    {
        // シングルトンの設定：すでに存在してたら自分を消す、なければ自分を保持
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // 👈 これが重要！シーンが変わっても消えなくなる
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // BGMを再生する関数
    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // SEを再生する関数
    public void PlayMenu(AudioClip clip)
    {
        seMenuSource.PlayOneShot(clip);
    }
    public void PlayTitle(AudioClip clip)
    {
        seTitleSource.PlayOneShot(clip);
    }
    public void PlayResultSuccess(AudioClip clip)
    {
        seResultSuccessSource.PlayOneShot(clip);
    }
    public void PlayResultFailure(AudioClip clip)
    {
        seResultFailureSource.PlayOneShot(clip);
    }
    public void PlayPiece(AudioClip clip)
    {
        sePieceSource.PlayOneShot(clip);
    }
    public void PlayIndent(AudioClip clip)
    {
        seIndentSource.PlayOneShot(clip);
    }
    public void PlayMiss(AudioClip clip)
    {
        seMissSource.PlayOneShot(clip);
    }
    public void PlayWindow(AudioClip clip)
    {
        seWindowSource.PlayOneShot(clip);
    }
}