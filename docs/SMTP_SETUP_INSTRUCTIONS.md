# SMTP Email Configuration - Quick Setup

## ✅ What Was Fixed

1. **EmailService now sends actual emails** (was just logging before)
2. **Sensitive credentials moved to User Secrets** (not in appsettings.json)
3. **Configuration validation updated** to allow User Secrets
4. **Added IsEnabled flag** to disable email in development if needed

## 🚀 Quick Start

### Step 1: Set Your Email Credentials

Run these commands in the `PetCarePlatform.Web` directory:

```powershell
# For Gmail (recommended for testing)
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-gmail-app-password"
dotnet user-secrets set "Email:FromEmail" "your-email@gmail.com"

# Optional: Override SMTP settings if needed
dotnet user-secrets set "Email:SmtpHost" "smtp.gmail.com"
dotnet user-secrets set "Email:SmtpPort" "587"
```

### Step 2: Get Gmail App Password

1. Go to your Google Account: https://myaccount.google.com/
2. Security → 2-Step Verification (enable if not already)
3. App passwords → Generate password for "Mail"
4. Use the 16-character password (not your regular password)

### Step 3: Test

1. Run the application
2. Trigger an email (create a booking, process payment, etc.)
3. Check your email inbox
4. Check application logs for email status

## 📝 Alternative: Use Mailtrap for Testing

If you don't want to use Gmail, use Mailtrap (free testing service):

```powershell
dotnet user-secrets set "Email:SmtpHost" "smtp.mailtrap.io"
dotnet user-secrets set "Email:SmtpPort" "2525"
dotnet user-secrets set "Email:Username" "your-mailtrap-username"
dotnet user-secrets set "Email:Password" "your-mailtrap-password"
dotnet user-secrets set "Email:EnableSsl" "false"
```

Sign up at: https://mailtrap.io/

## 🔧 Disable Email (Development Only)

If you want to disable email sending (logs only):

```powershell
dotnet user-secrets set "Email:IsEnabled" "false"
```

## 📚 Full Documentation

See `docs/EMAIL_SETUP_GUIDE.md` for:
- Detailed setup instructions
- Other email providers (Outlook, SendGrid, etc.)
- Production deployment (environment variables)
- Troubleshooting guide

## ✅ Verification

After setting up, verify your secrets:

```powershell
dotnet user-secrets list
```

You should see your Email settings listed.

