namespace GameHub.DAL.Common;
public static class BaseEntityExtension
{
    public static void InitializeAudit(this BaseEntity entity, int? userId = null)
    {
        // Don't set CreatedAt - let the database default (GETDATE()) handle it
        // entity.CreatedAt = DateTime.Now; // Commented out to avoid SQL datetime overflow
        entity.CreatedBy = userId;
        //entity.IsActive = true; // This is handled by database default, but safe to set explicitly
    }

    public static void UpdateAudit(this BaseEntity entity, int? userId = null, BaseEntity? oldEntity = null)
    {
        if (oldEntity != null)
        {
            entity.CreatedAt = oldEntity.CreatedAt;
            entity.CreatedBy = oldEntity.CreatedBy;
        }
        // Only set UpdatedAt when actually updating
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = userId;
    }
}