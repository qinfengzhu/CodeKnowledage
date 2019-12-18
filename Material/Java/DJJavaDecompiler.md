### 关于Java的反编译工具 `DJJavaDecompiler`

下面是一个有用的参考文档，说明如何批量编译

http://www.udl.es/usuaris/jordim/Progs/Readme.txt

安装好之后，找到它的安装路径，我的是：C:\Program Files\decomp。然后将该路径加入到环境变量path中。


首先将要反编译的jar文件，用Winrar解压到和jar文件名称一样的文件夹中

在命令输入框中输入如下命令：

jad -o -r -dF:\am -sjava F:\amclientsdk\**\*.class

其中：F:\am 表示编译后文件的存放路径

F:\amclientsdk\**\*.class 表示需要被反编译的文件

-o  - overwrite output files without confirmation (default: no) 无需确定覆写文件

-r  - restore package directory structrure 恢复包目录结构

-s <ext></ext>- output file extension (by default '.jad') 如果不设置为-sjava，则默认扩展名为.jad

其他的，F:\amclientsdk\**\*.class 中的两颗接连的星，表示任意层次的子目录。
