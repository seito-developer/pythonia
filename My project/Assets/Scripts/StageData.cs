using System;
using System.Collections.Generic;

[Serializable]
public class PieceData
{
    public int id;
    public string code;
    public int indent;
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
    public PieceData[] correctPieces;
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