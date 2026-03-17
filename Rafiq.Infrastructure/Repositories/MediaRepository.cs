using Microsoft.EntityFrameworkCore;
using Rafiq.Application.Interfaces.Repositories;
using Rafiq.Domain.Entities;
using Rafiq.Infrastructure.Data;

namespace Rafiq.Infrastructure.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly AppDbContext _context;

    public MediaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Media media, CancellationToken cancellationToken = default)
    {
        await _context.Media.AddAsync(media, cancellationToken);
    }

    public async Task<Media?> GetByIdAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        return await _context.Media
            .Where(x => !x.IsDeleted)
            .FirstOrDefaultAsync(x => x.Id == mediaId, cancellationToken);
    }

    public async Task<(IReadOnlyCollection<Media> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Media.Where(x => !x.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.UploadedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task SoftDeleteAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        var media = await _context.Media
            .FirstOrDefaultAsync(x => x.Id == mediaId && !x.IsDeleted, cancellationToken);

        if (media is null)
        {
            return;
        }

        media.IsDeleted = true;
        _context.Media.Update(media);
    }

    public async Task<bool> IsLinkedToExerciseAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        return await _context.Exercises
            .AnyAsync(x => x.MediaId == mediaId, cancellationToken);
    }

    public async Task<bool> IsLinkedToSessionAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        return await _context.Sessions
            .AnyAsync(x => x.MediaId == mediaId, cancellationToken);
    }

    public async Task<bool> IsLinkedToMedicalReportAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalReports
            .AnyAsync(x => x.MediaId == mediaId, cancellationToken);
    }

    public IQueryable<Media> Query()
    {
        return _context.Media.Where(x => !x.IsDeleted);
    }
}
