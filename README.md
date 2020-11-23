# 简介

- 这是GIGA戏画社Baldr Sky Zero 1+2 的翻译辅助工具。

- ThirdPartyTools文件夹里是一些三方工具。本项目会用到这些三方工具来进行一些提取和导入的中间步骤，操作顺序请阅读以下说明。

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

####剧本提取

- 使用NETFX 4.6.1 Tools的【ildasm】打开【bsz_Data/Managed/Assembly-CSharp.dll】，自动对其反编译，并将其转储为【bsz_Data/Managed/Assembly-CSharp.il】

- 使用本项目工具，【导出-IL脚本提取】，打开【bsz_Data/Managed/Assembly-CSharp.il】，自动将其提取到【bsz_Data/Managed/Assembly-CSharp.txt】

- 使用本项目工具，【导出-剧本字符串导出】，打开【bsz_Data/Managed/Assembly-CSharp.txt】，自动将其提取到【bsz_Data/Managed/msg】文件夹以及【bsz_Data/Managed/string.txt】文件

####剧本导入

- 使用本项目工具，【导入-剧本字符串导入】，打开【bsz_Data/Managed/Assembly-CSharp.txt】，自动将【bsz_Data/Managed/msg】文件夹以及【bsz_Data/Managed/string.txt】文件导入其中

- 使用本项目工具，【导入-IL脚本导入】，打开【bsz_Data/Managed/Assembly-CSharp.il】，自动将【bsz_Data/Managed/Assembly-CSharp.txt】导入其中

- 使用【ilasm】来编译【bsz_Data/Managed/Assembly-CSharp.il】，生成【bsz_Data/Managed/Assembly-CSharp.dll】。（注意事先备份dll）用cmd命令行完成此操作：

ilasm.exe Assembly-CSharp.il /dll /output:Assembly-CSharp.dll

####图片导出

- 使用【AssetStudioGUI】，载入文件【bsz_Data/resources.assets】，【Export-All assets】提取到合适的文件夹

####图片导入

- 使用【AssetBundleExtractor】，打开【bsz_Data/resources.assets】，选择对应的Texture2D的Assets，用【Plugins-Edit-Load Texture】来进行替换图片，全部替换完毕后保存。

####字符串导出与导入

- 待补充
