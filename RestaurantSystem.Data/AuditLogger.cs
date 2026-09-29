using System;
using System.Linq;
using RestaurantSystem.Domain.Models;

namespace RestaurantSystem.Data;

public static class AuditLogger
{
    public static void Log(AppDbContext db, int userId, string actionType, string entityName, string description)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = userId,
                ActionType = actionType,
                EntityName = entityName,
                Description = description,
                Timestamp = DateTime.Now
            };
            db.AuditLogs.Add(log);
            // Notice we do NOT call db.SaveChanges() here, as this method is usually called within an existing transaction or just before SaveChanges.
        }
        catch
        {
            // Ignore audit log errors so we don't break the main transaction if something minor goes wrong.
        }
    }
}
