using IA.Api.Application.Contracts;
using IA.Api.Domain.Models;

namespace IA.Api.Application.Services;

public class SumService : ISumService
{
    public SumResult Sum(int firstValue, int secondValue)
    {
        var total = firstValue + secondValue;

        return new SumResult(firstValue, secondValue, total);

    }
}
