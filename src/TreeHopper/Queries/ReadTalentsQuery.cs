using CsvHelper;
using MediatR;
using TreeHopper.Models;

namespace TreeHopper.Queries;

internal record ReadTalentsQuery(string path, Encoding encoding) : IRequest<IReadOnlyCollection<Talent>>;

internal class ReadTalentsQueryHandler : IRequestHandler<ReadTalentsQuery, IReadOnlyCollection<Talent>>
{
  public Task<IReadOnlyCollection<Talent>> Handle(ReadTalentsQuery query, CancellationToken cancellationToken)
  {
    using StreamReader reader = new(query.path, query.encoding);
    using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
    IReadOnlyCollection<Talent> talents = csv.GetRecords<Talent>().ToArray().AsReadOnly();
    return Task.FromResult(talents);
  }
}
