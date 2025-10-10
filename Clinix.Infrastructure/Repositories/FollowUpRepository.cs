//using System;
//using Clinix.Application.Interfaces;
//using Clinix.Application.Interfaces.RepoInterfaces;
//using Clinix.Domain.Entities.ApplicationUsers;
//using Clinix.Domain.Entities.FollowUps;
//using Clinix.Infrastructure.Data;
//using Clinix.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;

//namespace Clinix.Infrastructure.Repositories
//    {
//    public class FollowUpRepository : IFollowUpRepository
//        {
//        private readonly ClinixDbContext _db;
//        public FollowUpRepository(ClinixDbContext db) => _db = db;

//        public async Task AddAsync(FollowUp f, CancellationToken ct = default)
//            {
//            _db.FollowUps.Add(f);
//            await _db.SaveChangesAsync(ct);
//            }

//        public async Task<FollowUp?> GetByIdAsync(long id, CancellationToken ct = default) =>
//            await _db.FollowUps.FirstOrDefaultAsync(x => x.Id == id, ct);

//        public async Task UpdateAsync(FollowUp f, CancellationToken ct = default)
//            {
//            _db.FollowUps.Update(f);
//            await _db.SaveChangesAsync(ct);
//            }

//        public async Task<List<FollowUp>> GetPendingFollowUpsBeforeAsync(DateTime utcNow, CancellationToken ct = default) =>
//            await _db.FollowUps.Where(f => f.Status == FollowUpStatus.Scheduled && f.ScheduledAtUtc <= utcNow).ToListAsync(ct);
//        }
//    }
