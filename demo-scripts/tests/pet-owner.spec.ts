import { test, expect } from '@playwright/test';

test('Pet Owner: login and dashboard tour', async ({ page, baseURL }) => {
  const base = baseURL!;

  // Go to login
  await page.goto(base + '/Account/Login');
  await page.getByLabel('Email').fill('samantha.lee@example.com');
  await page.getByLabel('Password').fill('Password123!');
  await page.getByRole('button', { name: 'Login' }).click();

  // Should redirect to PetOwner dashboard
  await page.waitForURL('**/PetOwner/Dashboard');
  await expect(page).toHaveURL(/PetOwner\/Dashboard/);

  // Quick tour: My Pets
  if (await page.getByRole('link', { name: /My Pets/i }).isVisible().catch(() => false)) {
    await page.getByRole('link', { name: /My Pets/i }).click();
    await expect(page).toHaveURL(/PetOwner\/MyPets/);
  } else {
    // fallback via URL
    await page.goto(base + '/PetOwner/MyPets');
  }

  // View first pet if available
  const firstDetailsLink = page.getByRole('link', { name: /Details|View/i }).first();
  if (await firstDetailsLink.isVisible().catch(() => false)) {
    await firstDetailsLink.click();
  }

  // My Bookings
  await page.goto(base + '/PetOwner/MyBookings');

  // Browse services and open booking page
  await page.goto(base + '/Services');
  const firstService = page.getByRole('link', { name: /Book|Details/i }).first();
  if (await firstService.isVisible().catch(() => false)) {
    await firstService.click();
  } else {
    await page.goto(base + '/PetOwner/BookService');
  }

  // Back to dashboard
  await page.goto(base + '/PetOwner/Dashboard');
});


