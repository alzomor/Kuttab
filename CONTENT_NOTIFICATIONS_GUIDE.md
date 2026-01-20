# Content Notifications Management Guide

## 🎯 **Overview**

Your Kuttab app now supports **informational push notifications** beyond just app updates! You can send notifications about features, educational content, announcements, and more.

## 📱 **Notification Types Available**

### 1. **Feature Notifications** 🔧
- **Purpose**: Announce new app features and improvements
- **Style**: Feature notification channel (default priority)
- **Use Cases**: New recording features, enhanced search, UI improvements

### 2. **Educational Notifications** 📚
- **Purpose**: Share Tajweed tips, learning content, Quranic insights
- **Style**: Feature notification channel (default priority)
- **Use Cases**: Daily Tajweed tips, learning series, pronunciation guides

### 3. **Announcement Notifications** 📢
- **Purpose**: Important announcements, maintenance notices, events
- **Style**: General notification channel (default priority)
- **Use Cases**: Ramadan preparation, system maintenance, special events

## 🛠️ **How to Manage Notifications**

### **File Location**: `/home/hossam/Work/Quraan/notifications.json`

This JSON file controls all content notifications. The app checks this file every 6 hours.

### **Notification Structure**

```json
{
  "id": "unique_identifier",
  "title": "Notification Title",
  "message": "Detailed message content",
  "type": "feature|educational|announcement",
  "actionUrl": "optional_deep_link",
  "isActive": true,
  "targetAudience": ["all", "beginners", "advanced"],
  "schedule": {
    "startTime": "2025-01-20T00:00:00Z",
    "endTime": "2025-01-25T23:59:59Z",
    "daysOfWeek": ["monday", "wednesday", "friday"]
  },
  "createdAt": "2025-01-20T10:00:00Z"
}
```

### **Field Explanations**

#### **Required Fields**
- **`id`**: Unique identifier (no spaces, use underscores)
- **`title`**: Short title (shown in notification)
- **`message`**: Detailed message content
- **`type`**: `feature`, `educational`, or `announcement`
- **`isActive`**: `true` to show, `false` to hide

#### **Optional Fields**
- **`actionUrl`**: Deep link for when user taps notification
- **`targetAudience`**: Who should see this (currently only "all" works)
- **`schedule`**: When to show notification
- **`createdAt`**: Creation timestamp (auto-generated)

## 📝 **Creating New Notifications**

### **Step 1: Add to notifications.json**

```json
{
  "id": "new_feature_001",
  "title": "🎵 New Audio Features",
  "message": "Enhanced audio playback with better quality and new recitation options!",
  "type": "feature",
  "isActive": true,
  "targetAudience": ["all"]
}
```

### **Step 2: Deploy to GitHub**

1. Update `notifications.json` in your GitHub repository
2. Commit and push changes
3. Apps will automatically check within 6 hours

### **Step 3: Test the Notification**

```bash
# Force check immediately (in app code)
_contentService.CheckForContentNotificationsAsync(forceCheck: true);
```

## ⏰ **Scheduling Notifications**

### **Date Range**
```json
"schedule": {
  "startTime": "2025-01-20T00:00:00Z",
  "endTime": "2025-01-25T23:59:59Z"
}
```

### **Specific Days**
```json
"schedule": {
  "daysOfWeek": ["monday", "wednesday", "friday"]
}
```

### **Combined Schedule**
```json
"schedule": {
  "startTime": "2025-01-20T00:00:00Z",
  "endTime": "2025-01-25T23:59:59Z",
  "daysOfWeek": ["monday", "wednesday", "friday"]
}
```

## 🎨 **Notification Examples**

### **Feature Announcement**
```json
{
  "id": "recording_feature_v2",
  "title": "🎤 Enhanced Recording",
  "message": "New recording features: better quality, longer duration, and waveform visualization!",
  "type": "feature",
  "isActive": true,
  "targetAudience": ["all"],
  "schedule": {
    "startTime": "2025-01-25T00:00:00Z",
    "endTime": "2025-02-10T23:59:59Z"
  }
}
```

### **Educational Content**
```json
{
  "id": "tajweed_tip_ghunnah",
  "title": "📚 Learn: Ghunnah",
  "message": "Master the nasal sound (Ghunnah) in Tajweed. Practice with noon and meem letters!",
  "type": "educational",
  "isActive": true,
  "targetAudience": ["all"],
  "schedule": {
    "daysOfWeek": ["tuesday", "thursday"]
  }
}
```

### **Special Announcement**
```json
{
  "id": "ramadan_2025",
  "title": "🌙 Ramadan Preparation",
  "message": "Get ready for Ramadan with our special Tajweed recitation guides and tips!",
  "type": "announcement",
  "isActive": false,
  "targetAudience": ["all"],
  "schedule": {
    "startTime": "2025-02-20T00:00:00Z",
    "endTime": "2025-03-20T23:59:59Z"
  }
}
```

## 🔄 **Notification Lifecycle**

### **1. Creation**
- Add notification to `notifications.json`
- Set `isActive: true`
- Deploy to GitHub

### **2. Delivery**
- App checks every 6 hours
- Shows notification if conditions met
- Marks as shown in user preferences

### **3. Management**
- Set `isActive: false` to disable
- Update schedule to change timing
- Delete to remove permanently

### **4. User Experience**
- Each notification shown only once per user
- Users can dismiss notifications
- No automatic repeats

## 📊 **Best Practices**

### **Content Guidelines**
- **Keep titles short** (under 50 characters)
- **Messages should be informative** but concise
- **Use emojis** to make notifications engaging
- **Include relevant timing** for time-sensitive content

### **Scheduling Tips**
- **Educational content**: Weekday mornings
- **Feature announcements**: When features are ready
- **Announcements**: Based on events/seasons
- **Avoid spamming**: Limit to 1-2 notifications per week

### **Technical Notes**
- **Check frequency**: Every 6 hours
- **Unique IDs**: Never reuse notification IDs
- **Time format**: ISO 8601 UTC
- **File location**: Must be in GitHub repository root

## 🛠️ **Advanced Features**

### **Target Audiences (Future)**
```json
"targetAudience": ["beginners", "advanced", "teachers"]
```

### **Deep Links**
```json
"actionUrl": "kuttab://feature/recording"
```

### **Priority Levels**
- **High**: Critical updates (app updates)
- **Default**: Features and educational content
- **Low**: General announcements

## 🔍 **Troubleshooting**

### **Notification Not Showing**
1. Check `isActive: true`
2. Verify schedule dates are current
3. Ensure JSON syntax is valid
4. Check app has notification permission

### **JSON Validation**
```bash
# Validate JSON syntax
python -m json.tool notifications.json
```

### **Debug Commands**
```bash
# Monitor notification checks
adb logcat | grep "NotificationContentService"

# Check notification preferences
adb shell dumpsys package com.kuttab.app | grep notification
```

## 📈 **Analytics & Monitoring**

### **Current Tracking**
- Notification shown/not shown status
- User dismiss actions
- App open from notifications

### **Future Enhancements**
- Click-through rates
- User engagement metrics
- A/B testing capabilities

---

## 🎉 **Summary**

You now have a **complete content notification system** that allows you to:

✅ **Send informational notifications** about features and content  
✅ **Schedule notifications** by date and time  
✅ **Target specific user groups** (future enhancement)  
✅ **Track delivery** and user interactions  
✅ **Manage remotely** via GitHub repository  

The system is **live and ready** - just update the `notifications.json` file and your users will receive your content notifications automatically!
