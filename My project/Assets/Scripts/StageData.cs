using System;
using System.Collections.Generic;

[Serializable]
public class PieceData
{
    public int id;
    public string code;
}

[Serializable]
public class AnswerData
{
    public List<int> correctIds;
    public List<int> correctIndents;
}

[Serializable]
public class StageInfo
{
    public int id;
    public string stageName;
    public string question;
    public List<PieceData> handPieces;
    public AnswerData answer;
}

[Serializable]
public class CategoryInfo
{
    public string categoryName;
    public List<StageInfo> stages;
}

[Serializable]
public class StageDataWrapper
{
    public List<CategoryInfo> categories;
}