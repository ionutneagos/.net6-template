﻿using Domain.Base;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Common
{
    internal static class ContextHelper
    {
        public static int SaveChanges(DbContext context, dynamic? userId)
        {
            UpdateAuditEntries(context, userId);
            return context.SaveChanges();
        }

        public static Task<int> SaveChangesAsync(DbContext context, dynamic? userId, CancellationToken cancellationToken = default)
        {
            UpdateAuditEntries(context, userId);
            return context.SaveChangesAsync(cancellationToken);
        }

        public static void Rollback(DbContext context)
        {
            var changedEntries = context.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Modified))
            {
                entry.CurrentValues.SetValues(entry.OriginalValues);
                entry.State = EntityState.Unchanged;
            }
            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Detached))
            {
                entry.State = EntityState.Unchanged;
            }
            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Added))
            {
                entry.State = EntityState.Detached;
            }
            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Deleted))
            {
                entry.State = EntityState.Unchanged;
            }
        }

        #region Private Methods
        private static void UpdateAuditEntries(DbContext context, dynamic? userId)
        {
            var changedEntries = context.ChangeTracker.Entries<AuditEntity>()
                                       .Where(a => a.State != EntityState.Unchanged).ToList();

            changedEntries.ForEach(changedEntry =>
            {
                var entity = changedEntry.Entity;
                var state = changedEntry.State;
                switch (state)
                {
                    case EntityState.Added:
                        {
                            entity.CreatedDate = DateTime.Now.ToUniversalTime();
                            entity.CreatedBy = userId ?? string.Empty;
                            break;
                        }
                    case EntityState.Modified:
                        {
                            entity.UpdatedDate = DateTime.Now.ToUniversalTime();
                            entity.UpdatedBy = userId ?? string.Empty;
                            break;
                        }
                    case EntityState.Detached:
                        {
                            context.Entry(entity).State = EntityState.Modified;
                            context.Entry(entity).Reload();
                            entity.UpdatedBy = userId ?? string.Empty;
                            break;
                        }
                    case EntityState.Deleted:
                        break;
                }
            });
        }
        #endregion
    }
}
