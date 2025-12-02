using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMMS.Application.Features.foodmenu.DTOs;
public class PagedResult<T>
{
    public int PageIndex { get; set; }   // 1-based
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages =>
        PageSize == 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    public List<T> Items { get; set; } = new();
}
