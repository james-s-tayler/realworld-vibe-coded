namespace Server.SharedKernel.Pagination;

public record PaginatedResponse<TItem>(IReadOnlyList<TItem> Items, int Count);
