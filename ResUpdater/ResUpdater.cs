using System;
using System.Collections;
using UnityEngine;

namespace ResUpdater
{
    public delegate Coroutine StartCoroutineFunc(IEnumerator routine);

    public class ResUpdater : IDisposable
    {
        private readonly Downloader downloader;

        internal readonly Reporter Reporter;
        internal readonly StartCoroutineFunc StartCoroutine;

        internal readonly StateVersion stateVersion;
        internal readonly StateMd5 stateMd5;


        public ResUpdater(string[] hosts, int thread, Reporter reporter, StartCoroutineFunc startCoroutine)
        {
            downloader = new Downloader(hosts, thread, Application.persistentDataPath, DownloadDone);
            Reporter = reporter;
            StartCoroutine = startCoroutine;

            stateVersion = new StateVersion(this);
            stateMd5 = new StateMd5(this);
        }

        public void Start()
        {
            stateVersion.Start(true);
        }

        public void Dispose()
        {
            downloader.Dispose();
        }

        internal void StartDownload(string url, string fn, bool isHighPriority = false)
        {
            downloader.StartDownload(url, fn, isHighPriority);
        }
        
        private void DownloadDone(Exception err, string fn)
        {
            switch (fn)
            {
                case StateVersion.res_version_latest:
                    stateVersion.OnDownloadCompleted(err);
                    break;

                case StateMd5.res_md5_latest:
                    stateMd5.OnDownloadCompleted(err);
                    break;

                default:
                    break;
            }
        }
    }
}