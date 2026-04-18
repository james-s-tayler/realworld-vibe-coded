namespace Server.SharedKernel.Pagination;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount);
