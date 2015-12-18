using System;

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

        void VersionCheckOver(State nextState, int localVersion, int latestVersion);

        void DownloadLatestMd5Err(Exception err);



    }

}