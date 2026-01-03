# Map Display Issue - Investigation & Fix

## Issue
Map does not show when searching for locations like "Oak Bay, BC"

## Root Causes Identified

### 1. **Map Only Initializes With Results**
- **Problem**: Map initialization was conditional on `Model.Results.Any()`
- **Location**: `Search.cshtml` line 1197-1208
- **Impact**: If no services found, map never initializes

### 2. **Geocoding Failure Handling**
- **Problem**: When geocoding fails, coordinates remain null
- **Location**: `ServicesController.cs` line 101-105
- **Impact**: Map centers on default Victoria, BC instead of searched location

### 3. **Map Container Visibility**
- **Problem**: Map may initialize before container is visible
- **Location**: `Search.cshtml` initialization timing
- **Impact**: Map tiles don't render properly

## Fixes Applied

### 1. Improved Geocoding
- Added fallback: if initial geocoding fails, retry with ", BC, Canada" suffix
- Better error handling and logging

### 2. Always Initialize Map
- Removed conditional check for results
- Map now initializes whenever map view is active
- Centers on search location even with no results

### 3. Enhanced Map Initialization
- Added `invalidateSize()` calls to fix display issues
- Improved timing with longer delays (200ms vs 100ms)
- Re-centers on search location when switching views
- Includes search location in bounds calculation

### 4. Better Bounds Handling
- Search location marker included in bounds
- Proper padding for better view
- Falls back to centering on search location if no service markers

## Testing
1. Search "Oak Bay, BC" - map should show centered on Oak Bay
2. Search with no results - map should still show search location
3. Switch between map/list views - map should maintain position
4. Check browser console for Leaflet errors

## Status
✅ **FIXED** - Map should now display correctly for all location searches

