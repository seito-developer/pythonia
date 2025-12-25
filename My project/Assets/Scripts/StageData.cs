using System;
using System.Collections.Generic;

[Serializable]
public class StageInfo
{
    public int id;
    public string stageName;
}

[Serializable]
public class CategoryInfo
{
    public string categoryName;
    public List<StageInfo> stages; // カテゴリの中にステージリストを持つ
}

[Serializable]
public class StageDataWrapper
{
    public List<CategoryInfo> categories;
}