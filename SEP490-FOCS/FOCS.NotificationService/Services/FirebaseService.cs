using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FOCS.NotificationService.Services
{
    public class FirebaseService
    {
        private readonly FirebaseApp _firebaseApp;
        public FirebaseMessaging Messaging { get; }

        public FirebaseService(IConfiguration config, ILogger<FirebaseService> logger)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "firebase-service-account.json");

            if (!File.Exists(path))
            {
                logger.LogError("❌ firebase-service-account.json not found at: {Path}", path);
                return;
            }

            logger.LogInformation("✅ Found firebase-service-account.json at: {Path}", path);

            _firebaseApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(path)
            });

            Messaging = FirebaseMessaging.GetMessaging(_firebaseApp);
            logger.LogInformation("✅ Firebase initialized via FirebaseService.");
        }
    }
}
