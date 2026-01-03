# Stripe Payment Integration

This document provides a comprehensive guide to the Stripe payment integration implemented in the PetCare Platform.

## Overview

The Stripe integration provides secure payment processing for pet care service bookings. It includes:

- **Payment Intent Creation**: Server-side creation of secure payment intents
- **Frontend Integration**: Stripe Elements for secure card input
- **Webhook Handling**: Real-time payment status updates
- **Refund Processing**: Automated refund handling
- **Error Handling**: Comprehensive error management and logging

## Architecture

### Components

1. **StripePaymentService** (`PetCarePlatform.Infrastructure/Payment/StripePaymentService.cs`)
   - Core payment processing logic
   - Stripe API integration
   - Payment status management

2. **StripeWebhookController** (`PetCarePlatform.Web/Controllers/StripeWebhookController.cs`)
   - Handles Stripe webhook events
   - Updates payment and booking statuses
   - Signature verification for security

3. **PaymentsApiController** (`PetCarePlatform.Web/Controllers/Api/PaymentsApiController.cs`)
   - REST API endpoints for payment operations
   - Client-side integration support

4. **ProcessPayment View** (`PetCarePlatform.Web/Views/Payments/ProcessPayment.cshtml`)
   - Stripe Elements integration
   - Secure payment form
   - Real-time validation

## Configuration

### AppSettings Configuration

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

### Environment Variables (Production)

```bash
STRIPE_PUBLISHABLE_KEY=pk_live_...
STRIPE_SECRET_KEY=sk_live_...
STRIPE_WEBHOOK_SECRET=whsec_...
```

## API Endpoints

### Create Payment Intent
```
POST /api/PaymentsApi/create-payment-intent
Content-Type: application/json

{
  "bookingId": 123
}
```

**Response:**
```json
{
  "clientSecret": "pi_..._secret_...",
  "paymentId": 456,
  "amount": 50.00,
  "currency": "cad"
}
```

### Confirm Payment
```
POST /api/PaymentsApi/confirm-payment
Content-Type: application/json

{
  "paymentId": 456,
  "transactionId": "pi_..."
}
```

### Get Payment Status
```
GET /api/PaymentsApi/payment-status/{paymentId}
```

### Request Refund
```
POST /api/PaymentsApi/request-refund
Content-Type: application/json

{
  "paymentId": 456,
  "amount": 25.00,
  "reason": "Service cancelled"
}
```

### Get Publishable Key
```
GET /api/PaymentsApi/publishable-key
```

## Webhook Events

The webhook controller handles the following Stripe events:

- `payment_intent.succeeded` - Payment completed successfully
- `payment_intent.payment_failed` - Payment failed
- `payment_intent.requires_action` - Additional authentication required
- `charge.dispute.created` - Chargeback/dispute initiated

### Webhook Endpoint
```
POST /api/StripeWebhook
```

## Frontend Integration

### Stripe Elements Setup

```javascript
// Initialize Stripe
const stripe = Stripe('pk_test_...');
const elements = stripe.elements();

// Create card element
const cardElement = elements.create('card', {
  style: {
    base: {
      fontSize: '16px',
      color: '#424770',
    }
  }
});

cardElement.mount('#card-element');
```

### Payment Processing

```javascript
// Confirm payment
const {error, paymentIntent} = await stripe.confirmCardPayment(clientSecret, {
  payment_method: {
    card: cardElement,
    billing_details: {
      name: 'Customer Name',
    },
  }
});

if (error) {
  // Handle error
} else if (paymentIntent.status === 'succeeded') {
  // Payment successful
}
```

## Security Features

### 1. Webhook Signature Verification
- All webhook requests are verified using Stripe's signature
- Prevents unauthorized webhook calls

### 2. Server-Side Validation
- Payment amounts validated on server
- Booking ownership verified
- Duplicate payment prevention

### 3. PCI Compliance
- No card data stored locally
- Stripe handles all sensitive data
- Secure tokenization

### 4. Error Handling
- Comprehensive logging
- Graceful error recovery
- User-friendly error messages

## Testing

### Test Cards

Use Stripe's test cards for development:

- **Success**: `4242424242424242`
- **Decline**: `4000000000000002`
- **Requires Authentication**: `4000002500003155`
- **Insufficient Funds**: `4000000000009995`

### Test Webhooks

Use Stripe CLI for local webhook testing:

```bash
stripe listen --forward-to localhost:5000/api/StripeWebhook
```

## Error Handling

### Common Error Scenarios

1. **Invalid Booking**
   - Booking not found
   - Payment already exists
   - Invalid amount

2. **Stripe Errors**
   - Card declined
   - Insufficient funds
   - Network issues

3. **Webhook Errors**
   - Invalid signature
   - Duplicate events
   - Processing failures

### Error Response Format

```json
{
  "error": "Error message",
  "code": "ERROR_CODE",
  "details": "Additional details"
}
```

## Monitoring and Logging

### Log Levels

- **Information**: Payment creation, successful payments
- **Warning**: Payment failures, validation errors
- **Error**: Stripe API errors, system failures

### Key Metrics

- Payment success rate
- Average processing time
- Error frequency
- Webhook processing time

## Deployment Checklist

### Pre-Deployment

- [ ] Update Stripe keys to live environment
- [ ] Configure webhook endpoints in Stripe Dashboard
- [ ] Test with live test cards
- [ ] Verify webhook signature validation
- [ ] Check error handling and logging

### Post-Deployment

- [ ] Monitor payment success rates
- [ ] Check webhook delivery status
- [ ] Verify error logs
- [ ] Test refund functionality

## Troubleshooting

### Common Issues

1. **Webhook Not Receiving Events**
   - Check webhook URL configuration
   - Verify signature validation
   - Check firewall settings

2. **Payment Intent Creation Fails**
   - Verify Stripe API keys
   - Check booking data validity
   - Review error logs

3. **Frontend Payment Form Issues**
   - Verify publishable key
   - Check JavaScript console errors
   - Validate form submission

### Debug Mode

Enable debug logging in development:

```json
{
  "Logging": {
    "LogLevel": {
      "PetCarePlatform.Infrastructure.Payment": "Debug"
    }
  }
}
```

## Support

For Stripe-specific issues:
- [Stripe Documentation](https://stripe.com/docs)
- [Stripe Support](https://support.stripe.com/)

For platform-specific issues:
- Check application logs
- Review error handling
- Contact development team

## Version History

- **v1.0** - Initial Stripe integration
- **v1.1** - Added webhook handling
- **v1.2** - Enhanced error handling and logging
- **v1.3** - Added comprehensive testing
