using System;
using System.Collections.Generic;

namespace ResUpdater
{
    public enum Loc
    {
        Stream,
        Persistent,
        Latest
    }

    public enum State
    {
        VersionCheck,
        Md5Check,
        ResDownload,
        Success,
        Failed
    }

    public interface Reporter
    {
        void DownloadLatestVersionErr(Exception err);
        
        void GetVersionErr(Loc loc, string wwwErr, Exception parseErr);

        void VersionCheckOver(State nextState, int localVersion, int latestVersion); //md5check, success, failed


        void DownloadLatestMd5Err(Exception err);

        void GetMd5Err(Loc loc, string wwwwErr, Exception parseErr);

        void Md5CheckOver(State nextState, Dictionary<string, StateMd5.Info> downloadList); //resdownload, success, failed


        void DownloadOneResComplete(Exception err, string fn, StateMd5.Info info);

        void ResDownloadOver(State nextState, int errCount);
    }

}