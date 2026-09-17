using Applr.RestApi.Data;
using Applr.RestApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace Applr.RestApi.Repositories;

public sealed class CompanyRepository(ApplrDbContext dbContext) : ICompanyRepository
{
    public Task<Company?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
        dbContext.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default) =>
        dbContext.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name, cancellationToken);

    public async Task<Company> CreateAsync(Company company, CancellationToken cancellationToken = default)
    {
        dbContext.Companies.Add(company);
        await dbContext.SaveChangesAsync(cancellationToken);
        return company;
    }
}
