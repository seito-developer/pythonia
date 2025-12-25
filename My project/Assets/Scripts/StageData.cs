using System;
using System.Collections.Generic;

// 注意：MonoBehaviourを継承させない。クラスの外側の public class も消す。
[Serializable]
public class StageInfo
{
    public int id;
    public string stageName;
}

[Serializable]
public class StageDataWrapper
{
    public List<StageInfo> stages;
}