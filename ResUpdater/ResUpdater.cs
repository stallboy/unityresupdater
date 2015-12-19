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

        public StateVersion VersionState { get; }
        public StateMd5 Md5State { get; }
        public StateResDownload ResDownloadState { get; }

        public ResUpdater(string[] hosts, int thread, Reporter reporter, StartCoroutineFunc startCoroutine)
        {
            downloader = new Downloader(hosts, thread, Application.persistentDataPath, DownloadDone);
            Reporter = reporter;
            StartCoroutine = startCoroutine;

            VersionState = new StateVersion(this);
            Md5State = new StateMd5(this);
            ResDownloadState = new StateResDownload(this);
        }

        public void Start()
        {
            VersionState.Start();
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
                    VersionState.OnDownloadCompleted(err);
                    break;
                case StateMd5.res_md5_latest:
                    Md5State.OnDownloadCompleted(err);
                    break;
                default:
                    ResDownloadState.OnDownloadCompleted(err, fn);
                    break;
            }
        }
    }
}