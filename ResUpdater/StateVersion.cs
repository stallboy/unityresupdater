using System;
using UnityEngine;

namespace ResUpdater
{
    public class StateVersion : AbstractStateMeta
    {
        internal const string res_version = "res.version";
        internal const string res_version_latest = "res.version.latest";

        //0: err, >0 ok
        public int StreamVersion { get; private set; }
        public int PersistentVersion { get; private set; }
        public int LatestVersion { get; private set; }

        public int LocalVersion { get; private set; }
        public bool NeedUpdate { get; private set; }


        public StateVersion(ResUpdater updater) : base(updater, res_version, res_version_latest)
        {
        }

        internal void Start()
        {
            StreamVersion = -1;
            PersistentVersion = -1;
            LatestVersion = -1;
            LocalVersion = -1;
            NeedUpdate = false;
            Res.useStreamVersion = false;
            Res.resourcesInStreamWhenNotUseStreamVersion.Clear();
            DoStart(true, res_version + "?version=" + DateTime.Now.Ticks);
        }

        protected override void OnDownloadError(Exception err)
        {
            updater.Reporter.DownloadLatestVersionErr(err);
            LatestVersion = 0;
            check();
        }

        protected override void OnPersistentNotExists()
        {
            PersistentVersion = 0;
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
                    StreamVersion = version;
                    break;
                case Loc.Persistent:
                    PersistentVersion = version;
                    break;
                default:
                    LatestVersion = version;
                    break;
            }
            check();
        }

        private void check()
        {
            if (StreamVersion != -1 && PersistentVersion != -1 && LatestVersion != -1)
            {
                if (LatestVersion != 0)
                {
                    if (LatestVersion == StreamVersion)
                    {
                        Res.useStreamVersion = true;
                        updater.Reporter.VersionCheckOver(State.Success, LocalVersion, LatestVersion);
                    }
                    else
                    {
                        LocalVersion = Math.Max(StreamVersion, PersistentVersion);
                        NeedUpdate = LatestVersion != PersistentVersion;
                        updater.Reporter.VersionCheckOver(State.Md5Check, LocalVersion, LatestVersion);
                        updater.Md5State.Start(NeedUpdate);
                    }
                }
                else
                {
                    LocalVersion = Math.Max(StreamVersion, PersistentVersion);
                    updater.Reporter.VersionCheckOver(State.Failed, LocalVersion, LatestVersion);
                }
            }
        }
    }
}