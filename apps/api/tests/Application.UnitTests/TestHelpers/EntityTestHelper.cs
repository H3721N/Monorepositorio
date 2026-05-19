using Domain.Common;

namespace Application.UnitTests.TestHelpers;

internal static class EntityTestHelper
{
    public static T WithId<T>(this T entity, int id)
        where T : BaseEntity
    {
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .SetValue(entity, id);

        return entity;
    }
}
