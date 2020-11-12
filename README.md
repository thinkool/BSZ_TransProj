# 简介

- 这是GIGA戏画社Baldr Sky Zero 1+2 的翻译辅助工具。

- 里面会用到一些三方工具来进行一些提取和导入的中间步骤，操作顺序请阅读以下说明。

# 说明

- BSZ将老的NeXAS引擎嵌入到U3D引擎之中，文本分布变得十分零散，所以需要找出来分别进行处理。

### 文本图片的大致分布

- bsz_Data/Managed/Assembly-CSharp.dll

  ————大部分的剧本，游戏主程序功能等

  ————功能开发进度：导出100%，导入95%

- bsz_Data/resources.assets

  bsz_Data/sharedassets3.assets

  ————主要的系统图片，一部分的系统文本

  ————功能开发进度：导出100%，导入50%

- bsz_Data/StreamingAssets/Data/

  ————另外一部分的系统文本

  ————功能开发进度：导出100%，导入0%
  
### 操作流程

- 后续补充
