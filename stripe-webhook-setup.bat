@echo off
echo Setting up Stripe webhook forwarding for local development...
echo.

REM Check if Stripe CLI is installed
stripe --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Stripe CLI is not installed!
    echo Please install it from: https://stripe.com/docs/stripe-cli
    echo.
    pause
    exit /b 1
)

echo Stripe CLI found. Starting webhook forwarding...
echo.
echo This will forward Stripe webhooks to your local development server.
echo Make sure your application is running on https://localhost:5000
echo.
echo Press Ctrl+C to stop the webhook forwarding.
echo.

REM Start webhook forwarding
stripe listen --forward-to https://localhost:5000/api/StripeWebhook

pause
