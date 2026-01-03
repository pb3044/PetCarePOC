import { test, expect } from '@playwright/test';

test('Manual verify: Services/Search shows results for Victoria', async ({ page, baseURL }) => {
  const base = baseURL!;
  await page.goto(base + '/Services/Search');
  await page.locator('#location').fill('Victoria');
  await page.getByRole('button', { name: /Find Services/i }).click();
  const cards = page.locator('.service-card');
  await expect(cards.first()).toBeVisible({ timeout: 15000 });
  const count = await cards.count();
  console.log('Service cards found:', count);
  await page.screenshot({ path: 'test-results/validate-search.png', fullPage: true });
});


