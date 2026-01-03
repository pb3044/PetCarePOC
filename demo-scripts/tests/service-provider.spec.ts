import { test, expect } from '@playwright/test';

test('Service Provider: login, complete profile if needed, dashboard tour', async ({ page, baseURL }) => {
  const base = baseURL!;

  // Go to login
  await page.goto(base + '/Account/Login');
  await page.getByLabel('Email').fill('janedoe@example.com');
  await page.getByLabel('Password').fill('Password123!');
  await page.getByRole('button', { name: 'Login' }).click();

  // Redirect either to ServiceProvider dashboard or create profile
  await page.waitForLoadState('domcontentloaded');

  // If redirected to create profile, fill minimal fields and submit
  if (page.url().includes('/ServiceProvider/Create')) {
    await page.fill('#FirstName', 'Jane');
    await page.fill('#LastName', 'Doe');
    await page.fill('#Phone', '(604) 555-6789');
    await page.fill('#Address', '123 Demo St');
    await page.fill('#City', 'Vancouver');
    await page.fill('#Province', 'BC');
    await page.fill('#PostalCode', 'V6B 1V2');
    await page.fill('#Bio', 'Experienced vet for demo.');
    await page.fill('#BusinessName', 'Demo Vet Clinic');
    await page.selectOption('#BusinessType', { label: 'Small Business' });
    await page.fill('#ServiceRadius', '10');
    await page.fill('#Description', 'Veterinary services.');
    await page.fill('#ServiceArea', 'Vancouver');
    await page.fill('#Credentials', 'DVM');
    await page.fill('#Certifications', 'CVMA');
    await page.fill('#InsuranceInfo', 'Liability');
    await page.fill('#LicenseInfo', 'License 123');
    await page.fill('#BankingInfo', 'Bank info');
    await page.fill('#TaxInfo', 'GST 123');
    await page.check('#termsCheck');
    await page.check('#verificationCheck');
    await page.getByRole('button', { name: /Complete Profile Setup/i }).click();
  }

  // Ensure we are at dashboard
  await page.waitForURL('**/ServiceProvider/Dashboard');
  await expect(page).toHaveURL(/ServiceProvider\/Dashboard/);

  // Tour: My Services
  await page.goto(base + '/ServiceProvider/MyServices');

  // Tour: Schedule
  await page.goto(base + '/ServiceProvider/Schedule');

  // Tour: Earnings, Reviews, Reports, Settings
  await page.goto(base + '/ServiceProvider/Earnings');
  await page.goto(base + '/ServiceProvider/Reviews');
  await page.goto(base + '/ServiceProvider/Reports');
  await page.goto(base + '/ServiceProvider/Settings');

  // Back to dashboard
  await page.goto(base + '/ServiceProvider/Dashboard');
});


