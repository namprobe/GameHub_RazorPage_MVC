namespace GameHub.DAL.Common;
public static class BaseEntityExtension
{
    public static void InitializeAudit(this BaseEntity entity, int? userId = null)
    {
        entity.CreatedAt = DateTime.Now;
        entity.CreatedBy = userId;
    }

    public static void UpdateAudit(this BaseEntity entity, int? userId = null, BaseEntity? oldEntity = null)
    {
        if (oldEntity != null)
        {
            entity.CreatedAt = oldEntity.CreatedAt;
            entity.CreatedBy = oldEntity.CreatedBy;
        }
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = userId;
    }
}