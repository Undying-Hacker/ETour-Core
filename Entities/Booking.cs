﻿using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Core.Entities
{
    public class Booking : AuthoredTrackedEntityWithKey<Customer, int, string>
    {
        public static decimal MaxPointRatio = .8m;
        public Trip Trip { get; set; }
        public int TripID { get; set; }

        [StringLength(512)]
        public string Note { get; set; }

        [Range(0, double.PositiveInfinity, ErrorMessage = "Unrealistic monetary value")]
        public decimal Total { get; set; }

        public BookingStatus Status { get; set; }

        [Required]
        [Display(Name = "Most Valued Quality")]
        public BookingMostValued MostValued { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateDeposited { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? DateCompleted { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? PaymentDeadline { get; set; }

        public ICollection<CustomerInfo> CustomerInfos { get; set; } = new List<CustomerInfo>();

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(128, MinimumLength = 3)]
        public string ContactName { get; set; }
        [Required]
        [EmailAddress]
        [StringLength(128, MinimumLength = 3)]
        [Display(Name = "Email Address")]
        public string ContactEmail { get; set; }
        [Display(Name = "Phone Number")]
        [Required]
        [Phone]
        public string ContactPhone { get; set; }

        [Display(Name = "Home Address")]
        [StringLength(256, MinimumLength = 3)]
        public string ContactAddress { get; set; }

        [Range(1, int.MaxValue)]
        public int TicketCount { get; set; }

        public int? PointsApplied { get; set; }
        public decimal? Refunded { get; set; }

        public decimal? Deposit { get; set; }

        public int GetApplicablePoints(int points)
        {
            return Convert.ToInt32(Math.Floor(Math.Min(points, Total * MaxPointRatio)));
        }

        public decimal GetFinalPayment()
        {
            return Total - Deposit.Value;
        }

        public IEnumerable<BookingStatus> GetPossibleNextStatuses()
        {
            return Status switch
            {
                BookingStatus.Awaiting_Deposit => new BookingStatus[] { BookingStatus.Processing, BookingStatus.Awaiting_Payment, BookingStatus.Canceled },
                BookingStatus.Processing => new BookingStatus[] { BookingStatus.Awaiting_Payment, BookingStatus.Canceled },
                BookingStatus.Awaiting_Payment => new BookingStatus[] { BookingStatus.Completed, BookingStatus.Canceled },
                BookingStatus.Completed => new BookingStatus[] { BookingStatus.Canceled },
                BookingStatus.Canceled => new BookingStatus[] { },
                _ => throw new InvalidOperationException(),
            };
        }

        public void ChangeStatus(BookingStatus newStatus)
        {
            if (!GetPossibleNextStatuses().Contains(newStatus))
            {
                throw new InvalidOperationException("Invalid booking status change");
            }
            if (Status == BookingStatus.Awaiting_Deposit)
            {
                DateDeposited = DateTime.Today;
                PaymentDeadline = Trip.StartTime.AddDays(-5);
            }
            else if (Status == BookingStatus.Awaiting_Payment)
            {
                DateCompleted = DateTime.Today;
            }
            Status = newStatus;
        }

        public BookingCancelInfo GetBookingCancelInfo(DateTime cancelDate)
        {
            decimal amountPaid = 0;
            // Customer has paid the full amount
            if (DateCompleted.HasValue)
            {
                amountPaid = Total;
            }
            // Customer has not paid full but paid deposit
            else if (DateDeposited.HasValue)
            {
                amountPaid = Deposit.Value;
            }
            var daysEarly = (Trip.StartTime - cancelDate).TotalDays;
            var ratioLost = CalculateCancelRatio(daysEarly);
            var refund = Math.Max(0, amountPaid - Total * ratioLost);

            return new BookingCancelInfo
            {
                BookingID = ID,
                AmountLost = amountPaid - refund,
                Refund = refund,
                PointsLost = PointsApplied.Value,
                DaysEarly = Convert.ToInt32(daysEarly),
                Trip = Trip
            };
        }

        private decimal CalculateCancelRatio(double daysEarly)
        {
            if (daysEarly >= 20)
            {
                return .3m;
            } else if (daysEarly >= 15)
            {
                return .5m;
            } else if (daysEarly >= 10)
            {
                return .7m;
            } else if (daysEarly >= 5)
            {
                return .9m;
            }

            return 1;
        }

        public void SetDeposit(float depositPercentage)
        {
            Deposit = Total / 100 * (decimal)depositPercentage;
        }

        public int GetMemberCountByAgeGroup(CustomerInfo.CustomerAgeGroup ageGroup)
        {
            return CustomerInfos.Where(ci => ci.AgeGroup == ageGroup).Count();
        }

        public bool CanCancel(DateTime dateCancel)
        {
            return Status != BookingStatus.Canceled && Trip.StartTime >= dateCancel;
        }

        public void Cancel(DateTime dateCancel)
        {
            if (!CanCancel(dateCancel))
            {
                throw new InvalidOperationException("Attempting to cancel a uncancellable booking");
            }

            var cancelInfo = GetBookingCancelInfo(dateCancel);

            Refunded = cancelInfo.Refund;
            ChangeStatus(BookingStatus.Canceled);
        }

        public enum BookingStatus
        {
            Awaiting_Deposit,
            Processing,
            Awaiting_Payment,
            Completed,
            Canceled,
        }

        public enum BookingMostValued
        {
            Transportation,
            Accomodation,
            Activities,
            Cuisine
        }

        public enum BookingPaymentProvider
        {
            Zalo_Pay,
            MoMo,
            Google_Pay
        }

        public class BookingCancelInfo
        {
            public int BookingID { get; set; }
            public decimal Refund { get; set; }
            public int PointsLost { get; set; }
            public decimal AmountLost { get; set; }
            public int DaysEarly { get; set; }
            public Trip Trip { get; set; }
        }
    }
}
