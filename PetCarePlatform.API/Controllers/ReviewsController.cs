using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using PetCarePlatform.Core.Interfaces;
using PetCarePlatform.Core.Models;
using System.Collections.Generic;

namespace PetCarePlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IBookingService _bookingService;

        public ReviewsController(
            IReviewService reviewService,
            IBookingService bookingService)
        {
            _reviewService = reviewService;
            _bookingService = bookingService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }

        [HttpGet("booking/{bookingId}")]
        public async Task<IActionResult> GetReviewByBooking(int bookingId)
        {
            var review = await _reviewService.GetReviewByBookingIdAsync(bookingId);
            if (review == null)
            {
                return NotFound();
            }

            return Ok(review);
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetReviewsByService(int serviceId)
        {
            var reviews = await _reviewService.GetReviewsByServiceIdAsync(serviceId);
            return Ok(reviews);
        }

        [HttpGet("provider/{providerId}")]
        public async Task<IActionResult> GetReviewsByProvider(int providerId)
        {
            var reviews = await _reviewService.GetReviewsByProviderIdAsync(providerId);
            return Ok(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Check if booking can be reviewed
                if (!await _bookingService.CanBeReviewedAsync(request.BookingId))
                {
                    return BadRequest("This booking cannot be reviewed");
                }

                var review = new Review
                {
                    BookingId = request.BookingId,
                    ReviewerId = request.ReviewerId,
                    RevieweeId = request.RevieweeId,
                    ServiceId = request.ServiceId,
                    Rating = request.Rating,
                    Comment = request.Comment
                };

                var createdReview = await _reviewService.CreateReviewAsync(review);
                return CreatedAtAction(nameof(GetReview), new { id = createdReview.Id }, createdReview);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var review = await _reviewService.GetReviewByIdAsync(id);
                if (review == null)
                {
                    return NotFound();
                }

                // Update review fields
                review.Rating = request.Rating;
                review.Comment = request.Comment;

                await _reviewService.UpdateReviewAsync(review);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/response")]
        public async Task<IActionResult> AddResponseToReview(int id, [FromBody] AddResponseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _reviewService.AddResponseToReviewAsync(id, request.Response);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}/photos")]
        public async Task<IActionResult> GetReviewPhotos(int id)
        {
            try
            {
                var photos = await _reviewService.GetReviewPhotosAsync(id);
                return Ok(photos);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/photos")]
        public async Task<IActionResult> AddReviewPhoto(int id, [FromBody] AddReviewPhotoRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var photo = new ReviewPhoto
                {
                    ReviewId = id,
                    Url = request.Url,
                    Caption = request.Caption,
                    CreatedAt = DateTime.UtcNow
                };

                await _reviewService.AddReviewPhotoAsync(photo);
                return Ok(new { message = "Photo added successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("photos/{photoId}")]
        public async Task<IActionResult> DeleteReviewPhoto(int photoId)
        {
            try
            {
                await _reviewService.DeleteReviewPhotoAsync(photoId);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class CreateReviewRequest
    {
        public int BookingId { get; set; }
        public int ReviewerId { get; set; }
        public int RevieweeId { get; set; }
        public int ServiceId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public class UpdateReviewRequest
    {
        public int Rating { get; set; }
        public string Comment { get; set; }
    }

    public class AddResponseRequest
    {
        public string Response { get; set; }
    }

    public class AddReviewPhotoRequest
    {
        public string Url { get; set; }
        public string Caption { get; set; }
    }
}
