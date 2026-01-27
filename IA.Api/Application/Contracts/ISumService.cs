using IA.Api.Domain.Models;

namespace IA.Api.Application.Contracts;

public interface ISumService
{
    SumResult Sum(int firstValue, int secondValue);
}
