using System;

namespace Pnnl.Api.Approvals
{
    /// <summary>
    ///     Represents a range of dates.
    /// </summary>
    public class DateRange
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DateRange" /> structure to the specified start and end date.
        /// </summary>
        /// <param name="startDate">A string that contains that first date in the date range.</param>
        /// <param name="endDate">A string that contains the last date in the date range.</param>
        /// <exception cref="System.ArgumentNullException">
        ///		endDate or startDate are <c>null</c>.
        /// </exception>
        /// <exception cref="System.FormatException">
        ///     endDate or startDate do not contain a vaild string representation of a date and time.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        ///		endDate is not greater than or equal to startDate
        /// </exception>
        public DateRange(string startDate, string endDate)
        {
            if (string.IsNullOrWhiteSpace(startDate))
            {
                throw new ArgumentNullException("startDate");
            }

            if (string.IsNullOrWhiteSpace(endDate))
            {
                throw new ArgumentNullException("endDate");
            }

            Start = DateTime.Parse(startDate);
            End = DateTime.Parse(endDate);

            if(End.Hour == 0 && End.Minute == 0)
            {
                End.AddHours(23);
                End.AddMinutes(59);
                End.AddMilliseconds(999);
            }

            if (End < Start)
            {
                throw new ArgumentException("endDate must be greater than or equal to startDate");
            }
        }
        public DateRange()
        {
            Start = DateTime.Today;
            End = DateTime.Today;

            End.AddHours(23);
            End.AddMinutes(59);
            End.AddMilliseconds(999);
        }

        public DateRange(DateTime startDate, DateTime endDate)
        {

            if (startDate == null)
            {
                throw new ArgumentNullException(nameof(startDate));
            }

            if (endDate == null)
            {
                throw new ArgumentNullException(nameof(endDate));
            }

            Start = startDate;
            End = endDate;

            if(End.Hour == 0 && End.Minute == 0)
            {
                End.AddHours(23);
                End.AddMinutes(59);
                End.AddMilliseconds(999);
            }

            if (End < Start)
            {
                throw new ArgumentException("endDate must be greater than or equal to startDate");
            }
        }

        public DateRange(int days)
        {
            days--;

            End = DateTime.Today;
            Start = End.AddDays(-days);
        }

        /// <summary>
        ///     Gets the start date component of the date range.
        /// </summary>
        public DateTime Start { get;  set; }


        /// <summary>
        ///     Gets the end date component of the date range.
        /// </summary>
        public DateTime End { get;  set; }

        /// <summary>
        ///     Gets the number of whole days in the date range.
        /// </summary>
        public int Days
        {
            get { return (End - Start).Days + 1; }
        }
    }
}
