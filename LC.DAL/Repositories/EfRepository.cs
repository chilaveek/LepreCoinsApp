using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using LepreCoins.Models;
using Microsoft.EntityFrameworkCore;

namespace LC.DAL.Repositories
{
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly FamilybudgetdbContext context;

        public EfRepository(FamilybudgetdbContext context)
        {
            this.context = context;
        }

        public async Task<T> GetByIdAsync(int id) =>
            await context.Set<T>().FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await context.Set<T>().ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Func<T, bool> predicate) =>
            await Task.FromResult(context.Set<T>().AsNoTracking().Where(predicate).ToList());

        public async Task AddAsync(T entity)
        {
            await context.Set<T>().AddAsync(entity);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
