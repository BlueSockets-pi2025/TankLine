using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;

namespace Heartbeat
{
    public class HeartbeatManager : MonoBehaviour
    {
        public static HeartbeatManager Instance { get; private set; }

        [SerializeField] private string heartbeatUrl = "https://185.155.93.105:17008/api/auth/heartbeat";
        [SerializeField] private float heartbeatInterval = 30f;

        private float timeSinceLastHeartbeat = 0f;
        private bool isLoggedIn = false;

        private IEnumerator HeartbeatTimer()
        {
                Debug.Log("HEARTBEAT TRIGGERED...");
                while (true)
                {
                    // Attend 5 secondes
                    yield return new WaitForSeconds(5f);

                    
                    Debug.Log("HeartbeatTimer Is Logged In: " + isLoggedIn);
                    
                    if (isLoggedIn)
                    {
                        Debug.Log("Starting SendHeartbeat coroutine...");
                        yield return StartCoroutine(SendHeartbeat());
                    }
                }
            }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartCoroutine(HeartbeatTimer());  
        }

        private IEnumerator SendHeartbeat()
        {
            Debug.Log("Is Logged In: " + isLoggedIn);

            if (!isLoggedIn)
            {
                Debug.LogWarning("User is not logged in. Heartbeat request not sent.");
                yield break;
            }

            Debug.LogWarning("SEND HEARTBEAT");

            X509Certificate2 certificate = AuthController.GetTrustedCertificate(); // Retrieves AuthController certificate

            // Call the SendHeartbeatRequest method
            yield return SendHeartbeatRequest(
                heartbeatUrl,
                "POST",
                new Dictionary<string, string> { { "Content-Type", "application/json" } },
                null,
                certificate,
                onSuccess: (response) =>
                {
                    Debug.LogWarning("Heartbeat SUCCESS");
                },
                onError: (response) =>
                {
                    Debug.LogWarning("Heartbeat failed: " + response.error);
                }
            );
        }

        private IEnumerator SendHeartbeatRequest(
            string url, string method, Dictionary<string, string> headers, byte[] body, 
            X509Certificate2 certificate, System.Action<UnityWebRequest> onSuccess, 
            System.Action<UnityWebRequest> onError)
        {
            using (UnityWebRequest www = new UnityWebRequest(url, method))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                www.uploadHandler = new UploadHandlerRaw(body);
                foreach (var header in headers)
                {
                    www.SetRequestHeader(header.Key, header.Value);
                }

                // Apply the certificate to the request
                www.certificateHandler = new CertificateHandlerCustom(certificate);

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    onSuccess?.Invoke(www);
                }
                else
                {
                    onError?.Invoke(www);
                }
            }
        }



        public void SetLoggedIn(bool loggedIn) 
        {
            isLoggedIn = loggedIn ; 
        }

        // Custom CertificateHandler to apply the certificate
        public class CertificateHandlerCustom : CertificateHandler
        {
            private X509Certificate2 certificate;

            public CertificateHandlerCustom(X509Certificate2 cert)
            {
                certificate = cert;
            }

            protected override bool ValidateCertificate(byte[] certificateData)
            {
                X509Certificate2 cert = new X509Certificate2(certificateData);
                return cert.Equals(certificate);
            }
        }

    }


}
