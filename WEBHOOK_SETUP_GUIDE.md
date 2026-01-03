# Stripe Webhook Setup Guide

This guide will help you set up Stripe webhooks for the PetCare Platform.

## Prerequisites

- Stripe account with API keys
- Stripe CLI installed (for local development)
- Application running locally or deployed

## Step 1: Install Stripe CLI (for Local Development)

### Windows
```bash
# Download from: https://github.com/stripe/stripe-cli/releases
# Or use Chocolatey:
choco install stripe-cli
```

### macOS
```bash
brew install stripe/stripe-cli/stripe
```

### Linux
```bash
# Download from: https://github.com/stripe/stripe-cli/releases
# Or use snap:
snap install stripe
```

## Step 2: Configure Webhook Endpoint in Stripe Dashboard

1. **Go to Stripe Dashboard** → **Developers** → **Webhooks**
2. **Click "Add endpoint"**
3. **Set the endpoint URL:**
   - **Local Development**: `https://localhost:5000/api/StripeWebhook`
   - **Production**: `https://yourdomain.com/api/StripeWebhook`
4. **Select events to listen for:**
   - `payment_intent.succeeded`
   - `payment_intent.payment_failed`
   - `payment_intent.requires_action`
   - `charge.dispute.created`
5. **Click "Add endpoint"**
6. **Copy the webhook secret** (starts with `whsec_`)

## Step 3: Configure Application Settings

### Update appsettings.json

Replace the webhook secret in your configuration files:

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_YOUR_ACTUAL_WEBHOOK_SECRET_HERE"
  }
}
```

### Environment Variables (Production)

Set these environment variables in your production environment:

```bash
STRIPE_PUBLISHABLE_KEY=pk_live_...
STRIPE_SECRET_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...
```

## Step 4: Local Development Setup

### Option A: Using Stripe CLI (Recommended)

1. **Login to Stripe CLI:**
   ```bash
   stripe login
   ```

2. **Start webhook forwarding:**
   ```bash
   stripe listen --forward-to https://localhost:5000/api/StripeWebhook
   ```

3. **Copy the webhook secret** from the CLI output and update your `appsettings.Development.json`

4. **Run the setup script:**
   - Windows: `stripe-webhook-setup.bat`
   - Linux/Mac: `chmod +x stripe-webhook-setup.sh && ./stripe-webhook-setup.sh`

### Option B: Using ngrok (Alternative)

1. **Install ngrok:**
   ```bash
   # Download from: https://ngrok.com/download
   ```

2. **Expose your local server:**
   ```bash
   ngrok http 5000
   ```

3. **Use the ngrok URL** in your Stripe webhook endpoint configuration

## Step 5: Test Webhook Functionality

### Check Webhook Status

Visit: `https://localhost:5000/api/WebhookTest/status`

This will show you:
- Whether webhook secret is configured
- Your webhook endpoint URL
- Configuration status

### Test Webhook Processing

1. **Create a test payment** through your application
2. **Check the webhook logs** in your application
3. **Verify payment status** is updated correctly

### Manual Webhook Testing

You can test webhook processing manually:

```bash
# Test successful payment
curl -X POST https://localhost:5000/api/WebhookTest/test-payment-success \
  -H "Content-Type: application/json" \
  -d '{"transactionId": "pi_test_123456789"}'

# Test failed payment
curl -X POST https://localhost:5000/api/WebhookTest/test-payment-failure \
  -H "Content-Type: application/json" \
  -d '{"transactionId": "pi_test_123456789"}'
```

## Step 6: Production Deployment

### 1. Update Stripe Dashboard

- Change webhook endpoint URL to your production domain
- Update webhook secret in production configuration

### 2. Configure Environment Variables

Set the following environment variables in your production environment:

```bash
STRIPE_PUBLISHABLE_KEY=pk_live_...
STRIPE_SECRET_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...
```

### 3. Test Production Webhooks

- Make a test payment in production
- Check webhook delivery status in Stripe Dashboard
- Verify payment status updates in your application

## Monitoring and Troubleshooting

### Webhook Delivery Status

Check webhook delivery status in Stripe Dashboard:
1. Go to **Developers** → **Webhooks**
2. Click on your webhook endpoint
3. View the **Recent deliveries** tab

### Common Issues

#### 1. Webhook Not Receiving Events

**Symptoms:**
- No webhook events in application logs
- Payment status not updating

**Solutions:**
- Verify webhook URL is correct
- Check firewall settings
- Ensure application is running and accessible
- Verify webhook secret is correct

#### 2. Signature Verification Failed

**Symptoms:**
- "Invalid webhook signature" errors in logs

**Solutions:**
- Verify webhook secret is correct
- Check that webhook secret matches Stripe Dashboard
- Ensure raw request body is being read correctly

#### 3. Webhook Processing Errors

**Symptoms:**
- Webhook events received but processing fails
- Database errors in logs

**Solutions:**
- Check database connectivity
- Verify payment records exist
- Check application logs for specific errors

### Debug Mode

Enable debug logging for webhook processing:

```json
{
  "Logging": {
    "LogLevel": {
      "PetCarePlatform.Web.Controllers.StripeWebhookController": "Debug"
    }
  }
}
```

## Security Best Practices

### 1. Webhook Secret Security

- Never commit webhook secrets to version control
- Use environment variables for production
- Rotate webhook secrets regularly

### 2. HTTPS Only

- Always use HTTPS for webhook endpoints
- Stripe requires HTTPS for webhook delivery

### 3. Signature Verification

- Always verify webhook signatures
- Never process webhooks without signature verification

### 4. Idempotency

- Handle duplicate webhook events gracefully
- Use Stripe event IDs for idempotency

## Testing Checklist

- [ ] Webhook endpoint configured in Stripe Dashboard
- [ ] Webhook secret added to application configuration
- [ ] Local webhook forwarding working (if applicable)
- [ ] Payment success webhook processing correctly
- [ ] Payment failure webhook processing correctly
- [ ] Refund webhook processing correctly
- [ ] Error handling working properly
- [ ] Logging configured and working
- [ ] Production webhook endpoint accessible
- [ ] Production webhook secret configured

## Support

### Stripe Resources

- [Stripe Webhooks Documentation](https://stripe.com/docs/webhooks)
- [Stripe CLI Documentation](https://stripe.com/docs/stripe-cli)
- [Stripe Support](https://support.stripe.com/)

### Application Support

- Check application logs for detailed error information
- Use the webhook test endpoints for debugging
- Verify database connectivity and data integrity
