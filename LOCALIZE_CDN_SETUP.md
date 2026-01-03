# CDN Localization Setup Guide

This guide will help you complete the localization of all CDN resources for better security, performance, and reliability.

## ✅ What's Been Done

1. **Updated all layout files** to use local paths instead of CDN URLs:
   - `_Layout.cshtml` - Main layout
   - `_SearchLayout.cshtml` - Search page layout
   - `_PetOwnerLayout.cshtml` - Pet owner layout
   - `_ServiceProviderLayout.cshtml` - Service provider layout
   - `Search.cshtml` - Search page
   - `Earnings.cshtml` - Earnings page
   - `Analytics.cshtml` - Analytics page
   - `Schedule.cshtml` - Schedule page

2. **Updated CSP middleware** to remove CDN dependencies (strict security policy)

3. **Created download script** (`Download-LocalResources.ps1`) to automate resource downloads

## 📥 Next Steps: Download Resources

### Option 1: Run the PowerShell Script (Recommended)

```powershell
# From the project root directory
.\Download-LocalResources.ps1
```

This script will automatically download:
- Font Awesome 6.0.0
- Bootstrap Icons 1.10.0
- Leaflet 1.9.4
- Chart.js
- FullCalendar 5.11.3
- Inter font from Google Fonts

### Option 2: Manual Download

If the script fails, download manually:

#### 1. Font Awesome 6.0.0
- Download from: https://use.fontawesome.com/releases/v6.0.0/fontawesome-free-6.0.0-web.zip
- Extract to: `PetCarePlatform.Web\wwwroot\lib\fontawesome\`
- Structure should be: `lib\fontawesome\css\all.min.css`

#### 2. Bootstrap Icons 1.10.0
- Download from: https://github.com/twbs/icons/releases/download/v1.10.0/bootstrap-icons-1.10.0.zip
- Extract to: `PetCarePlatform.Web\wwwroot\lib\bootstrap-icons\`
- Structure should be: `lib\bootstrap-icons\font\bootstrap-icons.css`

#### 3. Leaflet 1.9.4
- Download from: https://github.com/Leaflet/Leaflet/releases/download/v1.9.4/leaflet.zip
- Extract to: `PetCarePlatform.Web\wwwroot\lib\leaflet\`
- Structure should be: `lib\leaflet\leaflet.css` and `lib\leaflet\leaflet.js`

#### 4. Chart.js
- Download from: https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js
- Save to: `PetCarePlatform.Web\wwwroot\lib\chartjs\chart.umd.min.js`

#### 5. FullCalendar 5.11.3
- Download CSS: https://cdn.jsdelivr.net/npm/fullcalendar@5.11.3/main.min.css
- Download JS: https://cdn.jsdelivr.net/npm/fullcalendar@5.11.3/main.min.js
- Save to: `PetCarePlatform.Web\wwwroot\lib\fullcalendar\`

#### 6. Inter Font
- Visit: https://fonts.google.com/specimen/Inter
- Download all font weights (300, 400, 500, 600, 700)
- Save to: `PetCarePlatform.Web\wwwroot\fonts\inter\`
- Create `inter.css` file with `@font-face` declarations pointing to local files

## 📁 Expected Directory Structure

```
PetCarePlatform.Web\wwwroot\
├── lib\
│   ├── bootstrap\          (already exists)
│   ├── jquery\            (already exists)
│   ├── fontawesome\
│   │   └── css\
│   │       └── all.min.css
│   ├── bootstrap-icons\
│   │   └── font\
│   │       └── bootstrap-icons.css
│   ├── leaflet\
│   │   ├── leaflet.css
│   │   └── leaflet.js
│   ├── chartjs\
│   │   └── chart.umd.min.js
│   └── fullcalendar\
│       ├── main.min.css
│       └── main.min.js
└── fonts\
    └── inter\
        ├── inter.css
        └── (font files: .woff2, .woff, etc.)
```

## ✅ Verification

After downloading, verify the files exist:

```powershell
# Check if all required files exist
Test-Path "PetCarePlatform.Web\wwwroot\lib\fontawesome\css\all.min.css"
Test-Path "PetCarePlatform.Web\wwwroot\lib\bootstrap-icons\font\bootstrap-icons.css"
Test-Path "PetCarePlatform.Web\wwwroot\lib\leaflet\leaflet.css"
Test-Path "PetCarePlatform.Web\wwwroot\lib\leaflet\leaflet.js"
Test-Path "PetCarePlatform.Web\wwwroot\lib\chartjs\chart.umd.min.js"
Test-Path "PetCarePlatform.Web\wwwroot\lib\fullcalendar\main.min.css"
Test-Path "PetCarePlatform.Web\wwwroot\lib\fullcalendar\main.min.js"
Test-Path "PetCarePlatform.Web\wwwroot\fonts\inter\inter.css"
```

## 🧪 Testing

1. **Build the project** to ensure no compilation errors
2. **Run the application** and check:
   - Font Awesome icons display correctly
   - Bootstrap Icons display correctly
   - Maps (Leaflet) load and work
   - Charts (Chart.js) render properly
   - Calendar (FullCalendar) displays correctly
   - Inter font loads and displays properly
3. **Check browser console** for any 404 errors
4. **Verify CSP** - no CDN-related CSP violations in console

## 🔒 Security Benefits

- ✅ No external dependencies
- ✅ No CDN outages affecting your app
- ✅ Faster page loads (no external requests)
- ✅ Works offline
- ✅ Stricter Content Security Policy
- ✅ Better privacy (no third-party requests)

## 🐛 Troubleshooting

### Icons not showing
- Check Font Awesome CSS path: `~/lib/fontawesome/css/all.min.css`
- Verify font files are in the correct directory

### Maps not loading
- Check Leaflet files exist: `~/lib/leaflet/leaflet.css` and `~/lib/leaflet/leaflet.js`
- Check browser console for 404 errors

### Fonts not loading
- Verify Inter font files are in `~/fonts/inter/`
- Check `inter.css` has correct `@font-face` declarations
- Ensure font file paths in CSS are relative to the CSS file location

### Charts not rendering
- Verify Chart.js file exists: `~/lib/chartjs/chart.umd.min.js`
- Check browser console for errors

## 📝 Notes

- The CSP middleware has been updated to remove CDN references
- All layout files have been updated to use local paths
- If you need to temporarily use CDNs, uncomment the CDN lines in `SecurityHeadersMiddleware.cs`

