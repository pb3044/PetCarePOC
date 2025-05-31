# Pet Care Booking Platform - Phase One Documentation

## Overview
This document provides an overview of the Pet Care Booking Platform phase one implementation. The platform is designed as a scalable, enterprise-level application for pet care service booking in Canada, with a focus on robust architecture, clean code, and production readiness.

## Architecture
The application follows an N-tier architecture with Domain-Driven Design principles:

1. **Presentation Layer**
   - Web UI (HTML5 + Bootstrap)
   - API endpoints for future integration

2. **Business Logic Layer**
   - Services for business operations
   - Domain logic encapsulation

3. **Data Access Layer**
   - Entity Framework Core with Code-First approach
   - Repository pattern implementation

4. **Database Layer**
   - SQL Server database

## Key Features Implemented in Phase One

### Core Domain Models
- User management with extended ASP.NET Identity
- Pet profiles with detailed information
- Service offerings with pricing
- Booking system with status tracking
- Canadian tax calculation system (GST, PST, HST)

### Authentication & Authorization
- ASP.NET Identity integration
- Role-based authorization
- Secure password policies

### Database Design
- Code-first approach with Entity Framework Core
- Properly defined relationships between entities
- Canadian tax rates seeded for all provinces

### Business Logic
- Booking service with CRUD operations
- Tax calculation service based on provincial rules
- Service provider management

### API Design
- RESTful API principles
- Swagger documentation
- Proper HTTP status codes

## Getting Started

### Prerequisites
- .NET 8.0 SDK or later
- SQL Server (LocalDB is configured by default)
- Visual Studio 2022 or other compatible IDE

### Setup Instructions
1. Clone the repository
2. Open the solution in Visual Studio
3. Update the connection string in appsettings.json if needed
4. Run the following commands in the Package Manager Console:
   ```
   Update-Database
   ```
5. Run the application

## Project Structure

```
PetCareBooking/
├── src/
│   ├── PetCareBooking.API/              # Web API project
│   ├── PetCareBooking.Web/              # MVC Web Application
│   ├── PetCareBooking.Core/             # Core domain models and interfaces
│   ├── PetCareBooking.Infrastructure/   # Data access, external services
│   ├── PetCareBooking.Services/         # Business logic and services
│   └── PetCareBooking.Common/           # Shared utilities and helpers
├── tests/
│   ├── PetCareBooking.UnitTests/        # Unit tests
│   ├── PetCareBooking.IntegrationTests/ # Integration tests
│   └── PetCareBooking.FunctionalTests/  # Functional/E2E tests
└── docs/                                # Documentation
```

## Canadian Tax System Implementation
The application implements the Canadian tax system with province-specific tax rates:
- GST (5%) applies federally, except in HST provinces
- PST applies in BC (7%), SK (6%), MB (7%), and QC (9.975% QST)
- HST combines GST and PST in ON (13%), NB (15%), NS (15%), NL (15%), and PEI (15%)
- AB, NT, NU, and YT have no provincial tax

Tax rates are stored in the database and can be updated as needed.

## Next Steps for Phase Two
- Implement UI components with Bootstrap
- Add payment gateway integration (Stripe/PayPal)
- Integrate Google Maps API for location services
- Implement notification system
- Add reporting and analytics features

## Conclusion
Phase one establishes a solid foundation for the Pet Care Booking Platform with a focus on architecture, core domain models, and essential business logic. The implementation follows best practices for enterprise applications and is ready for further development in subsequent phases.
