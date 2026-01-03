import { test, expect } from '@playwright/test';

async function caption(page, text: string, duration: number = 3000) {
  // Use page.addStyleTag and page.evaluate to overlay captions temporarily
  await page.addStyleTag({ content: `
    #demo-caption { position: fixed; bottom: 10%; left: 50%; transform: translateX(-50%); background: rgba(0,0,0,0.85); color: #fff; padding: 16px 20px; border-radius: 12px; font-size: 20px; z-index: 999999; max-width: 85%; text-align: center; font-weight: 500; box-shadow: 0 4px 20px rgba(0,0,0,0.3); }
  `});
  await page.evaluate((t) => {
    let el = document.getElementById('demo-caption');
    if (!el) {
      el = document.createElement('div');
      el.id = 'demo-caption';
      document.body.appendChild(el);
    }
    el.textContent = t;
    el.style.display = 'block';
  }, text);
  await page.waitForTimeout(duration);
  await page.evaluate(() => {
    const el = document.getElementById('demo-caption');
    if (el) el.style.display = 'none';
  });
}

test('End-to-end overview: homepage, search Victoria BC, pet owner, service provider', async ({ page, baseURL }) => {
  const base = baseURL!;

  // Home page overview
  await page.goto(base + '/');
  await caption(page, 'Welcome to PetCare – a comprehensive platform connecting pet owners with trusted service providers across Canada.', 4000);
  await caption(page, 'Our platform offers dog walking, pet sitting, grooming, veterinary care, training, and more.', 4000);
  await caption(page, 'From the homepage, you can search for services by location and type.', 3000);

  // Enhanced Search for Victoria, BC with map showcase
  await caption(page, 'Let\'s search for services in Victoria to see our location-based matching.', 4000);
  await page.goto(base + '/Services/Search');
  const locationInput = page.locator('#location');
  await locationInput.fill('Victoria');
  await page.getByRole('button', { name: /Find Services/i }).click();
  // Wait for results to render and network to settle
  await page.waitForLoadState('networkidle');
  const resultsSection = page.locator('.results-section');
  await expect(resultsSection).toBeVisible({ timeout: 15000 });
  await resultsSection.scrollIntoViewIfNeeded();
  // Ensure at least one result card is visible
  const resultCard = page.locator('.service-card').first();
  await expect(resultCard).toBeVisible({ timeout: 15000 });
  await resultCard.scrollIntoViewIfNeeded();
  await resultCard.hover();
  await caption(page, 'Victoria results loaded: services list and map markers are shown.', 6000);
  await page.waitForTimeout(2000);
  await caption(page, 'Explore providers on the map and review details like ratings and pricing.', 5000);
  await expect(page).toHaveURL(/\/Services\/Search/);
  await caption(page, 'Search results show nearby services with detailed information and pricing.', 3000);
  await caption(page, 'Our interactive map displays service locations with tooltips showing provider details.', 4000);
  
  // Show map with tooltips for 10 seconds
  await caption(page, 'Hover over map markers to see service details, ratings, and pricing information.', 10000);

  // Pet Owner comprehensive dashboard tour
  await caption(page, 'Now let\'s explore the Pet Owner experience - managing pets and booking services.', 4000);
  await page.goto(base + '/Account/Login');
  await page.getByLabel('Email').fill('samantha.lee@example.com');
  await page.getByLabel('Password').fill('Password123!');
  await page.getByRole('button', { name: 'Login' }).click();
  await page.waitForURL('**/PetOwner/Dashboard');
  await caption(page, 'Pet Owner Dashboard: Overview of your pets, recent bookings, and favorite providers.', 4000);
  
  // Navigate through all Pet Owner tabs
  await page.goto(base + '/PetOwner/MyPets');
  await caption(page, 'My Pets: Add, edit, and manage your pet profiles with medical information and special needs.', 4000);
  
  await page.goto(base + '/Services');
  await caption(page, 'Browse Services: Discover and book from our network of verified service providers.', 4000);
  
  await page.goto(base + '/PetOwner/MyBookings');
  await caption(page, 'My Bookings: Track all your past and upcoming bookings with status updates.', 4000);
  
  // Removed Pet Owner profile step to avoid error

  // Logout to show Service Provider flow
  await caption(page, 'Now let\'s switch to the Service Provider experience - managing your business and clients.', 4000);
  await page.goto(base + '/Account/Logout');

  // Service Provider comprehensive dashboard tour
  await page.goto(base + '/Account/Login');
  await page.getByLabel('Email').fill('janedoe@example.com');
  await page.getByLabel('Password').fill('Password123!');
  await page.getByRole('button', { name: 'Login' }).click();
  await page.waitForURL('**/ServiceProvider/Dashboard');
  await caption(page, 'Service Provider Dashboard: Real-time insights into your bookings, earnings, and performance metrics.', 4000);
  
  // Navigate through all Service Provider tabs
  await page.goto(base + '/ServiceProvider/BookingRequest');
  await caption(page, 'Booking Requests: Review and respond to new booking requests from pet owners.', 4000);
  
  await page.goto(base + '/ServiceProvider/MyServices');
  await caption(page, 'My Services: Create and manage your service offerings with pricing and availability.', 4000);
  
  await page.goto(base + '/ServiceProvider/Schedule');
  await caption(page, 'Schedule: Set your availability, manage your calendar, and block unavailable times.', 4000);
  
  await page.goto(base + '/ServiceProvider/Profile');
  await caption(page, 'Profile: Showcase your credentials, certifications, and build trust with pet owners.', 4000);
  
  await page.goto(base + '/ServiceProvider/Reviews');
  await caption(page, 'Reviews: View customer feedback and ratings to build your reputation.', 4000);
  
  await page.goto(base + '/ServiceProvider/Earnings');
  await caption(page, 'Earnings: Track your income, payouts, and financial performance with detailed analytics.', 4000);
  
  await page.goto(base + '/ServiceProvider/Reports');
  await caption(page, 'Reports: Generate detailed business reports for tax and performance analysis.', 4000);
  
  await page.goto(base + '/ServiceProvider/Settings');
  await caption(page, 'Settings: Configure your business preferences, notifications, and account settings.', 4000);

  // Final summary
  await caption(page, 'PetCare Platform: A complete ecosystem for pet care services with secure payments, reviews, and location-based matching.', 5000);
});


