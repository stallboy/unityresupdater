# unityresupdater
unity resource updater

##资源自动更新系统

###资源类型
1. res.version
2. res.md5
3. res[]

###资源目录
1. StreamingAssetPath 这个随包发布
2. PersistentAssetPath 这个之后下载，

    * 可能上次下载没完成（这时候文件写了一半这种情况不考虑，猜测现在os应该都是先写data，sync后再写inode），
    * 可能重装时上次安装留下，（比如导致这里的版本低于StreamingAssetPath里的）
    * 可能被用户删除回收（全部或部分）
    * 可能服务器回退版本

###目标
1. 客户端读取正确版本res
2. 适应上面所有情况自动修复。
3. 第一次下载安装不解压。
4. 如果用户手工改动单个文件且文件大小保持不变，不修复。（如果修复需要每次启动计算md5，太慢）


###基本流程

1. VersionCheck 下载res.version到res.version.latest，读取2个目录res.version和res.version.latest

    1. 如果下载或读取res.version.latest出错，则Failed
    2. 如果res.version.latest == stream里的version，则Succeed
    3. 进入Md5Check

2. Md5Check

    1. 如果res.version.latest == persistent里的额res.version，则不用下载res.md5.latest。只读取2个目录res.md5。
       检测persistent里的res.md5里对应的文件是否都存在，构建待下载列表（如果在stream里，比较Md5和Size，不在的话在Persistent目录下找如果找到取文件长度比较Size，如果长度不同则删除）。（以免用户删除部分文件,或2下载res没完成）。
       如果待下载列表为空则Succeed。
       否则，启动下载剩余的res，进入ResDownload。

    2. 如果version需要升级，则下载res.md5.lastest，读取2个目录res.md5和res.md5.latest。
       如果下载或读取res.md5.latest出错，则Failed。
       检测res.md5.latest里对应的文件是否都存在，构建待下载列表(如果在stream里，比较Md5和Size，不在的话在Persistent目录下找如果找到取文件长度比较Size，同时比较Persistent下res.md5里对应的Md5和Size，如果不同则删除)。
       把res.md5.lastest重命名为res.md5。
       把res.version.latest重命名为res.version。
       如果待下载列表为空则Succeed。
       否则，启动下载剩余的res，进入ResDownload。


3. ResDownload
    
    1. 如果都下载完成没错误，进入Succeed
    2. 否则Failed

4. Succeed

5. Failed

###使用

1. using(var ru = new ResUpdater(hosts, thread, reporter, startCoroutine)) { ru.Start() }

2. reporter来驱动UI

###Note

1. 虽然按照标准有query_string时http cache要带上query_string 作为key，但好像有的宽带提供商没按标准，所以最保险的策略可能是更新的时候改文件名字。
现在是加了"?version=md5"的方式。好像一般也没问题。

2. 下载完成没有计算文件的md5，来跟res.md5比对，感觉没必要，应该相信tcp，相信不会被中间缓存取成老版本。
