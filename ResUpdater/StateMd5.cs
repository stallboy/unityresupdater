using System;
using System.Collections.Generic;
using UnityEngine;

namespace ResUpdater
{
    internal class StateMd5 : StateMeta
    {
        internal class Info
        {
            internal string md5;
            internal int size;
        }

        
        internal const string res_md5 = "res.md5";
        internal const string res_md5_latest = "res.md5.latest";

        internal Dictionary<string, Info> streamInfo;

        internal bool persistentNotExist;
        internal Dictionary<string, Info> persistentInfo;

        internal bool latestDownloadErr;
        internal Dictionary<string, Info> latestInfo;


        public StateMd5(ResUpdater updater) : base(updater, res_md5, res_md5_latest)
        {
        }

        protected override void OnPersistentNotExists()
        {
            
        }

        protected override void OnWWW(Loc loc, WWW www)
        {
            
        }

        protected override void OnDownloadError(Exception err)
        {
            updater.Reporter.DownloadLatestMd5Err(err);
            latestDownloadErr = true;
            check();
        }

        private void check()
        {
            throw new NotImplementedException();
        }
    }
}