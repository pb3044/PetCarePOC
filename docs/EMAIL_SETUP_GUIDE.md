# Email Configuration Setup Guide

This guide explains how to configure SMTP email settings for the PetCare Platform.

## Overview

The application uses SMTP to send transactional emails (booking confirmations, payment receipts, etc.). Sensitive credentials (username and password) should be stored in User Secrets (development) or Environment Variables (production).

## Development Setup (User Secrets)

### Step 1: Initialize User Secrets

Run this command in the `PetCarePlatform.Web` project directory:

```bash
dotnet user-secrets init
```

### Step 2: Set Email Credentials

Set your SMTP credentials using User Secrets:

```bash
# Gmail example
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-app-password"

# For other providers, you may also need to set:
dotnet user-secrets set "Email:SmtpHost" "smtp.yourprovider.com"
dotnet user-secrets set "Email:SmtpPort" "587"
```

### Step 3: Verify Configuration

You can view your secrets (for verification):

```bash
dotnet user-secrets list
```

**Note**: Never commit User Secrets to source control. They are stored in your user profile.

## Gmail Setup

### Using Gmail SMTP

1. **Enable 2-Factor Authentication** on your Google account
2. **Generate an App Password**:
   - Go to Google Account settings
   - Security → 2-Step Verification → App passwords
   - Generate a password for "Mail"
3. **Use the App Password** (not your regular password) in User Secrets

### Gmail Configuration

```bash
dotnet user-secrets set "Email:SmtpHost" "smtp.gmail.com"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-16-char-app-password"
dotnet user-secrets set "Email:FromEmail" "your-email@gmail.com"
dotnet user-secrets set "Email:FromName" "PetCare Platform"
dotnet user-secrets set "Email:EnableSsl" "true"
```

## Other Email Providers

### Outlook/Office 365

```bash
dotnet user-secrets set "Email:SmtpHost" "smtp.office365.com"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:Username" "your-email@outlook.com"
dotnet user-secrets set "Email:Password" "your-password"
dotnet user-secrets set "Email:EnableSsl" "true"
```

### SendGrid

```bash
dotnet user-secrets set "Email:SmtpHost" "smtp.sendgrid.net"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:Username" "apikey"
dotnet user-secrets set "Email:Password" "your-sendgrid-api-key"
dotnet user-secrets set "Email:EnableSsl" "true"
```

### Mailtrap (Testing)

For development/testing, you can use Mailtrap:

```bash
dotnet user-secrets set "Email:SmtpHost" "smtp.mailtrap.io"
dotnet user-secrets set "Email:SmtpPort" "2525"
dotnet user-secrets set "Email:Username" "your-mailtrap-username"
dotnet user-secrets set "Email:Password" "your-mailtrap-password"
dotnet user-secrets set "Email:EnableSsl" "false"
```

## Production Setup (Environment Variables)

In production, use environment variables instead of User Secrets:

### Azure App Service

Set in Configuration → Application Settings:

```
Email__Username=your-email@domain.com
Email__Password=your-password
Email__SmtpHost=smtp.provider.com
Email__SmtpPort=587
Email__FromEmail=noreply@petcareplatform.com
Email__FromName=PetCare Platform
Email__EnableSsl=true
Email__IsEnabled=true
```

### Docker

Set in `docker-compose.yml` or as environment variables:

```yaml
environment:
  - Email__Username=your-email@domain.com
  - Email__Password=your-password
  - Email__SmtpHost=smtp.provider.com
  - Email__SmtpPort=587
```

### Linux/Windows Server

Set as system environment variables:

```bash
export Email__Username="your-email@domain.com"
export Email__Password="your-password"
export Email__SmtpHost="smtp.provider.com"
export Email__SmtpPort="587"
```

## Disabling Email (Development)

If you want to disable email sending during development (emails will be logged only):

In `appsettings.Development.json`:

```json
{
  "Email": {
    "IsEnabled": false
  }
}
```

Or via User Secrets:

```bash
dotnet user-secrets set "Email:IsEnabled" "false"
```

## Configuration Structure

The email configuration supports the following settings:

| Setting | Required | Default | Description |
|---------|----------|---------|-------------|
| `SmtpHost` | Yes* | - | SMTP server hostname |
| `SmtpPort` | Yes* | 587 | SMTP server port |
| `Username` | Yes* | - | SMTP username |
| `Password` | Yes* | - | SMTP password |
| `FromEmail` | Yes* | - | Sender email address |
| `FromName` | No | - | Sender display name |
| `EnableSsl` | No | true | Enable SSL/TLS |
| `IsEnabled` | No | true | Enable/disable email sending |

*Required only when `IsEnabled` is `true`

## Testing Email Configuration

After configuring, test the email service:

1. Run the application
2. Trigger an email (e.g., create a booking)
3. Check the logs for email sending status
4. Verify the email was received

## Troubleshooting

### "SMTP credentials are not configured"

- Ensure User Secrets are set correctly
- Verify the secrets are in the correct format: `Email:Username`, `Email:Password`
- Restart the application after setting secrets

### "SMTP error: Authentication failed"

- For Gmail: Use an App Password, not your regular password
- Verify credentials are correct
- Check if 2FA is enabled (required for Gmail App Passwords)

### "Connection timeout"

- Check firewall settings
- Verify SMTP host and port are correct
- Some networks block SMTP ports (587, 465)

### Email not sending but no errors

- Check `Email:IsEnabled` is set to `true`
- Verify logs for warning messages
- Ensure SMTP credentials are properly configured

## Security Best Practices

1. ✅ **Never commit credentials** to source control
2. ✅ **Use App Passwords** for Gmail (not regular passwords)
3. ✅ **Use environment variables** in production
4. ✅ **Rotate credentials** regularly
5. ✅ **Use dedicated email accounts** for transactional emails
6. ✅ **Enable SSL/TLS** for all SMTP connections

## Support

For issues with email configuration, check:
- Application logs for detailed error messages
- SMTP provider documentation
- Network/firewall settings

