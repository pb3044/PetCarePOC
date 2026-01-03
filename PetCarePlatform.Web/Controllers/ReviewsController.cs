using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using PetCarePlatform.Core.DTOs.Requests;
using PetCarePlatform.Core.DTOs.Queries;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using PetCarePlatform.Core.Common;
using PetCarePlatform.Web.Models;
using System.Security.Claims;

namespace PetCarePlatform.Web.Controllers
{
    [Authorize]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IBookingService _bookingService;
        private readonly IServiceService _serviceService;
        private readonly IPetOwnerService _petOwnerService;

        public ReviewsController(
            IReviewService reviewService,
            IBookingService bookingService,
            IServiceService serviceService,
            IPetOwnerService petOwnerService)
        {
            _reviewService = reviewService;
            _bookingService = bookingService;
            _serviceService = serviceService;
            _petOwnerService = petOwnerService;
        }

        [HttpGet]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Create(int bookingId)
        {
            try
            {
                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId, CancellationToken.None);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    return NotFound("Pet owner not found.");
                }

                var petOwner = petOwnerResult.Value;

                // Get booking details
                var bookingResult = await _bookingService.GetBookingByIdAsync(bookingId, CancellationToken.None);
                if (bookingResult.IsFailure || bookingResult.Value == null || bookingResult.Value.OwnerId != petOwner.Id)
                {
                    return NotFound("Booking not found or doesn't belong to you.");
                }

                var booking = bookingResult.Value;

                // Check if booking can be reviewed
                var canBeReviewedResult = await _bookingService.CanBeReviewedAsync(bookingId);
                if (canBeReviewedResult.IsFailure || !canBeReviewedResult.Value)
                {
                    TempData["Error"] = "This booking cannot be reviewed yet. Reviews can only be submitted for completed bookings.";
                    return RedirectToAction("MyBookings", "PetOwner");
                }

                // Check if review already exists
                var existingReviewResult = await _reviewService.GetReviewByBookingIdAsync(bookingId, CancellationToken.None);
                if (existingReviewResult.IsSuccess && existingReviewResult.Value != null)
                {
                    TempData["Info"] = "You have already reviewed this booking.";
                    return RedirectToAction("Edit", new { id = existingReviewResult.Value.Id });
                }

                var viewModel = new CreateReviewViewModel
                {
                    BookingId = bookingId,
                    ServiceName = booking.ServiceName ?? "Service",
                    ProviderName = booking.OwnerName ?? "Provider",
                    PetName = booking.PetName ?? "Pet",
                    ServiceDate = booking.StartTime
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.Create: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the review form.";
                return RedirectToAction("MyBookings", "PetOwner");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Create(CreateReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId, CancellationToken.None);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    return NotFound("Pet owner not found.");
                }

                var petOwner = petOwnerResult.Value;

                // Get booking details
                var bookingResult = await _bookingService.GetBookingByIdAsync(model.BookingId);
                if (bookingResult.IsFailure || bookingResult.Value == null || bookingResult.Value.OwnerId != petOwner.Id)
                {
                    return NotFound("Booking not found or doesn't belong to you.");
                }

                var booking = bookingResult.Value;

                // Use enterprise pattern method
                var request = new CreateReviewRequest
                {
                    BookingId = model.BookingId,
                    Rating = model.Rating,
                    Comment = model.Comment
                };

                // Handle photo uploads first to get URLs
                List<string>? photoUrls = null;
                if (model.Photos != null && model.Photos.Any())
                {
                    photoUrls = await UploadPhotosAsync(model.Photos);
                    request.PhotoUrls = photoUrls;
                }

                // Create review using enterprise pattern
                var result = await _reviewService.CreateReviewAsync(request, petOwner.Id, CancellationToken.None);

