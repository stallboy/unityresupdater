using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ResUpdater
{
    public class StateMd5 : AbstractStateMeta
    {
        public class Info
        {
            public readonly string Md5;
            public readonly int Size;

            public Info(string md5, int size)
            {
                Md5 = md5;
                Size = size;
            }

            public bool Equals(Info other)
            {
                return Md5.Equals(other.Md5) && Size == other.Size;
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
            DoStart(needUpdate, res_md5 + "?version=" + updater.VersionState.LatestVersion);
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
                if (updater.VersionState.NeedUpdate)
                {
                    if (LatestOk)
                    {
                        doCheckResource(LatestInfo, true);
                    }
                    else
                    {
                        updater.Reporter.Md5CheckOver(State.Failed, null);
                    }
                }
                else
                {
                    doCheckResource(PersistentInfo, false);
                }
            }
        }

        private void doCheckResource(Dictionary<string, Info> target, bool isTargetLatest)
        {
            var downloadList = new Dictionary<string, Info>();
            foreach (var kv in target)
            {
                var fn = kv.Key;
                var info = kv.Value;

                Info infoInStream;
                bool inStream = StreamInfo.TryGetValue(fn, out infoInStream) &&
                                infoInStream.Equals(info);

                if (inStream)
                {
                    Res.resourcesInStreamWhenNotUseStreamVersion.Add(fn);
                }
                else
                {
                    var fi = new FileInfo(Application.persistentDataPath + "/" + fn);
                    if (fi.Exists)
                    {
                        if (fi.Length != info.Size)
                        {
                            downloadList.Add(fn, info);
                            fi.Delete();
                        }
                        else if (isTargetLatest)
                        {
                            Info infoInPersistent;
                            bool inPersistent = PersistentInfo.TryGetValue(fn, out infoInPersistent) &&
                                                infoInPersistent.Equals(info);

                            if (!inPersistent)
                            {
                                downloadList.Add(fn, info);
                                fi.Delete();
                            }
                        }
                    }
                    else
                    {
                        downloadList.Add(fn, info);
                    }
                }
            }

            if (isTargetLatest)
            {
                File.Replace(Application.persistentDataPath + "/" + res_md5_latest,
                    Application.persistentDataPath + "/" + res_md5, null);
                File.Replace(Application.persistentDataPath + "/" + StateVersion.res_version_latest,
                    Application.persistentDataPath + "/" + StateVersion.res_version, null);
            }

            if (downloadList.Count == 0)
            {
                updater.Reporter.Md5CheckOver(State.Success, null);
            }
            else
            {
                updater.Reporter.Md5CheckOver(State.ResDownload, downloadList);
                updater.ResDownloadState.Start(downloadList);
            }
        }
    }
}