using System;
using System.Collections.Generic;
using UnityEngine;

namespace ResUpdater
{
    public class StateMd5 : AbstractStateMeta
    {
        public class Info
        {
            public string Md5 { get; private set; }
            public int Size { get; private set; }

            public Info(string md5, int size)
            {
                Md5 = md5;
                Size = size;
            }
        }

        private static readonly Dictionary<string, Info> empty = new Dictionary<string, Info>();

        internal const string res_md5 = "res.md5";
        internal const string res_md5_latest = "res.md5.latest";

        public Dictionary<string, Info> StreamInfo { get; private set; }
        public Dictionary<string, Info> PersistentInfo { get; private set; }

        public bool LatestOk { get; private set; }
        public Dictionary<string, Info> LatestInfo { get; private set; }


        public StateMd5(ResUpdater updater) : base(updater, res_md5, res_md5_latest)
        {
        }

        internal void Start(bool needUpdate)
        {
            DoStart(needUpdate);
            if (!needUpdate)
            {
                LatestInfo = empty;
            }
        }

        protected override void OnDownloadError(Exception err)
        {
            updater.Reporter.DownloadLatestMd5Err(err);
            LatestOk = false;
            LatestInfo = empty;
            check();
        }

        protected override void OnPersistentNotExists()
        {
            PersistentInfo = empty;
        }

        protected override void OnWWW(Loc loc, WWW www)
        {
            Dictionary<string, Info> info;
            bool ok = false;
            if (www.error != null)
            {
                updater.Reporter.GetMd5Err(loc, www.error, null);
                info = empty;
            }
            else
            {
                try
                {
                    info = new Dictionary<string, Info>();
                    foreach (var line in www.text.Split('\n'))
                    {
                        var sp = line.Split(' ');
                        var res = sp[0];
                        var md5 = sp[1];
                        var size = int.Parse(sp[2]);

                        info.Add(res, new Info(md5, size));
                    }
                    ok = true;
                }
                catch (Exception e)
                {
                    updater.Reporter.GetMd5Err(loc, null, e);
                    info = empty;
                }
            }

            switch (loc)
            {
                case Loc.Stream:
                    StreamInfo = info;
                    break;
                case Loc.Persistent:
                    PersistentInfo = info;
                    break;
                default:
                    LatestOk = ok;
                    LatestInfo = info;
                    break;
            }

            check();
        }


        private void check()
        {
            if (StreamInfo != null && PersistentInfo != null && LatestInfo != null)
            {
                State nextState;
                if (updater.VersionState.NeedUpdate)
                {
                    if (LatestOk)
                    {
                        //TODO
                        nextState = State.ResDownload;
                    }
                    else
                    {
                        nextState = State.Failed;
                    }
                }
                else
                {
                    //TODO
                    nextState = State.Success;
                }
                updater.Reporter.Md5CheckOver(nextState);
            }
        }
    }
}