# PetCare Platform - Search and Map Features

## Overview
This document describes the new search and map functionality that has been implemented in the PetCare platform to help users easily find pet care services in their area.

## New Features

### 1. Enhanced Home Page Search
- **Location**: `PetCarePlatform.Web/Views/Home/Index.cshtml`
- **Features**:
  - Prominent search box on the home page
  - Service type dropdown with all available service types
  - Location input field for city, address, or zip code
  - Direct integration with the Services/Search functionality
  - Responsive design with modern styling

### 2. Advanced Search Page with Map Integration
- **Location**: `PetCarePlatform.Web/Views/Services/Search.cshtml`
- **Features**:
  - Comprehensive search form with multiple filters
  - Google Maps integration showing service locations
  - Interactive map with custom markers for each service type
  - Side-by-side layout: Map (8 columns) and Results List (4 columns)
  - Real-time filtering and sorting options
  - Location-based search with geocoding support

### 3. Map Functionality
- **Features**:
  - Google Maps API integration
  - Custom colored markers for different service types
  - Interactive info windows with service details
  - Click-to-highlight functionality between map and list
  - Automatic bounds fitting to show all services
  - Default center on Victoria, BC with fallback coordinates

### 4. Location-Based Search
- **Backend**: `PetCarePlatform.Web/Controllers/ServicesController.cs`
- **Features**:
  - Geocoding support using Google Maps API
  - Automatic conversion of addresses to coordinates
  - Default 25km search radius
  - Error handling for invalid locations
  - Support for city names, addresses, and zip codes

## Technical Implementation

### Models Updated
- **SearchServicesViewModel**: Added `Location` property for location-based searches
- **Service Model**: Already includes `Latitude` and `Longitude` properties

### Services Used
- **ILocationService**: Google Maps integration for geocoding and distance calculations
- **IServiceService**: Enhanced search functionality with location support

### Dependencies Added
- Google Maps JavaScript API
- Font Awesome icons for enhanced UI
- Bootstrap for responsive design

## Configuration Required

### Google Maps API Key
To use the map functionality, you need to configure a Google Maps API key in your `appsettings.json`:

```json
{
  "GoogleMaps": {
    "ApiKey": "your-google-maps-api-key-here"
  }
}
```

### Required Google Maps APIs
- Maps JavaScript API
- Geocoding API
- Distance Matrix API (for future distance calculations)

## Usage Examples

### Search by Location
1. Go to the home page
2. Enter a location (e.g., "Victoria, BC")
3. Select a service type (optional)
4. Click "Find Services"
5. View results on the map and in the list

### Search by Service Type
1. Navigate to Services > Search Services
2. Select a service type from the dropdown
3. Optionally add location and price filters
4. View filtered results on the map

### Interactive Map Features
- Click on map markers to see service details
- Click on service items in the list to highlight the corresponding marker
- Use the sort dropdown to organize results by price, rating, or distance

## Styling and UI Enhancements

### CSS Updates
- **Location**: `PetCarePlatform.Web/wwwroot/css/site.css`
- **Features**:
  - Modern gradient search box design
  - Hover effects for service items
  - Custom map marker styling
  - Responsive layout for mobile devices
  - Enhanced hero section styling

### Navigation Updates
- Added "Search Services" link to main navigation
- Updated hero section buttons to link to search functionality
- Improved user flow from home page to search

## Future Enhancements

### Planned Features
1. **Distance-based filtering**: Show services within specific distance ranges
2. **Real-time availability**: Show which services are currently available
3. **Route planning**: Provide directions to service providers
4. **Mobile optimization**: Enhanced mobile map experience
5. **Service clustering**: Group nearby services on the map
6. **Advanced filters**: Filter by provider rating, availability, etc.

### Technical Improvements
1. **Caching**: Cache geocoding results for better performance
2. **Spatial indexing**: Database optimization for location queries
3. **Offline support**: Basic offline map functionality
4. **Analytics**: Track search patterns and popular locations

## Testing

### Manual Testing Checklist
- [ ] Home page search form functionality
- [ ] Location-based search with geocoding
- [ ] Map marker display and interaction
- [ ] Service list and map synchronization
- [ ] Responsive design on mobile devices
- [ ] Error handling for invalid locations
- [ ] Sort and filter functionality

### Automated Testing
Consider adding unit tests for:
- Geocoding functionality
- Search service integration
- Map marker generation
- Location validation

## Troubleshooting

### Common Issues
1. **Map not loading**: Check Google Maps API key configuration
2. **No search results**: Verify location format and service data
3. **Geocoding errors**: Ensure valid addresses and API quota
4. **Mobile responsiveness**: Test on various screen sizes

### Performance Considerations
- Implement lazy loading for map markers
- Cache geocoding results
- Optimize database queries for location searches
- Consider CDN for map tiles and assets

## Support

For issues or questions about the search and map features, please refer to:
- Google Maps API documentation
- ASP.NET Core documentation
- Bootstrap documentation for styling
- Font Awesome for icons
