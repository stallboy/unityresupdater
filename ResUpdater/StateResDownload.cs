using System;
using System.Collections.Generic;

namespace ResUpdater
{
    public class StateResDownload
    {
        private readonly ResUpdater updater;
        public Dictionary<string, StateMd5.Info> DownloadList { get; private set; }
        public int OkCount { get; private set; }
        public int ErrCount { get; private set; }

        public StateResDownload(ResUpdater resUpdater)
        {
            updater = resUpdater;
        }

        public void Start(Dictionary<string, StateMd5.Info> needDownloads)
        {
            DownloadList = needDownloads;
            foreach (var kv in DownloadList)
            {
                updater.StartDownload(kv.Key + "?version=" + kv.Value.Md5, kv.Key, false);
            }
        }

        internal void OnDownloadCompleted(Exception err, string fn)
        {
            updater.Reporter.DownloadOneResComplete(err, fn, DownloadList[fn]);
            if (err != null)
                OkCount++;
            else
                ErrCount++;

            if ((OkCount + ErrCount) == DownloadList.Count)
            {
                if (ErrCount == 0)
                    updater.Reporter.ResDownloadOver(State.Success, 0);
                else
                    updater.Reporter.ResDownloadOver(State.Failed, ErrCount);
            }
        }
    }
}