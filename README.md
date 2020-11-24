# 简介

- 这是GIGA戏画社Baldr Sky Zero 1+2 的翻译辅助工具。

- ThirdPartyTools文件夹里是一些三方工具。本项目会用到这些三方工具来进行一些提取和导入的中间步骤，操作顺序请阅读以下说明。

- resources文件夹里是修改过的一些资源文件，包括字库图片和偏移文件等。这些将在导入流程中使用。

# 说明

- BSZ将老的NeXAS引擎嵌入到U3D引擎之中，文本分布变得十分零散，所以需要找出来分别进行处理。

## 文本图片的大致分布

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
  
## 操作流程

###剧本提取

- 使用NETFX 4.6.1 Tools的【ildasm】打开【bsz_Data/Managed/Assembly-CSharp.dll】，自动对其反编译，并将其转储为【bsz_Data/Managed/Assembly-CSharp.il】

- 使用本项目工具，【导出-IL脚本提取】，打开【bsz_Data/Managed/Assembly-CSharp.il】，自动将其提取到【bsz_Data/Managed/Assembly-CSharp.txt】

- 使用本项目工具，【导出-剧本字符串导出】，打开【bsz_Data/Managed/Assembly-CSharp.txt】，自动将其提取到【bsz_Data/Managed/msg】文件夹以及【bsz_Data/Managed/string.txt】文件

###脚本代码修改

- 需要对il脚本文件做一处修改，来匹配稍后的字库修改工作：

**il文件3737724行**

```
    IL_0154:  brfalse    IL_015f

    IL_0159:  ldarg.0
    IL_015a:  call       instance void UIFont::Trim()
    IL_015f:  ldarg.0
    IL_0160:  ldfld      valuetype [UnityEngine]UnityEngine.Rect UIFont::mUVRect
    IL_0165:  ret
  } // end of method UIFont::get_uvRect
```

**修改为**

```
    IL_0154:  brfalse    IL_015f

    IL_0159: nop
    IL_015a: nop
    IL_015b: nop
    IL_015c: nop
    IL_015d: nop
    IL_015e: nop
    IL_015f:  ldarg.0
    IL_0160:  ldfld      valuetype [UnityEngine]UnityEngine.Rect UIFont::mUVRect
    IL_0165:  ret
  } // end of method UIFont::get_uvRect
```

###剧本导入

- 使用本项目工具，【导入-剧本字符串导入】，打开【bsz_Data/Managed/Assembly-CSharp.txt】，自动将【bsz_Data/Managed/msg】文件夹以及【bsz_Data/Managed/string.txt】文件导入其中

- 使用本项目工具，【导入-IL脚本导入】，打开【bsz_Data/Managed/Assembly-CSharp.il】，自动将【bsz_Data/Managed/Assembly-CSharp.txt】导入其中

- 使用【ilasm】来编译【bsz_Data/Managed/Assembly-CSharp.il】，生成【bsz_Data/Managed/Assembly-CSharp.dll】。（注意事先备份dll）用cmd命令行完成此操作：

```
ilasm.exe Assembly-CSharp.il /dll /output:Assembly-CSharp.dll
```

###图片导出

- 使用【AssetStudioGUI】，载入文件【bsz_Data/resources.assets】，【Export-All assets】提取到合适的文件夹

###字库文件导入

- 使用【AssetBundleExtractor】，打开【bsz_Data/resources.assets】，选择Path ID为118的MonoBehaviour，用【Import Raw】替换成【FOT_NewRodinPro_B_26_Out23-sharedassets3o.assets-26.dat】

- 选择Path ID为10的Material【FOT_NewRodinPro_B_26_Out23】，用【Plugins-Edit-Load Texture】替换成新的字库图片【FOT_NewRodinPro_B_26_Out23-sharedassets3o.assets-26.png】

- 下接图片导入部分操作，完成后保存assets。

###图片导入

- 使用【AssetBundleExtractor】，打开【bsz_Data/resources.assets】，选择对应的Texture2D的Assets，用【Plugins-Edit-Load Texture】来进行替换图片，全部替换完毕后保存。

###字符串导出与导入

- 待补充
