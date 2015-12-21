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

1. VersionCheck 下载res.version到res.version.latest，读取2个目录version， 比较。

2. Md5Check

    1. 如果version 需要升级，则下载res.md5.lastest，读取2个目录res.md5，比较， 删除掉md5和size不对的res，
       把res.md5.lastest重命名为res.md5；把res.version.latest重命名为res.version； 启动下载剩余的res

    2. 如果version 不需要升级，则检测已有res.md5，对应的文件是否都存在。（以免用户删除部分文件,或2下载res没完成），
       如果都存在则成功；如果有不存在的，启动下载剩余的res

3. ResDownload，（下载完成后需要再计算一遍md5吗？如果需要，全部下载完成后，计算完一遍，创建一个checked文件做标记，暂时不计算）

4. Succeed

5. Failed

###使用

1. using(var ru = new ResUpdater(hosts, thread, reporter, startCoroutine)) { ru.Start() }

2. reporter来驱动UI

###TODO

1. 虽然按照标准有query_string时http cache要带上query_string 作为key，但好像有的宽带提供商没按标准，所以最保险的是更新的时候改文件名字？
现在是加了"?version=<md5>"的方式


