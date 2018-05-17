using Xmu.Crms.Services.HighGrade;
using Xmu.Crms.Shared.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class InsomniaExtensions
    {
        public static IServiceCollection AddHighGradeSeminarService(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<ISeminarService, SeminarService>();

        public static IServiceCollection AddHighGradeSchoolService(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<ISchoolService, SchoolService>();

        public static IServiceCollection AddHighGradeLoginService(this IServiceCollection serviceCollection) =>
            serviceCollection.AddScoped<ILoginService, LoginService>();
    }
}