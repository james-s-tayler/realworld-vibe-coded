using Microsoft.EntityFrameworkCore;
using Server.Core.Interfaces;
using Server.Infrastructure.Data;

namespace Server.Infrastructure.Data.Queries;

public class ListTagsQueryService(AppDbContext _context) : IListTagsQueryService
{
  public async Task<IEnumerable<string>> ListAsync()
  {
    return await _context.Tags
      .AsNoTracking()
      .Select(t => t.Name)
      .ToListAsync();
  }
}
