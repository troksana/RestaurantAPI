using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantAPI.Authorization;
using RestaurantAPI.Entities;
using RestaurantAPI.Exceptions;
using RestaurantAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace RestaurantAPI.Services
{
    public interface IRestaurantService
    {
        PagedResult<RestaurantDto> GetAll(RestaurantQuery query);
        RestaurantDto GetById(int id);
        int Create (CreateRestaurantDto dto);
        public void Delete(int id);
        public void Update(int id, UpdateRestaurantDto dto);
    }
    public class RestaurantService : IRestaurantService
    {
        private readonly RestaurantDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserContextService _userContextService;

        public RestaurantService(RestaurantDbContext dbContext, IMapper mapper
            ,IUserContextService userContextService, ILogger<RestaurantService> logger,
            IAuthorizationService authorizationService)
        {
              _dbContext = dbContext;
              _mapper = mapper;
              _logger = logger;
            _authorizationService = authorizationService;
            _userContextService = userContextService;
        }
        //usuwa restauracje
        public void Delete(int id)
        {
            _logger.LogError($"Restaurant with id: {id} DELETE action invoked");
            var restaurants = _dbContext
              .Restaurants
              .FirstOrDefault(r => r.Id == id);

            if (restaurants is null)
            {
                throw new NotFoundException("Restaurant not found!"); //nic nie zostalo usuniete
            }

            var authorizationResult = _authorizationService.AuthorizeAsync(_userContextService.User, restaurants,
                new ResourceOperationRequirement(ResourceOperation.Delete)).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }


            _dbContext.Restaurants.Remove(restaurants);
            _dbContext.SaveChanges();
            //return true;
        }

        //pobiera restauracje na podstawie id
        public RestaurantDto GetById(int id)
        {
            var restaurants = _dbContext
               .Restaurants
               .Include(r => r.Address)
               .Include(r => r.Dishes)
               .FirstOrDefault(r => r.Id == id);

                if (restaurants is null)
                {
                    throw new NotFoundException("Restaurant not found!");
                }
            var result = _mapper.Map<RestaurantDto>(restaurants);
            return result;
        }

        //pobiera wszystkie restauracje
        public PagedResult<RestaurantDto> GetAll(RestaurantQuery query)
        {
            var baseQuery = _dbContext
                .Restaurants
                .Include(r => r.Address)
                .Include(r => r.Dishes)
                .Where(r => query.SearchPhrase == null ||
                    (r.Name.ToLower().Contains(query.SearchPhrase.ToLower()) ||
                    (r.Description != null && r.Description.ToLower().Contains(query.SearchPhrase.ToLower()))));


            if(!string.IsNullOrEmpty(query.SortBy))
            {
                var columnsSelectors = new Dictionary<string, Expression<Func<Restaurant, object>>>
                {
                    {nameof(Restaurant.Name), r=>r.Name },
                    {nameof(Restaurant.Description), r=>r.Description },
                    {nameof(Restaurant.Category), r=>r.Category },
                };

                var selectColumn = columnsSelectors[query.SortBy];

                baseQuery = query.SortDirection == SortDirection.ASC 
                    ? baseQuery.OrderBy(selectColumn)
                    : baseQuery.OrderByDescending(selectColumn);
            }

            var restaurants =
                baseQuery
                .Skip(query.PageSize * (query.PageNumber - 1))
                .Take(query.PageSize)
                .ToList(); //zwracamy liste

            var totalItemsCount = baseQuery.Count();

            var restaurantsDtos = _mapper.Map<List<RestaurantDto>>(restaurants);

            var result = new PagedResult<RestaurantDto>(restaurantsDtos, totalItemsCount, query.PageSize, query.PageNumber);
            return result;
        }

        //tworzy restauracje
        public int Create(CreateRestaurantDto dto)
        {
            var restaurant = _mapper.Map<Restaurant>(dto);
            restaurant.CreatedById = _userContextService.GetUserId;
            _dbContext.Restaurants.Add(restaurant);
            
            return restaurant.Id;
        }
        //modyfikuje parametry restauracji
        public void Update(int id, UpdateRestaurantDto dto)
        {
            
            var restaurants = _dbContext
              .Restaurants
              .FirstOrDefault(r => r.Id == id);
            if (restaurants is null)
            {
                throw new NotFoundException("Restaurant not found!");
            }

           var authorizationResult= _authorizationService.AuthorizeAsync(_userContextService.User, restaurants, 
                new ResourceOperationRequirement(ResourceOperation.Update)).Result;

            if (!authorizationResult.Succeeded)
            {
                throw new ForbidException();
            }

            restaurants.Name = dto.Name;
            restaurants.Description = dto.Description;
            restaurants.HasDelivery = dto.hasDelivery;
            _dbContext.SaveChanges();
            //return true;
        }
    }
}
