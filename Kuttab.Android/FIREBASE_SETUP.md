# Firebase Cloud Messaging Setup Guide

This guide explains how to set up Firebase Cloud Messaging (FCM) for the Kuttab Android app to enable push notifications and app updates.

## 📋 Prerequisites

- Google account
- Android Studio (recommended)
- Firebase project

## 🚀 Setup Steps

### 1. Create Firebase Project

1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Click "Add project"
3. Enter project name: `kuttab-app`
4. Enable Google Analytics (optional but recommended)
5. Click "Create project"

### 2. Add Android App to Firebase

1. In Firebase Console, click "Add app"
2. Select Android icon
3. Package name: `com.kuttab.app`
4. App nickname: `Kuttab`
5. Debug signing certificate SHA-1 (optional for now)
6. Click "Register app"

### 3. Download Firebase Config

1. Download `google-services.json` from Firebase Console
2. Place it in `/Kuttab.Android/` folder
3. Make sure it's named exactly `google-services.json`

### 4. Configure Firebase Services

#### Enable Cloud Messaging
1. In Firebase Console, go to Project Settings
2. Click "Cloud Messaging" tab
3. Ensure Cloud Messaging API is enabled
4. Note your Server Key (for sending notifications)

#### Enable App Check (Optional but recommended)
1. Go to App Check in Firebase Console
2. Enable App Check for Android
3. Choose SafetyNet or Play Integrity

### 5. Test the Setup

#### Build and Run
```bash
cd /home/hossam/Work/Quraan/Kuttab.Android
dotnet build -c Release
dotnet run
```

#### Check FCM Token
1. Open the app
2. Check logcat for FCM token message:
   ```
   adb logcat | grep "FCM Token"
   ```

#### Send Test Notification
Use Firebase Console or API to send test notification:

**Using Firebase Console:**
1. Go to Cloud Messaging
2. Click "Send your first message"
3. Enter title and body
4. Target: App instance
5. Send

**Using API:**
```bash
curl -X POST https://fcm.googleapis.com/fcm/send \
  -H "Authorization: key=YOUR_SERVER_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "to": "FCM_TOKEN_HERE",
    "notification": {
      "title": "Test Notification",
      "body": "This is a test message"
    },
    "data": {
      "type": "test",
      "url": "https://example.com"
    }
  }'
```

## 🔧 Configuration Files

### google-services.json Structure
```json
{
  "project_info": {
    "project_number": "123456789012",
    "project_id": "kuttab-app",
    "storage_bucket": "kuttab-app.appspot.com"
  },
  "client": [
    {
      "client_info": {
        "mobilesdk_app_id": "1:123456789012:android:abcdef1234567890",
        "android_client_info": {
          "package_name": "com.kuttab.app"
        }
      },
      "oauth_client": [
        {
          "client_id": "123456789012-abcdef1234567890.apps.googleusercontent.com",
          "client_type": 3
        }
      ],
      "api_key": [
        {
          "current_key": "AIzaSyAbCdEfGhIjKlMnOpQrStUvWxYz1234567"
        }
      ]
    }
  ]
}
```

## 📱 Notification Types

### 1. App Updates
- Automatically checks GitHub releases
- Shows update notification with download link
- Can be dismissed to ignore specific version

### 2. Feature Announcements
- New features and improvements
- Educational content
- Tips and usage guidance

### 3. General Notifications
- System messages
- Maintenance notices
- Important announcements

## 🛠️ Implementation Details

### Services Created
- `KuttabFirebaseMessagingService` - Handles FCM messages
- `KuttabFirebaseInstanceIdService` - Manages FCM tokens
- `AppUpdateService` - Checks for app updates
- `NotificationHelper` - Manages notifications and permissions

### Notification Channels
- `kuttab_general` - General notifications
- `kuttab_updates` - App updates (high priority)
- `kuttab_features` - New features

### Permissions Added
- `POST_NOTIFICATIONS` - Android 13+ notifications
- `VIBRATE` - Notification vibration
- `RECEIVE_BOOT_COMPLETED` - Boot receiver for background tasks

## 🔍 Troubleshooting

### Common Issues

#### No Notifications Received
1. Check if app has notification permission
2. Verify `google-services.json` is correct
3. Check logcat for FCM errors
4. Ensure device has internet connection

#### FCM Token Not Generated
1. Verify Firebase project setup
2. Check package name matches Firebase
3. Ensure app has internet permission
4. Reinstall the app

#### Update Checks Not Working
1. Check internet connectivity
2. Verify GitHub API accessibility
3. Check WorkManager initialization
4. Review logcat for errors

### Debug Commands
```bash
# Check FCM registration
adb logcat | grep "Firebase"

# Check notification permission
adb shell dumpsys package com.kuttab.app | grep notification

# Check WorkManager tasks
adb logcat | grep "WorkManager"

# Force update check (in app code)
_appUpdateService.CheckForUpdatesAsync(forceCheck: true);
```

## 📚 Next Steps

1. **Set up Firebase project** - Follow steps above
2. **Test notifications** - Verify FCM is working
3. **Implement notification preferences** - Add user settings
4. **Set up server integration** - For sending targeted notifications
5. **Monitor analytics** - Track notification performance

## 📞 Support

If you encounter issues:
1. Check Firebase documentation
2. Review Android logcat
3. Verify all configuration files
4. Test with different Android versions

---

**Note**: This implementation uses GitHub API for update checking. For production, consider using your own update server or Google Play In-App Updates API.
