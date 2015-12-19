using System;

namespace ResUpdater
{
    public class StateResDownload
    {
        private ResUpdater updater;

        public StateResDownload(ResUpdater resUpdater)
        {
            updater = resUpdater;
        }

        internal void OnDownloadCompleted(Exception err, string fn)
        {
        }
    }
}