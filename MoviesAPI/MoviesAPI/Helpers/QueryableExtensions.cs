using Microsoft.EntityFrameworkCore;
using MoviesAPI.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Helpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginationDTO pagination, int totalAmountPages)
        {
            var currentPage = pagination.Page > totalAmountPages ? totalAmountPages : pagination.Page;
            return queryable
                .Skip((currentPage - 1) * pagination.RecordsPerPage)
                .Take(pagination.RecordsPerPage);
        }

        public async static Task<int> PaginationTotalPages<T>(this IQueryable<T> queryable, int recordsPerPage)
        {
            double count = await queryable.CountAsync();
            double totalAmountPages = Math.Ceiling(count / recordsPerPage);
            return (int)totalAmountPages;
        }
    }
}