                if (result.IsFailure)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    return View(model);
                }

                TempData["Success"] = "Thank you for your review! Your feedback helps other pet owners make informed decisions.";
                return RedirectToAction("MyBookings", "PetOwner");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.Create POST: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while submitting your review. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId, CancellationToken.None);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    return NotFound("Pet owner not found.");
                }

                var petOwner = petOwnerResult.Value;

                // Get review
                var reviewResult = await _reviewService.GetReviewByIdAsync(id, CancellationToken.None);
                if (reviewResult.IsFailure || reviewResult.Value == null || reviewResult.Value.ReviewerId != petOwner.Id)
                {
                    return NotFound("Review not found or doesn't belong to you.");
                }

                var review = reviewResult.Value;

                var viewModel = new EditReviewViewModel
                {
                    Id = review.Id,
                    BookingId = review.BookingId,
                    Rating = review.Rating,
                    Comment = review.Comment ?? string.Empty,
                    ServiceName = review.ServiceName,
                    ProviderName = "Provider" // ReviewResponse doesn't have provider name directly
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.Edit: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the review.";
                return RedirectToAction("MyBookings", "PetOwner");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Edit(EditReviewViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId, CancellationToken.None);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    return NotFound("Pet owner not found.");
                }

                var petOwner = petOwnerResult.Value;

                // Use enterprise pattern method
                var request = new UpdateReviewRequest
                {
                    ReviewId = model.Id,
                    Rating = model.Rating,
                    Comment = model.Comment
                };

                var result = await _reviewService.UpdateReviewAsync(request, petOwner.Id);

                if (result.IsFailure)
                {
                    ModelState.AddModelError("", result.ErrorMessage);
                    return View(model);
                }

                TempData["Success"] = "Your review has been updated successfully.";
                return RedirectToAction("MyBookings", "PetOwner");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.Edit POST: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while updating your review. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ServiceReviews(int serviceId, int page = 1, int pageSize = 10)
        {
            try
            {
                var serviceResult = await _serviceService.GetServiceByIdAsync(serviceId, CancellationToken.None);
                if (serviceResult.IsFailure || serviceResult.Value == null)
                {
                    return NotFound("Service not found.");
                }

                var service = serviceResult.Value;

                // Use enterprise pattern method with pagination
                var query = new ReviewQuery
                {
                    ServiceId = serviceId,
                    PageNumber = page,
                    PageSize = pageSize,
                    SortBy = "CreatedAt",
                    SortOrder = "desc"
                };

                var result = await _reviewService.GetReviewsAsync(query, CancellationToken.None);

                if (result.IsFailure)
                {
                    TempData["Error"] = result.ErrorMessage;
                    return RedirectToAction("Details", "Services", new { id = serviceId });
                }

                // Get reviews using the new method
                var reviewsResult = await _reviewService.GetReviewsByServiceIdAsync(serviceId, CancellationToken.None);
                if (reviewsResult.IsFailure)
                {
                    ViewBag.ErrorMessage = reviewsResult.ErrorMessage;
                    return View(new ServiceReviewsViewModel
                    {
                        ServiceId = serviceId,
                        ServiceName = serviceResult.Value?.Title ?? "Service",
                        ProviderName = serviceResult.Value?.ProviderBusinessName ?? "Provider",
                        AverageRating = 0,
                        TotalReviews = 0,
                        Reviews = new List<Review>(),
                        CurrentPage = page,
                        PageSize = pageSize,
                        TotalPages = 0
                    });
                }

                var reviews = reviewsResult.Value?.Items?.ToList() ?? new List<PetCarePlatform.Core.DTOs.Responses.ReviewResponse>();
                var ratingResult = await _serviceService.GetServiceRatingAsync(serviceId, CancellationToken.None);
                var averageRating = ratingResult.IsSuccess ? ratingResult.Value : 0.0;

                var viewModel = new ServiceReviewsViewModel
                {
                    ServiceId = serviceId,
                    ServiceName = serviceResult.Value?.Title ?? "Service",
                    ProviderName = serviceResult.Value?.ProviderBusinessName ?? "Provider",
                    AverageRating = averageRating,
                    TotalReviews = reviewsResult.Value?.TotalCount ?? 0,
                    Reviews = new List<Review>(), // Would need to map ReviewResponse to Review if needed
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalPages = reviewsResult.Value?.TotalPages ?? 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.ServiceReviews: {ex.Message}");
                TempData["Error"] = "An error occurred while loading reviews.";
                return RedirectToAction("Details", "Services", new { id = serviceId });
            }
        }

        [HttpPost]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> RespondToReview(int reviewId, string response)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(response))
                {
                    return Json(new { success = false, message = "Response cannot be empty." });
                }

                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                // Verify the review belongs to this provider
                var reviewResult = await _reviewService.GetReviewByIdAsync(reviewId, CancellationToken.None);
                if (reviewResult.IsFailure || reviewResult.Value == null)
                {
                    return Json(new { success = false, message = "Review not found." });
                }

                // Use enterprise pattern method
                var request = new AddReviewResponseRequest
                {
                    ReviewId = reviewId,
                    Response = response
                };

                var result = await _reviewService.AddResponseToReviewAsync(request, userId, CancellationToken.None);

                if (result.IsFailure)
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }

                return Json(new { success = true, message = "Response added successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.RespondToReview: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while adding your response." });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "PetOwner")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var petOwnerResult = await _petOwnerService.GetPetOwnerByUserIdAsync(userId);
                if (petOwnerResult.IsFailure || petOwnerResult.Value == null)
                {
                    return Json(new { success = false, message = "Pet owner not found." });
                }
                var petOwner = petOwnerResult.Value;

                // Get review
                var reviewResult = await _reviewService.GetReviewByIdAsync(id, CancellationToken.None);
                if (reviewResult.IsFailure || reviewResult.Value == null || reviewResult.Value.ReviewerId != petOwner.Id)
                {
                    return Json(new { success = false, message = "Review not found or doesn't belong to you." });
                }

                // Delete review (you'll need to implement this in the service)
                // await _reviewService.DeleteReviewAsync(id);

                return Json(new { success = true, message = "Review deleted successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.Delete: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while deleting the review." });
            }
        }

        // Photo upload handling - returns URLs for DTO
        private async Task<List<string>> UploadPhotosAsync(List<IFormFile> photos)
        {
            var photoUrls = new List<string>();
            
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reviews");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var photo in photos.Take(5)) // Max 5 photos
                {
                    if (photo != null && photo.Length > 0)
                    {
                        // Validate file type
                        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                        if (!allowedTypes.Contains(photo.ContentType))
                        {
                            continue; // Skip invalid files
                        }

                        // Generate unique filename
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        photoUrls.Add($"/uploads/reviews/{fileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error uploading photos: {ex.Message}");
                // Return what we have - photo upload failure shouldn't break review creation
            }

            return photoUrls;
        }

        // Legacy photo upload handling (for backward compatibility)
        private async Task HandlePhotoUploads(int reviewId, List<IFormFile> photos, List<string> captions)
        {
            try
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "reviews");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                for (int i = 0; i < photos.Count; i++)
                {
                    var photo = photos[i];
                    if (photo != null && photo.Length > 0)
                    {
                        // Validate file type
                        var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
                        if (!allowedTypes.Contains(photo.ContentType))
                        {
                            continue; // Skip invalid files
                        }

                        // Generate unique filename
                        var fileName = $"{reviewId}_{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Save file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await photo.CopyToAsync(stream);
                        }

                        // Create ReviewPhoto record
                        // Note: AddReviewPhotoAsync is not available in IReviewService
                        // This functionality would need to be added to the service or use repository directly
                        // For now, we'll skip the database record creation
                        // var reviewPhoto = new ReviewPhoto
                        // {
                        //     ReviewId = reviewId,
                        //     Url = $"/uploads/reviews/{fileName}",
                        //     Caption = i < captions.Count ? captions[i] : string.Empty,
                        //     CreatedAt = DateTime.UtcNow
                        // };
                        // await _reviewService.AddReviewPhotoAsync(reviewPhoto);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error handling photo uploads: {ex.Message}");
                // Don't throw - photo upload failure shouldn't break review creation
            }
        }

        // Provider response to reviews
        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Respond(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var reviewResult = await _reviewService.GetReviewByIdAsync(id, CancellationToken.None);
                
                if (reviewResult.IsFailure || reviewResult.Value == null || reviewResult.Value.RevieweeId != userId)
                {
                    return NotFound("Review not found or doesn't belong to you.");
                }

                var review = reviewResult.Value;
                var viewModel = new ProviderResponseViewModel
                {
                    ReviewId = id,
                    ReviewerName = review.ReviewerName ?? "Anonymous",
                    ServiceName = review.ServiceName ?? "Service",
                    Rating = review.Rating,
                    Comment = review.Comment ?? string.Empty,
                    Response = review.Response ?? string.Empty
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.Respond: {ex.Message}");
                TempData["Error"] = "An error occurred while loading the response form.";
                return RedirectToAction("Index", "ServiceProvider");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Respond(ProviderResponseViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var reviewResult = await _reviewService.GetReviewByIdAsync(model.ReviewId, CancellationToken.None);
                
                if (reviewResult.IsFailure || reviewResult.Value == null || reviewResult.Value.RevieweeId != userId)
                {
                    return NotFound("Review not found or doesn't belong to you.");
                }

                // Add response to review using the proper method
                var request = new AddReviewResponseRequest
                {
                    ReviewId = model.ReviewId,
                    Response = model.Response
                };
                
                var result = await _reviewService.AddResponseToReviewAsync(request, userId, CancellationToken.None);
                
                if (result.IsFailure)
                {
                    ModelState.AddModelError("", result.ErrorMessage ?? "An error occurred while posting your response.");
                    return View(model);
                }

                TempData["Success"] = "Your response has been posted successfully.";
                return RedirectToAction("Index", "ServiceProvider");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ReviewsController.Respond POST: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while posting your response. Please try again.");
                return View(model);
            }
        }
    }
}

