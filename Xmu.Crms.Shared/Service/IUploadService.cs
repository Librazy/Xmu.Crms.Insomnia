using System.Threading.Tasks;

namespace Xmu.Crms.Shared.Service
{
    internal interface IUploadService : Orleans.IGrainWithGuidKey
    {
        /// <summary>
        ///     上传选课名单
        ///     老师上传本班级的学生名单
        /// </summary>
        /// <param name="classId">班级Id</param>
        /// <param name="pathName">文件路径</param>
        Task UploadRoster(long classId, string pathName);

        /// <summary>
        ///     上传小组报告
        ///     上传讨论课的报告
        /// </summary>
        /// <param name="seminaId">讨论课Id</param>
        /// <param name="pathName">文件路径</param>
        Task UploadReport(long seminaId, string pathName);

        /// <summary>
        ///     上传用户头像名单
        ///     上传用户头像
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="pathName">文件路径</param>
        Task UploadAvater(long userId, string pathName);
    }
}