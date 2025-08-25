using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FOCS.NotificationService.Services
{
    public class FirebaseService
    {
        private readonly FirebaseApp _firebaseApp;
        public FirebaseMessaging Messaging { get; }

        public FirebaseService(IConfiguration config, ILogger<FirebaseService> logger)
        {
            var path = "/root/firebase-adminsdk.json";
            // var path = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS") 
            //            ?? Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account.json");

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"❌ firebase credential file not found at: {path}");
            }

            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    _firebaseApp = FirebaseApp.Create(new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(path)
                    });
                    logger.LogInformation("✅ Firebase initialized successfully via FirebaseService.");
                }
                else
                {
                    _firebaseApp = FirebaseApp.DefaultInstance;
                    logger.LogInformation("⚠ FirebaseApp already initialized, using existing instance.");
                }

                Messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Failed to initialize Firebase.");
                throw; 
            }
        }
    }
}
