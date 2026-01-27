using Core.Interfaces;
using Data.Context; // Lưu ý namespace này
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly QuanLyKhoContext _context;

        public GenericRepository(QuanLyKhoContext context)
        {
            _context = context;
        }

        public void Add(T entity) => _context.Set<T>().Add(entity);
        public void AddRange(IEnumerable<T> entities) => _context.Set<T>().AddRange(entities);
        public IEnumerable<T> Find(Expression<Func<T, bool>> expression) => _context.Set<T>().Where(expression);
        public IEnumerable<T> GetAll() => _context.Set<T>().ToList();
        public T GetById(object id) => _context.Set<T>().Find(id);
        public void Remove(T entity) => _context.Set<T>().Remove(entity);
        public void RemoveRange(IEnumerable<T> entities) => _context.Set<T>().RemoveRange(entities);
        public void Update(T entity) => _context.Set<T>().Update(entity);
    }
}