﻿using Core.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IQuery<TEntity>
    {
        public IQueryable<TEntity> Queryable { get; }
    }

    public interface IFindAsync<TEntity, TKey> where TEntity : IEntityWithKey<TKey>
    {
        public Task<TEntity> FindAsync(TKey key);
    }

    public interface IAdd<TEntity>
    {
        public Task<TEntity> AddAsync(TEntity entity);
    }

    public interface IUpdate<TEntity>
    {
        public Task<TEntity> UpdateAsync(TEntity entity);
    }

    public interface IDelete<TEntity>
    {
        public Task<TEntity> DeleteAsync(TEntity entity);
    }

    public interface IFilteredQuery<TEntity>
    {
        public IEnumerable<TEntity> QueryFiltered(Expression<Func<TEntity, bool>> filterExpression);
    }

    public interface ITourRepository : IQuery<Tour>, IDelete<Tour>, IUpdate<Tour>, IFindAsync<Tour, int>
    {
        public Task<Tour> AddAsync(Tour tour, IFormFileCollection images);
        public Task<Tour> UpdateAsync(Tour tour, IFormFileCollection images);
    }

    public interface ITripRepository : IQuery<Trip>, IFindAsync<Trip, int>, IUpdate<Trip>
    {
        public Task<Trip> AddAsync(Trip trip, int[] discountIDs);
        public Task<Trip> UpdateAsync(Trip trip, int[] discountIDs);
    }

    public interface ITourReviewRepository : IQuery<TourReview>, IAdd<TourReview>
    {
        public IEnumerable<TourReview> GetReviewsForTour(Tour tour);
        public IEnumerable<TourReview> GetReviewsForCustomer(Customer customer);
    }

    public interface ICustomerRepository : IQuery<Customer>, IFindAsync<Customer, string>, IAdd<Customer>, IDelete<Customer>, IUpdate<Customer>
    {
        public void Follow(Customer customer, Tour tour);
        public void UnFollow(Customer customer, Tour tour);
        public void AddPointLog(PointLog pointLog);
        public IEnumerable<PointLog> GetPointsLogs(Customer customer);
    }

    public interface IEmployeeRepository<TEmployee> : IQuery<TEmployee>, IFindAsync<TEmployee, string>, IUpdate<TEmployee>, IFilteredQuery<TEmployee> where TEmployee : IEmployee
    {
        public IEnumerable<IRole> GetAllRoles();
        public Task UpdateAsync(TEmployee employee, string[] roleIds);
    }

    public interface IBookingRepository : IQuery<Booking>, IFindAsync<Booking, int>, IDelete<Booking>, IAdd<Booking>, IUpdate<Booking>
    {

    }

    public interface IItineraryRepository : IQuery<Itinerary>, IFindAsync<Itinerary, int>, IAdd<Itinerary>, IUpdate<Itinerary>, IDelete<Itinerary>, IFilteredQuery<Itinerary>
    {
        public Task<Itinerary> CopyTo(int tripID, Itinerary sourceItinerary);
    }

    public interface IQuestionRepository : IQuery<Question>, IAdd<Question>, IUpdate<Question>, IDelete<Question>
    {

    }

    public interface IAnswerRepository : IQuery<Answer>, IAdd<Answer>, IUpdate<Answer>, IDelete<Answer>, IFilteredQuery<Answer>
    {

    }

    public interface IDiscountRepository : IFindAsync<Discount, int>, IQuery<Discount>, IAdd<Discount>, IUpdate<Discount>, IDelete<Discount>
    {
        public void UpdateTripApplications(Discount discount, int[] tripIDs);
    }

    public interface ILogRepository : IQuery<Log>, IAdd<Log>
    {

    }

    public interface IInvoiceRepository : IQuery<Invoice>, IFindAsync<Invoice, int>, IAdd<Invoice>, IUpdate<Invoice>, IDelete<Invoice> { 
    
    }

    public interface IPostRepository<TPost, TEmployee> : IQuery<TPost>, IFindAsync<TPost, int> where TEmployee : IEmployee where TPost : IPost<TEmployee>
    {
        public Task<TPost> AddAsync(TPost post, IFormFile coverImg);
        public Task<TPost> UpdateAsync(TPost post, IFormFile coverImg);
        public Task AddComment(Comment comment);
    }
}
