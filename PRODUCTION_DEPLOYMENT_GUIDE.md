# PetCare Platform - Production Deployment Guide

## 🚀 Pre-Deployment Checklist

### 1. Environment Configuration
- [ ] Update `appsettings.Production.json` with production values
- [ ] Configure production database connection string
- [ ] Set up Google Maps API key for production
- [ ] Configure Stripe production keys
- [ ] Set up email service credentials

### 2. Database Setup
- [ ] Create production database
- [ ] Run Entity Framework migrations
- [ ] Seed initial data if needed
- [ ] Configure database backups

### 3. External Services
- [ ] Google Maps API - Enable billing and set usage limits
- [ ] Stripe - Switch to live mode and configure webhooks
- [ ] Email Service - Configure SMTP settings
- [ ] SSL Certificate - Ensure HTTPS is enabled

### 4. Security Configuration
- [ ] Enable HTTPS redirect
- [ ] Configure HSTS headers
- [ ] Set up proper CORS policies
- [ ] Configure authentication settings

## 🔧 Production Configuration Steps

### Database Migration
```bash
# Update database to latest schema
dotnet ef database update --project PetCarePlatform.Infrastructure --startup-project PetCarePlatform.Web --environment Production
```

### Environment Variables
Set these environment variables in your production environment:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_URLS=https://+:443;http://+:80`

### Stripe Webhook Configuration
1. Go to Stripe Dashboard > Webhooks
2. Add endpoint: `https://yourdomain.com/stripe/webhook`
3. Select events: `payment_intent.succeeded`, `payment_intent.payment_failed`
4. Copy webhook secret to `appsettings.Production.json`

## 📋 Testing Checklist

### Functional Testing
- [ ] User registration and login
- [ ] Pet owner profile management
- [ ] Service provider onboarding
- [ ] Service creation and management
- [ ] Booking workflow
- [ ] Payment processing
- [ ] Email notifications
- [ ] Location services

### Performance Testing
- [ ] Load testing with expected user volume
- [ ] Database performance under load
- [ ] API response times
- [ ] Memory usage monitoring

### Security Testing
- [ ] Authentication and authorization
- [ ] Input validation
- [ ] SQL injection prevention
- [ ] XSS protection
- [ ] CSRF protection

## 🚨 Critical Issues to Address

### High Priority
1. **Stripe Webhook Secret** - Must be configured for production
2. **Database Connection** - Ensure production database is accessible
3. **SSL Certificate** - Required for HTTPS and Stripe webhooks
4. **Email Configuration** - Needed for user notifications

### Medium Priority
1. **Google Maps API** - Set up billing and usage limits
2. **Error Logging** - Configure production logging
3. **Performance Monitoring** - Set up application monitoring
4. **Backup Strategy** - Implement database backups

## 🔍 Monitoring and Maintenance

### Health Checks
- Database connectivity
- External service availability
- Application performance metrics

### Logging
- Application logs
- Error tracking
- Performance metrics
- Security events

### Backup Strategy
- Daily database backups
- Configuration backups
- Code repository backups

## 📞 Support Contacts

- **Technical Issues**: [Your technical support contact]
- **Payment Issues**: Stripe Support
- **Maps Issues**: Google Cloud Support
- **Email Issues**: [Your email provider support]

---

**Note**: This is a comprehensive guide. Ensure all items are completed before going live with the PetCare Platform.
