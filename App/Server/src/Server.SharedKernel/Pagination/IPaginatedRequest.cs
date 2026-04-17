namespace Server.SharedKernel.Pagination;

public interface IPaginatedRequest
{
  int Limit { get; set; }

  int Offset { get; set; }
}
