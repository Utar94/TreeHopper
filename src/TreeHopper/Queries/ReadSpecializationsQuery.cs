using CsvHelper;
using MediatR;
using TreeHopper.Models;

namespace TreeHopper.Queries;

internal record ReadSpecializationsQuery(string path, Encoding encoding) : IRequest<IReadOnlyCollection<Specialization>>;

internal class ReadSpecializationsQueryHandler : IRequestHandler<ReadSpecializationsQuery, IReadOnlyCollection<Specialization>>
{
  public Task<IReadOnlyCollection<Specialization>> Handle(ReadSpecializationsQuery query, CancellationToken cancellationToken)
  {
    using StreamReader reader = new(query.path, query.encoding);
    using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
    IReadOnlyCollection<Specialization> talents = csv.GetRecords<Specialization>().ToArray().AsReadOnly();
    return Task.FromResult(talents);
  }
}
