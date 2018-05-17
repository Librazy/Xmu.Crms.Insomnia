using Xmu.Crms.Services.ViceVersa;
using Xmu.Crms.Services.ViceVersa.Daos;
using Xmu.Crms.Services.ViceVersa.Services;
using Xmu.Crms.Shared.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ViceVersaExtensions
    {
        // 为每一个你写的Service写一个这样的函数，把 UserService 替换为你实现的 Service
        public static IServiceCollection AddViceVersaClassService(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<IClassService, ClassService>();

        // 为每一个你写的Dao写一个这样的函数，把 UserDao 替换为你实现的 Dao
        public static IServiceCollection AddViceVersaClassDao(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<IClassDao, ClassDao>();

        public static IServiceCollection AddViceVersaGradeService(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<IGradeService, GradeService>();

        public static IServiceCollection AddViceVersaGradeDao(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<IGradeDao, GradeDao>();

        public static IServiceCollection AddViceVersaCourseService(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<ICourseService, CourseService>();

        public static IServiceCollection AddViceVersaCourseDao(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<ICourseDao, CourseDao>();
    }
}