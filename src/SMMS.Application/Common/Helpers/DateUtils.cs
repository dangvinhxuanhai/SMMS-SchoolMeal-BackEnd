using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Common.Helpers;
public static class DateOnlyUtils
{
    public static int CountWeekdays(DateOnly dateFrom, DateOnly dateTo)
    {
        if (dateFrom > dateTo) return 0;

        int count = 0;
        for (var date = dateFrom; date <= dateTo; date = date.AddDays(1))
        {
            if (date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday)
            {
                count++;
            }
        }
        return count;
    }

    public static List<DateOnly> GetWeekendDates(DateOnly dateFrom, DateOnly dateTo)
    {
        var result = new List<DateOnly>();
        for (var date = dateFrom; date <= dateTo; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday)
            {
                result.Add(date);
            }
        }
        return result;
    }
    public static (DateOnly from, DateOnly to) GetMonthRange(int year, int month)
    {
        var from = new DateOnly(year, month, 1);
        var to = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        return (from, to);
    }
}
