#!/bin/bash

echo "Setting up Stripe webhook forwarding for local development..."
echo

# Check if Stripe CLI is installed
if ! command -v stripe &> /dev/null; then
    echo "ERROR: Stripe CLI is not installed!"
    echo "Please install it from: https://stripe.com/docs/stripe-cli"
    echo
    exit 1
fi

echo "Stripe CLI found. Starting webhook forwarding..."
echo
echo "This will forward Stripe webhooks to your local development server."
echo "Make sure your application is running on https://localhost:5000"
echo
echo "Press Ctrl+C to stop the webhook forwarding."
echo

# Start webhook forwarding
stripe listen --forward-to https://localhost:5000/api/StripeWebhook
