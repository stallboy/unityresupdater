using System;
using UnityEngine;

namespace ResUpdater
{
    public class StateVersion : AbstractStateMeta
    {
        internal const string res_version = "res.version";
        internal const string res_version_latest = "res.version.latest";

        //0: err, >0 ok
        public int StreamVersion { get; private set; } = -1;
        public int PersistentVersion { get; private set; } = -1;
        public int LatestVersion { get; private set; } = -1;

        public int LocalVersion { get; private set; } = -1;
        public Loc LocalVersionLoc { get; private set; }

        public bool NeedUpdate { get; private set; }


        public StateVersion(ResUpdater updater) : base(updater, res_version, res_version_latest)
        {
        }

        internal void Start()
        {
            DoStart(true);
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
                if (PersistentVersion != 0)
                {
                    LocalVersionLoc = Loc.Persistent;
                    LocalVersion = PersistentVersion;
                }

                if (StreamVersion != 0 && StreamVersion > LocalVersion)
                {
                    LocalVersionLoc = Loc.Stream;
                    LocalVersion = StreamVersion;
                }

                var nextState = State.Failed;
                if (LatestVersion != 0)
                {
                    NeedUpdate = LocalVersion < LatestVersion;
                    if (!NeedUpdate && LocalVersion != -1 && LocalVersionLoc == Loc.Stream)
                    {
                        nextState = State.Success;
                    }
                    else
                    {
                        nextState = State.Md5Check;
                        updater.Md5State.Start(NeedUpdate);
                    }
                }
                updater.Reporter.VersionCheckOver(nextState, LocalVersion, LatestVersion);
            }
        }
    }
}