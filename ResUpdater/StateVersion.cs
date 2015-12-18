using System;
using UnityEngine;

namespace ResUpdater
{
    internal class StateVersion : StateMeta
    {
        internal const string res_version = "res.version";
        internal const string res_version_latest = "res.version.latest";

        //0: err, >0 ok
        internal int streamVersion = -1;
        internal int persistentVersion = -1;
        internal int latestVersion = -1;
        
        internal int localVersion = -1;
        internal Loc localVersionLoc;


        public StateVersion(ResUpdater updater) : base(updater, res_version, res_version_latest)
        {
        }

        protected override void OnDownloadError(Exception err)
        {
            updater.Reporter.DownloadLatestVersionErr(err);
            latestVersion = 0;
            check();
        }

        protected override void OnPersistentNotExists()
        {
            persistentVersion = 0;
        }

        protected override void OnWWW(Loc loc, WWW www)
        {
            int version = 0;
            if (www.error != null)
            {
                updater.Reporter.GetVersionErr(loc, www.error, null);
            }
            else
            {
                try
                {
                    version = int.Parse(www.text);
                }
                catch (Exception e)
                {
                    updater.Reporter.GetVersionErr(loc, null, e);
                }
            }

            switch (loc)
            {
                case Loc.Stream:
                    streamVersion = version;
                    break;
                case Loc.Persistent:
                    persistentVersion = version;
                    break;
                default:
                    latestVersion = version;
                    break;
            }
            check();
        }
        

        private void check()
        {
            if (streamVersion != -1 && persistentVersion != -1 && latestVersion != -1)
            {
                findNewerVersion(Loc.Persistent, persistentVersion);
                findNewerVersion(Loc.Stream, streamVersion);

                var nextState = State.Failed;
                if (latestVersion != 0)
                {
                    bool downloadMd5 = localVersion < latestVersion;
                    updater.stateMd5.Start(downloadMd5);
                    nextState = State.Md5Check;
                }
                updater.Reporter.VersionCheckOver(nextState, localVersion, latestVersion);
            }
        }

        private void findNewerVersion(Loc loc, int version)
        {
            if (version != 0 && version > localVersion)
            {
                localVersionLoc = loc;
                localVersion = version;
            }
        }
    }
}