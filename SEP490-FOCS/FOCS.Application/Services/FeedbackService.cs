using AutoMapper;
using FOCS.Application.DTOs.AdminServiceDTO;
using FOCS.Common.Enums;
using FOCS.Common.Exceptions;
using FOCS.Common.Interfaces;
using FOCS.Common.Interfaces.Focs.Application.Interfaces;
using FOCS.Common.Models;
using FOCS.Common.Utils;
using FOCS.Infrastructure.Identity.Common.Repositories;
using FOCS.Infrastructure.Identity.Identity.Model;
using FOCS.Order.Infrastucture.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using OrderEntity = FOCS.Order.Infrastucture.Entities.Order;

namespace FOCS.Application.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IRepository<Feedback> _feedbackRepository;
        private readonly IRepository<OrderEntity> _orderRepo;
        private readonly IMapper _mapper;

        private readonly ICloudinaryService _cloudImageService;

        public FeedbackService(IRepository<Feedback> feedbackRepository, IRepository<OrderEntity> orderRepo, IMapper mapper, ICloudinaryService cloudImageService)
        {
            _feedbackRepository = feedbackRepository;
            _mapper = mapper;
            _cloudImageService = cloudImageService;
            _orderRepo = orderRepo;
        }

        public async Task<bool> SubmitFeedbackAsync(CreateFeedbackRequest request, string storeId)
        {
            try
            {
                var images = await _cloudImageService.UploadImageFeedbackAsync(request.Files, storeId, request.OrderId.ToString());

                var mappingFeedbackModel = _mapper.Map<Feedback>(request);

                mappingFeedbackModel.Id = Guid.NewGuid();
                mappingFeedbackModel.Images = images.Select(x => x.Url).ToList();
                mappingFeedbackModel.CreatedAt = DateTime.UtcNow;
                mappingFeedbackModel.CreatedBy = request.ActorId.ToString();
                mappingFeedbackModel.StoreId = Guid.Parse(storeId);
                mappingFeedbackModel.IsPublic = true;

                await _feedbackRepository.AddAsync(mappingFeedbackModel);
                await _feedbackRepository.SaveChangesAsync();

                return true;
            } catch(Exception ex)
            {
                return false;
            }
        }

        public async Task<FeedbackDTO> UpdatePublicCommentRequest(Guid id, UpdatePublicCommentRequest request, string storeId)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id);

            ConditionCheck.CheckCondition(feedback != null, Errors.Common.NotFound);

            feedback.IsPublic = request.IsPublic;
            feedback.UpdatedAt = DateTime.UtcNow;
            feedback.UpdatedBy = storeId;

            _feedbackRepository.Update(feedback);
            await _feedbackRepository.SaveChangesAsync();

            return _mapper.Map<FeedbackDTO>(feedback);
        }

        public async Task<PagedResult<FeedbackDTO>> GetAllFeedbacksAsync(UrlQueryParameters query, string storeId)
        {
            var feedbackQuery = _feedbackRepository.AsQueryable().Where(p => p.StoreId == Guid.Parse(storeId));

            feedbackQuery = ApplyFilters(feedbackQuery, query);
            feedbackQuery = ApplySearch(feedbackQuery, query);
            feedbackQuery = ApplySort(feedbackQuery, query);

            var total = await feedbackQuery.CountAsync();
            var items = await feedbackQuery
                .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

            var mapped = _mapper.Map<List<FeedbackDTO>>(items);
            return new PagedResult<FeedbackDTO>(mapped, total, query.Page, query.PageSize);
        }

        public async Task<FeedbackDTO> GetFeedbackByIdAsync(Guid feedbackId, string storeId)
        {
            var feedback = await _feedbackRepository.AsQueryable()
                .Include(f => f.Order).ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(x => x.Id == feedbackId && x.StoreId == Guid.Parse(storeId));

            ConditionCheck.CheckCondition(feedback != null, Errors.Common.NotFound);

            return _mapper.Map<FeedbackDTO>(feedback);
        }

        public async Task<FeedbackDTO> GetFeedbackByOrderIdAsync(Guid orderId, string storeId)
        {
            var feedback = await _feedbackRepository.AsQueryable().FirstOrDefaultAsync(x => x.OrderId == orderId && x.StoreId == Guid.Parse(storeId));

            ConditionCheck.CheckCondition(feedback != null, Errors.Common.NotFound);

            return _mapper.Map<FeedbackDTO>(feedback);
        }

        public Task ReplyToFeedbackAsync(Guid feedbackId, string reply, string storeId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<FeedbackDTO>> GetFeedbackByMenuItemAsync(Guid menuItemId, string storeId)
        {
            var feedbacks = await _feedbackRepository.AsQueryable()
                .Include(x => x.Order)
                    .ThenInclude(x => x.OrderDetails)
                .Where(fb => fb.Order.OrderDetails.Any(od => od.MenuItemId == menuItemId)
                             && fb.Order.StoreId == Guid.Parse(storeId))
                .ToListAsync();

            return _mapper.Map<List<FeedbackDTO>>(feedbacks);
        }


        public async Task<bool> SetFeedbackVisibilityAsync(Guid feedbackId, bool isPublic, string storeId)
        {
            try
            {
                var feedback = await _feedbackRepository.AsQueryable().FirstOrDefaultAsync(x => x.Id == feedbackId && x.StoreId == Guid.Parse(storeId));

                feedback.IsPublic = isPublic;
                feedback.UpdatedAt = DateTime.UtcNow;
                feedback.UpdatedBy = storeId;

                _feedbackRepository.Update(feedback);
                await _feedbackRepository.SaveChangesAsync();

                return true;
            } catch(Exception ex)
            {
                return false;
            }
        }


        #region private methods

        private static IQueryable<Feedback> ApplyFilters(IQueryable<Feedback> query, UrlQueryParameters parameters)
        {
            if (parameters.Filters?.Any() != true) return query;

            foreach (var (key, value) in parameters.Filters)
            {
                query = key.ToLowerInvariant() switch
                {
                    "rating" => query.Where(p => p.Rating == int.Parse(value)),
                    "is_public" => query.Where(p => p.IsPublic),
                    "created_date_from" => query.Where(p => p.CreatedAt > DateTime.Parse(value)),
                    "create_date_to" => query.Where(p => p.CreatedAt < DateTime.Parse(value)),
                };
            }

            return query;
        }

        private static IQueryable<Feedback> ApplySearch(IQueryable<Feedback> query, UrlQueryParameters parameters)
        {
            if (string.IsNullOrWhiteSpace(parameters.SearchBy) || string.IsNullOrWhiteSpace(parameters.SearchValue))
                return query;

            var searchValue = parameters.SearchValue.ToLowerInvariant();

            return parameters.SearchBy.ToLowerInvariant() switch
            {
                "comment" => query.Where(p => p.Comment.ToLower().Contains(searchValue)),
                _ => query
            };
        }

        private static IQueryable<Feedback> ApplySort(IQueryable<Feedback> query, UrlQueryParameters parameters)
        {
            //if (string.IsNullOrWhiteSpace(parameters.SortBy)) return query.OrderBy(p => p.StartDate);

            var isDescending = string.Equals(parameters.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

            if(parameters.SortBy != null)
            {
                query = parameters.SortBy.ToLowerInvariant() switch
                {
                    "created_date" => isDescending
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt),
                    "rating" => isDescending
                        ? query.OrderByDescending(p => p.Rating)
                        : query.OrderBy(p => p.Rating)
                };
            }

            return query;
        }

        #endregion
    }
}
