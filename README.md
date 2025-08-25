# Onix1 Bonsai Library
[Bonsai](https://bonsai-rx.org/) library for the [Open Ephys Onix
Acquisition System](https://open-ephys.github.io/onix-docs).

- Open Ephys store: https://open-ephys.org/onix
- Library documentation: https://open-ephys.github.io/bonsai-onix1-docs
- Hardware documentation: https://open-ephys.github.io/onix-docs

### Run in Visual Studio 2022
1. Double click the `Setup.cmd` file in the `.bonsai` folder. This will install a
   portable version of Bonsai in the folder along with its required packages.
1. Open `OpenEphys.Onix1.sln` in Visual Studio (Community Edition is fine).
1. Select which project to compile and run Bonsai against.
   - `OpenEphys.Onix1`: core library (No GUI elements will be loaded)
   - `OpenEphys.Onix1.Design`: core library and GUI elements
1. Press the Play button to
   - Compile the library selected in step 3
   - Run the Bonsai application installed in step 1
   - Instruct Bonsai to load the compiled library

<img alt="Select which library to compile" src="./images/build-and-run.png" width="60%" />

### Enable debugging

#### Enable child process debugging
1. Download and install [Microsoft Child Process Debugging Power Tool 2022](https://marketplace.visualstudio.com/items?itemName=vsdbgplat.MicrosoftChildProcessDebuggingPowerTool2022).
1. Navigate to child process debugging settings.
1. Check `Enable child process debugging`.
1. Click <kbd>Save</kbd>.

![Child process debugging](./images/child-process-debugging.webp)

#### Enable native code debugging
1.  Navigate to debug properties.
1.  Check `Enable native code debugging`.

![Enable native code debugging](./images/native-code-debugging.webp)

## 发布形式

格式：压缩文件

内部文件结构：

```
# 文件结构
NeuracleEphys/
├── bonsai/  # 项目主入口
│   ├── ...
│   ├── Extensions/  # 打包后dll存放位置
│   └── Bonsai.exe
├── python/  # python环境
│   ├── ...
│   └── main.py  # sorting入口
├── Recompile-GUI/ # 重新编译后的Open Ephys Gui
├── xxx.layout  # 同xxx.bonsai成对使用，workflow布局文件
├── xxx.bonsai  # 同上
└── README.md
```



## 当前打包流程

1. 新建文件夹NeuracleEphys

2. 复制项目根目录.bonsai文件夹至 NeuracleEphys/bonsai

3. vs项目内右键OpenEphys.Onix1点击生成

4. 将生成的dll(项目根目录/artifacts/bin/OpenEphys.Onix1/release/)拷贝至NeuracleEphys/bonsai/Extensions/ 

5. 将项目根目录workflow布局文件(xxx.layout xxx.bonsai)和测试数据(xxx.csv) 复制至NeuracleEphys/

6. 参考 项目根目录/python/python环境搭建.md搭建 python环境 并移动至 NeuracleEphys/python

7. 将 项目根目录/python/main.py 拷贝至 NeuracleEphys/python/

8. 将 项目根目录/.bonsai/release_net472和release_netstandard2.0替换NeuracleEphys/bonsai/Packages/Bonsai.Scripting.Python.0.3.0/lib内的同名文件

9. 将 项目根目录/.bonsai/Bonsai.Editor.dll替换NeuracleEphys/bonsai/Packages/Bonsai.Editor.2.8.5/lib/net472内的同名文件

10. 可选：将重新编译好(解除采样率上限)的Open Ephys Gui移动至 NeuracleEphys/Recompile-GUI

11. 在目标电脑上创建指向NeuracleEphys/bonsai/Bonsai.exe的快捷方式并移动至NeuracleEphys/Bonsai.exe，右键打开属性修改目标一栏，增加--editor-scale 1.0参数。意思是默认缩放是1.0，用户可以修改1.0为其他缩放

12. 压缩NeuracleEphys文件夹


    



### 备注

#### 调整界面缩放比例

1. 打包后创建一个指向bonsai/Bonsai.exe的快捷方式
2. 然后打开属性，修改目标一栏，增加--editor-scale 1.0参数。意思是默认缩放是1.0，用户可以修改1.0为其他缩放