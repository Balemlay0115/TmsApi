namespace TmsApi.Infrastructure.Caching;

public static class CacheKeys
{
    private const string SchemaVersion = "v2";
    public const string CoursesTag = "courses";

    public static string CourseById(int id) => $"{SchemaVersion}:courses:id:{id}";
    public static string CourseByCode(string code) => $"{SchemaVersion}:courses:code:{code}";

    public static string CoursesPaged(int page, int pageSize, string? search, string? orderBy, bool descending) =>
        $"{SchemaVersion}:courses:page={page}:size={pageSize}:search={search ?? "none"}:sort={orderBy ?? "default"}:desc={descending}";
}