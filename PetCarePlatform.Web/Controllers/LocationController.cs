using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using PetCarePlatform.Core.Interfaces;

namespace PetCarePlatform.Web.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCoordinates(string address)
        {
            try
            {
                var result = await _locationService.GeocodeAddressAsync(address);
                return Json(new { 
                    success = true, 
                    latitude = result.Latitude, 
                    longitude = result.Longitude,
                    formattedAddress = result.FormattedAddress,
                    placeId = result.PlaceId
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDistance(double lat1, double lng1, double lat2, double lng2)
        {
            try
            {
                var distance = await _locationService.CalculateDistanceAsync(lat1, lng1, lat2, lng2);
                return Json(new { success = true, distance });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> FindNearbyProviders(double latitude, double longitude, double radiusKm, string serviceType = null)
        {
            try
            {
                var providers = await _locationService.FindNearbyServiceProvidersAsync(latitude, longitude, radiusKm, serviceType);
                return Json(new { success = true, providers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetStaticMapUrl(double latitude, double longitude, int zoom = 14, int width = 600, int height = 400)
        {
            try
            {
                var mapUrl = _locationService.GenerateStaticMapUrl(latitude, longitude, zoom, width, height);
                return Json(new { success = true, mapUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetDirectionsUrl(string originAddress, string destinationAddress)
        {
            try
            {
                var directionsUrl = _locationService.GenerateDirectionsUrl(originAddress, destinationAddress);
                return Json(new { success = true, directionsUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var testResults = new
            {
                timestamp = DateTime.UtcNow,
                serviceType = _locationService.GetType().Name,
                tests = new List<object>()
            };

            var results = new List<object>();

            // Test 1: Geocoding - Valid Address
            var test1 = await RunTest("Geocoding - Valid Address (Toronto)", async () =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var result = await _locationService.GeocodeAddressAsync("Toronto, ON, Canada");
                    sw.Stop();
                    return new
                    {
                        success = true,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        data = new
                        {
                            latitude = result.Latitude,
                            longitude = result.Longitude,
                            formattedAddress = result.FormattedAddress,
                            placeId = result.PlaceId
                        }
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new
                    {
                        success = false,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        error = ex.Message,
                        errorType = ex.GetType().Name,
                        innerException = ex.InnerException?.Message
                    };
                }
            });
            results.Add(test1);

            // Test 2: Geocoding - Another Valid Address
            await Task.Delay(1100); // Respect rate limit (1 req/sec)
            var test2 = await RunTest("Geocoding - Valid Address (Vancouver)", async () =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var result = await _locationService.GeocodeAddressAsync("Vancouver, BC, Canada");
                    sw.Stop();
                    return new
                    {
                        success = true,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        data = new
                        {
                            latitude = result.Latitude,
                            longitude = result.Longitude,
                            formattedAddress = result.FormattedAddress,
                            placeId = result.PlaceId
                        }
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new
                    {
                        success = false,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        error = ex.Message,
                        errorType = ex.GetType().Name,
                        innerException = ex.InnerException?.Message
                    };
                }
            });
            results.Add(test2);

            // Test 3: Geocoding - Invalid Address
            await Task.Delay(1100);
            var test3 = await RunTest("Geocoding - Invalid Address", async () =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var result = await _locationService.GeocodeAddressAsync("XYZ123InvalidAddress999");
                    sw.Stop();
                    return new
                    {
                        success = true,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        data = new
                        {
                            latitude = result.Latitude,
                            longitude = result.Longitude,
                            formattedAddress = result.FormattedAddress
                        },
                        warning = "Unexpectedly succeeded with invalid address"
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new
                    {
                        success = false,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        error = ex.Message,
                        errorType = ex.GetType().Name,
                        expected = true // This failure is expected
                    };
                }
            });
            results.Add(test3);

            // Test 4: Distance Calculation
            var test4 = await RunTest("Distance Calculation", async () =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    // Toronto to Vancouver (approximate)
                    var distance = await _locationService.CalculateDistanceAsync(43.6532, -79.3832, 49.2827, -123.1207);
                    sw.Stop();
                    return new
                    {
                        success = true,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        data = new
                        {
                            from = "Toronto, ON (43.6532, -79.3832)",
                            to = "Vancouver, BC (49.2827, -123.1207)",
                            distanceKm = Math.Round(distance, 2),
                            expectedApproxKm = 3364 // Approximate distance
                        }
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new
                    {
                        success = false,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        error = ex.Message,
                        errorType = ex.GetType().Name
                    };
                }
            });
            results.Add(test4);

            // Test 5: Static Map URL Generation
            var test5 = await RunTest("Static Map URL Generation", async () =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var mapUrl = _locationService.GenerateStaticMapUrl(43.6532, -79.3832, 14, 600, 400);
                    sw.Stop();
                    return new
                    {
                        success = true,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        data = new
                        {
                            mapUrl = mapUrl,
                            latitude = 43.6532,
                            longitude = -79.3832
                        }
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new
                    {
                        success = false,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        error = ex.Message,
                        errorType = ex.GetType().Name
                    };
                }
            });
            results.Add(test5);

            // Test 6: Directions URL Generation
            var test6 = await RunTest("Directions URL Generation", async () =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var directionsUrl = _locationService.GenerateDirectionsUrl("Toronto, ON", "Vancouver, BC");
                    sw.Stop();
                    return new
                    {
                        success = true,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        data = new
                        {
                            directionsUrl = directionsUrl,
                            origin = "Toronto, ON",
                            destination = "Vancouver, BC"
                        }
                    };
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return new
                    {
                        success = false,
                        responseTimeMs = sw.ElapsedMilliseconds,
                        error = ex.Message,
                        errorType = ex.GetType().Name
                    };
                }
            });
            results.Add(test6);

            // Test 7: Rate Limiting Test (Multiple rapid requests)
            await Task.Delay(1100);
            var test7 = await RunTest("Rate Limiting Test (3 rapid requests)", async () =>
            {
                var sw = Stopwatch.StartNew();
                var rapidResults = new List<object>();
                var errors = 0;
                
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var reqSw = Stopwatch.StartNew();
                        await _locationService.GeocodeAddressAsync("Montreal, QC, Canada");
                        reqSw.Stop();
                        rapidResults.Add(new
                        {
                            requestNumber = i + 1,
                            success = true,
                            responseTimeMs = reqSw.ElapsedMilliseconds
                        });
                    }
                    catch (Exception ex)
                    {
                        errors++;
                        rapidResults.Add(new
                        {
                            requestNumber = i + 1,
                            success = false,
                            error = ex.Message,
                            errorType = ex.GetType().Name
                        });
                    }
                    // No delay between requests to test rate limiting
                }
                
                sw.Stop();
                return new
                {
                    success = errors == 0,
                    responseTimeMs = sw.ElapsedMilliseconds,
                    data = new
                    {
                        totalRequests = 3,
                        successfulRequests = 3 - errors,
                        failedRequests = errors,
                        requests = rapidResults,
                        note = errors > 0 ? "Rate limiting may be working (some requests failed)" : "All requests succeeded (rate limiting may not be enforced)"
                    }
                };
            });
            results.Add(test7);

            // Summary
            var passed = results.Count(r => ((dynamic)r).success == true);
            var failed = results.Count - passed;

            return Json(new
            {
                timestamp = DateTime.UtcNow,
                serviceType = _locationService.GetType().Name,
                summary = new
                {
                    totalTests = results.Count,
                    passed = passed,
                    failed = failed,
                    successRate = $"{Math.Round((double)passed / results.Count * 100, 2)}%"
                },
                tests = results
            });
        }

        private async Task<object> RunTest(string testName, Func<Task<object>> testAction)
        {
            try
            {
                var result = await testAction();
                return new
                {
                    testName = testName,
                    timestamp = DateTime.UtcNow,
                    result = result
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    testName = testName,
                    timestamp = DateTime.UtcNow,
                    result = new
                    {
                        success = false,
                        error = $"Test execution failed: {ex.Message}",
                        errorType = ex.GetType().Name
                    }
                };
            }
        }
    }
}

